using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
// BLE 필요한 Using 파일
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace WindowsFormsApp1 {
  public partial class Form1 : Form {

    static bool _doWork = true;
    static string CLRF = (Console.IsOutputRedirected) ? "" : "\r\n";

    static List<DeviceInformation> _deviceList = new List<DeviceInformation>();
    static BluetoothLEDevice _selectedDevice = null;
    static TimeSpan _timeout = TimeSpan.FromSeconds(3);

    // "Magic" string for all BLE devices
    static string _aqsAllBLEDevices = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";
    static string[] _requestedBLEProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.Bluetooth.Le.IsConnectable", };

    public Form1() {
      InitializeComponent();
    }

    // 블루투스 리스트 가져오기
    static async Task LoadBLEList(string[] args) {
      // 블루투스 리스트 가져오기 시작.
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

    // 디바이스 연결
    static async Task<int> OpenDevice(string deviceName) {
      int retVal = 0;
      // 해당 deviceID를 기반으로 연결을 시도 한다.
      _selectedDevice = await BluetoothLEDevice.FromIdAsync(deviceName).AsTask().TimeoutAfter(_timeout);
      // 연결 결과 및 서비스 등을 가져 온다.
      var result = await _selectedDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);
      if (result.Status == GattCommunicationStatus.Success) {
        for (int i = 0; i < result.Services.Count; i++) {

        }
      }

      return retVal;
    }
    private async void button1_Click(object sender, EventArgs e) {
      // 블루투스 리스트 가져오기
      string[] args = new string[0];
      var BLELoadTask = LoadBLEList(args);
      BLELoadTask.Wait();

      // 리스트 추가 전 클리어
      listBox1.Items.Clear();
      listBox2.Items.Clear();

      // 리스트가 있으면 추가를 한다.
      if (_deviceList.Count != 0) {
        for (int i = 0; i < _deviceList.Count(); i++) {
          if (_deviceList[i].Name != "") {
            listBox1.Items.Add(_deviceList[i].Name);    // 블루투스 이름
            listBox2.Items.Add(_deviceList[i].Id);        // 블루투스 주소
          }

        }

      }

    }



    private async void button2_Click(object sender, EventArgs e) {
      // 선택된 리스트박스1 의 위치 값을 가져온다.
      var listid = listBox1.SelectedIndex;
      // 블루투스 주소를 연결하기 위하여 전달을 해준다.
      var OpenDeviceTask = OpenDevice(listBox2.Items[listid].ToString());
      // var items = await OpenDeviceTask;

    }


  }

}
