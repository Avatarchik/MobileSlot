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
        BonusSpin,
        ApplySpinResult
    }

    SlotConfig _config;
    public SlotConfig Config { get { return _config; } }

    public Stack<MachineState> State { get; private set; }

    [SerializeField]
    MachineState _currentState;
    public MachineState CurrentState { get { return _currentState; } }
    Dictionary<MachineState, Func<IEnumerator>> _stateEnterMap;
    Dictionary<MachineState, Func<IEnumerator>> _stateExitMap;
    Func<IEnumerator> _stateEnter;
    Func<IEnumerator> _stateExit;

    SlotMachineUI _ui;
    SlotModel _model;

    PaylineDrawer _paylineDrawer;
    ReelContainer _reelContainer;
    SlotBetting _betting;
    WinTable _winTable;

    void Awake()
    {
        State = new Stack<MachineState>();

        CacheStateBehaviour();

        _ui = FindObjectOfType<SlotMachineUI>() as SlotMachineUI;
        if (_ui == null) Debug.LogError("can't find ui");

        _model = SlotModel.Instance;

        _paylineDrawer = GetComponentInChildren<PaylineDrawer>();

        _reelContainer = GetComponentInChildren<ReelContainer>();
        _reelContainer.OnPlayAllWin += OnPlayAllWinHandler;
        _reelContainer.OnPlayEachWin += OnPlayEachWinHandler;
        _reelContainer.OnReelStopComplete += OnReelStopCompleteHandler;

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

    public void Run(SlotConfig config)
    {
        _config = config;

        if (_config == null)
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
        GameServerCommunicator.Instance.Connect(_config.COMMON.Host, _config.COMMON.Port);

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

        _betting = _config.COMMON.Betting;

        _model.Initialize(this, dto);
        _ui.Initialize(this);
        _reelContainer.Initialize(this);

        _winTable = GetComponentInChildren<WinTable>();

        SetState(MachineState.Idle);

        GameManager.Instance.GameReady();
    }

    IEnumerator Idle_Enter()
    {
        _ui.Idle();
        yield break;
    }

    SendData _testSendData;

    public void TrySpin(SendData testSendData = null)
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
                    _testSendData = testSendData;
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
        if (_paylineDrawer != null) _paylineDrawer.Clear();
        if (_winTable != null) _winTable.Clear();


        _ui.Spin();
        _reelContainer.Spin();

        if (_testSendData != null)
        {
            GameServerCommunicator.Instance.Send(_testSendData);
            _testSendData = null;
        }
        else
        {
            GameServerCommunicator.Instance.Spin(_betting.LineBet);
        }

        yield break;
    }

    ResDTO.Spin.Payout.SpinInfo _lastSpinInfo;

    void SpinComplete(ResDTO.Spin dto)
    {
        _model.SetSpinData(dto);
        _lastSpinInfo = _model.NextSpin();

        SetState(MachineState.ReceivedSymbol);
    }

    IEnumerator ReceivedSymbol_Enter()
    {
        _reelContainer.ReceivedSymbol(_lastSpinInfo);

        yield break;
    }

    void OnReelStopCompleteHandler()
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
        _reelContainer.FreeSpinTrigger();
        yield break;
    }

    IEnumerator FreeSpinTrigge_Exit()
    {
        //프리스핀 트리거 리셋
        yield break;
    }

    IEnumerator Win_Enter()
    {
        _reelContainer.FindAllWinPayInfo();

        yield return new WaitForSeconds(_config.transition.ReelStopCompleteAfterDealy);

        //빅윈,메가윈,잭팟, progressive 등을 체크하자
        //경우에 따라 팝업 을 띄워야 할 수도 있음.

        yield return _reelContainer.PlaySpecialWinDirection();

        _reelContainer.PlayAllWin();

        yield return new WaitForSeconds(_config.transition.PlayAllSymbols_WinBalance);

        var balanceInfo = GetWinBalanceInfo();
        yield return _ui.AddWinBalance(balanceInfo);

        SetState(MachineState.AfterWin);
    }

    void OnPlayAllWinHandler(WinItemList info)
    {
        _ui.PlayAllWin(info);
        if (_paylineDrawer != null) _paylineDrawer.DrawAll(info);
        if (_winTable != null) _winTable.PlayAllWin(info);
    }

    void OnPlayEachWinHandler(WinItemList.Item item)
    {
        _ui.PlayEachWin(item);
        if (_paylineDrawer != null) _paylineDrawer.DrawLine(item);
        if (_winTable != null) _winTable.PlayEachWin(item);
    }

    IEnumerator AfterWin_Enter()
    {
        if (_model.HasBonusSpin)
        {
            SetState(MachineState.BonusSpin);
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

    IEnumerator BonusSpin_Enter()
    {
        _paylineDrawer.Clear();
        _lastSpinInfo = _model.NextSpin();

        yield return _reelContainer.LockReel(_lastSpinInfo.fixedreel);

        //전광판 보너스 스핀 연출

        _reelContainer.BonusSpin(_lastSpinInfo);

        yield break;
    }

    IEnumerator ApplySpinResult_Enter()
    {
        //모든 연출이 끝났다.
        //결과를 실제 유저 객체에 반영한다.

        if (_model.AutoSpin == false) _reelContainer.PlayEachWin();

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

public class WinItemList : IEnumerable<WinItemList.Item>
{
    List<WinItemList.Item> _items;
    public int ItemCount { get { return _items.Count; } }

    List<int> _winTablesIndices;
    public List<int> WinTablesIndices { get { return _winTablesIndices; } }

    public int PaylineItemCount { get; private set; }
    public double Payout { get; private set; }
    public List<Symbol> AllSymbols { get; private set; }

    public WinItemList()
    {
        _items = new List<WinItemList.Item>();
        _winTablesIndices = new List<int>();

        AllSymbols = new List<Symbol>();
    }

    public void AddItem(WinItemList.Item item)
    {
        if (item == null || _items.Contains(item)) return;

        _items.Add(item);

        Payout += item.Payout;

        if (item.PaylineIndex != null) ++PaylineItemCount;
        if (item.WinTable != null) _winTablesIndices.Add(item.WinTable.Value);

        IncludeSymbols(item.Symbols);
    }

    void IncludeSymbols(List<Symbol> symbols)
    {
        if (symbols == null) return;
        var count = symbols.Count;

        for (var i = 0; i < count; ++i)
        {
            var symbol = symbols[i];
            if (symbol == null || AllSymbols.Contains(symbol)) continue;
            AllSymbols.Add(symbol);
        }
    }

    public WinItemList.Item GetItem(int idx)
    {
        if (idx < 0 || idx >= _items.Count) return null;
        else return _items[idx];
    }

    public void Clear()
    {
        _items.Clear();
        _winTablesIndices.Clear();
        AllSymbols.Clear();
        PaylineItemCount = 0;
        Payout = 0;
    }

    public IEnumerator<WinItemList.Item> GetEnumerator()
    {
        return new WinItemListEnumerator(_items);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    public class Item
    {
        public enum ItemType
        {
            Progressive,
            Payline
        }

        public ItemType Type { get; set; }
        public int[] PaylineRows { get; set; }
        public int? PaylineIndex { get; set; }
        public double Payout { get; set; }
        public int? WinTable { get; set; }

        List<Symbol> _symbols;
        public List<Symbol> Symbols { get { return _symbols; } }

        public Item()
        {
            _symbols = new List<Symbol>();
        }

        public void AddSymbol(Symbol symbol)
        {
            if (symbol == null || _symbols.Contains(symbol)) return;
            _symbols.Add(symbol);
        }
    }

    class WinItemListEnumerator : IEnumerator<WinItemList.Item>
    {
        List<WinItemList.Item> _items;
        int _position = -1;

        public WinItemListEnumerator(List<WinItemList.Item> items)
        {
            _items = items;
        }
        public bool MoveNext()
        {
            ++_position;
            return (_position < _items.Count);
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public WinItemList.Item Current
        {
            get
            {
                try
                {
                    return _items[_position];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        public void Dispose()
        {

        }

        public void Reset()
        {
            _position = -1;
        }
    }
}