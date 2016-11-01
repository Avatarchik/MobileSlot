using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

using System.IO;

public class SlotSocket
{
    public enum SocketState
    {
        Null,
        Closed,
        Connecting,
        Connected,
        Sending,
        Receive
    }

    public enum CloseReason
    {
        ApplicationQuit,
        ConnectFail
    }

    public class StateObject
    {
        public const int BufferSize = 4096;//4096 or 8192
        public Socket workSocket = null;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();
    }

    protected ArraySegment<byte> _endSegment = new ArraySegment<byte>(new byte[] { 0 });

    public event Action OnConnected;
    public event Action OnDisConnected;
    public event Action<string> OnDataReceived;


    Socket _socket;
    SocketState _currentState;

    public SlotSocket()
    {
        State = SocketState.Closed;
    }

    public SocketState State
    {
        get { return _currentState; }
        private set
        {
            if (_currentState == value) return;

            //Debug.LogFormat("SocektState Changed.{0} > {1}", _currentState, value);
            _currentState = value;

            switch (_currentState)
            {
                case SocketState.Connected:
                    break;
                case SocketState.Closed:
                    break;
            }
        }
    }

    void CreateSocket()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    public void Connect(string host, int port)
    {
        if (State != SocketState.Closed) return;

        IPHostEntry ipHostInfo = Dns.GetHostEntry(host);
        IPAddress ipAddress = ipHostInfo.AddressList[0];
        IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

        if (_socket == null)
        {
            CreateSocket();
        }

        try
        {
            State = SocketState.Connecting;
            _socket.BeginConnect(remoteEP, new AsyncCallback(ConnectCallBack), _socket);
        }
        catch (Exception ex)
        {
            ErrorLog("connect fail", ex);
        }
    }

    void ConnectCallBack(IAsyncResult ar)
    {
        if (State != SocketState.Connecting) return;

        try
        {
            Socket client = (Socket)ar.AsyncState;
            client.EndConnect(ar);

            Debug.LogFormat("Socket connected to {0}", client.RemoteEndPoint.ToString());

            if (_socket != client)
            {
                ErrorLog("mismatch");
                return;
            }

            State = SocketState.Connected;

            Receive(client);

            if (OnConnected != null) OnConnected();
        }
        catch (Exception ex)
        {
            Close(CloseReason.ConnectFail);
            ErrorLog("connect callback fail", ex);
        }
    }

    void Receive(Socket client)
    {
        try
        {
            StateObject state = new StateObject();
            state.workSocket = client;

            //  _socket.ReceiveBufferSize = StateObject.BufferSize;
            //  _socket.SendBufferSize = StateObject.BufferSize;

            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
        }
        catch (Exception ex)
        {
            Close(CloseReason.ConnectFail);
            ErrorLog("Receive fail", ex);
        }
    }

    void ReceiveCallback(IAsyncResult ar)
    {
        if (State == SocketState.Closed) return;

        try
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket client = state.workSocket;

            if (_socket != client)
            {
                ErrorLog("mismatch");
                return;
            }

            int byteSize = client.EndReceive(ar);

            //마지막 \0 이 들어왔는지 체크해야한다. 아니라면 나머지 데이터를 받을때까지 기다려야함.
            //귀찮다 일단 넘어가자
            if (byteSize > 0)
            {
                // var data = Encoding.ASCII.GetString(state.buffer, 0, byteSize - _endSegment.Array.Length );
                // state.sb.Append(data);
                // Debug.Log("data: " + data);

                var response = Encoding.ASCII.GetString(state.buffer, 0, byteSize - _endSegment.Array.Length);
                Debug.Log("data: " + response);
                if (OnDataReceived != null) OnDataReceived(response);
            }

            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);

        }
        catch (ObjectDisposedException ex)
        {
            ErrorLog("ObjectDisposedException", ex);
            return;
        }
        catch (SocketException ex)
        {
            ErrorLog(ex.SocketErrorCode.ToString(), ex);
        }
        catch (Exception ex)
        {
            ErrorLog("ReceiveCB", ex);
        }
    }

    public void Send(String data)
    {
        if (State != SocketState.Connected) return;

        byte[] byteData = Encoding.ASCII.GetBytes(data);
        Send(byteData);
    }

    void Send(byte[] data)
    {
        List<ArraySegment<byte>> sendBuffers = new List<ArraySegment<byte>>();

        sendBuffers.Add(new ArraySegment<byte>(data));
        sendBuffers.Add(_endSegment);

        State = SocketState.Sending;

        _socket.BeginSend(sendBuffers, SocketFlags.None, new AsyncCallback(SendCallback), _socket);
    }

    void SendCallback(IAsyncResult ar)
    {
        try
        {
            State = SocketState.Connected;

            Socket client = (Socket)ar.AsyncState;

            if (_socket != client)
            {
                ErrorLog("mismatch");
                return;
            }

            int bytesSent = client.EndSend(ar);

            Debug.LogFormat("Sent {0} bytes to server.", bytesSent);
        }
        catch (Exception ex)
        {
            ErrorLog("Send", ex);
        }
    }

    public void Close(CloseReason reason)
    {
        try
        {
            if (_socket != null)
            {
                _socket.Close();
                _socket = null;
            }

            State = SocketState.Closed;

            if (OnDisConnected != null) OnDisConnected();
        }
        catch (Exception ex)
        {
            ErrorLog("Close", ex);
        }
    }

    public void ErrorLog(string type, Exception ex = null)
    {
        if (ex == null) Debug.LogFormat("Socket Error type: {0}", type);
        else Debug.LogFormat("Socket Error type: {0} ex: {1}", type, ex.ToString());
    }
}
