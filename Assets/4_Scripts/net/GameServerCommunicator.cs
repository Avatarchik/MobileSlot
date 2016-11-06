using UnityEngine;
using System.Collections;

using System;
using System.Text;
using LitJson;
using lpesign;

// return "lbgames.sloticagames.com";
// "182.252.135.251";
//shining 13100
//highdia 13500

public class Command
{
    public const string LOGIN = "login";
    public const string SPIN = "spin";
}

public class SendData
{
    public string cmd;
    public DTO data;
}

public class GameServerCommunicator : SingletonSimple<GameServerCommunicator>
{
    public event Action OnConnect;
    public event Action<ResDTO.Login> OnLogin;
    public event Action<ResDTO.Spin> OnSpin;

    public bool IsLogin { get; private set; }
    SlotSocket _socket;

    Coroutine _checkRoutine;

    void createSocket()
    {
        // _socket = new SlotSocket();
        _socket = new CrownGamesSocket();

        _checkRoutine = StartCoroutine(CheckQueue());

        // //병신같은 litjson
        JsonMapper.RegisterImporter<Int64, double>((Int64 value) =>
        {
            return System.Convert.ToDouble(value);
        });
    }

    IEnumerator CheckQueue()
    {
        WaitForSeconds waitSec = new WaitForSeconds(0.1f);

        while (true)
        {
            if ( _socket != null && _socket.Connected )
            {
                SocketEventHandler(_socket.HasEvent());
            }
            
            yield return waitSec;
        }
    }


    void SocketEventHandler(SlotSocket.SocketEvent socketEvent)
    {
        if (socketEvent == null) return;

        switch (socketEvent.Type)
        {
            case SlotSocket.SocketEvent.EventType.Connect:
                Debug.Log("Server Connected");
                if (OnConnect != null) OnConnect();
                break;

            case SlotSocket.SocketEvent.EventType.Data:
                DataReceived(socketEvent.Packet);
                break;

            case SlotSocket.SocketEvent.EventType.DisConnect:
                Debug.Log("Server DisConnected");
                break;
        }
    }

    void DataReceived(byte[] packet)
    {
        if (packet == null || packet.Length == 0) return;

        var receivedJson = Encoding.ASCII.GetString(packet, 0, packet.Length);
        Debug.Log("< receive\n" + receivedJson);

        JsonData obj = JsonMapper.ToObject(receivedJson);

        string cmd = (string)obj["cmd"];
        string jsonData = obj["data"].ToJson();

        switch (cmd)
        {
            case Command.LOGIN:
                IsLogin = true;
                var loginData = JsonMapper.ToObject<ResDTO.Login>(jsonData);
                if (OnLogin != null) OnLogin(loginData);
                break;

            case Command.SPIN:
                var spinData = JsonMapper.ToObject<ResDTO.Spin>(jsonData);
                if (OnSpin != null) OnSpin(spinData);
                break;
        }
    }

    public void Connect(string host, int port)
    {
        if (_socket == null) createSocket();

        _socket.Connect(host, port);
    }

    public void Login( int userID, string signedRequest )
    {
        if (_socket == null || _socket.Connected == false)
        {
            Debug.Log("Sockt에 먼저 연결 되어야 합니다");
            return;
        }

        SendData data = new SendData()
        {
            cmd = "login",
            data = new ReqDTO.Login()
            {
                userID = userID,
                signedRequest = signedRequest
            }
        };

        Send(data);
    }

    public void Spin(float linebet)
    {
        SendData data = new SendData()
        {
            cmd = "spin",
            data = new ReqDTO.Spin()
            {
                lineBet = linebet
            }
        };

        Send(data);
    }

    void Send(SendData data)
    {
        Send(JsonMapper.ToJson(data));
    }

    void Send(string data)
    {
        Debug.Log("send >\n" + data);
        _socket.Send(data);
    }

    public void Close(SlotSocket.CloseReason reason)
    {
        IsLogin = false;

        if (_socket != null)
        {
            _socket.Close(reason);
            _socket = null;
        }
    }

    override protected void OnDestroy()
    {
        base.OnDestroy();
        StopCoroutine(_checkRoutine);
        Close(SlotSocket.CloseReason.Destory);
    }

    void OnApplicationQuit()
    {
        Close(SlotSocket.CloseReason.ApplicationQuit);
    }
}