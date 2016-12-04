﻿using UnityEngine;
using UnityEngine.Events;

using System.Collections;
using System.Collections.Generic;
using System.Text;
using DG.Tweening;

public class Reel : MonoBehaviour
{
    public event UnityAction<Reel> OnStop;

    const int SPIN_COUNT_LIMIT = 50;

    public GameObject expectObject;


    public bool IsLocked { get; private set; }
    public int StartOrder { get; set; }

    Transform _symbolContainer;
    public Transform SymbolContainer { get { return _symbolContainer; } }

    protected int _column;
    protected SlotConfig _config;
    protected List<Symbol> _symbols;

    protected float _spinDis;

    [SerializeField]
    protected string[] _lastSymbolNames;
    [SerializeField]
    protected string[] _receivedSymbolNames;

    protected ReelStrip _currentStrip;

    [SerializeField]
    int _spinCount;
    int _symbolNecessaryCount; //화면에 보여야할 심볼 수 ( row ) + 위아래 여유 수 ( marinCount * 2 )
    bool _isReceived = true;
    Vector3 _spinDestination;

    void Awake()
    {
        _symbols = new List<Symbol>();
        _symbolContainer = transform.Find("symbols");

        if (expectObject != null) expectObject.SetActive(false);
    }

    public void Initialize(SlotConfig config)
    {
        _config = config;
        _symbolNecessaryCount = _config.Row + _config.MarginSymbolCount * 2;
        _lastSymbolNames = new string[_config.Row];
        _receivedSymbolNames = new string[_config.Row];

        CreateStartSymbols();
    }

    void CreateStartSymbols()
    {
        _symbols = new List<Symbol>();

        for (var i = 0; i < _symbolNecessaryCount; ++i)
        {
            var sname = _config.GetStartSymbolAt(_column, i);
            AddSymbolToTail(CreateSymbol(sname));
        }

        AlignSymbols();

        for (var i = 0; i < _config.Row; ++i)
        {
            _lastSymbolNames[i] = _config.GetStartSymbolAt(_column, _config.MarginSymbolCount + i);
        }
    }

    public void AlignSymbols()
    {
        var ypos = GetStartSymbolPos();

        var len = _symbols.Count;
        for (var i = 0; i < len; ++i)
        {
            var symbol = _symbols[i];
            symbol.Y = ypos;
            ypos -= symbol.Height;
        }
    }

    virtual protected float GetStartSymbolPos()
    {
        var res = 0f;
        for (var i = 0; i < _config.MarginSymbolCount; ++i)
        {
            res += _symbols[i].Height;
        }

        return res;
    }

    bool _isSpinnining;
    bool _isStopping;

    public void StopSpin()
    {
        if (_isSpinnining == false || _isReceived == false || _isStopping == true) return;

        Debug.Log("StopReel");

        return;

        _isStopping = true;
        if (_spinTween != null)
        {
            _spinTween.Kill();
            _spinTween = null;
        }

        TweenAbort();
    }

    void TweenAbort()
    {
        _spinDis = 0f;

        if (_isTweenLast)
        {

        }
        else
        {
            AddInterpolationSymbols();
            AddResultSymbols();
            AddSpiningSymbols(_config.MarginSymbolCount);
            _spinDestination = _symbolContainer.localPosition - new Vector3(0f, _spinDis, 0f);
        }

        var duration = 2.2f;
        var tween = _symbolContainer.DOLocalMove(_spinDestination, duration);
        tween.SetEase(Ease.Linear);
        tween.OnComplete(TweenLastComplete);
        tween.Play();

        _spinTween = tween;

        // RemoveSymbolsExceptNecessary();
        // TweenLast();
    }

    public void Spin(ResDTO.Spin.Payout.SpinInfo spinInfo = null)
    {
        _isSpinnining = true;
        _isReceived = false;
        _spinCount = 0;
        _currentStrip = GetCurrentStrip();

        if (spinInfo != null) ReceivedSymbol(spinInfo);

        UpdateSymbolState(Symbol.SymbolState.Spin);

        SpinReel();
    }

    public void BonusSpin(ResDTO.Spin.Payout.SpinInfo spinInfo)
    {
        Spin(spinInfo);
        //연출
    }

    public void ReceivedSymbol(ResDTO.Spin.Payout.SpinInfo spinInfo)
    {
        if (spinInfo == null) return;

        _isReceived = true;

        string[] reelData = spinInfo.GetReelData(_column, _config.Row);

        for (var i = 0; i < reelData.Length; ++i)
        {
            _receivedSymbolNames[i] = _config.NameMap.GetSymbolName(reelData[i]);
        }

        Debug.Log(string.Join(",", _receivedSymbolNames));
    }

    Tween _spinTween;
    bool _isTweenLast;

    void SpinReel()
    {
        if (_spinCount >= SPIN_COUNT_LIMIT)
        {
            ServerTooLate();
            return;
        }

        _spinDis = 0;
        _spinTween = null;
        RemoveSymbolsExceptNecessary();

        ++_spinCount;
        if (_spinCount == 1)
        {
            TweenFirst();
        }
        else if (_spinCount <= _config.SpinCountThreshold || _isReceived == false)
        {
            TweenLoop();
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

        if (_config.tweenFirstBackInfo.distance > 0)
        {
            var backPos = _symbolContainer.position + new Vector3(0f, _config.tweenFirstBackInfo.distance, 0f);
            tweenBack = _symbolContainer.DOMove(backPos, _config.tweenFirstBackInfo.duration);
            tweenBack.SetEase(Ease.OutSine);
            _spinDis += _config.tweenFirstBackInfo.distance;
        }

        AddSpiningSymbols(_config.SpiningSymbolCount);

        _spinDestination = _symbolContainer.position - new Vector3(0f, _spinDis, 0f);
        var duration = _spinDis / _config.SpinSpeedPerSec;
        var tween = _symbolContainer.DOMove(_spinDestination, duration);
        // tween.SetEase(Ease.Linear);
        tween.SetEase(Ease.InCubic);

        //todo
        //시퀀스 매 생성하지 않고 재활용 하기
        var startDelay = StartOrder * _config.DelayEachReel;
        Sequence firstTweenSequence = DOTween.Sequence();
        firstTweenSequence.PrependInterval(startDelay);
        if (tweenBack != null) firstTweenSequence.Append(tweenBack);
        firstTweenSequence.Append(tween);
        firstTweenSequence.AppendCallback(() =>
        {
            SpinReel();
        });
        firstTweenSequence.Play();

        _spinTween = firstTweenSequence;
    }

    virtual protected void TweenLoop()
    {
        AddSpiningSymbols(_config.SpiningSymbolCount);

        var duration = _spinDis / _config.SpinSpeedPerSec;
        _spinDestination = _symbolContainer.position - new Vector3(0f, _spinDis, 0f);

        var tween = _symbolContainer.DOMove(_spinDestination, duration);
        tween.OnComplete(() =>
        {
            SpinReel();
        });
        tween.Play();

        _spinTween = tween;
    }

    virtual protected void TweenLast()
    {
        _isTweenLast = true;

        ComposeLastSpiningSymbols();

        _spinDis += _config.tweenLastBackInfo.distance;

        var duration = _spinDis / _config.SpinSpeedPerSec;
        _spinDestination = _symbolContainer.localPosition - new Vector3(0f, _spinDis, 0f);

        var tween = _symbolContainer.DOLocalMove(_spinDestination, duration);
        tween.SetEase(Ease.Linear);
        tween.OnComplete(TweenLastComplete);
        tween.Play();

        _spinTween = tween;
    }

    virtual protected void ComposeLastSpiningSymbols()
    {
        AddSpiningSymbols(StartOrder * _config.IncreaseCount);
        AddInterpolationSymbols();
        AddResultSymbols();
        AddSpiningSymbols(_config.MarginSymbolCount);
    }

    void TweenLastComplete()
    {
        _isSpinnining = false;
        _isStopping = false;
        _spinTween = null;
        _isTweenLast = false;

        _lastSymbolNames = _receivedSymbolNames;

        RemoveSymbolsExceptNecessary();

        AlignSymbols();

        _symbolContainer.localPosition = new Vector3(0f, -_config.tweenLastBackInfo.distance, 0f);

        if (_config.tweenLastBackInfo.distance != 0)
        {
            var backOutTween = _symbolContainer.DOLocalMove(Vector3.zero, _config.tweenLastBackInfo.duration);
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

    void UpdateSymbolState(Symbol.SymbolState state)
    {
        var count = _symbols.Count;
        for (var i = 0; i < count; ++i)
        {
            _symbols[i].SetState(state);
        }
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
            GamePool.Instance.DespawnSymbol(symbol);
        }
    }

    void AddSymbolToTail(Symbol symbol, float ypos = 0f)
    {
        if (symbol == null) throw new System.ArgumentNullException("symbol", "symbol can't be null");

        _symbols.Add(symbol);
        symbol.SetParent(this, ypos);
    }

    protected void AddSymbolToHead(Symbol symbol, float ypos = 0f)
    {
        if (symbol == null) throw new System.ArgumentNullException("symbol", "symbol can't be null");

        _symbols.Insert(0, symbol);
        symbol.SetParent(this, ypos, true);
    }

    protected void AddSpiningSymbol(string symbolname)
    {
        var symbol = CreateSymbol(symbolname);
        var h = symbol.Height;

        AddSymbolToHead(symbol, _symbols[0].Y + h);
        _spinDis += h;
    }

    protected void AddSpiningSymbols(int count)
    {
        while (count-- > 0) AddSpiningSymbol(GetSpiningSymbolName());
    }

    protected void AddResultSymbols()
    {
        var count = _receivedSymbolNames.Length;
        while (count-- > 0) AddSpiningSymbol(_receivedSymbolNames[count]);
    }

    virtual protected void AddInterpolationSymbols()
    {
        //현재 스핀 중인 심볼과 결과 심볼 사이에 부드럽게 이어줄 심볼들을 만들어 넣는다.
        //null 심볼이나 stack 심볼, big 심볼등이 있을 수 있다
    }

    virtual protected string GetSpiningSymbolName()
    {
        return _currentStrip.GetRandom(_column);
    }


    public void FreeSpinTrigger()
    {

    }

    protected Symbol CreateSymbol(string symbolName)
    {
        Symbol symbol = GamePool.Instance.SpawnSymbol(symbolName);

        if (symbol.IsInitialized == false) symbol.Initialize(symbolName, _config);

        return symbol;
    }

    public Symbol GetSymbolAt(int row)
    {
        row += _config.MarginSymbolCount;
        if (row < 0 || _symbols.Count <= row) throw new System.ArgumentOutOfRangeException();
        return _symbols[row];
    }

    ReelStrip GetCurrentStrip()
    {
        return _config.NormalStrip;
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
