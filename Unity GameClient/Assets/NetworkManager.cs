using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using static publicPacketProtocol.PacketProtocol;
using UnityEngine.UI;
using System.IO;

public class NetworkManager : MonoBehaviour {

  private static NetworkManager _instance = null;
  private static string _nowBPM = "";
  private static List<string> _bleList = new List<string>();

  public static NetworkManager Instance {
    get {
      if (_instance == null) {
        _instance = FindObjectOfType(typeof(NetworkManager)) as NetworkManager;

        if (_instance == null) {
          Debug.LogError("There's no active NetworkManager object");
        }
      }

      return _instance;
    }
  }

  public string NowBPM                  // 현재 BPM 수치 프로퍼티.
   {
    get {
      return _nowBPM;
    }
    set {
      _nowBPM = value;
    }
  }

  public List<string> BleList                  // 블루투스 연결 리스트 프로퍼티.
   {
    get {
      return _bleList;
    }
    set {
      _bleList = value;
    }
  }

  public class StateObject {
    // Client socket.  
    public Socket workSocket = null;
    // Size of receive buffer.  
    public const int BufferSize = 1024 * 4;
    // Receive buffer.  
    public byte[] buffer = new byte[BufferSize];
    // Received data string.  
    public StringBuilder sb = new StringBuilder();
  }

  // The port number for the remote device.  
  private const int port = 11000;
  private readonly static IPAddress iPAddress = IPAddress.Parse("127.0.0.1");
  public static Socket client;

  public delegate void SocketConnect(Socket sock);

  // ManualResetEvent instances signal completion.  
  private static ManualResetEvent connectDone =
      new ManualResetEvent(false);
  private static ManualResetEvent sendDone =
      new ManualResetEvent(false);
  private static ManualResetEvent receiveDone =
      new ManualResetEvent(false);

  // The response from the remote device.  
  private static String response = String.Empty;

  public static Socket GetSocket() {
    return client;
  }

  public void StartClient() {
    // Connect to a remote device.  
    try {
      // Establish the remote endpoint for the socket.  
      // The name of the   
      // remote device is "host.contoso.com".  
      IPEndPoint remoteEP = new IPEndPoint(iPAddress, port);

      //// Create a TCP/IP socket.  
      client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

      // Connect to the remote endpoint.  
      client.BeginConnect(remoteEP,
          new AsyncCallback(ConnectCallback), client);
      connectDone.WaitOne();

      // Send test data to the remote device.  
      publicPostData clientPostData = new publicPostData();
      clientPostData.packet_Type = (int)ClientPacketType.LoadBLEList;
      clientPostData.data = 0;
      Send(client, Packet.Serialize(clientPostData));
      sendDone.WaitOne();

      // Receive the response from the remote device.  
      Receive(client);
      //receiveDone.WaitOne();

      // Write the response to the console.  
      Console.WriteLine("Response received : {0}", response);


    } catch (Exception e) {
      Console.WriteLine(e.ToString());
    }
  }

  private static void ConnectCallback(IAsyncResult ar) {
    try {
      // Retrieve the socket from the state object.  
      client = (Socket)ar.AsyncState;

      // Complete the connection.  
      client.EndConnect(ar);

      Console.WriteLine("Socket connected to {0}",
          client.RemoteEndPoint.ToString());

      // Signal that the connection has been made.  
      connectDone.Set();
    } catch (Exception e) {
      Console.WriteLine(e.ToString());
    }
  }

  private static void Receive(Socket client) {
    try {
      // Create the state object.  
      StateObject state = new StateObject();
      state.workSocket = client;

      // Begin receiving the data from the remote device.  
      client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
          new AsyncCallback(ReceiveCallback), state);
    } catch (Exception e) {
      Console.WriteLine(e.ToString());
    }
  }

  private static void ReceiveCallback(IAsyncResult ar) {
    try {
      // Retrieve the state object and the client socket   
      // from the asynchronous state object.  
      StateObject state = (StateObject)ar.AsyncState;
      client = state.workSocket;

      // Read data from the remote device.  
      int bytesRead = client.EndReceive(ar);

      if (bytesRead > 0) {
        // There might be more data, so store the data received so far.  

        Packet receiveclass = (Packet)Packet.Deserialize(state.buffer);

        ProcessPacket(receiveclass.packet_Type, state.buffer);

        // Get the rest of the data.  
        client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
            new AsyncCallback(ReceiveCallback), state);
      } else {
        // All the data has arrived; put it in response.  
        if (state.sb.Length > 1) {
          response = state.sb.ToString();
        }
        // Signal that all bytes have been received.  
        receiveDone.Set();
      }
    } catch (Exception e) {
      Console.WriteLine(e.ToString());
    }
  }

  private static void ProcessPacket(int packet_Type, byte[] buffer) {

    switch (packet_Type) {
      case (int)ServerPacketType.BLEList: {
          Debug.Log("BLEList : ");
          ServerBLEList receiveclass = (ServerBLEList)Packet.Deserialize(buffer);
          foreach (var data in receiveclass.BLEList) {
            _bleList.Add(data);
            Debug.Log(data.ToString());
          }
        }
        break;

      case (int)ServerPacketType.BPMData: {
          Debug.Log("BPMData : ");
          ServerBPMData receiveclass = (ServerBPMData)Packet.Deserialize(buffer);
          _nowBPM = receiveclass.nowBPM.ToString();
        }
        break;

      case (int)ServerPacketType.SendPacket: {
          Debug.Log("SendPacket : ");
          publicPostData receiveclass = (publicPostData)Packet.Deserialize(buffer);

          if (receiveclass.data == (int)ServerStatus.BLEConnectComplete) {
            // BLE 연결에 성공 했을 경우
            publicPostData clientPostData = new publicPostData();
            clientPostData.packet_Type = (int)ClientPacketType.AuthBLE;
            clientPostData.data = 0;
            Send(client, Packet.Serialize(clientPostData));
          } else if (receiveclass.data == (int)ServerStatus.BLEAuthComplete) {
            // Auth 성공 했을 경우
            publicPostData clientPostData = new publicPostData();
            clientPostData.packet_Type = (int)ClientPacketType.StartBLE;
            clientPostData.data = 0;
            Send(client, Packet.Serialize(clientPostData));
          }

        }
        break;
    }

  }

  public static void Send(Socket handler, Byte[] data) {
    // Begin sending the data to the remote device.  
    handler.BeginSend(data, 0, data.Length, 0,
        new AsyncCallback(SendCallback), handler);
  }



  private static void SendCallback(IAsyncResult ar) {
    try {
      // Retrieve the socket from the state object.  
      client = (Socket)ar.AsyncState;

      // Complete sending the data to the remote device.  
      int bytesSent = client.EndSend(ar);
      Console.WriteLine("Sent {0} bytes to server.", bytesSent);

      // Signal that all bytes have been sent.  
      sendDone.Set();
    } catch (Exception e) {
      Console.WriteLine(e.ToString());
    }
  }

  // Start is called before the first frame update
  void Start() {
    // 변수 초기화, 처음 모두 0개
    if (_instance)                     // 인스턴스가 이미 생성 되었는가?
    {
      DestroyImmediate(gameObject);   // 또 만들 필요가 없다 -> 삭제
      return;
    }
    _instance = this;                  // 유일한 인스턴스로 만듬
    DontDestroyOnLoad(gameObject);      // 씬이 바뀌어도 계속 유지 시킨다

    NetworkManager.Instance.NowBPM = "";

    Debug.Log(Path.GetFullPath(".") + "\\MiBandServer.exe");
    System.Diagnostics.Process.Start(Path.GetFullPath(".") + "\\MiBandServer.exe");
    // 서버 연결 실행
    StartClient();

  }

}
