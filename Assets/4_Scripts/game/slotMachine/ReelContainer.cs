using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ReelContainer : MonoBehaviour
{
    public event Action OnReelStopComplete;

    SlotConfig _config;

    List<Reel> _reels;

    Transform _tf;

    int _nextStopIndex;

    ResDTO.Spin.Payout.SpinInfo _lastSpinInfo;

    Reel _lastStoppedReel;

    List<WinPayInfo> _payInfos = new List<WinPayInfo>();
    List<Symbol> _allSymbol = new List<Symbol>();

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

    public void Spin()
    {
        _nextStopIndex = 0;

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

    public void PlayAllSymbols()
    {
        //필요한 경우 여기서 winsymbol 의 수와 종류를 파악. 사운드 재생등을 커스텀 시킨다.

        var count = _allSymbol.Count;
        for (var i = 0; i < count; ++i)
        {
            _allSymbol[i].SetState(Symbol.SymbolState.Win);
        }
    }

    public void RegisterWinSymbol(WinPayInfo payInfo)
    {
        var count = payInfo.Symbols.Count;

        for (var i = 0; i < count; ++i)
        {
            var symbol = payInfo.Symbols[i];

            if (symbol == null || _allSymbol.Contains(symbol)) continue;

            _allSymbol.Add(symbol);
        }
    }

    public List<WinPayInfo> FindAllWinPayInfo()
    {
        _allSymbol.Clear();
        _payInfos.Clear();

        //todo
        //win 한 정보를 시상에 따라 내림차순 정렬해야함

        for (var i = 0; i < _lastSpinInfo.payLines.Length; ++i)
        {
            var lineData = _lastSpinInfo.payLines[i];

            var payInfo = new WinPayInfo();
            payInfo.Payout = lineData.payout;

            if (lineData.IsLineMatched == false)
            {
                //todo
                //게임 별 override 가 필요할 것 같다.
                payInfo.Type = WinPayInfo.WinType.Progressive;
                payInfo.PaylineRows = null;
                payInfo.Symbols = null;
                payInfo.PaylineIndex = null;
            }
            else
            {
                payInfo.Type = WinPayInfo.WinType.Payline;
                payInfo.PaylineRows = _config.paylineTable[lineData.line];
                payInfo.PaylineIndex = lineData.line;

                for (var col = 0; col < lineData.matches; ++col)
                {
                    var row = payInfo.PaylineRows[col];
                    var reel = _reels[col];
                    var symbol = reel.GetSymbolAt(row);
                    payInfo.AddSymbol(symbol);
                }
            }

            RegisterWinSymbol(payInfo);
            _payInfos.Add(payInfo);
        }

        return _payInfos;
    }
}