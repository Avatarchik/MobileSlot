using UnityEngine;
using UnityEngine.Events;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DG.Tweening;

namespace Game
{
    public class Reel : MonoBehaviour
    {
        public event UnityAction<Reel> OnStop;

        const int SPIN_COUNT_LIMIT = 50;

        public GameObject expectObject;

        #region Property
        public bool IsLocked { get; private set; }
        public int StartOrder { get; set; }
        public ExpectReelType ExpectType { get; set; }
        public bool IsExpectable { get { return ExpectType != ExpectReelType.Null; } }
        public bool IsLoop { get; set; }

        Transform _symbolContainer;
        public Transform SymbolContainer { get { return _symbolContainer; } }
        #endregion

        protected int _column;
        protected List<Symbol> _symbols;

        protected float _spinDis;

        [SerializeField]
        protected SymbolNames _lastSymbolNames;
        [SerializeField]
        protected SymbolNames _receivedSymbolNames;

        protected ReelStrips _currentStrips;

        [SerializeField]
        int _spinCount;
        int _symbolNecessaryCount; //화면에 보여야할 심볼 수 ( row ) + 위아래 여유 수 ( marinCount * 2 )
        bool _isReceived = true;
        Vector3 _spinDestination;
        Tween _spinTween;
        bool _isTweenLast;
        bool _isExpecting;
        Vector3 _spinStartPos;

        bool _isSpinning;
        bool _isStopping;

        void Awake()
        {
            _symbols = new List<Symbol>();
            _symbolContainer = transform.Find("symbols");

            if (expectObject != null) expectObject.SetActive(false);
        }

        protected SlotMachine _machine;
        protected MachineConfig _machineConfig;

        public void Initialize(SlotMachine relativeMachine)
        {
            _machine = relativeMachine;
            _machineConfig = _machine.MachineConfig;

            _symbolNecessaryCount = _machineConfig.row + _machineConfig.MarginSymbolCount * 2;

            _lastSymbolNames = _machineConfig.GetStartSymbolNames(_column);
            _receivedSymbolNames = new SymbolNames(_machineConfig.row);

            //CreateStartSymbols
            CreateStartSymbols();
            AlignSymbols();
        }

        void CreateStartSymbols()
        {
            for (var i = 0; i < _symbolNecessaryCount; ++i)
            {
                var symbol = CreateSymbol(_lastSymbolNames[i]);
                AddSymbolToTail(symbol);
            }

            CheckTopChildSymbol();
        }

        void CheckTopChildSymbol()
        {
            if (_machineConfig.TopChildSymbolType == SymbolType.Blank) return;

            var len = _symbols.Count;
            for (var i = 0; i < len; ++i)
            {
                var symbol = _symbols[i];
                if( symbol.Type == _machineConfig.TopChildSymbolType ) symbol.GoToTop();
            }
        }

        void AlignSymbols(float offsetY = 0f, SymbolState state = SymbolState.Null )
        {
            var ypos = GetStartSymbolPos();

            var len = _symbols.Count;
            for (var i = 0; i < len; ++i)
            {
                var symbol = _symbols[i];
                symbol.Y = ypos;
                ypos -= symbol.Height;

                if( state != SymbolState.Null ) symbol.SetState( state );
            }

            _symbolContainer.localPosition = new Vector3(0f, offsetY, 0f);
        }

        virtual protected float GetStartSymbolPos()
        {
            var res = 0f;
            for (var i = 0; i < _machineConfig.MarginSymbolCount; ++i)
            {
                res += _symbols[i].Height;
            }

            return res;
        }


        void KillSpinTween()
        {
            if (_spinTween == null) return;
            if (_spinTween.IsPlaying() == false) return;

            _spinTween.Kill();
            _spinTween = null;
        }

        void UpdateSpinDestination()
        {
            _spinDestination = _spinStartPos - new Vector3(0f, _spinDis, 0f);
        }

        public void Spin(ResDTO.Spin.Payout.SpinInfo spinInfo = null)
        {
            _isSpinning = true;
            _isReceived = false;
            _spinCount = 0;
            _currentStrips = GetCurrentStrip();

            if (spinInfo != null) ReceivedSymbol(spinInfo);

            UpdateSymbolState(SymbolState.Spin);

            SpinReel();
        }

        public void ReceivedSymbol(ResDTO.Spin.Payout.SpinInfo spinInfo)
        {
            if (spinInfo == null) return;

            _isReceived = true;

            _receivedSymbolNames.Value = spinInfo.GetReelData(_column, _machineConfig.row);

            /*
            string[] reelData = spinInfo.GetReelData(_column, _config.Main.Row);

            for (var i = 0; i < reelData.Length; ++i)
            {
                _receivedSymbolNames[i] = _config.Main.nameMap[reelData[i]];
            }
            */

            //Debug.Log(string.Join(",", _receivedSymbolNames));
        }

        public void FreeSpin(ResDTO.Spin.Payout.SpinInfo spinInfo)
        {
            Spin(spinInfo);
        }

        public void BonusSpin(ResDTO.Spin.Payout.SpinInfo spinInfo)
        {
            Spin(spinInfo);
            //연출
        }

        void SpinReel()
        {
            if (_spinCount >= SPIN_COUNT_LIMIT)
            {
                ServerTooLate();
                return;
            }

            _spinDis = 0;
            _spinStartPos = _symbolContainer.localPosition;

            KillSpinTween();
            RemoveSymbolsExceptNecessary();

            ++_spinCount;
            if (_spinCount == 1)
            {
                TweenFirst();
            }
            else if (_spinCount <= _machineConfig.SpinCountThreshold || _isReceived == false || IsLoop)
            {
                TweenLiner();
            }
            else
            {
                TweenLast();
            }
        }

        void ServerTooLate()
        {
            //데이터 어디갔니?
            //뭔가 문제가 일어남. 설정한 최대 스핀이 돌동안 서버로부터 응답이 안왔음
        }

        virtual protected void TweenFirst()
        {
            Tweener tweenBack = null;

            if (_machineConfig.tweenFirstBackInfo.distance > 0)
            {
                var backPos = _symbolContainer.position + new Vector3(0f, _machineConfig.tweenFirstBackInfo.distance, 0f);
                tweenBack = _symbolContainer.DOMove(backPos, _machineConfig.tweenFirstBackInfo.duration);
                tweenBack.SetEase(Ease.OutSine);
                _spinDis += _machineConfig.tweenFirstBackInfo.distance;
            }

            AddSpinningSymbols(_machineConfig.SpinningSymbolCount);
            CheckTopChildSymbol();
            UpdateSpinDestination();

            var duration = _spinDis / _machineConfig.SpinSpeedPerSec;
            var tween = _symbolContainer.DOLocalMove(_spinDestination, duration);
            // tween.SetEase(Ease.Linear);
            tween.SetEase(Ease.InCubic);

            //todo
            //시퀀스 매 생성하지 않고 재활용 하기
            var startDelay = StartOrder * _machineConfig.DelayEachSpin;
            Sequence firstTweenSequence = DOTween.Sequence();
            firstTweenSequence.PrependInterval(startDelay);
            if (tweenBack != null) firstTweenSequence.Append(tweenBack);
            firstTweenSequence.Append(tween);
            firstTweenSequence.AppendCallback(SpinReel).Play();

            _spinTween = firstTweenSequence;
        }

        virtual protected void TweenLiner()
        {
            AddSpinningSymbols(_machineConfig.SpinningSymbolCount);
            CheckTopChildSymbol();

            var duration = _spinDis / _machineConfig.SpinSpeedPerSec;

            UpdateSpinDestination();

            var tween = _symbolContainer.DOLocalMove(_spinDestination, duration);
            tween.OnComplete(SpinReel).Play();

            _spinTween = tween;
        }

        virtual protected void TweenLast()
        {
            _isTweenLast = true;

            AddSpinningSymbols(StartOrder * _machineConfig.IncreaseCount);
            ComposeLastSpinningSymbols();

            _spinDis -= _machineConfig.tweenFirstBackInfo.distance;
            _spinDis += _machineConfig.tweenLastBackInfo.distance;
            UpdateSpinDestination();

            var duration = _spinDis / _machineConfig.SpinSpeedPerSec;
            var tween = _symbolContainer.DOLocalMove(_spinDestination, duration);
            tween.SetEase(Ease.Linear);
            tween.OnComplete(OnSpinReelComplete);
            tween.Play();

            _spinTween = tween;
        }

        virtual protected void ComposeLastSpinningSymbols()
        {
            AddInterpolationSymbols();
            AddResultSymbols();
            AddSpinningSymbols(_machineConfig.MarginSymbolCount);
            CheckTopChildSymbol();
        }

        public void StopSpin()
        {
            if (_isSpinning == false || _isReceived == false || _isStopping == true) return;

            _isStopping = true;

            KillSpinTween();
            TweenSkip();
        }

        void TweenSkip()
        {
            if (_isTweenLast == false)
            {
                ComposeLastSpinningSymbols();

                _spinDis -= _machineConfig.tweenFirstBackInfo.distance;
                _spinDis += _machineConfig.tweenLastBackInfo.distance;
                UpdateSpinDestination();
            }

            var duration = 0.1f;
            var tween = _symbolContainer.DOLocalMove(_spinDestination, duration);
            tween.SetEase(Ease.Linear);
            tween.OnComplete(OnSpinReelComplete);
            tween.Play();

            _spinTween = tween;
        }

        void OnSpinReelComplete()
        {
            if (_isExpecting) HideExpect();

            _isTweenLast = false;
            _isSpinning = false;
            _isStopping = false;
            _isExpecting = false;

            _spinTween = null;
            _lastSymbolNames.Value = _receivedSymbolNames.Value;
            ExpectType = ExpectReelType.Null;

            RemoveSymbolsExceptNecessary();
            AlignSymbols(-_machineConfig.tweenLastBackInfo.distance);

            SlotSoundList.ReelStop();

            if (_machineConfig.tweenLastBackInfo.distance != 0)
            {
                var backOutTween = _symbolContainer.DOLocalMove(Vector3.zero, _machineConfig.tweenLastBackInfo.duration);
                backOutTween.SetEase(Ease.OutBack);
                backOutTween.Play();
            }

            UnLock();

            if (OnStop != null) OnStop(this);
        }

        public void Lock()
        {
            if (IsLocked) return;

            IsLocked = true;
        }

        public void UnLock()
        {
            if (IsLocked == false) return;

            IsLocked = false;
        }

        public void SpinToExpect()
        {
            _isExpecting = true;
            //bg expect;
            //effect expect
            ShowExpect();
        }

        void ShowExpect()
        {
            if (expectObject != null) expectObject.SetActive(true);
        }

        void HideExpect()
        {
            if (expectObject != null) expectObject.SetActive(false);
        }

        void RemoveSymbolsExceptNecessary()
        {
            //필요조건 수 를 제외하고 모두 지운다.
            int count = _symbols.Count - _symbolNecessaryCount;
            while (count-- > 0)
            {
                var idx = _symbols.Count - 1;
                var symbol = _symbols[idx];
                _symbols.RemoveAt(idx);
                symbol.Clear();
            }
        }

        void AddSymbolToTail(Symbol symbol, float ypos = 0f)
        {
            if (symbol == null) throw new System.ArgumentNullException("symbol", "symbol can't be null");

            _symbols.Add(symbol);
            symbol.SetReel(this, ypos);
        }

        protected void AddSpinningSymbol(string symbolname, bool increaseDis = true)
        {
            var symbol = CreateSymbol(symbolname);
            var h = symbol.Height;

            AddSymbolToHead(symbol, _symbols[0].Y + h);

            if (increaseDis) _spinDis += h;
        }

        protected void AddSymbolToHead(Symbol symbol, float ypos = 0f)
        {
            if (symbol == null) throw new System.ArgumentNullException("symbol", "symbol can't be null");

            _symbols.Insert(0, symbol);
            symbol.SetReel(this, ypos, true);
        }

        protected void AddSpinningSymbols(int count)
        {
            while (count-- > 0) AddSpinningSymbol(GetSpinningSymbolName());
        }

        protected void AddResultSymbols()
        {
            var count = _receivedSymbolNames.Length;
            while (count-- > 0) AddSpinningSymbol(_receivedSymbolNames[count]);
        }

        virtual protected void AddInterpolationSymbols()
        {
            //현재 스핀 중인 심볼과 결과 심볼 사이에 부드럽게 이어줄 심볼들을 만들어 넣는다.
            //null 심볼이나 stack 심볼, big 심볼등이 있을 수 있다
        }

        virtual protected string GetSpinningSymbolName()
        {
            return _currentStrips.GetRandom(_column);
        }


        public void FreeSpinTrigger()
        {
            //bg freeSpinTrigger
            //effect freeSpinTrigger
            // reel.updateSymbolAorB( SymbolType.FREESPIN, SymbolState.TRIGGER,SymbolState.IDLE);

            UpdateSymbolState(SymbolType.FSScatter, SymbolState.Trigger);
        }

        protected Symbol CreateSymbol(string symbolName)
        {
            Symbol symbol = GamePool.SpawnSymbol(symbolName);

            if (symbol == null) throw new System.NullReferenceException("Symbol '" + symbolName + "' is null");

            if (symbol.IsInitialized == false) symbol.Initialize(symbolName, _machineConfig);

            return symbol;
        }

        void UpdateSymbolState(SymbolState state)
        {
            var count = _symbols.Count;
            for (var i = 0; i < count; ++i)
            {
                _symbols[i].SetState(state);
            }
        }

        void UpdateSymbolState(SymbolType type, SymbolState state,
                               SymbolState restState = SymbolState.Null,
                               bool includeMarginSymbol = false,
                               string[] ignoreNames = null)
        {
            var startIndex = _machineConfig.MarginSymbolCount;
            var length = _symbols.Count - _machineConfig.MarginSymbolCount;

            if (includeMarginSymbol)
            {
                startIndex = 0;
                length = length + _machineConfig.MarginSymbolCount;
            }

            for (var i = startIndex; i < length; ++i)
            {
                var symbol = _symbols[i];

                if ((symbol.Type != type) ||
                    (ignoreNames != null && Array.IndexOf(ignoreNames, symbol.SymbolName) != -1))
                {
                    if (restState != SymbolState.Null) symbol.SetState(restState);

                    continue;
                }

                symbol.SetState(state);
            }
        }

        public Symbol GetSymbolAt(int row, bool includeMarginSymbol = false)
        {
            if (includeMarginSymbol == false) row += _machineConfig.MarginSymbolCount;

            if (row < 0 || _symbols.Count <= row) throw new System.ArgumentOutOfRangeException();
            return _symbols[row];
        }

        public List<Symbol> ContainsByType(SymbolType symbolType, bool includeMarginSymbol = false)
        {
            var res = new List<Symbol>();

            var startIndex = _machineConfig.MarginSymbolCount;
            var length = _symbols.Count - _machineConfig.MarginSymbolCount;

            if (includeMarginSymbol)
            {
                startIndex = 0;
                length = length + _machineConfig.MarginSymbolCount;
            }

            for (var i = startIndex; i < length; ++i)
            {
                var symbol = _symbols[i];
                if (symbol.Type == symbolType) res.Add( symbol );
            }

            return res;
        }

        ReelStrips GetCurrentStrip()
        {
            return _machineConfig.reelStripsBundle.GetStrips();
        }

        string GetAddedSymbolNames()
        {
            StringBuilder sb = new StringBuilder();
            int count = _symbols.Count;
            for (var i = 0; i < count; ++i)
            {
                if (i > 0) sb.Append(",");
                sb.Append(_symbols[i].SymbolName);
            }

            return sb.ToString(); ;
        }

        public int Column
        {
            get { return _column; }
            set
            {
                _column = value;
                gameObject.name = "Reel" + _column;
            }
        }
    }
}
