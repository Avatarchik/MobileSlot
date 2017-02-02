using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Game
{
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
            FreeSpinReady,
            FreeSpin,
            FreeSpinEnd,
            PlayWin,
            TakeCoin,
            CheckNextSpin,
            BonusSpin,
            ApplySpinResult
        }

        public MachineConfig MachineConfig { get; private set; }
        public Stack<MachineState> State { get; private set; }

        [SerializeField]
        MachineState _currentState;
        public MachineState CurrentState { get { return _currentState; } }
        Dictionary<MachineState, Func<IEnumerator>> _stateEnterMap;
        Dictionary<MachineState, Func<IEnumerator>> _stateExitMap;
        Func<IEnumerator> _stateEnter;
        Func<IEnumerator> _stateExit;
        Coroutine _currentStateRoutine;

        SlotMachineUI _ui;
        SlotModel _model;

        PaylineDisplayer _paylineDisplayer;
        ReelContainer _reelContainer;
        SlotBetting _betting;
        Topboard _topboard;

        bool _bookedSpin;

        ResDTO.Spin.Payout.SpinInfo _lastSpinInfo;
        WinBalanceInfo _lastWinBalanceInfo;
        float _takeCoinStartTime;
        SendData _testSendData;

        FreeSpinDirector _freeSpinDirector;

        SlotConfig _slotConfig;
        bool _isSummary;

        void Awake()
        {
            State = new Stack<MachineState>();
            CacheStateBehaviour();

            GameServerCommunicator.Instance.OnConnect += OnConnectListener;
            GameServerCommunicator.Instance.OnLogin += OnLoginListener;
            GameServerCommunicator.Instance.OnSpin += OnSpinListener;

            GamePool.SetParent(transform.parent);
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

        void StateEnter()
        {
            if (_currentStateRoutine != null) StopCoroutine(_currentStateRoutine);

            if (_stateExit != null) StartCoroutine(_stateExit());

            _stateEnter = _stateEnterMap[_currentState];
            _stateExit = _stateExitMap[_currentState];

            if (_stateEnter != null) _currentStateRoutine = StartCoroutine(_stateEnter());
        }


        void Start()
        {
            _slotConfig = FindObjectOfType<SlotConfig>();
            if (_slotConfig == null) throw new NullReferenceException("SlotConfig can not be null!");

            MachineConfig = _slotConfig.MainMachine;

            _model = SlotModel.Instance;
            _model.Initialize(_slotConfig, this);
            _betting = _model.Betting;

            if (_slotConfig.DebugTestSpin) gameObject.AddComponent<DebugHelper>();

            GamePool.SymbolLoad(_slotConfig);

            SetState(MachineState.Connecting);
        }


        IEnumerator Connecting_Enter()
        {
            GameServerCommunicator.Instance.Connect(_slotConfig.host, _slotConfig.port);

            yield break;
        }

        void OnConnectListener()
        {
            GameServerCommunicator.Instance.Login(_slotConfig.accessID, "good");
        }

        void OnLoginListener(ResDTO.Login dto)
        {
            StartCoroutine(Initialize(dto));
        }

        IEnumerator Initialize(ResDTO.Login dto)
        {
            //필요한 리소스 Pool 들의 preload 가 완료 되길 기다린다.
            while (GamePool.IsReady == false)
            {
                Debug.Log("Wait Pool...");
                yield return new WaitForSeconds(0.2f);
            }

            Debug.Log("Pool readied");

            _model.SetLoginData(dto);

            //-----------------------------------------------------------------
            // essential module
            // ReelContainer & Topboard & SlotMachineUI
            //-----------------------------------------------------------------

            _ui = FindObjectOfType<SlotMachineUI>() as SlotMachineUI;
            _ui.Initialize(this);

            _reelContainer = GetComponentInChildren<ReelContainer>();
            _reelContainer.Initialize(this);
            _reelContainer.OnPlayAllWin += OnPlayAllWinHandler;
            _reelContainer.OnPlayEachWin += OnPlayEachWinHandler;
            _reelContainer.OnReelStopComplete += OnReelStopCompleteListener;

            _topboard = GetComponentInChildren<Topboard>();

            _paylineDisplayer = GetComponentInChildren<PaylineDisplayer>();
            if( _paylineDisplayer != null ) _paylineDisplayer.Initialize(this);

            _freeSpinDirector = GetComponentInChildren<FreeSpinDirector>();
            if( _freeSpinDirector != null ) _freeSpinDirector.Initialize(this);

            SlotSoundList.Initialize();

            GameManager.Instance.SceneReady();

            SetState(MachineState.Idle);
        }

        IEnumerator Idle_Enter()
        {
            SlotSoundList.PlayBGM();

            _ui.Idle();

            if (_model.IsAutoSpin)
            {
                _model.UseAutoSpin();

                yield return new WaitForSeconds(_model.IsFastSpin ? 0f : MachineConfig.transition.AutoSpinDelay );
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
            //당첨되어 돈올라가는 도중이면 스킵하고 다음 스핀
            //닫을 수 있는 팝업창이 띄워져 있으면 팝업창 닫고
            if (_currentState == MachineState.ReceivedSymbol ||
                _currentState == MachineState.FreeSpin)
            {
                StopSpin();
            }
            else if (_currentState == MachineState.TakeCoin)
            {
                stopTakeCoin();
            }
        }

        void OpenCoinShop()
        {
            Debug.Log("돈없어. 그지");
        }

        IEnumerator Spin_Enter()
        {
            _bookedSpin = false;

            if (_paylineDisplayer != null) _paylineDisplayer.Clear();

            _betting.Save();
            
            _ui.Spin();
            _reelContainer.Spin();
            _topboard.Spin();

            SlotSoundList.Spin();

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

        void OnSpinListener(ResDTO.Spin dto)
        {
            if (_currentState == MachineState.Spin)
            {
                _model.SetSpinData(dto);
                SetState(MachineState.ReceivedSymbol);
            }
            else if (MachineConfig.TriggerType == FreeSpinTriggerType.Select &&
                     _currentState == MachineState.FreeSpinReady)
            {                
                _model.SetFreeSpinData(dto);
                SetState(MachineState.FreeSpin);
            }
            else
            {
                Debug.Log("Incorrect SpinListener");
            }
        }

        IEnumerator ReceivedSymbol_Enter()
        {
            _lastSpinInfo = _model.NextSpin();
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

        void OnReelStopCompleteListener()
        {
            SlotSoundList.StopSpin();
            SetState(MachineState.ReelStopComplete);
        }

        IEnumerator ReelStopComplete_Enter()
        {
            yield return new WaitForSeconds(MachineConfig.transition.ReelStopAfterDelay);

            //결과 심볼들을 바탕으로 미리 계산 해야 하는 일들이 있다면 여기서 미리 계산한다.
            _ui.ReelStopComplete();

            if (_model.HasNudge) SetState(MachineState.Nudge);
            else if (_model.IsFreeSpinTrigger) SetState(MachineState.FreeSpinTrigger);
            else if (_lastSpinInfo.totalPayout > 0) SetState(MachineState.PlayWin);
            else SetState(MachineState.CheckNextSpin);

            yield break;
        }

        IEnumerator Nudge_Enter()
        {
            yield break;
        }

        IEnumerator FreeSpinTrigger_Enter()
        {
            SlotSoundList.PlayFreeSpinTrigger();

            _reelContainer.FreeSpinTrigger();
            _topboard.FreeSpinTrigger();
            _ui.FreeSpinTrigger();

            yield return new WaitForSeconds(MachineConfig.transition.FreeSpinTriggerDuration);

            if (_lastSpinInfo.totalPayout > 0) SetState(MachineState.PlayWin);
            else SetState(MachineState.CheckNextSpin);
        }

        IEnumerator FreeSpinTrigger_Exit()
        {
            SlotSoundList.StopFreeSpinTrigger();
            yield break;
        }

        IEnumerator FreeSpinReady_Enter()
        {
            if (_paylineDisplayer != null) _paylineDisplayer.Clear();

            SlotSoundList.PlayFreeSpinReady();

            _reelContainer.FreeSpinReady();
            _topboard.FreeSpinReady();

            if (_freeSpinDirector == null)
            {
                yield return new WaitForSeconds(2f);

                SetState(MachineState.FreeSpin);
            }
            else if (_model.IsFreeSpinReTrigger)
            {
                yield return StartCoroutine(_freeSpinDirector.Retrigger());

                SetState(MachineState.FreeSpin);
            }
            else if (MachineConfig.TriggerType == FreeSpinTriggerType.Select)
            {
                yield return StartCoroutine(_freeSpinDirector.Select());

                GameServerCommunicator.Instance.FreeSpin(_betting.LineBet, _freeSpinDirector.SelectedKind.Value);
            }
            else
            {
                yield return StartCoroutine(_freeSpinDirector.Trigger());

                SetState(MachineState.FreeSpin);
            }
        }

        IEnumerator FreeSpinReady_Exit()
        {
            SlotSoundList.StopFreeSpinReady();
            _freeSpinDirector.Close();
            yield break;
        }

        IEnumerator FreeSpin_Enter()
        {
            _lastSpinInfo = _model.UseFreeSpin();

            if (_lastSpinInfo == null) Debug.LogError("freeSpin info null");

            if (_model.FreeSpinCurrentCount == 1)
            {
                yield return StartCoroutine(FreeSpinModeStart());
            }

            _topboard.FreeSpin();
            _ui.FreeSpin();
            _reelContainer.FreeSpin(_lastSpinInfo);

            SlotSoundList.FreeSpin();
        }

        IEnumerator FreeSpinEnd_Enter()
        {
            if (_paylineDisplayer != null) _paylineDisplayer.Clear();

            _model.FreeSpinEnd();

            if (_freeSpinDirector == null)
            {
                yield return new WaitForSeconds(2f);
            }
            else
            {
                yield return StartCoroutine(_freeSpinDirector.Summary());
            }

            yield return StartCoroutine(FreeSpinModeStop());

            _isSummary = true;

            SetState(MachineState.TakeCoin);
        }

        IEnumerator FreeSpinModeStart()
        {
            Debug.Log("FreeSpinMode!");
            /*
            if (mIsFreeSpinMode) return;

            mIsFreeSpinMode = true;

            if (mCabinetFree)
            {
                mCabinetFree.visible = true;
                TweenLite.to(mCabinetFree, 0.5,{ alpha: 1, ease: Cubic.easeOut });
            }

            mTopboard.freeSpinMode();
            Background.instance.freeSpinMode();
            mReelContainer.freeSpinMode();
            mInfoPanel.freeSpinMode();
            */

            yield break;
        }

        IEnumerator FreeSpinModeStop()
        {
            /*
            if (mIsFreeSpinMode == false) return;

            mIsFreeSpinMode = false;

            //			if( mModel.isAuto )	mModel.isAuto = false;

            if (mCabinetFree)
            {
                TweenLite.to(mCabinetFree, 0.3,{
                alpha: 0, ease: Cubic.easeOut, onComplete: function():void{
                        mCabinetFree.visible = false;
                    }
                })
                }

            mInfoPanel.freeSpinModeEnd();
            mTopboard.freeSpinModeEnd();
            mReelContainer.freeSpinModeEnd();
            Background.instance.freeSpinModeEnd();
            */

            yield break;
        }

        IEnumerator PlayWin_Enter()
        {
            _reelContainer.FindAllWinPayInfo();

            //빅윈,메가윈,잭팟, progressive 등을 체크하자
            //경우에 따라 팝업 을 띄워야 할 수도 있음.

            yield return _reelContainer.PlaySpecialWinDirection();

            _reelContainer.playSymbolsWin();

            yield return new WaitForSeconds(MachineConfig.transition.PlaySymbolAfterDelay);

            SetState(MachineState.TakeCoin);
        }

        IEnumerator TakeCoin_Enter()
        {
            _takeCoinStartTime = Time.time;
            _lastWinBalanceInfo = GetWinBalanceInfo();

            Debug.Log(_lastWinBalanceInfo.ToString());

            _ui.TakeCoin(_lastWinBalanceInfo, _isSummary);
            _topboard.TakeCoin(_lastWinBalanceInfo, _isSummary);

            yield return new WaitForSeconds(_lastWinBalanceInfo.duration);

            _isSummary = false;

            SetState(MachineState.CheckNextSpin);
        }

        WinBalanceInfo GetWinBalanceInfo()
        {
            double winBalance = _isSummary ? _model.TotalPayout : _lastSpinInfo.totalPayout;
            float multiplier = (float)(winBalance / _betting.TotalBet);
            float skipDelay = 0f;
            float duration = 1f;

            if (_model.IsJMBWin)
            {
                switch (_model.WinType)
                {
                    case PayoutWinType.BIGWIN:
                        skipDelay = 1f;
                        duration = 9f;
                        break;
                    case PayoutWinType.MEGAWIN:
                        skipDelay = 1f;
                        duration = 12.5f;
                        break;
                    case PayoutWinType.JACPOT:
                        skipDelay = 1f;
                        duration = 14f;
                        break;
                }
            }
            else if (multiplier >= 2)
            {
                skipDelay = 0f;
                duration = 2f;
            }
            else
            {
                skipDelay = 0f;
                duration = 1f;
            }

            if (_model.IsFastSpin)
            {
                skipDelay *= 0.5f;
                duration *= 0.5f;
            }

            return new WinBalanceInfo(winBalance, duration, skipDelay, (float)multiplier, _model.WinType);
        }

        void stopTakeCoin()
        {
            if (CanSkipTakeCoin() == false) return;

            _ui.SkipTakeCoin();

            //todo
            //jmb win 일때도 스핀 예약해야 할까?
            if (_model.IsFreeSpinTrigger == false &&
                _model.IsFreeSpinReTrigger == false &&
                _model.HasNextSpin == false)
            {
                _bookedSpin = true;
            }

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

            if (_paylineDisplayer != null) _paylineDisplayer.DrawAll(info);
        }

        void OnPlayEachWinHandler(WinItemList.Item item)
        {
            _ui.PlayEachWin(item);
            _topboard.PlayEachWin(item);

            if (_paylineDisplayer != null) _paylineDisplayer.DrawLine(item);
        }

        IEnumerator CheckNextSpin_Enter()
        {
            if (_model.HasBonusSpin) SetState(MachineState.BonusSpin);
            else if (_model.IsFreeSpinTrigger) SetState(MachineState.FreeSpinReady);
            else if (_model.IsFreeSpinning && _model.FreeSpinRemain > 0) SetState(MachineState.FreeSpin);
            else if (_model.IsFreeSpinning && _model.FreeSpinRemain <= 0) SetState(MachineState.FreeSpinEnd);
            else SetState(MachineState.ApplySpinResult);

            yield break;
        }

        IEnumerator BonusSpin_Enter()
        {
            if (_paylineDisplayer != null) _paylineDisplayer.Clear();

            _lastSpinInfo = _model.NextSpin();

            yield return _reelContainer.LockReel(_lastSpinInfo.fixedreel);

            _topboard.BonusSpin();

            yield return new WaitForSeconds(MachineConfig.transition.LockReelAfterDelay);

            _reelContainer.BonusSpin(_lastSpinInfo);
            SlotSoundList.Spin();

            yield break;
        }

        IEnumerator ApplySpinResult_Enter()
        {
            //모든 연출이 끝났다.
            //결과를 실제 유저 객체에 반영한다.

            if (_model.IsAutoSpin == false ) _reelContainer.PlayEachWin();

            _ui.ApplyUserBalance();
            _model.Reset();
            //반영 후 레벨업이 되었다면 연출한다.

            SetState(MachineState.Idle);
            yield break;
        }
    }

    public struct WinBalanceInfo
    {
        public double win;
        public float duration;
        public float skipDelay;
        public float winMultiplier;
        public PayoutWinType winType;

        public WinBalanceInfo(double win, float duration, float skipDelay,
                              float winMultiplier, PayoutWinType winType)
        {
            this.win = win;
            this.duration = duration;
            this.skipDelay = skipDelay;
            this.winMultiplier = winMultiplier;
            this.winType = winType;
        }

        public override string ToString()
        {
            return string.Format("{0} win. ( duration:{1}, skipDelay:{2}, multi:{3}, type:{4})", win, duration, skipDelay, winMultiplier, winType);
        }
    }

    //todo
    //간단한 이터레이터 구현으로 고치자
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

        public void PlaySymbolsWin()
        {
            var count = AllSymbols.Count;
            for (var i = 0; i < count; ++i)
            {
                AllSymbols[i].SetState(SymbolState.Win);
            }
        }

        public void Reset()
        {
            _items.Clear();
            _winTablesIndices.Clear();
            AllSymbols.Clear();
            PaylineItemCount = 0;
            Payout = 0;
        }

        //todo
        //심플 IEnumerator 구현으로 바꾸자
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
            public void PlaySymbolsWin()
            {
                var len = _symbols.Count;
                for( var i = 0; i < len; ++i )
                {
                    _symbols[i].SetState( SymbolState.Win );
                }
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
}