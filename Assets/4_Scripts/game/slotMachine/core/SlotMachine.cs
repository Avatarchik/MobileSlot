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
        ReelStopComplete,
        Nudge,
        FreeSpinTrigger,
        PlayWin,
        TakeCoin,
        CheckNextSpin,
        BonusSpin,
        FreeSpin,
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

    PaylineModule _paylineModule;
    ReelContainer _reelContainer;
    SlotBetting _betting;
    Topboard _topboard;

    bool _bookedSpin;

    ResDTO.Spin.Payout.SpinInfo _lastSpinInfo;
    WinBalanceInfo _lastWinBalanceInfo;
    float _takeCoinStartTime;
    SendData _testSendData;

    void Awake()
    {
        _model = SlotModel.Instance;

        State = new Stack<MachineState>();
        CacheStateBehaviour();

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

            if (s != MachineState.Null && enter == DoNothing) Debug.LogWarning("can't found '" + stateName + "' enter Method");

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

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
    void Update()
    {
        //if (Input.GetKey(KeyCode.Space))
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TrySpin();
        }
    }
#endif


    protected void SetState(MachineState next)
    {
        if (_currentState == next) return;

        if (_currentState == MachineState.Idle && next == MachineState.Spin) State.Clear();

        //Debug.Log("Update State: " + _currentState.ToString() + " > " + next.ToString());

        State.Push(next);
        _currentState = next;

        StateEnter();
    }

    Coroutine _currentStateRoutine;
    void StateEnter()
    {
        if (_currentStateRoutine != null) StopCoroutine(_currentStateRoutine);

        if (_stateExit != null) StartCoroutine(_stateExit());

        _stateEnter = _stateEnterMap[_currentState];
        _stateExit = _stateExitMap[_currentState];

        if (_stateEnter != null) _currentStateRoutine = StartCoroutine(_stateEnter());
    }

    //EntryPoint
    public void Run(SlotConfig config)
    {
        _config = config;

        if (_config == null)
        {
            Debug.LogError("SlotConfig은 반드시 정의 되어야 합니다");
            return;
        }

        _betting = _config.COMMON.Betting;

        Debug.Log("Run SlotMachine");

        SetState(MachineState.Connecting);
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

        _model.Initialize(this, dto);

        _ui = FindObjectOfType<SlotMachineUI>() as SlotMachineUI;
        if (_ui == null) Debug.LogError("can't find ui");
        _ui.Initialize(this);

        _reelContainer = GetComponentInChildren<ReelContainer>();
        if (_ui == null) Debug.LogError("can't find ReelContainer");
        _reelContainer.Initialize(this);
        _reelContainer.OnPlayAllWin += OnPlayAllWinHandler;
        _reelContainer.OnPlayEachWin += OnPlayEachWinHandler;
        _reelContainer.OnReelStopComplete += OnReelStopCompleteHandler;

        _topboard = GetComponentInChildren<Topboard>();
        if (_topboard == null) Debug.LogError("can't find Topboard");

        _paylineModule = GetComponentInChildren<PaylineModule>();

        SetState(MachineState.Idle);

        GameManager.Instance.GameReady();
    }

    IEnumerator Idle_Enter()
    {
        _ui.Idle();


        if (_model.IsAutoSpin)
        {
            _model.UseAutoSpin();

            yield return new WaitForSeconds(_model.IsFastSpin ? 0f : 0.2f);
            TrySpin();
        }
        else if (_bookedSpin)
        {
            TrySpin();
        }

        yield break;
    }

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
        if (_currentState == MachineState.ReceivedSymbol)
        {
            StopSpin();
        }
        else if (_currentState == MachineState.TakeCoin)
        {
            SkipTakeWin();
        }
    }

    void OpenCoinShop()
    {
        Debug.Log("돈없어. 그지");
    }

    IEnumerator Spin_Enter()
    {
        _bookedSpin = false;

        if (_paylineModule != null) _paylineModule.Clear();

        _ui.Spin();
        _reelContainer.Spin();
        _topboard.Spin();

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


    void SpinComplete(ResDTO.Spin dto)
    {
        _model.SetSpinData(dto);
        _lastSpinInfo = _model.NextSpin();

        SetState(MachineState.ReceivedSymbol);
    }

    IEnumerator ReceivedSymbol_Enter()
    {
        _ui.ReceivedSymbol();
        _reelContainer.ReceivedSymbol(_lastSpinInfo);

        yield break;
    }

    void StopSpin()
    {
        if (_reelContainer.IsExpecting) return;

        _ui.StopSpin();
        _reelContainer.StopSpin();
    }

    void OnReelStopCompleteHandler()
    {
        SetState(MachineState.ReelStopComplete);
    }

    IEnumerator ReelStopComplete_Enter()
    {
        yield return new WaitForSeconds(_config.transition.ReelStopCompleteAfterDealy);

        //결과 심볼들을 바탕으로 미리 계산 해야 하는 일들이 있다면 여기서 미리 계산한다.
        _ui.ReelStopComplete();

        if (_model.HasNudge)
        {
            SetState(MachineState.Nudge);
        }
        else if (_model.IsFreeSpinTrigger)
        {
            SetState(MachineState.FreeSpinTrigger);
        }
        else if (_lastSpinInfo.totalPayout > 0)
        {
            SetState(MachineState.PlayWin);
        }
        else
        {
            SetState(MachineState.CheckNextSpin);
        }

        yield break;
    }

    IEnumerator Nudge_Enter()
    {
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

    IEnumerator PlayWin_Enter()
    {
        _reelContainer.FindAllWinPayInfo();

        //빅윈,메가윈,잭팟, progressive 등을 체크하자
        //경우에 따라 팝업 을 띄워야 할 수도 있음.

        yield return _reelContainer.PlaySpecialWinDirection();

        _reelContainer.PlayAllWin();

        yield return new WaitForSeconds(_config.transition.PlayAllSymbols_WinBalance);

        SetState(MachineState.TakeCoin);
    }

    IEnumerator TakeCoin_Enter()
    {
        _takeCoinStartTime = Time.time;
        _lastWinBalanceInfo = GetWinBalanceInfo();

        _ui.TakeCoin(_lastWinBalanceInfo);
        _topboard.TakeCoin(_lastWinBalanceInfo);

        yield return new WaitForSeconds(_lastWinBalanceInfo.duration);

        SetState(MachineState.CheckNextSpin);
    }

    void SkipTakeWin()
    {
        if (CanSkipTakeCoin() == false) return;

        _ui.SkipTakeCoin();

        //todo
        //jmb win 일때도 스핀 예약해야 할까?
        if (_model.HasNextSpin == false) _bookedSpin = true;

        SetState(MachineState.CheckNextSpin);
    }

    bool CanSkipTakeCoin()
    {
        if (_currentState != MachineState.TakeCoin) return false;

        var elapsedTime = Time.time - _takeCoinStartTime;
        return elapsedTime > _lastWinBalanceInfo.skipDelay;
    }

    void OnPlayAllWinHandler(WinItemList info)
    {
        _ui.PlayAllWin(info);
        _topboard.PlayAllWin(info);

        if (_paylineModule != null) _paylineModule.DrawAll(info);
    }

    void OnPlayEachWinHandler(WinItemList.Item item)
    {
        _ui.PlayEachWin(item);
        _topboard.PlayEachWin(item);

        if (_paylineModule != null) _paylineModule.DrawLine(item);
    }

    IEnumerator CheckNextSpin_Enter()
    {
        if (_model.HasNextSpin)
        {
            if (_model.HasBonusSpin) SetState(MachineState.BonusSpin);
            else if (_model.IsFreeSpinning) SetState(MachineState.FreeSpin);
        }
        else
        {
            SetState(MachineState.ApplySpinResult);
        }
        yield break;
    }

    IEnumerator BonusSpin_Enter()
    {
        _paylineModule.Clear();
        _lastSpinInfo = _model.NextSpin();

        yield return _reelContainer.LockReel(_lastSpinInfo.fixedreel);

        _topboard.BonusSpin();

        yield return new WaitForSeconds(_config.transition.LockReel_BonusSpin);

        _reelContainer.BonusSpin(_lastSpinInfo);

        yield break;
    }

    IEnumerator FreeSpin_Enter()
    {
        yield break;
    }

    IEnumerator ApplySpinResult_Enter()
    {
        //모든 연출이 끝났다.
        //결과를 실제 유저 객체에 반영한다.

        if (_model.IsAutoSpin == false) _reelContainer.PlayEachWin();

        _ui.ApplyUserBalance();
        //반영 후 레벨업이 되었다면 연출한다.

        SetState(MachineState.Idle);
        yield break;
    }

    //todo slotconfig 로 설정하자
    WinBalanceInfo GetWinBalanceInfo()
    {
        var skipDelay = 0f;
        var duration = 1f;

        if (_model.IsJMBWin)
        {
            switch (_model.WinType)
            {
                case SlotConfig.WinType.BIGWIN:
                    duration = 9f;
                    skipDelay = 1f;
                    break;
                case SlotConfig.WinType.MEGAWIN:
                    duration = 12.5f;
                    skipDelay = 1f;
                    break;
                case SlotConfig.WinType.JACPOT:
                    duration = 14f;
                    skipDelay = 1f;
                    break;
            }
        }

        return new WinBalanceInfo(_lastSpinInfo.totalPayout, duration, skipDelay, _model.WinMultiplier, _model.WinType);
    }
}

public struct WinBalanceInfo
{
    public double balance;
    public float duration;
    public float skipDelay;
    public float winMultiplier;
    public SlotConfig.WinType winType;
    public bool IsJMBWin { get { return winType == SlotConfig.WinType.BIGWIN || winType == SlotConfig.WinType.MEGAWIN || winType == SlotConfig.WinType.JACPOT; } }

    public WinBalanceInfo(double balance, float duration, float skipDelay, float winMultiplier, SlotConfig.WinType winType)
    {
        this.balance = balance;
        this.duration = duration;
        this.skipDelay = skipDelay;
        this.winMultiplier = winMultiplier;
        this.winType = winType;
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