using UnityEngine;
using System.Collections;

using System;
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

public class GameServerCommunicator : SingletonSimple<GameServerCommunicator>
{
    public event Action<ResponseDTO.LoginDTO> OnLogin;
    public event Action<ResponseDTO.SpinDTO> OnSpin;

    SlotSocket _socket;

    void createSocket()
    {
        _socket = new SlotSocket();
        _socket.OnConnected += ConnectedHandler;
        _socket.OnDisConnected += DisConnectedHandler;
        _socket.OnDataReceived += DataReceived;

        // //병신같은 litjson
        JsonMapper.RegisterImporter<Int64, double>((Int64 value) =>
        {
            return System.Convert.ToDouble( value );
        });
    }

    void DisConnectedHandler()
    {
        Debug.Log("Socket Disconnect" + _socket.State);
    }

    public void Connect(string host, int port)
    {
        if (_socket == null) createSocket();

        _socket.Connect(host, port);
    }

    void ConnectedHandler()
    {
        Debug.Log("socket connect complete! " + _socket.State);
        Login();
    }

    void Login()
    {
        SendData sdata = new SendData()
        {
            cmd = "login",
            data = new ReqDTO.LoginData()
            {
                userID = 0,
                signedRequest = "good"
            }
        };

        Send(JsonMapper.ToJson(sdata));
    }

    void DataReceived(string receive)
    {
        Debug.Log( receive );
        JsonData obj = JsonMapper.ToObject(receive);
        string cmd = (string)obj["cmd"];

        switch (cmd)
        {
            case Command.LOGIN:
                if (OnLogin != null) OnLogin(JsonMapper.ToObject<ResponseDTO.LoginDTO>(obj["data"].ToJson()));
                break;

            case Command.SPIN:
                // if (OnSpin != null) OnSpin(data);
                break;
        }
    }

    public void Spin(float linebet = 100f)
    {
        Debug.Log("spin");
        SendData sdata = new SendData()
        {
            cmd = "spin",
            data = new ReqDTO.SpinData()
            {
                lineBet = 10
            }
        };

        Send(JsonMapper.ToJson(sdata));
    }

    public void Send(string data)
    {
        Debug.Log("send >\n" + data);
        _socket.Send(data);
    }

    void OnApplicationQuit()
    {
        _socket.Close(SlotSocket.CloseReason.ApplicationQuit);
    }
}

public class SendData
{
    public string cmd;
    public DTO data;
}

