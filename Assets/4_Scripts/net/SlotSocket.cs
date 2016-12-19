using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;

using System.IO;

public abstract class SlotSocket
{
    public enum ErrorType
    {
        Connect,
        Receive,
        Send,
        Close,
        HandleData,
        InvalidData,
        Mismatch
    }

    public enum SocketState
    {
        Null,
        Closed,
        Connecting,
        Connected,
    }

    public enum CloseReason
    {
        ApplicationQuit,
        Destory,
        Error
    }

    public class SocketEvent
    {
        public enum EventType
        {
            Connect,
            Data,
            DisConnect
        }

        public EventType Type { get; private set; }

        public byte[] Packet { get; private set; }
        public SocketEvent(EventType type, byte[] packet = null)
        {
            Packet = packet;
            Type = type;
        }
    }

    public class BufferObject
    {
        public Socket workSocket;
        public byte[] buffer;
        public StringBuilder sb;

        public BufferObject(int BufferSize)
        {
            buffer = new byte[BufferSize];
            sb = new StringBuilder();
        }
    }


    int _bufferSize;
    protected Socket _socket;
    protected MemoryStream _ms;
    private int _availableSize = 0;
    BufferObject _bufferObject;

    SocketState _currentState;
    Queue<SocketEvent> _eventQueue;

    AsyncCallback _connectCallback;
    AsyncCallback _sendCallback;
    AsyncCallback _receiveCallback;

    public SlotSocket(int BufferSize = 8192)
    {
        _bufferSize = BufferSize;

        _eventQueue = new Queue<SocketEvent>();

        _bufferObject = new BufferObject(_bufferSize);
        _ms = new MemoryStream();

        _connectCallback = new AsyncCallback(ConnectComplete);
        _sendCallback = new AsyncCallback(SendComplete);
        _receiveCallback = new AsyncCallback(ReceivedData);

        State = SocketState.Closed;
    }

    protected abstract bool IsReceivedDone();

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
        _socket.ReceiveBufferSize = _bufferSize;
        _socket.SendBufferSize = _bufferSize;
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
            _socket.BeginConnect(remoteEP, _connectCallback, _socket);
        }
        catch (Exception ex)
        {
            LogError(ErrorType.Receive, ex);
        }
    }

    void ConnectComplete(IAsyncResult ar)
    {
        if (State != SocketState.Connecting) return;

        try
        {
            Socket client = (Socket)ar.AsyncState;
            client.EndConnect(ar);

            Debug.LogFormat("Socket connected to {0}", client.RemoteEndPoint.ToString());

            if (_socket != client)
            {
                LogError(ErrorType.Mismatch);
                return;
            }

            State = SocketState.Connected;

            Receive(client);

            EnqueueEvent(SocketEvent.EventType.Connect);
        }
        catch (Exception ex)
        {
            LogError(ErrorType.Connect, ex);
        }
    }


    void Receive(Socket client)
    {
        try
        {
            _bufferObject.workSocket = client;
            client.BeginReceive(_bufferObject.buffer, 0, _bufferSize, SocketFlags.None, _receiveCallback, _bufferObject);
        }
        catch (Exception ex)
        {
            LogError(ErrorType.Receive, ex);
        }
    }

    void ReceivedData(IAsyncResult ar)
    {
        if (State == SocketState.Closed) return;
        try
        {

            BufferObject obj = (BufferObject)ar.AsyncState;
            Socket client = obj.workSocket;

            if (_socket != client)
            {
                LogError(ErrorType.Mismatch);
                return;
            }

            int byteSize = client.EndReceive(ar);
            if (byteSize <= 0)
            {
                LogError(ErrorType.InvalidData);
                return;
            }

            //Debug.Log("byteSize : " + byteSize);

            WriteBuffer(obj.buffer, byteSize);

            if (IsReceivedDone())
            {
                byte[] packet = ReadBuffer();
                //Debug.Log("packesize: " + packet.Length);
                if (packet != null && packet.Length > 0) EnqueueEvent(SocketEvent.EventType.Data, packet);
            }

            Receive(client);

        }
        catch (ObjectDisposedException ex)
        {
            LogError(ErrorType.HandleData, ex);
            return;
        }
        catch (SocketException ex)
        {
            LogError(ErrorType.HandleData, ex.SocketErrorCode.ToString());
        }
        catch (Exception ex)
        {
            LogError(ErrorType.HandleData, ex);
        }
    }

    void WriteBuffer(byte[] buffer, int count)
    {
        _ms.Write(buffer, 0, count);
        _availableSize += count;
    }

    byte[] ReadBuffer()
    {
        byte[] packet = new byte[_availableSize];

        _ms.Position = 0;
        _ms.Read(packet, 0, _availableSize);

        _ms.Position = 0;
        _ms.SetLength(0);
        _availableSize = 0;
        return packet;
    }

    protected void EnqueueEvent(SocketEvent.EventType type, byte[] packet = null)
    {
        _eventQueue.Enqueue(new SocketEvent(type, packet));
    }

    public SocketEvent HasEvent()
    {
        if (_eventQueue.Count > 0) return _eventQueue.Dequeue();
        else return null;
    }

    public void Send(String data)
    {
        if (State != SocketState.Connected) return;

        byte[] byteData = Encoding.ASCII.GetBytes(data);
        Send(byteData);
    }

    void Send(byte[] data)
    {
        try
        {
            IList<ArraySegment<byte>> sendBuffers = CreateSendPacket(data);

            _socket.BeginSend(sendBuffers, SocketFlags.None, _sendCallback, _socket);
        }
        catch (Exception ex)
        {
            LogError(ErrorType.Send, ex);
        }
    }

    virtual protected IList<ArraySegment<byte>> CreateSendPacket(byte[] data)
    {
        List<ArraySegment<byte>> sendBuffers = new List<ArraySegment<byte>>();
        sendBuffers.Add(new ArraySegment<byte>(data));
        return sendBuffers;
    }

    void SendComplete(IAsyncResult ar)
    {
        try
        {
            Socket client = (Socket)ar.AsyncState;

            if (_socket != client)
            {
                LogError(ErrorType.Mismatch);
                return;
            }

            client.EndSend(ar);

            // int bytesSent = client.EndSend(ar);
            // Debug.LogFormat("Sent {0} bytes to server.", bytesSent);
        }
        catch (Exception ex)
        {
            LogError(ErrorType.Send, ex);
        }
    }

    public void Close(CloseReason reason)
    {
        try
        {
            if (_socket != null && _socket.Connected)
            {
                _socket.Close();
                _socket = null;
            }

            State = SocketState.Closed;

            EnqueueEvent(SocketEvent.EventType.DisConnect);
        }
        catch (Exception ex)
        {
            LogError(ErrorType.Close, ex);
        }
    }

    void LogError(ErrorType type, Exception ex)
    {
        LogError(type, ex.ToString());
    }

    void LogError(ErrorType type, string errorMessage = "")
    {
        //todo
        //ErrorType 에 맞게 소켓을 닫던지 적절히 조치하자
        Debug.LogFormat("Socket Error! type: {0} message: {1}", type, errorMessage);

        Close(CloseReason.Error);
    }

    public bool Connected
    {
        get
        {
            if (_socket == null) return false;
            else if (_socket.Connected == false) return false;
            else if (_currentState != SocketState.Connected) return false;
            return true;
        }

    }
}
