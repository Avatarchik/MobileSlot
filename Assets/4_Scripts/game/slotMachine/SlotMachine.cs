using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class SlotMachine : MonoBehaviour
{
    public enum MachineState
    {
        Null,
        Connecting,
        Idle,
        Spin,
        ReceivedSymbol,
        CheckSpinResult,
        FreeSpinTrigger,
        Win,
        AfterWin,
        ApplySpinResult
    }

    public SlotUI ui;

    public SlotConfig Config { get; set; }

    SlotModel _model;

    public Stack<MachineState> State {get; private set; }

    [SerializeField]
    MachineState _currentState;

    ReelContainer _reelContainer;

    void Awake()
    {
        State = new Stack<MachineState>();

        _model = SlotModel.Instance;
        _model.Reset();

        _reelContainer = GetComponentInChildren<ReelContainer>();
        _reelContainer.OnReelStopComplete += ReelStopComplete;

        GameServerCommunicator.Instance.OnConnect += ConnectComplete;
        GameServerCommunicator.Instance.OnLogin += LoginComplete;
        GameServerCommunicator.Instance.OnSpin += SpinComplete;
    }

    public void Run()
    {
        if (Config == null)
        {
            Debug.LogError("SlotConfig은 반드시 정의 되어야 합니다");
            return;
        }

        Debug.Log("Run SlotMachine");

        SetState(MachineState.Connecting);
    }

    protected void SetState(MachineState next)
    {
        if ( _currentState == next ) return;

        //Debug.Log(string.Format("STATE CHANGED. {0} >>> {1}", _currentState, next));

        if( State.Count > 0 &&
            State.Peek() != MachineState.Connecting &&
            next == MachineState.Idle )
        {
            State.Clear();
        }

        State.Push( next );
        _currentState = State.Peek();

        switch ( _currentState )
        {
            case MachineState.Connecting:
                ConnectServer();
                break;

            case MachineState.Idle:
                Idle();
                break;

            case MachineState.Spin:
                Spin();
                break;

            case MachineState.ReceivedSymbol:
                ReceivedSymbol();
                break;

            case MachineState.CheckSpinResult:
                CheckSpinResult();
                break;

            case MachineState.FreeSpinTrigger:
                FreeSpinTrigger();
                break;

            case MachineState.Win:
                Win();
                break;

            case MachineState.AfterWin:
                AfterWin();
                break;

            case MachineState.ApplySpinResult:
                ApplySpinResult();
                break;
        }
    }

    void ConnectServer()
    {
        GameServerCommunicator.Instance.Connect(SlotConfig.Host, SlotConfig.Port);
    }

    void ConnectComplete()
    {
        GameServerCommunicator.Instance.Login(0, "good");
    }

    void LoginComplete(ResDTO.Login dto)
    {
        _model.SetLoginData(dto);

        StartCoroutine(Initialize());
    }

    IEnumerator Initialize()
    {
        //필요한 리소스 Pool 들의 preload 가 완료 되길 기다린다.
        while (GamePool.Instance.IsReady == false)
        {
            yield return null;
        }

        Debug.Log("Initialize");

        _reelContainer.Initialize(Config);

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
                    SetState(MachineState.Spin);
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
        _reelContainer.Spin();
        GameServerCommunicator.Instance.Spin(10000);
    }

    ResDTO.Spin.Payout.SpinInfo _lastSpinInfo;

    void SpinComplete(ResDTO.Spin dto)
    {
        _model.SetSpinData(dto);
        _lastSpinInfo = _model.LastSpinInfo;

        SetState(MachineState.ReceivedSymbol);
    }

    void ReceivedSymbol()
    {
        _reelContainer.ReceivedSymbol(_lastSpinInfo);
    }

    void ReelStopComplete()
    {
        SetState(MachineState.CheckSpinResult);
    }

    void CheckSpinResult()
    {
        //결과 심볼들을 바탕으로 미리 계산 해야 하는 일들이 있다면 여기서 미리 계산한다.

        if( "nudge 시킬 릴이 있다면 넛지" == null )
        {
            //do nudge
        }
        else if( _model.IsFreeSpinTrigger )
        {
            SetState( MachineState.FreeSpinTrigger );
        }
        else if( _lastSpinInfo.totalPayout > 0 )
        {
            SetState( MachineState.Win );
        }
        else
        {
            SetState(MachineState.AfterWin);
        }
    }

    void FreeSpinTrigger()
    {
        Debug.Log("삐리리리리 프리스핀 트리거!" );
    }

    void Win()
    {

    }

    void AfterWin()
    {
        if( "보너스 스핀 ( 시상이 독립적인 추가 스핀 ) 이 있다면 돌린다" == null )
        {
            //do
        }
        else if( "프리스핀 진행 중이라면 프리스핀 한다" == null )
        {
            //do
        }
        else
        {
            SetState( MachineState.ApplySpinResult );
        }
    }

    void ApplySpinResult()
    {
        //모든 연출이 끝났다.
        //결과를 실제 유저 객체에 반영한다.
        //반영 후 레벨업이 되었다면 연출한다.

        SetState( MachineState.Idle );
    }
}
