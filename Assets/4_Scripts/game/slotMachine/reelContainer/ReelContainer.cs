using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class ReelContainer : MonoBehaviour
{
    public event Action OnReelStopComplete;
    public event Action<WinItemList> OnPlayAllWin;
    public event Action<WinItemList.Item> OnPlayEachWin;

    SlotConfig _config;

    List<Reel> _reels;

    Transform _tf;

    int[] _defaltOrder;
    int[] _spinStartOrder;
    int _nextSpinIndex;

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

    public void Initialize(SlotMachine slot)
    {
        _config = slot.Config;

        _winItemList = new WinItemList();

        CreateSpinOrder();
        CreateReels();
    }

    void CreateSpinOrder()
    {
        _defaltOrder = new int[_config.Column];
        for (var i = 0; i < _config.Column; ++i)
        {
            _defaltOrder[i] = i;
        }
    }

    void UpdateStartOrder(int[] startOrder = null)
    {
        _spinStartOrder = startOrder ?? _defaltOrder;
    }

    void DescSpinOrder(out int[] spinOrder)
    {
        spinOrder = _defaltOrder.OrderByDescending(i => i).ToArray();
    }

    void CreateReels()
    {
        if (_reels != null) return;

        int count = _config.Column;

        _reels = new List<Reel>(count);

        for (var i = 0; i < count; ++i)
        {
            Reel reel = Instantiate(_config.ReelPrefab) as Reel;
            reel.Column = i;

            reel.OnStop += OnStopListener;

            reel.transform.SetParent(_tf);
            reel.transform.localPosition = Vector3.right * _config.ReelSpace * i;
            reel.Initialize(_config);

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

        _winItemList.Clear();

        _nextSpinIndex = 0;
    }

    public void Spin()
    {
        Reset();

        UpdateStartOrder();
        // DescSpinOrder( out _spinStartOrder );        //시작 순서를 반대로 할 수 있다.
        // UpdateStartOrder(new int[] { 2, 1, 0 });     //시작 순서를 커스텀 시킬 수 있다.

        for (var i = 0; i < _config.Column; ++i)
        {
            var reelIndex = _spinStartOrder[i];
            var reel = _reels[reelIndex];
            reel.StartOrder = i;
            reel.Spin();
        }

        CheckNextReel();
    }

    public void StopSpin()
    {
        Debug.Log("StopSpin");

        for (var i = 0; i < _config.Column; ++i)
        {
            var reelIndex = _spinStartOrder[i];
            var reel = _reels[reelIndex];
            reel.StopSpin();
        }
    }

    public void ReceivedSymbol(ResDTO.Spin.Payout.SpinInfo spinInfo)
    {
        _lastSpinInfo = spinInfo;

        for (var i = 0; i < _config.Column; ++i)
        {
            _reels[i].ReceivedSymbol(spinInfo);
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
                if (i > 0) yield return new WaitForSeconds(_config.transition.EachLockReel);

                _reels[i].Lock();
            }
        }
    }

    public void BonusSpin(ResDTO.Spin.Payout.SpinInfo spinInfo)
    {
        Reset();

        _lastSpinInfo = spinInfo;

        UpdateStartOrder();

        for (var i = 0; i < _config.Column; ++i)
        {
            var continueCount = 0;
            var reelIndex = _spinStartOrder[i];
            var reel = _reels[reelIndex];

            if (reel.IsLocked)
            {
                ++continueCount;
                continue;
            }

            reel.StartOrder = i - continueCount;
            reel.BonusSpin(spinInfo);
        }

        CheckNextReel();
    }

    void OnStopListener(Reel reel)
    {
        // Debug.Log("reel " + reel.Column + " stopped");
        _lastStoppedReel = reel;

        PlayStopEffect();

        ++_nextSpinIndex;

        CheckNextReel();
    }

    void PlayStopEffect()
    {

    }

    void CheckNextReel()
    {
        if (_nextSpinIndex >= _config.Column)
        {
            ReelAllCompleted();
            return;
        }

        var nextOrder = _spinStartOrder[_nextSpinIndex];
        var nextReel = _reels[nextOrder];

        if (nextReel.IsLocked)
        {
            ++_nextSpinIndex;
            CheckNextReel();
            return;
        }

        if ("돌아가야 할 릴이 고조라면" == null)
        {
            //해당 릴을 고조한다.
            //해당 릴 이후의 릴들은 고조가 끝날 때까지 스핀을 loop 시킨다
        }
        else if ("돌아가야 할 릴이 고조가 아니고 이전이 고조 였다면" == null)
        {
            //해당릴 이후의 릴들의 스핀 loop를 해제한다.
        }
    }

    void ReelAllCompleted()
    {
        if (OnReelStopComplete != null) OnReelStopComplete();
    }

    public void FreeSpinTrigger()
    {
        var count = _config.Column;
        while (count-- > 0) _reels[count].FreeSpinTrigger();
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
                winItem.PaylineRows = _config.paylineTable[lineData.line];
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

                yield return new WaitForSeconds(_config.transition.EachWin);
            }
            else
            {
                PlayAllWin();
                enume.Reset();

                yield return new WaitForSeconds(_config.transition.EachWinSummary);
            }
        }
    }

    void SetSymbolsToWin(List<Symbol> symbols)
    {
        var count = symbols.Count;
        for (var i = 0; i < count; ++i)
        {
            symbols[i].SetState(Symbol.SymbolState.Win);
        }
    }
}