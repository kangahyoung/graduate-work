using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.IO;  // 파일 입출력 스트림
using System.Runtime.Serialization.Formatters.Binary;      // 바이너리 포맷
using static publicPacketProtocol.PacketProtocol;


namespace WindowsFormsApp1 {
  public class AsynchronousSocketServer {
    public class StateObject {
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
    public static Socket g_listener = null;
    private static readonly int Port = 11000;
    private readonly static IPAddress iPAddress = IPAddress.Parse("127.0.0.1");
    Thread AcceptThread;

    public delegate void ChangePacketType(int packet_type, byte[] buffer);
    public static event ChangePacketType PacketTypeChanged;

    public Socket getSocket() {
      return g_listener;
    }

    public void Server_Start() {
      IPEndPoint localEndPoint = new IPEndPoint(iPAddress, Port);

      // Create a TCP/IP socket.  
      g_listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

      // Bind the socket to the local endpoint and listen for incoming connections.  
      try {
        g_listener.Bind(localEndPoint);
        g_listener.Listen(100);

        AcceptThread = new Thread(new ThreadStart(acceptThread));
        AcceptThread.Start();
        Console.WriteLine("Server Listen On..!");

      } catch (Exception e) {
        Console.WriteLine(e.ToString());
      }

      //Console.WriteLine("\nPress ENTER to continue...");
      //Console.Read();
    }


    private void acceptThread() {

      if (g_listener != null) {
        allDone.Reset();
        g_listener.BeginAccept(new AsyncCallback(AcceptCallback), g_listener);
        allDone.WaitOne();
      }
    }

    public static void AcceptCallback(IAsyncResult ar) {
      // Signal the main thread to continue.  
      allDone.Set();

      // Get the socket that handles the client request.  
      g_listener = (Socket)ar.AsyncState;
      Socket handler = g_listener.EndAccept(ar);
      g_listener = handler;
      // Create the state object.  
      StateObject state = new StateObject();
      state.workSocket = handler;
      handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
          new AsyncCallback(ReadCallback), state);
    }

    public static void ReadCallback(IAsyncResult ar) {
      String content = String.Empty;

      // Retrieve the state object and the handler socket  
      // from the asynchronous state object.  
      StateObject state = (StateObject)ar.AsyncState;
      Socket handler = state.workSocket;

      // Read data from the client socket.   
      int bytesRead = handler.EndReceive(ar);

      if (bytesRead > 0) {

        // There might be more data, so store the data received so far.  
        Packet receiveclass = (Packet)Packet.Deserialize(state.buffer);

        // delegate로 PacketType을 전달해 준다.
        PacketTypeChanged.Invoke(receiveclass.packet_Type, state.buffer);

        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
        new AsyncCallback(ReadCallback), state);
      } else {
        // Not all data received. Get more.  
        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
        new AsyncCallback(ReadCallback), state);
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
        Socket handler = (Socket)ar.AsyncState;

        int bytesSent = handler.EndSend(ar);
        Console.WriteLine("Send Data to Client : " + bytesSent);

        // Create the state object.  
        StateObject state = new StateObject();
        state.workSocket = handler;

        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
            new AsyncCallback(ReadCallback), state);

      } catch (Exception e) {
        Console.WriteLine("SendCallback Error : " + e.ToString());
      }
    }

  }
}
