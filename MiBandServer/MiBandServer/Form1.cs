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
using static publicPacketProtocol.PacketProtocol;

namespace WindowsFormsApp1 {
  public partial class Form1 : Form {

    // 블루투스 연결을 위한 블루투스 클래스
    MiBand_BluetoothManager _deviceManager = new MiBand_BluetoothManager();
    // 미밴드 작동 처리를 위한 클래스
    MiBand device = null;
    // 서버를 만든다.
    AsynchronousSocketServer asyncServer = new AsynchronousSocketServer();

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
          publicPostData serverPostData = new publicPostData();
          serverPostData.data = (int)ServerStatus.BLEAuthComplete;
          serverPostData.packet_Type = (int)ServerPacketType.SendPacket;
          AsynchronousSocketServer.Send(asyncServer.getSocket(), Packet.Serialize(serverPostData));
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

        ServerBPMData serverPostData = new ServerBPMData();
        serverPostData.nowBPM = (int)v;
        serverPostData.packet_Type = (int)ServerPacketType.BPMData;
        AsynchronousSocketServer.Send(asyncServer.getSocket(), Packet.Serialize(serverPostData));

        //asyncServer.SendPacket(v, asyncServer.GetSocket);
      }
    }


    // PacketType이 변경 되었는지 반환 해준다.
    delegate void ChangePacketType(int packet_type, byte[] buffer);
    void ChangedPacketType(int packet_type, byte[] buffer) {
      if (InvokeRequired) {
        ChangePacketType c = new ChangePacketType(ChangedPacketType);
        Invoke(c, new object[] { packet_type, buffer });
      } else {
        //Console.WriteLine("변화가 있다..! : " + packet_type);
        ProcessPacket(packet_type, buffer);
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

        if (v == true) {
          // 연결에 성공 했을시
          button3.Enabled = true;
          publicPostData serverPostData = new publicPostData();
          serverPostData.data = (int)ServerStatus.BLEConnectComplete;
          serverPostData.packet_Type = (int)ServerPacketType.SendPacket;
          AsynchronousSocketServer.Send(asyncServer.getSocket(), Packet.Serialize(serverPostData));

        } else {
          // 연결에 실패 했을시
          button2.Enabled = true;
        }

      }
    }

    public Form1() {
      InitializeComponent();
      //ServerThread = new Thread(new ThreadStart(asyncServer.Server_Start));
      //ServerThread.Start();
      asyncServer.Server_Start();
      AsynchronousSocketServer.PacketTypeChanged += ChangedPacketType;
    }

    private void ProcessPacket(int packet_Type, byte[] buffer) {

      switch (packet_Type) {
        case (int)ClientPacketType.LoadBLEList: {
            Console.WriteLine("LoadBLEList");
            while (true) {
              if (comboBox1.Items.Count != 0)
                break;
              BLEList();
            }

            ServerBLEList serverPostData = new ServerBLEList();
            serverPostData.BLEList = new List<string>();
            foreach (var data in comboBox1.Items) {
              serverPostData.BLEList.Add(data.ToString());
            }
            serverPostData.packet_Type = (int)ServerPacketType.BLEList;
            AsynchronousSocketServer.Send(asyncServer.getSocket(), Packet.Serialize(serverPostData));
          }
          break;

        case (int)ClientPacketType.ConnectBLE: {
            Console.WriteLine("ConnectBLE");
            publicPostData receiveclass = (publicPostData)Packet.Deserialize(buffer);
            comboBox1.SelectedIndex = receiveclass.data;
            ConnectBLE();
          }
          break;

        case (int)ClientPacketType.AuthBLE: {
            Console.WriteLine("AuthBLE");
            AuthBLE();
          }
          break;

        case (int)ClientPacketType.StartBLE: {
            Console.WriteLine("StartBLE");
            StartBPM();
          }
          break;

        case (int)ClientPacketType.StopBLE: {
            Console.WriteLine("StopBLE");
            StopBPM();
          }
          break;

        case (int)ClientPacketType.SendPacket: {
            Console.WriteLine("SendPacket");
            //  딱히 쓸일 없을 듯 지금은..!
            publicPostData receiveclass = (publicPostData)Packet.Deserialize(buffer);

          }
          break;
      }
    }

    private void BLEList() {
      var bleList = _deviceManager.DeviceList;

      // 리스트 추가 전 클리어
      comboBox1.Items.Clear();
      listBox2.Items.Clear();

      // 리스트가 있으면 추가를 한다.
      if (bleList.Count != 0) {
        for (int i = 0; i < bleList.Count(); i++) {
          if (bleList[i].Name != "" && bleList[i] != null) {
            comboBox1.Items.Add(bleList[i].Name);// 블루투스 이름
                                                 //listBox1.Items.Add(bleList[i].Name);    // 블루투스 이름
            listBox2.Items.Add(bleList[i].Id);        // 블루투스 주소
            System.Diagnostics.Debug.WriteLine($"device ID : {bleList[i].Id}");
          }
        }
      } else {
        // 블루투스 리스트가 없으면 다시 로드 한다.
        _deviceManager.reLoadBluetoothDeviceList();
      }
    }

    private void ConnectBLE() {
      // 선택된 리스트박스1 의 위치 값을 가져온다.
      var listid = comboBox1.SelectedIndex;
      // 블루투스 주소를 연결하기 위하여 전달을 해준다.
      _deviceManager.OpenDevice(listBox2.Items[listid].ToString());
      _deviceManager.ConnectChanged += ConnectChange;
      button2.Enabled = false;
    }

    private void AuthBLE() {
      // 기기 인증
      device = new MiBand(_deviceManager);
      device.Authenticate(OnAuth);
      button3.Enabled = false;
    }

    private void StartBPM() {
      device.StartMonitorHeartrate();
      device.HeartrateChanged += OnHeartrateChange;
      button4.Enabled = false;
      button5.Enabled = true;
    }

    private void StopBPM() {
      device.StopMonitorHeartrate();
      device.HeartrateChanged -= OnHeartrateChange;
      button4.Enabled = true;
      button5.Enabled = false;
    }

    private async void button1_Click(object sender, EventArgs e) {
      while (true) {
        if (comboBox1.Items.Count != 0)
          break;
        BLEList();
      }
    }

    private async void button2_Click(object sender, EventArgs e) {
      ConnectBLE();
    }

    private async void button3_Click(object sender, EventArgs e) {
      AuthBLE();
    }

    private void button4_Click(object sender, EventArgs e) {
      StartBPM();
    }

    private void button5_Click(object sender, EventArgs e) {
      StopBPM();
    }

    private void Form1_Load(object sender, EventArgs e) {
      // 폼 최소화로 실행 시키기.
      this.WindowState = FormWindowState.Minimized;
    }

    private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
      Application.ExitThread();
      Environment.Exit(0);
      Application.Exit();
    }

  }

}
