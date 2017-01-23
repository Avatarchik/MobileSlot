using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Game
{

    public class ReelContainer : SlotMachineModule
    {

        public event Action OnReelStopComplete;
        public event Action<WinItemList> OnPlayAllWin;
        public event Action<WinItemList.Item> OnPlayEachWin;

        public bool IsExpecting { get; private set; }

        List<Reel> _reels;
        Transform _tf;

        int[] _defaltOrder;
        int[] _spinStartOrder;
        int _nextStopIndex;

        ResDTO.Spin.Payout.SpinInfo _lastSpinInfo;

        Reel _lastStoppedReel;

        WinItemList _winItemList;

        Coroutine _eachWin;



        void Awake()
        {
            _tf = transform;

            Reel[] tempReels = GetComponentsInChildren<Reel>();
            if (tempReels != null && tempReels.Length > 0)
            {
                foreach (var r in tempReels)
                {
                    Destroy(r.gameObject);
                }
            }
        }

        override public void Initialize(SlotMachine relativeMachine)
        {
            base.Initialize(relativeMachine);

            _winItemList = new WinItemList();

            CreateDefaultSpinOrder();
            CreateReels();
        }

        void CreateDefaultSpinOrder()
        {
            _defaltOrder = new int[_machineConfig.column];
            for (var i = 0; i < _machineConfig.column; ++i)
            {
                _defaltOrder[i] = i;
            }
        }

        void UpdateStartOrder(int[] startOrder = null, Action<Reel> action = null)
        {
            _spinStartOrder = startOrder ?? _defaltOrder;

            for (var i = 0; i < _machineConfig.column; ++i)
            {
                var lockCount = 0;
                var reelIndex = _spinStartOrder[i];
                var reel = _reels[reelIndex];

                if (reel.IsLocked)
                {
                    ++lockCount;
                    continue;
                }
                reel.StartOrder = i - lockCount;

                if (action != null) action(reel);
            }
        }

        void UpdateStartOrder(Action<Reel> action = null)
        {
            UpdateStartOrder(null, action);
        }

        void DescSpinOrder(Action<Reel> action = null)
        {
            UpdateStartOrder(_defaltOrder.OrderByDescending(i => i).ToArray(), action);
        }

        void CreateReels()
        {
            if (_reels != null) return;

            int count = _machineConfig.column;

            _reels = new List<Reel>(count);

            var prefab = _machineConfig.ReelPrefab;

            for (var i = 0; i < count; ++i)
            {
                Reel reel = Instantiate(prefab) as Reel;
                reel.Column = i;

                reel.OnStop += OnStopListener;

                reel.transform.SetParent(_tf);
                reel.transform.localPosition = Vector3.right * _machineConfig.ReelSpace * i;
                reel.Initialize( _machine );

                _reels.Add(reel);
            }
        }

        void Reset()
        {
            if (_eachWin != null)
            {
                StopCoroutine(_eachWin);
                _eachWin = null;
            }

            _winItemList.Reset();

            _nextStopIndex = 0;
        }

        void UpdateSpinInfo(ResDTO.Spin.Payout.SpinInfo spinInfo)
        {
            _lastSpinInfo = spinInfo;

            UpdateExpectSetting();
        }

        public void Spin()
        {
            Reset();

            // DescSpinOrder(x => x.Spin());                            //시작 순서를 반대로 할 수 있다.
            // UpdateStartOrder(new int[] { 2, 1, 0 },x => x.Spin());   //시작 순서를 커스텀 시킬 수 있다.
            UpdateStartOrder(x => x.Spin());
        }

        public void ReceivedSymbol(ResDTO.Spin.Payout.SpinInfo spinInfo)
        {
            UpdateSpinInfo(spinInfo);

            for (var i = 0; i < _machineConfig.column; ++i)
            {
                _reels[i].ReceivedSymbol(spinInfo);
            }
        }

        public void BonusSpin(ResDTO.Spin.Payout.SpinInfo spinInfo)
        {
            Reset();
            UpdateSpinInfo(spinInfo);
            UpdateStartOrder(x => x.BonusSpin(spinInfo));
            CheckNextReel();
        }

        public void FreeSpin(ResDTO.Spin.Payout.SpinInfo spinInfo)
        {
            Reset();
            UpdateSpinInfo(spinInfo);
            UpdateStartOrder(x => x.FreeSpin(spinInfo));
            CheckNextReel();
        }

        public void StopSpin()
        {
            for (var i = 0; i < _machineConfig.column; ++i)
            {
                var reelIndex = _spinStartOrder[i];
                var reel = _reels[reelIndex];
                reel.StopSpin();
            }
        }

        public YieldInstruction LockReel(int[] fixedReel)
        {
            return StartCoroutine(LockReelRoutine(fixedReel));
        }

        IEnumerator LockReelRoutine(int[] fixedReel)
        {
            for (var i = 0; i < fixedReel.Length; ++i)
            {
                if (fixedReel[i] == 1)
                {
                    if (i > 0) yield return new WaitForSeconds(_machineConfig.transition.EachLockReel);

                    _reels[i].Lock();
                }
            }
        }

        virtual protected void UpdateExpectSetting()
        {
            //todo
            //게임 별 구체화
            if (_machine.CurrentState == SlotMachine.MachineState.BonusSpin)
            {
                for (var i = 0; i < _machineConfig.column; ++i) if (_reels[i].IsLocked == false) _reels[i].ExpectType = ExpectReelType.BonusSpin;
            }
        }

        void OnStopListener(Reel reel)
        {
            // Debug.Log("^^^^^^ reel " + reel.Column + " stopped");
            // Debug.Log("!!!! try CheckScatters");
            _lastStoppedReel = reel;

            CheckScatters();

            ++_nextStopIndex;

            CheckNextReel();
        }

        void CheckScatters()
        {
            if (_machineConfig.ScatterInfos == null) return;

            var count = _machineConfig.ScatterInfos.Count;

            Debug.Log("------ CheckScatters: " + count);

            for (var i = 0; i < count; ++i)
            {
                var info = _machineConfig.ScatterInfos[i];
                AudioClip stopSound;
                if (info.CheckScattered(_lastStoppedReel, out stopSound))
                {
                    SlotSoundList.PlaySFX(stopSound);
                }
                else
                {
                }
            }

            /*
            var res: Vector.<ASymbol> = mStoppedReel.getSymbolsByType( SymbolType.FREESPIN );
			var count: int = res.length;
			if( count == 0 ) return;
			
			if( mStoppedReel.column == 2 && getSymbolCount( SymbolType.FREESPIN ) < 1 )
			{
				return;
			}
			else if(  mStoppedReel.column == 4 && getSymbolCount( SymbolType.FREESPIN ) < 2)
			{
				return;
			}
			
			var symbol: ASymbol;
			for( var i: int = 0; i < count; ++i)
			{
				symbol = res[i];
				symbol.setState( SymbolState.SCATTER );
			}
			
			addSymbolCount( SymbolType.FREESPIN );
			
			SoundPlayer.manager.playScatterStop( SymbolType.FREESPIN );
            */
        }

        void CheckNextReel()
        {
            if (_nextStopIndex >= _machineConfig.column)
            {
                ReelStopCompleted();
                return;
            }

            var nextOrder = _spinStartOrder[_nextStopIndex];
            var nextReel = _reels[nextOrder];

            if (nextReel.IsLocked)
            {
                ++_nextStopIndex;
                CheckNextReel();
                return;
            }

            if (nextReel.IsExpectable)
            {
                IsExpecting = true;

                for (var i = _nextStopIndex; i < _machineConfig.column; ++i)
                {
                    var reel = _reels[_spinStartOrder[i]];
                    if (i == _nextStopIndex) reel.Loop = false;
                    else reel.Loop = true;
                }

                nextReel.SpinToExpect();
                SlotSoundList.Expect(null);
                return;
            }

            if (IsExpecting)
            {
                IsExpecting = false;

                for (var i = _nextStopIndex; i < _machineConfig.column; ++i)
                {
                    var reel = _reels[_spinStartOrder[i]];
                    reel.Loop = false;
                }
            }
        }

        void ReelStopCompleted()
        {
            if (OnReelStopComplete != null) OnReelStopComplete();
        }

        public void FreeSpinTrigger()
        {
            var count = _machineConfig.column;
            while (count-- > 0) _reels[count].FreeSpinTrigger();
        }

        public void FreeSpinReady()
        {
            Reset();
        }

        public WinItemList FindAllWinPayInfo()
        {
            //todo
            //win 한 정보를 시상에 따라 내림차순 정렬해야함

            for (var i = 0; i < _lastSpinInfo.payLines.Length; ++i)
            {
                var lineData = _lastSpinInfo.payLines[i];

                var winItem = new WinItemList.Item();
                winItem.Payout = lineData.payout;
                winItem.WinTable = lineData.winTable;

                if (lineData.IsLineMatched == false)
                {
                    //todo
                    //게임 별 override 가 필요할 것 같다.
                    winItem.Type = WinItemList.Item.ItemType.Progressive;
                    winItem.PaylineRows = null;
                    winItem.PaylineIndex = null;
                }
                else
                {
                    winItem.Type = WinItemList.Item.ItemType.Payline;
                    winItem.PaylineRows = _machineConfig.paylineTable.GetPayline(lineData.line).rows;
                    winItem.PaylineIndex = lineData.line;

                    for (var col = 0; col < lineData.matches; ++col)
                    {
                        var row = winItem.PaylineRows[col];
                        var reel = _reels[col];
                        var symbol = reel.GetSymbolAt(row);
                        winItem.AddSymbol(symbol);
                    }
                }

                _winItemList.AddItem(winItem);
            }

            return _winItemList;
        }

        public Coroutine PlaySpecialWinDirection()
        {
            return StartCoroutine(PlaySpecialWinDirectionRoutine());
        }

        //경우에 따라 스택 심볼의 병합된 이미지라던지
        //팝업이라던지
        //기타등등을 보여줘야할 경우가 있따
        IEnumerator PlaySpecialWinDirectionRoutine()
        {
            yield break;
        }

        public void PlayAllWin()
        {
            //필요한 경우 여기서 winsymbol 의 수와 종류를 파악. 사운드 재생등을 커스텀 시킨다.

            SetSymbolsToWin(_winItemList.AllSymbols);
            if (OnPlayAllWin != null) OnPlayAllWin(_winItemList);
        }

        //todo
        //중복 호출 될 수 있다. 체크하자
        public void PlayEachWin()
        {
            if (_winItemList.ItemCount <= 1) return;

            _eachWin = StartCoroutine(PlayEachWinRoutine());
        }

        IEnumerator PlayEachWinRoutine()
        {
            var enume = _winItemList.GetEnumerator();

            while (true)
            {
                if (enume.MoveNext())
                {
                    var item = enume.Current;
                    SetSymbolsToWin(item.Symbols);
                    if (OnPlayEachWin != null) OnPlayEachWin(item);

                    yield return new WaitForSeconds(_machineConfig.transition.EachWin);
                }
                else
                {
                    PlayAllWin();
                    enume.Reset();

                    yield return new WaitForSeconds(_machineConfig.transition.EachWinSummary);
                }
            }
        }

        void SetSymbolsToWin(List<Symbol> symbols)
        {
            var count = symbols.Count;
            for (var i = 0; i < count; ++i)
            {
                symbols[i].SetState(SymbolState.Win);
            }
        }
    }
}