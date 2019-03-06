﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;
//
using MiBand_BLEManager;

namespace MiBand_Auth {
  public class AuthHelpers {
    private static Guid AUTH_SERVICE = new Guid("0000FEE1-0000-1000-8000-00805F9B34FB");
    private static Guid AUTH_CHARACTERISTIC = new Guid("00000009-0000-3512-2118-0009af100700");
    private static byte[] AUTH_SECRET_KEY = new byte[] { 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45 };

    private static EventWaitHandle _WaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
    private static GattCharacteristic _AuthCharacteristic = null;
    MiBand_BluetoothManager BLE = null;
    public MiBand_BluetoothManager Device { get { return BLE; } }

    public AuthHelpers() {
    }

    /// <summary>
    /// Check is band reached auth level 1 (tap on the band)
    /// </summary>
    /// <returns></returns>
    public bool IsAuthenticated() {
      return false;
    }


    public async void Authenticate(MiBand_BluetoothManager GetBLE) {
      BLE = GetBLE;
      _AuthCharacteristic = await GetCharacteristicByServiceUuid(BLE.Device, AUTH_SERVICE, AUTH_CHARACTERISTIC);
      await _AuthCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);

      if (!IsAuthenticated()) {
        Debug.WriteLine("Level 1 started");

        List<byte> sendKey = new List<byte>();
        sendKey.Add(0x01);
        sendKey.Add(0x08);
        sendKey.AddRange(AUTH_SECRET_KEY);

        if (await _AuthCharacteristic.WriteValueAsync(sendKey.ToArray().AsBuffer()) == GattCommunicationStatus.Success) {
          Debug.WriteLine("Level 1 success");
          _AuthCharacteristic.ValueChanged += AuthCharacteristic_ValueChanged;
        }
      } else {
        Debug.WriteLine("Already authorized (Level 1 successful)");
        Debug.WriteLine("Level 2 started");

        if (await SendAuthNumberAsync())
          Debug.WriteLine("Level 2 success");

        _AuthCharacteristic.ValueChanged += AuthCharacteristic_ValueChanged;
      }

      _WaitHandle.WaitOne();
    }

    /// <summary>
    /// AuthCharacteristic handler. Checking input requests to device from Band
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private async void AuthCharacteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args) {
      if (sender.Uuid == AUTH_CHARACTERISTIC) {
        List<byte> request = args.CharacteristicValue.ToArray().ToList();
        byte authResponse = 0x10;
        byte authSendKey = 0x01;
        byte authRequestRandomAuthNumber = 0x02;
        byte authRequestEncryptedKey = 0x03;
        byte authSuccess = 0x01;
        byte authFail = 0x04;

        if (request[2] == authFail)
          Debug.WriteLine("Authentication error");

        if (request[0] == authResponse && request[1] == authSendKey && request[2] == authSuccess) {
          Debug.WriteLine("Level 2 started");

          if (await SendAuthNumberAsync())
            Debug.WriteLine("Level 2 success");
        } else if (request[0] == authResponse && request[1] == authRequestRandomAuthNumber && request[2] == authSuccess) {
          Debug.WriteLine("Level 3 started");

          if (await SendEncryptedRandomKeyAsync(args))
            Debug.WriteLine("Level 3 success");
        } else if (request[0] == authResponse && request[1] == authRequestEncryptedKey && request[2] == authSuccess) {
          Debug.WriteLine("Authentication completed");
          //_LocalSettings.Values["isAuthorized"] = true;
        }

        _WaitHandle.Set();
      }
    }

    /// <summary>
    /// Encrypt Secret key and last 16 bytes from response in AES/ECB/NoPadding Encryption.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private byte[] Encrypt(byte[] data) {
      byte[] secretKey = new byte[] { 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45 };
      IBuffer key = secretKey.AsBuffer();
      SymmetricKeyAlgorithmProvider algorithmProvider = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesEcb);
      CryptographicKey ckey = algorithmProvider.CreateSymmetricKey(key);

      IBuffer buffEncrypt = CryptographicEngine.Encrypt(ckey, data.AsBuffer(), null);
      return buffEncrypt.ToArray();
    }

    /// <summary>
    /// Sending auth number to band (Auth Level 2)
    /// </summary>
    /// <returns></returns>
    private async Task<bool> SendAuthNumberAsync() {
      Debug.WriteLine("Sending Auth Number");
      List<byte> authNumber = new List<byte>();
      authNumber.Add(0x02);
      authNumber.Add(0x08);

      return await _AuthCharacteristic.WriteValueAsync(authNumber.ToArray().AsBuffer()) == GattCommunicationStatus.Success;
    }

    /// <summary>
    /// Sending Encrypted random key to band (Auth Level 3)
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    private async Task<bool> SendEncryptedRandomKeyAsync(GattValueChangedEventArgs args) {
      // Thanks superhans205 for this solution: https://github.com/superhans205/FitForMiBand/blob/master/ClassesCollection/CustomMiBand.vb#L370
      List<byte> randomKey = new List<byte>();
      List<byte> relevantResponsePart = new List<byte>();
      var responseValue = args.CharacteristicValue.ToArray();

      for (int i = 0; i < responseValue.Count(); i++) {
        if (i >= 3)
          relevantResponsePart.Add(responseValue[i]);
      }

      randomKey.Add(0x03);
      randomKey.Add(0x08);
      randomKey.AddRange(Encrypt(relevantResponsePart.ToArray()));

      return await _AuthCharacteristic.WriteValueAsync(randomKey.ToArray().AsBuffer()) == GattCommunicationStatus.Success;
    }


    public static async Task<GattCharacteristic> GetCharacteristicByServiceUuid(BluetoothLEDevice serviceDevice, Guid serviceUuid, Guid characteristicUuid) {
      if (serviceDevice == null)
        throw new Exception("Cannot get characteristic from service: Device is disconnected.");

      GattDeviceServicesResult service = await serviceDevice.GetGattServicesForUuidAsync(serviceUuid);
      GattCharacteristicsResult currentCharacteristicResult = await service.Services[0].GetCharacteristicsForUuidAsync(characteristicUuid);
      GattCharacteristic characteristic;

      if (currentCharacteristicResult.Status == GattCommunicationStatus.AccessDenied || currentCharacteristicResult.Status == GattCommunicationStatus.ProtocolError) {
        System.Diagnostics.Debug.WriteLine($"Error while getting characteristic: {characteristicUuid.ToString()} - {currentCharacteristicResult.Status}");
        characteristic = null;
      } else {
        characteristic = currentCharacteristicResult.Characteristics[0];
      }

      return characteristic;
    }


  }
}
