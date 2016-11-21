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


    public SlotConfig Config { get; set; }
    public Stack<MachineState> State { get; private set; }

    [SerializeField]
    MachineState _currentState;
    Dictionary<MachineState, Func<IEnumerator>> _stateEnterMap;
    Dictionary<MachineState, Func<IEnumerator>> _stateExitMap;
    Func<IEnumerator> _stateEnter;
    Func<IEnumerator> _stateExit;

    SlotMachineUI _ui;
    SlotModel _model;
    ReelContainer _reelContainer;

    void Awake()
    {
        State = new Stack<MachineState>();

        CacheStateBehaviour();

        _ui = FindObjectOfType<SlotMachineUI>() as SlotMachineUI;

        _model = SlotModel.Instance;
        _model.Reset();

        _reelContainer = GetComponentInChildren<ReelContainer>();
        _reelContainer.OnReelStopComplete += ReelStopComplete;

        GameServerCommunicator.Instance.OnConnect += ConnectComplete;
        GameServerCommunicator.Instance.OnLogin += LoginComplete;
        GameServerCommunicator.Instance.OnSpin += SpinComplete;
    }

    void CacheStateBehaviour()
    {
        _stateEnterMap = new Dictionary<MachineState, Func<IEnumerator>>();
        _stateExitMap = new Dictionary<MachineState, Func<IEnumerator>>();

        var states = Enum.GetValues(typeof(MachineState));
        foreach (MachineState s in states)
        {
            string stateName = s.ToString();

            var enter = FindMethod(stateName + "_Enter");
            var exit = FindMethod(stateName + "_Exit");

            _stateEnterMap[s] = enter;
            _stateExitMap[s] = exit;
        }
    }

    Func<IEnumerator> FindMethod(string methodName)
    {
        System.Reflection.MethodInfo methodInfo = GetType().GetMethod
        (
            methodName,
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic
        );

        if( methodInfo == null) return DoNothing;
        else return Delegate.CreateDelegate(typeof(Func<IEnumerator>),this, methodInfo) as Func<IEnumerator>;
    }

    IEnumerator DoNothing()
    {
        yield break;
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
        if (_currentState == next) return;

        if (State.Count > 0 &&
            State.Peek() != MachineState.Connecting &&
            next == MachineState.Idle)
        {
            State.Clear();
        }

        //Debug.Log(string.Format("STATE CHANGED. {0} >>> {1}", _currentState, next));

        State.Push(next);
        _currentState = State.Peek();

        StateEnter();
    }

    void StateEnter()
    {
        if (_stateExit != null) StartCoroutine(_stateExit());
        
        _stateEnter = _stateEnterMap[_currentState];
        _stateExit = _stateExitMap[_currentState];
        
        if (_stateEnter != null) StartCoroutine(_stateEnter());
    }

    IEnumerator Connecting_Enter()
    {
        GameServerCommunicator.Instance.Connect(SlotConfig.Host, SlotConfig.Port);

        yield break;
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

    IEnumerator Idle_Enter()
    {
        _ui.Idle();
        yield break;
    }

    public void TrySpin()
    {
        switch (_currentState)
        {
            case MachineState.Idle:
                if (_model.Owner.Balance < _model.TotalBet)
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

    IEnumerator Spin_Enter()
    {
        //더미 심볼 돌리자
        _reelContainer.Spin();
        GameServerCommunicator.Instance.Spin(10000);

        yield break;
    }

    ResDTO.Spin.Payout.SpinInfo _lastSpinInfo;

    void SpinComplete(ResDTO.Spin dto)
    {
        _model.SetSpinData(dto);
        _lastSpinInfo = _model.LastSpinInfo;

        SetState(MachineState.ReceivedSymbol);
    }

    IEnumerator ReceivedSymbol_Enter()
    {
        _reelContainer.ReceivedSymbol(_lastSpinInfo);

        yield break;
    }

    void ReelStopComplete()
    {
        SetState(MachineState.CheckSpinResult);
    }

    IEnumerator CheckSpinResult_Enter()
    {
        //결과 심볼들을 바탕으로 미리 계산 해야 하는 일들이 있다면 여기서 미리 계산한다.

        if ("nudge 시킬 릴이 있다면 넛지" == null)
        {
            //do nudge
        }
        else if (_model.IsFreeSpinTrigger)
        {
            SetState(MachineState.FreeSpinTrigger);
        }
        else if (_lastSpinInfo.totalPayout > 0)
        {
            SetState(MachineState.Win);
        }
        else
        {
            SetState(MachineState.AfterWin);
        }

        yield break;
    }

    IEnumerator FreeSpinTrigger_Enter()
    {
        Debug.Log("삐리리리리 프리스핀 트리거!");
        yield break;
    }

    IEnumerator FreeSpinTrigge_Exit()
    {
        //프리스핀 트리거 리셋
        yield break;
    }

    IEnumerator Win_Enter()
    {
        _reelContainer.DisplayWinSymbols();
        yield break;
    }

    IEnumerator AfterWin_Enter()
    {
        if ("보너스 스핀 ( 시상이 독립적인 추가 스핀 ) 이 있다면 돌린다" == null)
        {
            //do
        }
        else if ("프리스핀 진행 중이라면 프리스핀 한다" == null)
        {
            //do
        }
        else
        {
            SetState(MachineState.ApplySpinResult);
        }
        yield break;
    }

    IEnumerator ApplySpinResult_Enter()
    {
        //모든 연출이 끝났다.
        //결과를 실제 유저 객체에 반영한다.

        _ui.UpdateBalance();
        //반영 후 레벨업이 되었다면 연출한다.

        SetState(MachineState.Idle);
        yield break;
    }
}
