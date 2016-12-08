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
    public enum ErrorType
    {
        Connect,
        Receive,
        Send,
        Close,
        HandleData,
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
        public const int BufferSize = 8192;
        public Socket workSocket = null;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();
    }

    static private byte[] END_BYTE = new byte[] { 0 };

    Socket _socket;
    SocketState _currentState;

    Queue<SocketEvent> _eventQueue;

    ArraySegment<byte> _endSegment = new ArraySegment<byte>(END_BYTE);

    BufferObject _bufferObject;
    AsyncCallback _connectCallback;
    AsyncCallback _sendCallback;
    AsyncCallback _receiveCallback;

    public SlotSocket()
    {
        _eventQueue = new Queue<SocketEvent>();

        _bufferObject = new BufferObject();

        _connectCallback = new AsyncCallback(ConnectComplete);
        _sendCallback = new AsyncCallback(SendComplete);
        _receiveCallback = new AsyncCallback(DataReceived);

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
        _socket.ReceiveBufferSize = BufferObject.BufferSize;
        _socket.SendBufferSize = BufferObject.BufferSize;
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
            client.BeginReceive(_bufferObject.buffer, 0, BufferObject.BufferSize, SocketFlags.None, _receiveCallback, _bufferObject);
        }
        catch (Exception ex)
        {
            LogError(ErrorType.Receive, ex);
        }
    }

    void DataReceived(IAsyncResult ar)
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

            Debug.Log("byteSize: " + byteSize + ( obj.buffer[byteSize-1] == END_BYTE[0] ));

            if (byteSize > 0)
            {
                //TODO
                /*
                (END_BYTE) 가 들어왔는지 체크하자. 없다면 덜 받은거임. Stream 에 넣은 뒤 다음에 받고 합쳐야함
                현재 테스트로 CrownGames 서버와 통신하며 패킷 구조는 아래와 같은 json 이다
                {
                    cmd:"commandName",
                    data:
                    {
                        value:'abc'
                    }
                }
                cmd 부분을 의 값만을 미리 알아낸 뒤 data 만 파싱하는게 옳지만 일단 byte를 그대로 저장하고 넘어가자
                */

                int availableSize = byteSize - END_BYTE.Length;
                byte[] packet = new byte[availableSize];
                Array.Copy(obj.buffer, packet, availableSize);

                //여기는 메인쓰레드가 아니므로 직접 event 등을 사용하면 callstack 도 꼬이고 유니티에서 짜증난다
                //완료 된 패킷은 큐에 넣고 꺼내쓰자
                EnqueueEvent(SocketEvent.EventType.Data, packet);
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

    void EnqueueEvent(SocketEvent.EventType type, byte[] packet = null)
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
            List<ArraySegment<byte>> sendBuffers = new List<ArraySegment<byte>>();

            //canvas flash socket 에 연결중. 마지막 0 byte 삽입해야함

            sendBuffers.Add(new ArraySegment<byte>(data));
            sendBuffers.Add(_endSegment);

            _socket.BeginSend(sendBuffers, SocketFlags.None, _sendCallback, _socket);
        }
        catch (Exception ex)
        {
            LogError(ErrorType.Send, ex);
        }
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
            if (_socket != null && _socket.Connected )
            {
                _socket.Close();
                _socket = null;
            }

            State = SocketState.Closed;

            EnqueueEvent( SocketEvent.EventType.DisConnect );
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

        Close( CloseReason.Error );
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
