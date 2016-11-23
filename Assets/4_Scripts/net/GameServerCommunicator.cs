using UnityEngine;
using System.Collections;

using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    static public bool ENABLE_LOG = false;

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
    }

    IEnumerator CheckQueue()
    {
        WaitForSeconds waitSec = new WaitForSeconds(0.1f);

        while (true)
        {
            if (_socket != null && _socket.Connected)
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
                if (ENABLE_LOG) Debug.Log("Server Connected");
                if (OnConnect != null) OnConnect();
                break;

            case SlotSocket.SocketEvent.EventType.Data:
                DataReceived(socketEvent.Packet);
                break;

            case SlotSocket.SocketEvent.EventType.DisConnect:
                if (ENABLE_LOG) Debug.Log("Server DisConnected");
                break;
        }
    }

    void DataReceived(byte[] packet)
    {
        if (packet == null || packet.Length == 0) return;

        var receivedJson = Encoding.ASCII.GetString(packet, 0, packet.Length);
        var receivedObj = JObject.Parse(receivedJson);

        var isSuccess = receivedObj["success"].Value<bool>();
        var cmd = receivedObj["cmd"].Value<string>();
        var data = receivedObj["data"];

        if (ENABLE_LOG) Debug.Log("< " + cmd + "\n" + data.ToString());

        if (isSuccess == false)
        {
            HandleServerError();
            return;
        }

        switch (cmd)
        {
            case Command.LOGIN:
                IsLogin = true;

                var loginData = data.ToObject<ResDTO.Login>();

                if (OnLogin != null) OnLogin(loginData);
                break;

            case Command.SPIN:

                var spinData = data.ToObject<ResDTO.Spin>();
                if (OnSpin != null) OnSpin(spinData);
                break;
        }
    }

    void HandleServerError()
    {

    }

    public void Connect(string host, int port)
    {
        if (_socket == null) createSocket();

        _socket.Connect(host, port);
    }

    bool CheckConnection()
    {
        return _socket == null || _socket.Connected == false;
    }

    public void Login(int userID, string signedRequest)
    {
        Send(GameCommand.LOGIN, new ReqDTO.Login()
        {
            userID = userID,
            signedRequest = signedRequest
        });
    }

    public void Spin(float linebet)
    {
        Send(GameCommand.SPIN, new ReqDTO.Spin()
        {
            lineBet = linebet
        });
    }

    public void Send(string command, ReqDTO dto)
    {
        SendData data = new SendData()
        {
            cmd = command,
            data = dto
        };

        if (ENABLE_LOG) Debug.Log(command + " >\n" + JsonConvert.SerializeObject(dto, Formatting.Indented));
        Send(JsonConvert.SerializeObject(data, Formatting.Indented));
    }

    void Send(string data)
    {
        if (CheckConnection() == false)
        {
            if (ENABLE_LOG) Debug.Log("Sockt에 먼저 연결 되어야 합니다");
            return;
        }

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

public class GameCommand
{
    public const string LOGIN = "login";
    public const string SPIN = "spin";
}

public class SendData
{
    public string cmd;
    public ReqDTO data;
}
