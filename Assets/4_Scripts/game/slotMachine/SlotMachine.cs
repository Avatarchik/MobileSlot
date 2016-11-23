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


    const float TRANSITION_PLAYALL_TO_BALANCE = 0F;

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

    PaylineDrawer _paylineDrawer;
    ReelContainer _reelContainer;
    SlotBetting _betting;

    void Awake()
    {
        State = new Stack<MachineState>();

        CacheStateBehaviour();

        _ui = FindObjectOfType<SlotMachineUI>() as SlotMachineUI;
        if (_ui == null) Debug.LogError("can't find ui");

        _model = SlotModel.Instance;

        _paylineDrawer = GetComponentInChildren<PaylineDrawer>();

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

        if (methodInfo == null) return DoNothing;
        else return Delegate.CreateDelegate(typeof(Func<IEnumerator>), this, methodInfo) as Func<IEnumerator>;
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
        GameServerCommunicator.Instance.Connect(Config.Common.Host, Config.Common.Port);

        yield break;
    }

    void ConnectComplete()
    {
        GameServerCommunicator.Instance.Login(0, "good");
    }

    void LoginComplete(ResDTO.Login dto)
    {
        StartCoroutine(Initialize(dto));
    }

    IEnumerator Initialize(ResDTO.Login dto)
    {
        //필요한 리소스 Pool 들의 preload 가 완료 되길 기다린다.
        while (GamePool.Instance.IsReady == false)
        {
            yield return null;
        }

        _betting = Config.Common.Betting;

        _model.Initialize(this, dto);
        _ui.Initialize(this);
        _reelContainer.Initialize(this);

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
                if (_model.Owner.Balance < _betting.TotalBet)
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
        _paylineDrawer.Clear();

        _ui.Spin();
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
        var payInfos = _reelContainer.FindAllWinPayInfo();

        //빅윈,메가윈,잭팟, progressive 등을 체크하자
        //경우에 따라 팝업 을 띄워야 할 수도 있음.

        yield return _reelContainer.PlaySpecialWinDirection();

        _reelContainer.PlayAllSymbols();
        if (_paylineDrawer != null) _paylineDrawer.DrawAll(payInfos);

        yield return new WaitForSeconds(TRANSITION_PLAYALL_TO_BALANCE);

        var balanceInfo = GetWinBalanceInfo();
        yield return _ui.AddWinBalance(balanceInfo);

        SetState(MachineState.AfterWin);
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

    WinBalanceInfo GetWinBalanceInfo()
    {
        //시상에 따라 길게 보여줄 수도 있다
        return new WinBalanceInfo(_lastSpinInfo.totalPayout, 1f, 0f);
    }
}

public struct WinBalanceInfo
{
    public double balance;
    public float duration;
    public float skipDelay;

    public WinBalanceInfo(double balance, float duration, float skipDelay)
    {
        this.balance = balance;
        this.duration = duration;
        this.skipDelay = skipDelay;
    }
}

public class WinPayInfo
{
    public enum WinType
    {
        Progressive,
        Payline
    }

    public WinType Type { get; set; }
    public int[] PaylineRows { get; set; }
    public int? PaylineIndex { get; set; }
    public double Payout { get; set; }

    List<Symbol> _symbols;
    public WinPayInfo()
    {

    }

    public void AddSymbol(Symbol symbol)
    {
        if (_symbols == null) _symbols = new List<Symbol>();

        _symbols.Add(symbol);
    }

    public List<Symbol> Symbols
    {
        get { return _symbols; }
        set { _symbols = value; }
    }
}