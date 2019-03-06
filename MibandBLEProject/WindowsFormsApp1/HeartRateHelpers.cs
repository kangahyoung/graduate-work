using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Foundation;
using Windows.Storage.Streams;
//
using MiBand_BLEManager;

namespace MiBand_HeartRate {
  public class HeartRateHelpers {
    private static int lastHeartRate = 0;

    public int LastHeartRate {
      get { return lastHeartRate; }
    }

    public enum SleepHeartRateMeasurement {
      ENABLE = 1,
      DISABLE = 0
    }

    public enum RealtimeHeartRateMeasurements {
      ENABLE = 1,
      DISABLE = 0
    }

    private EventWaitHandle _WaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

    private Guid HEART_RATE_SERVICE = new Guid("0000180d-0000-1000-8000-00805f9b34fb");
    private Guid HEART_SENSOR_CONTROL = new Guid("00000001-0000-3512-2118-0009af100700");
    private Guid HEART_RATE_MEASUREMENT_CHARACTERISTIC = new Guid("00002a37-0000-1000-8000-00805f9b34fb");
    private Guid HEART_RATE_CONTROLPOINT_CHARACTERISTIC = new Guid("00002a39-0000-1000-8000-00805f9b34fb");
    private byte[] HEART_RATE_START_COMMAND = new byte[] { 21, 2, 1 };
    private GattCharacteristic _heartRateMeasurementCharacteristic;
    private GattCharacteristic _heartRateControlPointCharacteristic;
    private GattCharacteristic HMRcharacteristic = null;
    private GattCharacteristic HMCCharacteristic = null;
    private GattCharacteristic SNSCharacteristic = null;
    Thread pingThread = null;
    MiBand_BluetoothManager BLE = null;

    // getHeartRate 가져오기
    public async void StartMonitorHeartrate(MiBand_BluetoothManager GetBLE) {
      // ble를 복사해준다.
      BLE = GetBLE;
      // Enabling sensor
      GattDeviceServicesResult servicesResult = await BLE.Device.GetGattServicesForUuidAsync(HEART_RATE_SERVICE);
      if (servicesResult.Status == GattCommunicationStatus.Success && servicesResult.Services.Count > 0) {
        GattDeviceService service = servicesResult.Services[0];

        GattCharacteristicsResult characteristicsResult = await service.GetCharacteristicsForUuidAsync(HEART_SENSOR_CONTROL);
        if (characteristicsResult.Status == GattCommunicationStatus.Success && characteristicsResult.Characteristics.Count > 0) {
          GattCharacteristic characteristic = characteristicsResult.Characteristics[0];
          SNSCharacteristic = characteristic;
          BLE.Write(characteristic, new byte[] { 0x01, 0x03, 0x19 });
        }
      }

      // Enabling Heartrate measurements
      servicesResult = await BLE.Device.GetGattServicesForUuidAsync(HEART_RATE_SERVICE);
      if (servicesResult.Status == GattCommunicationStatus.Success && servicesResult.Services.Count > 0) {
        GattDeviceService service = servicesResult.Services[0];
        // Enabling notification
        GattCharacteristicsResult characteristicsResult = await service.GetCharacteristicsForUuidAsync(HEART_RATE_MEASUREMENT_CHARACTERISTIC);
        if (characteristicsResult.Status == GattCommunicationStatus.Success && characteristicsResult.Characteristics.Count > 0) {
          HMRcharacteristic = characteristicsResult.Characteristics[0];

          GattCommunicationStatus status = await HMRcharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
          if (status == GattCommunicationStatus.Success) {

            HMRcharacteristic.ValueChanged += (GattCharacteristic sender, GattValueChangedEventArgs args) => {
              var reader = DataReader.FromBuffer(args.CharacteristicValue);
              UInt16 heartrate = reader.ReadUInt16();

              if (heartrate > 0) { // Other wise there is a high probability that sensor failed to retreive heart rate or that you're dead ;)
                System.Diagnostics.Debug.WriteLine($"HeartRate is {heartrate} bpm");
              }

              //if (!continuousMode)
                BLE.Write(HMCCharacteristic, new byte[] { 0x15, 0x02, 0x01 });
            };

          }
        }

        // Enable measurements
        characteristicsResult = await service.GetCharacteristicsForUuidAsync(HEART_RATE_CONTROLPOINT_CHARACTERISTIC);
        if (characteristicsResult.Status == GattCommunicationStatus.Success && characteristicsResult.Characteristics.Count > 0) {
          HMCCharacteristic = characteristicsResult.Characteristics[0];

            BLE.Write(HMCCharacteristic, new byte[] { 0x15, 0x01, 0x01 });

          if (SNSCharacteristic != null) {
            BLE.Write(SNSCharacteristic, new byte[] { 0x02 });
          }
        }

        // Enable ping HMC every 5 sec.
        pingThread = new Thread(new ThreadStart(RunPingSensor));
        pingThread.Start();

        //HMSService = service;

      }
    }


    void RunPingSensor() {
      while (HMCCharacteristic != null) {
        BLE.Write(HMCCharacteristic, new byte[] { 0x16 });
        Thread.Sleep(5000);
      }
    }






    /// <summary>
    /// Subscribe to HeartRate notifications from band.
    /// </summary>
    public async Task SubscribeToHeartRateNotificationsAsync(BluetoothLEDevice _selectedDevice) {
      _heartRateMeasurementCharacteristic = await GetCharacteristicByServiceUuid(_selectedDevice, HEART_RATE_SERVICE, HEART_RATE_MEASUREMENT_CHARACTERISTIC);

      Debug.WriteLine("Subscribe to HeartRate notifications from band...");
      if (await _heartRateMeasurementCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify) == GattCommunicationStatus.Success)
        _heartRateMeasurementCharacteristic.ValueChanged += HeartRateMeasurementCharacteristicValueChanged;
    }

    /// <summary>
    /// Subscribe to HeartRate notifications from band.
    /// </summary>
    /// <param name="eventHandler">Handler for interact with heartRate values</param>
    /// <returns></returns>
    public async Task SubscribeToHeartRateNotificationsAsync(BluetoothLEDevice _selectedDevice, TypedEventHandler<GattCharacteristic, GattValueChangedEventArgs> eventHandler) {
      _heartRateMeasurementCharacteristic = await GetCharacteristicByServiceUuid(_selectedDevice, HEART_RATE_SERVICE, HEART_RATE_MEASUREMENT_CHARACTERISTIC);

      Debug.WriteLine("Subscribe to HeartRate notifications from band...");
      if (await _heartRateMeasurementCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify) == GattCommunicationStatus.Success)
        _heartRateMeasurementCharacteristic.ValueChanged += eventHandler;
    }

    /// <summary>
    /// Measure current heart rate
    /// </summary>
    /// <returns></returns>
    public async Task<int> GetHeartRateAsync(BluetoothLEDevice _selectedDevice) {
      int heartRate = 0;
      if (await StartHeartRateMeasurementAsync(_selectedDevice) == GattCommunicationStatus.Success)
        heartRate = lastHeartRate;

      return heartRate;
    }

    /// <summary>
    /// Starting heart rate measurement
    /// </summary>
    /// <returns></returns>
    private async Task<GattCommunicationStatus> StartHeartRateMeasurementAsync(BluetoothLEDevice _selectedDevice) {
      var servicesResult = await _selectedDevice.GetGattServicesForUuidAsync(HEART_RATE_SERVICE);
      if (servicesResult.Status == GattCommunicationStatus.Success && servicesResult.Services.Count > 0) {
        GattDeviceService service = servicesResult.Services[0];

        // Enabling notification
        GattCharacteristicsResult characteristicsResult = await service.GetCharacteristicsForUuidAsync(HEART_RATE_MEASUREMENT_CHARACTERISTIC);
        if (characteristicsResult.Status == GattCommunicationStatus.Success && characteristicsResult.Characteristics.Count > 0) {
          var HMRcharacteristic = characteristicsResult.Characteristics[0];

          GattCommunicationStatus status = await HMRcharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
          if (status == GattCommunicationStatus.Success) {




            HMRcharacteristic.ValueChanged += (GattCharacteristic sender, GattValueChangedEventArgs args) => {
              var reader = DataReader.FromBuffer(args.CharacteristicValue);
              System.Diagnostics.Debug.WriteLine($"HeartRate is {reader} bpm");
              UInt16 heartrate = reader.ReadUInt16();
              if (heartrate > 0) { // Other wise there is a high probability that sensor failed to retreive heart rate or that you're dead ;)
                System.Diagnostics.Debug.WriteLine($"HeartRate is {heartrate} bpm");
                //HeartrateChanged?.Invoke(this, heartrate);
              }

              // if (!continuousMode)
              //manager.Write(HMCCharacteristic, new byte[] { 0x15, 0x02, 0x01 });
            };
            //var alwaysEnable = await SetRealtimeHeartRateMeasurement(service, RealtimeHeartRateMeasurements.ENABLE);

          }

        }

      }




      return 0;
      /*
      _heartRateMeasurementCharacteristic = await GetCharacteristicByServiceUuid(_selectedDevice, HEART_RATE_SERVICE, HEART_RATE_MEASUREMENT_CHARACTERISTIC);
      _heartRateControlPointCharacteristic = await GetCharacteristicByServiceUuid(_selectedDevice, HEART_RATE_SERVICE, HEART_RATE_CONTROLPOINT_CHARACTERISTIC);
      GattCommunicationStatus status = GattCommunicationStatus.ProtocolError;

      if (await _heartRateMeasurementCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify) == GattCommunicationStatus.Success) {
        Debug.WriteLine("Checking Heart Rate");

        if (await _heartRateControlPointCharacteristic.WriteValueAsync(HEART_RATE_START_COMMAND.AsBuffer()) == GattCommunicationStatus.Success) {
          _heartRateMeasurementCharacteristic.ValueChanged += HeartRateMeasurementCharacteristicValueChanged;
          status = GattCommunicationStatus.Success;
          _WaitHandle.WaitOne();
        }
      }

      return status;
      */
    }

    /// <summary>
    /// Handle incoming requests with heart rate from the band.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void HeartRateMeasurementCharacteristicValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args) {
      Debug.WriteLine("Getting HeartRate");
      if (sender.Uuid.ToString() == HEART_RATE_MEASUREMENT_CHARACTERISTIC.ToString())
        lastHeartRate = args.CharacteristicValue.ToArray()[1];

      System.Diagnostics.Debug.WriteLine($"HeartRate is {lastHeartRate} bpm");
      _WaitHandle.Set();
    }

    /// <summary>
    /// Set Heart Rate Measurements while sleep
    /// </summary>
    /// <param name="sleepMeasurement"></param>
    /// <returns></returns>
    public async Task<bool> SetHeartRateSleepMeasurement(BluetoothLEDevice _selectedDevice, SleepHeartRateMeasurement sleepMeasurement) {
      _heartRateControlPointCharacteristic = await GetCharacteristicByServiceUuid(_selectedDevice, HEART_RATE_SERVICE, HEART_RATE_CONTROLPOINT_CHARACTERISTIC);
      byte[] command = null;

      switch (sleepMeasurement) {
        case SleepHeartRateMeasurement.ENABLE:
          command = new byte[] { 0x15, 0x00, 0x01 };
          break;

        case SleepHeartRateMeasurement.DISABLE:
          command = new byte[] { 0x15, 0x00, 0x00 };
          break;
      }

      return await _heartRateControlPointCharacteristic.WriteValueAsync(command.AsBuffer()) == GattCommunicationStatus.Success;
    }

    /// <summary>
    /// Set Realtime (Continuous) Heart Rate Measurements
    /// </summary>
    /// <param name="measurements"></param>
    /// <returns></returns>
    public async Task<GattCommunicationStatus> SetRealtimeHeartRateMeasurement(BluetoothLEDevice _selectedDevice, RealtimeHeartRateMeasurements measurements) {
      _heartRateMeasurementCharacteristic = await GetCharacteristicByServiceUuid(_selectedDevice, HEART_RATE_SERVICE, HEART_RATE_MEASUREMENT_CHARACTERISTIC);
      _heartRateControlPointCharacteristic = await GetCharacteristicByServiceUuid(_selectedDevice, HEART_RATE_SERVICE, HEART_RATE_CONTROLPOINT_CHARACTERISTIC);
      GattCommunicationStatus status = GattCommunicationStatus.ProtocolError;

      if (await _heartRateMeasurementCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify) == GattCommunicationStatus.Success) {
        byte[] manualCmd = null;
        byte[] continuousCmd = null;

        switch (measurements) {
          case RealtimeHeartRateMeasurements.ENABLE:
            manualCmd = new byte[] { 0x15, 0x02, 0 };
            continuousCmd = new byte[] { 0x15, 0x01, 1 };
            break;

          case RealtimeHeartRateMeasurements.DISABLE:
            manualCmd = new byte[] { 0x15, 0x02, 1 };
            continuousCmd = new byte[] { 0x15, 0x01, 0 };
            break;
        }

        if (await _heartRateControlPointCharacteristic.WriteValueAsync(manualCmd.AsBuffer()) == GattCommunicationStatus.Success
            && await _heartRateControlPointCharacteristic.WriteValueAsync(continuousCmd.AsBuffer()) == GattCommunicationStatus.Success) {
          status = GattCommunicationStatus.Success;
          _heartRateMeasurementCharacteristic.ValueChanged += HeartRateMeasurementCharacteristicValueChanged;
        }
      }

      return status;
    }

    /// <summary>
    /// Sets Heart Rate Measurement interval in minutes (0 is off)
    /// </summary>
    /// <param name="minutes"></param>
    /// <returns></returns>
    public async Task<bool> SetHeartRateMeasurementInterval(BluetoothLEDevice _selectedDevice, int minutes) {
      _heartRateControlPointCharacteristic = await GetCharacteristicByServiceUuid(_selectedDevice, HEART_RATE_SERVICE, HEART_RATE_CONTROLPOINT_CHARACTERISTIC);
      return await _heartRateControlPointCharacteristic.WriteValueAsync(new byte[] { 0x14, (byte)minutes }.AsBuffer()) == GattCommunicationStatus.Success;
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
