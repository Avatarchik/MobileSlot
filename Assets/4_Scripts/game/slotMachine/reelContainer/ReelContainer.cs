using UnityEngine;
using System;
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

    int _nextStopIndex;

    ResDTO.Spin.Payout.SpinInfo _lastSpinInfo;

    Reel _lastStoppedReel;

    WinItemList _winItemList;

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

        CreateReels();
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

        _nextStopIndex = 0;
    }

    public void Spin()
    {
        Reset();

        for (var i = 0; i < _config.Column; ++i)
        {
            _reels[i].Spin();
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


    void OnStopListener(Reel reel)
    {
        _lastStoppedReel = reel;

        //Debug.Log("reel " + _lastStoppedReel.Column + " stopped");

        ++_nextStopIndex;

        if (_nextStopIndex < _config.Column)
        {
            CheckNextReel();
        }
        else
        {
            ReelAllCompleted();
        }
    }

    void CheckNextReel()
    {
        //다음 릴이 lock 이 걸렸는지
        //고조를 해야 하는지 등등 처리

    }

    void ReelAllCompleted()
    {
        if (OnReelStopComplete != null) OnReelStopComplete();
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

    Coroutine _eachWin;

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

    public WinItemList FindAllWinPayInfo()
    {
        //todo
        //win 한 정보를 시상에 따라 내림차순 정렬해야함

        for (var i = 0; i < _lastSpinInfo.payLines.Length; ++i)
        {
            var lineData = _lastSpinInfo.payLines[i];

            var winItem = new WinItemList.Item();
            winItem.Payout = lineData.payout;

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
}