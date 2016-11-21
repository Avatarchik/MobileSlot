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

    public void Initialize(SlotConfig config)
    {
        _config = config;

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


    Reel _lastStoppedReel;

    void OnStopListener(Reel reel)
    {
        _lastStoppedReel = reel;

        Debug.Log("reel " + _lastStoppedReel.Column + " stopped");

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
        Debug.Log("ReelAllCompleted");
        if (OnReelStopComplete != null) OnReelStopComplete();
    }

    public Coroutine DisplayWin()
    {
        return StartCoroutine( DisplayWinRoutine() );
    }

    IEnumerator DisplayWinRoutine()
    {
        //scattered, payline 당첨 모두 동일하게 처리
        //시상 skip 지원 (threshold 존재해야함)
        //시상이 된 심볼들을 한번에 하이라이트 한다.

        FindAllWinInfo();

        //경우에 따라 스택 심볼의 병합된 이미지라던지
        //팝업이라던지
        //기타등등을 보여줘야할 경우가 있따
        yield return StartCoroutine( PlaySpecialWinDirection() );

        DisplayAllWinSymbol();

        yield return new WaitForSeconds(1f);
    }

    IEnumerator PlaySpecialWinDirection()
    {
        yield break;
    }

    public void DisplayAllWinSymbol()
    {
        Debug.Log("DisplayAllWinSymbol");
        //필요한 경우 여기서 winsymbol 의 수와 종류를 파악. 사운드 재생등을 커스텀 시킨다.

        var count = _allSymbol.Count;
        for( var i = 0; i < count; ++i )
        {
            var symbol = _allSymbol[i];
            symbol.Win();
        }
    }

    List<PayWinInfo> _winInfos = new List<PayWinInfo>();
    List<Symbol> _allSymbol = new List<Symbol>();

    public void RegisterWinSymbol(PayWinInfo winInfo)
    {
        var count = winInfo.Symbols.Count;

        for (var i = 0; i < count; ++i)
        {
            var symbol = winInfo.Symbols[i];

            if (symbol == null || _allSymbol.Contains(symbol)) continue;

            _allSymbol.Add(symbol);
        }
    }

    void FindAllWinInfo()
    {
        _allSymbol.Clear();
        _winInfos.Clear();

        //win 한 정보를 시상에 따라 내림차순 정렬

        Debug.LogFormat("find {0} win info",_lastSpinInfo.payLines.Length );

        for (var i = 0; i < _lastSpinInfo.payLines.Length; ++i)
        {
            var winInfo = new PayWinInfo();

            var lineData = _lastSpinInfo.payLines[i];
            if (lineData.IsLineMatched == false)
            {
                //todo 게임 별 override 가 필요할 것 같다.
                winInfo.Type = PayWinInfo.WinType.Progressive;
                winInfo.Payline = null;
                winInfo.SetSymbols(null);
            }
            else
            {
                winInfo.Type = PayWinInfo.WinType.Payline;
                winInfo.Payline = _config.paylineTable[lineData.line];

                for (var col = 0; col < lineData.matches; ++col)
                {
                    var row = winInfo.Payline[col];
                    var reel = _reels[col];
                    var symbol = reel.GetSymbolAt(row);
                    winInfo.AddSymbol(symbol);
                }
            }

            RegisterWinSymbol(winInfo);
            _winInfos.Add( winInfo );
        }
    }
}

public class PayWinInfo
{
    public enum WinType
    {
        Progressive,
        Payline
    }

    public WinType Type { get; set; }
    public int[] Payline { get; set; }

    List<Symbol> _symbols;
    public List<Symbol> Symbols { get { return _symbols; } }
    public PayWinInfo()
    {
        _symbols = new List<Symbol>();
    }

    public void AddSymbol(Symbol symbol)
    {
        _symbols.Add(symbol);
    }

    public void SetSymbols(List<Symbol> symbols)
    {
        _symbols = symbols;
    }
}
