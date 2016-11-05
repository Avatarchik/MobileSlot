using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using LitJson;


public class SlotInfo
{
    //------------------------------------------------------------------
    //게임 전반적인 설정
    //------------------------------------------------------------------

    static public SlotInfo Main;
    static public int Port;
    static public string Version;

    //------------------------------------------------------------------
    //게임 내 슬롯 설정 ( 게임 속 다수의 슬롯 머신이 존재할 수 도 있다 )
    //------------------------------------------------------------------
    public int Row, Column;

    public Rect SymbolRect;
    public Rect ReelRect;

    public int ReelGap;
    
    public ReelStrips Strips;
}

public class SlotMachine : MonoBehaviour
{
    public enum MachineState
    {
        Null,
        Connecting,
        Idle,
        Spininig,
        ReceivedSymbol,
        ShowResult
    }

    public SlotUI ui;

    [Header("prefabs")]
    public Reel ReelPrefab;

    protected SlotModel _model;

    protected MachineState _currentState;

    protected ReelContainer _reelContainer;

    virtual protected void Awake()
    {
        _model = SlotModel.Instance;

        _reelContainer = GetComponentInChildren<ReelContainer>();
        _reelContainer.CreateReels(ReelPrefab);
    }

    void Start()
    {
        Debug.Log("game Start");

        SetState(MachineState.Connecting);
    }

    protected void SetState(MachineState next)
    {
        if (_currentState == next) return;

        Debug.LogFormat("<STATE CHANGED>.{0} > {1}", _currentState, next);
        _currentState = next;

        switch (_currentState)
        {
            case MachineState.Connecting:
                ConnectServer();
                break;

            case MachineState.Idle:
                Idle();
                break;

            case MachineState.Spininig:
                Spin();
                break;

            case MachineState.ReceivedSymbol:
                ReceivedSymbol();
                break;

            case MachineState.ShowResult:
                ShowResult();
                break;
        }
    }

    void ConnectServer()
    {
        GameServerCommunicator.Instance.OnLogin += LoginComplete;
        GameServerCommunicator.Instance.OnSpin += SpinCompelte;
        GameServerCommunicator.Instance.Connect("182.252.135.251", SlotInfo.Port);
    }

    void LoginComplete(ResDTO.Login dto)
    {
        Debug.Log("game login complete.");

        _model.SetLoginData(dto);

        SetState(MachineState.Idle);

        GameManager.Instance.GameReady();
    }

    void Idle()
    {
        ui.Idle();
    }

    public void TrySpin()
    {
        switch (_currentState)
        {
            case MachineState.Idle:
                if (_model.Balance < _model.TotalBet)
                {
                    OpenCoinShop();
                    return;
                }
                else
                {
                    SetState(MachineState.Spininig);
                }

                break;

            default:
                Stop();
                break;
        }
    }

    void Stop()
    {
        //spin 중이면 스핀 멈추고,
        //닫을 수 있는 팝업창이 띄워져 있으면 팝업창 닫고
        //당첨되어 돈올라가는 도중이면 스킵하고 다음 스핀
    }

    void OpenCoinShop()
    {
        Debug.Log("돈없어. 그지");
    }

    void Spin()
    {
        //더미 심볼 돌리자
        GameServerCommunicator.Instance.Spin(10000);
    }

    void SpinCompelte(ResDTO.Spin dto)
    {
        _model.SetSpinData(dto);

        SetState(MachineState.ReceivedSymbol);
    }

    void ReceivedSymbol()
    {
        //결과 심볼 넣어서 스핀 마무리

        //모든 릴이 멈추면
        SetState(MachineState.ShowResult);
    }

    void ShowResult()
    {
        SetState(MachineState.Idle);
    }
}
