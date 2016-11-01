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
    public event Action<ResDTO.Login> OnLogin;
    public event Action<ResDTO.Spin> OnSpin;

    SlotSocket _socket;

    Coroutine _checkRoutine;

    void Start()
    {
        _checkRoutine = StartCoroutine(CheckQueue());
    }

    void createSocket()
    {
        _socket = new SlotSocket();
        _socket.OnConnected += ConnectedHandler;
        _socket.OnDisConnected += DisConnectedHandler;

        // //병신같은 litjson
        JsonMapper.RegisterImporter<Int64, double>((Int64 value) =>
        {
            return System.Convert.ToDouble(value);
        });
    }

    void DisConnectedHandler()
    {
        Debug.Log("Socket Disconnect");
    }

    public void Connect(string host, int port)
    {
        if (_socket == null) createSocket();

        _socket.Connect(host, port);
    }

    void ConnectedHandler()
    {
        Login();
    }

    void Login()
    {
        SendData data = new SendData()
        {
            cmd = "login",
            data = new ReqDTO.Login()
            {
                userID = 0,
                signedRequest = "good"
            }
        };

        Send(data);
    }

    IEnumerator CheckQueue()
    {
        WaitForSeconds waitSec = new WaitForSeconds(0.1f);

        while (true)
        {
            if (_socket == null || _socket.Connected == false) continue;

            DataReceived(_socket.NextPacket());
            yield return waitSec;
        }
    }

    void DataReceived(byte[] packet)
    {
        if (packet == null) return;

        var receivedJson = Encoding.ASCII.GetString(packet, 0, packet.Length);
        Debug.Log("< receive\n" + receivedJson);

        JsonData obj = JsonMapper.ToObject(receivedJson);

        string cmd = (string)obj["cmd"];
        string jsonData = obj["data"].ToJson();

        switch (cmd)
        {
            case Command.LOGIN:
                var loginData = JsonMapper.ToObject<ResDTO.Login>(jsonData);
                if (OnLogin != null) OnLogin(loginData);
                break;

            case Command.SPIN:
                var spinData = JsonMapper.ToObject<ResDTO.Spin>(jsonData);
                if (OnSpin != null) OnSpin(spinData);
                break;
        }
    }

    public void Spin(float linebet )
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
        if (_socket == null) return;

        _socket.Close(reason);
        _socket = null;
    }

    void OnDestroy()
    {
        StopCoroutine(_checkRoutine);
        Close(SlotSocket.CloseReason.Destory);
    }

    void OnApplicationQuit()
    {
        Close(SlotSocket.CloseReason.ApplicationQuit);
    }
}