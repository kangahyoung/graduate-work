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
// 클래스 파일 로드
using MiBand_BLEManager;


namespace WindowsFormsApp1 {
  public partial class Form1 : Form {

    // 블루투스 연결을 위한 블루투스 클래스
    MiBand_BluetoothManager _deviceManager = new MiBand_BluetoothManager();
    // 미밴드 작동 처리를 위한 클래스
    MiBand device = null;

    // Auth 성공여부를 판단하여 미밴드 클래스를 넘겨주는 delegate
    delegate void OnAuthHandler(MiBand d, bool s);
    // delegate 처리 함수
    void OnAuth(MiBand d, bool s) {
      if (InvokeRequired) {
        OnAuthHandler c = new OnAuthHandler(OnAuth);
        Invoke(c, new object[] { d, s });
      } else {
        if (s) {
          // 연결에 성공 했을시
          button4.Enabled = true;
        } else {
          MessageBox.Show("Auth failed !", "Device error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }
    }

    // Heartrate 를 반환해주는 delegate
    delegate void OnHeartrateChangeHandler(MiBand d, UInt16 v);
    // delegate 처리 함수
    void OnHeartrateChange(MiBand d, UInt16 v) {
      if (InvokeRequired) {
        OnHeartrateChangeHandler c = new OnHeartrateChangeHandler(OnHeartrateChange);
        Invoke(c, new object[] { d, v });
      } else {
        listBox3.Items.Add(String.Format("{0} bpm", v));
      }
    }

    // 블루투스 연결 확인 결과를 알려주는 delegate
    delegate void ConnectBLEDevice(bool v);
    // delegate 처리 함수
    void ConnectChange(bool v) {
      if (InvokeRequired) {
        ConnectBLEDevice c = new ConnectBLEDevice(ConnectChange);
        Invoke(c, new object[] { v });
      } else {

        if ( v== true) {
          // 연결에 성공 했을시
          button3.Enabled = true;
        } else {
          // 연결에 실패 했을시
          button2.Enabled = true;
        }

      }
    }

    public Form1() {
      InitializeComponent();
    }

    private async void button1_Click(object sender, EventArgs e) {
      // 블루투스 리스트 가져오기
      var bleList = _deviceManager.DeviceList;

      // 리스트 추가 전 클리어
      comboBox1.Items.Clear();
      listBox2.Items.Clear();

      // 리스트가 있으면 추가를 한다.
      if (bleList.Count != 0) {
        for (int i = 0; i < bleList.Count(); i++) {
          if (bleList[i].Name != "") {
            comboBox1.Items.Add(bleList[i].Name);// 블루투스 이름
            //listBox1.Items.Add(bleList[i].Name);    // 블루투스 이름
            listBox2.Items.Add(bleList[i].Id);        // 블루투스 주소
            System.Diagnostics.Debug.WriteLine($"device ID : {bleList[i].Id}");
          }
        }
      } else {
        // 블루투스 리스트가 없으면 다시 로드 한다.
        _deviceManager.reLoadBluetoothDeviceList();
        button1_Click(sender, e);
      }


    }

    private async void button2_Click(object sender, EventArgs e) {
      // 선택된 리스트박스1 의 위치 값을 가져온다.
      var listid = comboBox1.SelectedIndex;
      // 블루투스 주소를 연결하기 위하여 전달을 해준다.
      _deviceManager.OpenDevice(listBox2.Items[listid].ToString());
      _deviceManager.ConnectChanged += ConnectChange;
      button2.Enabled = false;
    }

    private async void button3_Click(object sender, EventArgs e) {
      // 기기 인증
      device = new MiBand(_deviceManager);
      device.Authenticate(OnAuth);
      button3.Enabled = false;
    }

    private void button4_Click(object sender, EventArgs e) {
      device.StartMonitorHeartrate();
      device.HeartrateChanged += OnHeartrateChange;
      button4.Enabled = false;
      button5.Enabled = true;
    }

    private void button5_Click(object sender, EventArgs e) {
      device.StopMonitorHeartrate();
      device.HeartrateChanged -= OnHeartrateChange;
      button4.Enabled = true;
      button5.Enabled = false;
    }


  }

}
