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
            case GameCommand.LOGIN:
                IsLogin = true;

                var loginData = SafeDeserialize<ResDTO.Login>(data);
                if (OnLogin != null) OnLogin(loginData);
                break;

            case GameCommand.SPIN:

                AvoidFreeSpinCountException(data);

                var spinData = SafeDeserialize<ResDTO.Spin>(data);
                if (OnSpin != null) OnSpin(spinData);
                break;
        }
    }

    //todo
    //high diamonds 가 프리스핀이 걸릴 경우 freeSpinCount에 배열이 들어온다.
    // 자료구조와 달라서 DESERIALIZE 에러뜸. 체크하는 코드.
    //....꼭 이래야 하나
    void AvoidFreeSpinCountException(JToken obj)
    {
        var payouts = obj["payouts"];
        if (payouts == null) return;

        var spinArr = payouts["spins"] as JArray;
        if (spinArr == null) return;

        var freeSpinCount = spinArr[0]["freeSpinCount"];
        if (freeSpinCount.Type == JTokenType.Integer) return;

        for (var i = 0; i < spinArr.Count; ++i)
        {
            var spin = spinArr[i];
            spin["freeSpinSuggestion"] = spin["freeSpinCount"];
            spin["freeSpinCount"] = 0;
        }
    }

    T SafeDeserialize<T>(JToken obj) where T : ResDTO
    {
        T res = null;
        try
        {
            res = obj.ToObject<T>();
        }
        catch (Exception e)
        {
            Debug.LogError("!! Deserialize json fail !!\n" + e.ToString());
        }
        return res;
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
        if (_socket == null || _socket.Connected == false) return false;
        return true;
    }

    public void Login(int userID, string signedRequest)
    {
        Send(GameCommand.LOGIN, new ReqDTO.Login()
        {
            userID = userID,
            signedRequest = signedRequest
        });
    }

    public void Spin(double linebet)
    {
        Send(GameCommand.SPIN, new ReqDTO.Spin()
        {
            lineBet = linebet
        });
    }

    public void FreeSpin(double linebet, int kind = 1)
    {
        Send(GameCommand.FREE_SPIN + kind, new ReqDTO.Spin()
        {
            lineBet = linebet
        });
    }

    public void Send(string command, ReqDTO dto)
    {
        SendData sendData = new SendData()
        {
            cmd = command,
            data = dto
        };

        Send(sendData);
    }

    public void Send(SendData sendData)
    {
        if (ENABLE_LOG) Debug.Log(sendData.cmd + " >\n" + JsonConvert.SerializeObject(sendData.data, Formatting.Indented));
        Send(JsonConvert.SerializeObject(sendData, Formatting.Indented));
    }

    public void Send(string data)
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
    public const string FREE_SPIN = "free_";
}

public class SendData
{
    public string cmd;
    public ReqDTO data;
}
