using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// BLE 필요한 Using 파일
using Windows.Storage.Streams;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace MiBand_BLEManager {
  public class MiBand_BluetoothManager {

    static List<DeviceInformation> _deviceList = new List<DeviceInformation>();
    public static BluetoothLEDevice _selectedDevice = null;
    static TimeSpan _timeout = TimeSpan.FromSeconds(3);

    // "Magic" string for all BLE devices
    static string _aqsAllBLEDevices = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";
    static string[] _requestedBLEProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.Bluetooth.Le.IsConnectable", };

    //MiBand device = null;

    public MiBand_BluetoothManager() {
      _deviceList.Clear();
      LoadBluetoothDeviceList();
    }



    public void reLoadBluetoothDeviceList() {
      LoadBluetoothDeviceList();
    }

    public List<DeviceInformation> DeviceList { get { return _deviceList; } }

    public BluetoothLEDevice Device { get { return _selectedDevice; } }

    // 블루투스 리스트 가져오기.
    public async void LoadBluetoothDeviceList() {
      var watcher = DeviceInformation.CreateWatcher(_aqsAllBLEDevices, _requestedBLEProperties, DeviceInformationKind.AssociationEndpoint);

      watcher.Added += (DeviceWatcher sender, DeviceInformation devInfo) => {
        if (_deviceList.FirstOrDefault(d => d.Id.Equals(devInfo.Id) || d.Name.Equals(devInfo.Name)) == null) _deviceList.Add(devInfo);
      };

      watcher.Updated += (_, __) => { }; // We need handler for this event, even an empty!
                                         //Watch for a device being removed by the watcher
                                         //watcher.Removed += (DeviceWatcher sender, DeviceInformationUpdate devInfo) =>
                                         //{
                                         //    _deviceList.Remove(FindKnownDevice(devInfo.Id));
                                         //};
      watcher.EnumerationCompleted += (DeviceWatcher sender, object arg) => { sender.Stop(); };
      watcher.Stopped += (DeviceWatcher sender, object arg) => { _deviceList.Clear(); sender.Start(); };
      watcher.Start();
    }

    public delegate void ConnectBLEDevice(bool value);

    public event ConnectBLEDevice ConnectChanged;

    // 해당 기기와 연결을 한다.
    public async void OpenDevice(string deviceName) {
      // 해당 deviceID를 기반으로 연결을 시도 한다.
      _selectedDevice = await BluetoothLEDevice.FromIdAsync(deviceName).AsTask();
      // 연결 확인을 처리한다.
      var result = await _selectedDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);
      if (result.Status == GattCommunicationStatus.Success) {
        ConnectChanged?.Invoke(true);
        System.Diagnostics.Debug.WriteLine($"device Name : {deviceName} Connect..!");
        // 연결 성공시 해당 기기와 Auth를 진행 한다.
      } else {
        // 연결 실패시..
        ConnectChanged?.Invoke(false);
        System.Diagnostics.Debug.WriteLine($"device Name : {deviceName} Not Connect..!");
      }

    }

    // 해당 기기의 HeartRate를 가져온다.
    public async void getHeartRate() {
      //heartrate.StartMonitorHeartrate(this);
    }

    async public void Write(GattCharacteristic c, byte[] data) {
      using (var stream = new DataWriter()) {
        stream.WriteBytes(data);

        try {
          GattCommunicationStatus result = await c.WriteValueAsync(stream.DetachBuffer());
          if (result != GattCommunicationStatus.Success) {
            Console.WriteLine(String.Format("Write {0} on {1} failed !", BitConverter.ToString(data), c.Uuid));
          }
        } catch (Exception e) {
          Console.WriteLine(e.Message);
        }
      }
    }



  }
}
