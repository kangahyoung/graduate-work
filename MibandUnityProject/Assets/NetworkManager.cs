using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;


public class NetworkManager : MonoBehaviour
{

  private static NetworkManager _instance = null;
  private string nowBPM = "";

  public static NetworkManager Instance {
    get {
      if (_instance == null)
      {
        _instance = FindObjectOfType(typeof(NetworkManager)) as NetworkManager;

        if (_instance == null)
        {
          Debug.LogError("There's no active NetworkManager object");
        }
      }

      return _instance;
    }
  }

  public string NowBPM                  // 현재 BPM 수치 프로퍼티.
   {
    get {
      return nowBPM;
    }
    set {
      nowBPM = value;
    }
  }

  public class StateObject
  {
    // Client socket
    public Socket workSocket = null;
    // Size of receive buffer
    public const int BufferSize = 1024;
    // Receive buffer
    public byte[] buffer = new byte[BufferSize];
    // Received data string
    public StringBuilder sb = new StringBuilder();
  }

  // Thread signal.
  public static ManualResetEvent allDone = new ManualResetEvent(false);
  // Golobal Sockeet define for g_listener
  public Socket g_listener = null;
  private static readonly int Port = 11000;
  private readonly static IPAddress iPAddress = IPAddress.Parse("127.0.0.1");


  IEnumerator Server_Start()
  {
    IPEndPoint localEndPoint = new IPEndPoint(iPAddress, Port);

    // Create a TCP/IP socket.  
    g_listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    // Bind the socket to the local endpoint and listen for incoming connections.  
    try
    {
      g_listener.Bind(localEndPoint);
      g_listener.Listen(100);
      Debug.Log("Server Listen On..!");
    }
    catch (Exception e)
    {
      Debug.Log(e.ToString());
    }
    yield return null;
  }

    public static void AcceptCallback(IAsyncResult ar)
  {
    // Signal the main thread to continue.  
    allDone.Set();

    // Get the socket that handles the client request.  
    Socket g_listener = (Socket)ar.AsyncState;
    Socket handler = g_listener.EndAccept(ar);

    // Create the state object.  
    StateObject state = new StateObject();
    state.workSocket = handler;
    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
        new AsyncCallback(ReadCallback), state);
  }

  public static void ReadCallback(IAsyncResult ar)
  {
    String content = String.Empty;

    // Retrieve the state object and the handler socket  
    // from the asynchronous state object.  
    StateObject state = (StateObject)ar.AsyncState;
    Socket handler = state.workSocket;

    // Read data from the client socket.   
    int bytesRead = handler.EndReceive(ar);

    if (bytesRead > 0)
    {
      // There  might be more data, so store the data received so far.  
      state.sb.Append(Encoding.ASCII.GetString(
          state.buffer, 0, bytesRead));

      // Check for end-of-file tag. If it is not there, read   
      // more data.  
      content = state.sb.ToString();
      if (content.IndexOf("<EOF>") > -1)
      {
        // All the data has been read from the   
        // client. Display it on the console.  
        content = content.Replace("<EOF>", "");
        NetworkManager.Instance.NowBPM =  content;
        Debug.Log("Read " + content.Length + " bytes from socket. \n Data : " + content);
        // Echo the data back to the client.  
        Send(handler, content);
        state.workSocket = handler;
      }
      else
      {
        // Not all data received. Get more.  
        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
        new AsyncCallback(ReadCallback), state);
      }
    }
  }

  private static void Send(Socket handler, String data)
  {
    // Convert the string data to byte data using ASCII encoding.  
    byte[] byteData = Encoding.ASCII.GetBytes(data);

    // Begin sending the data to the remote device.  
    handler.BeginSend(byteData, 0, byteData.Length, 0,
        new AsyncCallback(SendCallback), handler);
  }

  private static void SendCallback(IAsyncResult ar)
  {
    try
    {
      // Retrieve the socket from the state object.  
      Socket handler = (Socket)ar.AsyncState;

      // Complete sending the data to the remote device.  
      //int bytesSent = handler.EndSend(ar);
      //Debug.Log("Sent " + bytesSent + " bytes to client.");


      // Create the state object.  
      StateObject state = new StateObject();
      state.workSocket = handler;
      handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
          new AsyncCallback(ReadCallback), state);

      //handler.Shutdown(SocketShutdown.Both);
      //handler.Close();

    }
    catch (Exception e)
    {
      Debug.Log(e.ToString());
    }
  }

  // Start is called before the first frame update
  void Start()
  {
    // 변수 초기화, 처음 모두 0개
    if (_instance)                     // 인스턴스가 이미 생성 되었는가?
    {
      DestroyImmediate(gameObject);   // 또 만들 필요가 없다 -> 삭제
      return;
    }
    _instance = this;                  // 유일한 인스턴스로 만듬
    DontDestroyOnLoad(gameObject);      // 씬이 바뀌어도 계속 유지 시킨다

    NetworkManager.Instance.NowBPM = "";

    // 서버 실행
    StartCoroutine(Server_Start());
    
  }

  // Update is called once per frame
  void Update()
  {

    // Accept가 될 경우 콜백으로 전달한다.
    if (g_listener != null)
    g_listener.BeginAccept(
       new AsyncCallback(AcceptCallback),
       g_listener);

  }
}
