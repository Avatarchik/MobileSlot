using UnityEngine;
using UnityEngine.Events;

using System.Collections;
using System.Collections.Generic;
using System.Text;
using DG.Tweening;

public class Reel : MonoBehaviour
{
    public event UnityAction<Reel> OnStop;

    const int SPIN_COUNT_LIMIT = 50;

    [SerializeField]
    GameObject _expectObject;

    int _column;
    SlotConfig _config;
    ReelStrip _currentStrip;
    List<Symbol> _symbols;
    int _symbolNecessaryCount; //화면에 보여야할 심볼 수 ( row ) + 위아래 여유 수 ( dummyCount * 2 )

    Transform _symbolContainer;
    int _spinCount;
    float _spinDis;


    [SerializeField]
    float _symbolStartPosOffset;


    [SerializeField]
    string[] _lastResultSymbolNames;

    string[] _resultSymbolNames;

    void Awake()
    {
        _symbols = new List<Symbol>();
        _symbolContainer = transform.Find("symbols");

        if (_expectObject != null) _expectObject.SetActive(false);
    }

    [SerializeField]
    float nullSymbolOffesetY;

    public void Initialize(SlotConfig config)
    {
        _config = config;
        _symbolNecessaryCount = _config.Row + _config.DummySymbolCount * 2;

        nullSymbolOffesetY = (_config.ReelSize.height - (_config.SymbolSize.height + _config.NullSymbolSize.height * 2)) * 0.5f;


        CreateStartSymbols();

        _resultSymbolNames = null;

    }

    void CreateStartSymbols()
    {
        _symbols = new List<Symbol>();

        for (var i = 0; i < _symbolNecessaryCount; ++i)
        {
            var sname = _config.GetStartSymbolAt(_column, i);
            var symbol = GetSymbol(sname);

            if (symbol == null)
            {
                Debug.LogError(sname + " was null");
                continue;
            }

            AddSymbolToTail(symbol);
        }

        AlignSymbols();

        _lastResultSymbolNames = new string[_config.Row];
        for (var i = 0; i < _config.Row; ++i)
        {
            _lastResultSymbolNames[i] = _config.GetStartSymbolAt(_column, _config.DummySymbolCount + i);
        }
    }

    public void AlignSymbols()
    {
        //최종 결과로 3개의 심볼이 릴에 보여진다면 (예를들어 5 by 3 Slot) 3개의 심볼 위아래로 최소 1 이상의 여유 심볼이 필요하다.
        //전달 받은 symbosl 파라메터는 여유 심볼이 포함되어 있는 배열이므로 결과 심볼이 릴에 제대로 보이게 시작 위치를 조정해야한다.
        //각 심볼은 크기가 다를 수 있으니 ( NullSymbol 의 경우 높이가 0이거나 타 심볼의 반이하일 수 있다 ) 실제 심볼 크기를 계산한다.

        Symbol firstSymbol = _symbols[_config.DummySymbolCount];

        _symbolStartPosOffset = 0f;

        for (var i = 0; i < _config.DummySymbolCount; ++i)
        {
            _symbolStartPosOffset += _symbols[i].Height;
        }

        //첫번째 심볼이 Null이라면 위치 보정이 필요하다
        if (firstSymbol is NullSymbol)
        {
            _symbolStartPosOffset -= nullSymbolOffesetY;
        }

        var ypos = _symbolStartPosOffset;

        var len = _symbols.Count;
        for (var i = 0; i < len; ++i)
        {
            var symbol = _symbols[i];
            symbol.Y = ypos;
            ypos -= symbol.Height;
        }
    }

    void AddSymbolToHead(Symbol symbol, float ypos = 0f)
    {
        _symbols.Insert(0, symbol);
        symbol.SetParent(_symbolContainer, ypos, true);
    }

    void AddSymbolToTail(Symbol symbol, float ypos = 0f)
    {
        _symbols.Add(symbol);
        symbol.SetParent(_symbolContainer, ypos);
    }

    public void Spin()
    {
        //Log("Spin");

        _spinCount = 0;
        _currentStrip = _config.NormalStrip;

        UpdateSymbolState( Symbol.SymbolState.Spin );
        
        SpinReel();
    }

    void UpdateSymbolState( Symbol.SymbolState state )
    {
        var count = _symbols.Count;
        for( var i = 0; i < count; ++i )
        {
            _symbols[i].SetState( state );
        }
    }

    public void ReceivedSymbol(ResDTO.Spin.Payout.SpinInfo spinInfo)
    {
        string[] reelData = spinInfo.GetReelData(_column, _config.Row);
        _resultSymbolNames = new string[reelData.Length];

        for (var i = 0; i < reelData.Length; ++i)
        {
            _resultSymbolNames[i] = _config.NameMap.GetSymbolName(reelData[i]);
        }
    }

    void SpinReel()
    {
        ++_spinCount;

        if (_spinCount >= SPIN_COUNT_LIMIT)
        {
            ServerTooLate();
            return;
        }

        if (_spinCount == 1)
        {
            TweenFirst();
        }
        else if (_spinCount > _config.SpinCountThreshold && _resultSymbolNames != null)
        {
            TweenLast();
        }
        else
        {
            TweenLinear();
        }
    }

    void AddSpiningSymbols(int count)
    {
        _spinDis = 0;

        bool nullOrder = _symbols[0] is NullSymbol == false;

        while (count-- > 0)
        {
            var symbolName = nullOrder ? NullSymbol.EMPTY : _currentStrip.GetRandom(_column);
            var symbol = GetSymbol(symbolName);
            var h = symbol.Height;

            AddSymbolToHead(symbol, _symbols[0].Y + h);

            _spinDis += h;

            nullOrder = !nullOrder;
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

    void TweenFirst()
    {
        AddSpiningSymbols(_config.SpiningSymbolCount);

        var backPos = _symbolContainer.position + new Vector3(0f, _config.tweenFirstBackInfo.distance, 0f);
        var tweenBack = _symbolContainer.DOMove(backPos, _config.tweenFirstBackInfo.duration);
        tweenBack.SetEase(Ease.OutSine);

        _spinDis += _config.tweenFirstBackInfo.distance;
        var tgPos = _symbolContainer.position - new Vector3(0f, _spinDis, 0f);
        var duration = _spinDis / _config.SpinSpeedPerSec;

        var tween = _symbolContainer.DOMove(tgPos, duration);
        tween.SetEase(Ease.InCubic);

        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(tweenBack);
        mySequence.Append(tween);
        mySequence.PrependInterval(_config.DelayEachReel * _column);
        mySequence.AppendCallback(() =>
        {
            RemoveSymbolsExceptNecessary();
            SpinReel();
        });
        mySequence.Play();

    }

    virtual protected void TweenLinear()
    {
        AddSpiningSymbols(_config.SpiningSymbolCount);

        var duration = _spinDis / _config.SpinSpeedPerSec;
        var tgPos = _symbolContainer.position - new Vector3(0f, _spinDis, 0f);

        var tween = _symbolContainer.DOMove(tgPos, duration);
        tween.OnComplete(() =>
        {
            RemoveSymbolsExceptNecessary();
            SpinReel();
        });
        tween.Play();
    }

    virtual protected void TweenLast()
    {
        AddSpiningSymbols(_column * _config.IncreaseCount);
        AddInterpolationSymbols();
        AddResultSymbols();
        AddDummySymbols();

        if (_lastResultSymbolNames[0] == NullSymbol.EMPTY && _resultSymbolNames[0] != NullSymbol.EMPTY)
        {
            _spinDis -= nullSymbolOffesetY;
        }
        else if (_lastResultSymbolNames[0] != NullSymbol.EMPTY && _resultSymbolNames[0] == NullSymbol.EMPTY)
        {
            _spinDis -= nullSymbolOffesetY;
        }

        _spinDis += _config.tweenLastBackInfo.distance;

        var duration = _spinDis / _config.SpinSpeedPerSec;
        var tgPos = _symbolContainer.localPosition - new Vector3(0f, _spinDis, 0f);

        Tweener tween = _symbolContainer.DOLocalMove(tgPos, duration);
        tween.SetEase(Ease.Linear);
        tween.OnComplete(TweenFinishComplete);
        tween.Play();
    }

    void TweenFinishComplete()
    {
        RemoveSymbolsExceptNecessary();
        AlignSymbols();

        _symbolContainer.localPosition = new Vector3(0f, -_config.tweenLastBackInfo.distance, 0f);

        if (_config.tweenLastBackInfo.distance != 0)
        {
            var backOutTween = _symbolContainer.DOLocalMove(Vector3.zero, _config.tweenLastBackInfo.duration);
            backOutTween.SetEase(Ease.OutBack);
            backOutTween.Play();
        }

        if (_resultSymbolNames != null && _resultSymbolNames.Length > 0)
        {
            _lastResultSymbolNames = _resultSymbolNames;
        }

        if (OnStop != null) OnStop(this);
    }

    public Symbol GetSymbolAt(int row)
    {
        row += _config.DummySymbolCount;
        if (row < 0 || _symbols.Count <= row) throw new System.ArgumentOutOfRangeException();
        return _symbols[row];
    }

    void ServerTooLate()
    {
        //데이터 어디갔니?
        //뭔가 문제가 일어남. 설정한 최대 스핀이 돌동안 서버로부터 응답이 안왔음
    }

    void AddResultSymbols()
    {
        var count = _resultSymbolNames.Length;
        while (count-- > 0)
        {
            var symbolName = _resultSymbolNames[count];
            var symbol = GetSymbol(symbolName);
            var h = symbol.Height;
            var ypos = _symbols[0].Y + h;

            AddSymbolToHead(symbol, ypos);

            _spinDis += h;
        }
    }

    //현재 스핀 중인 심볼과 결과 심볼 사이에 부드럽게 이어줄 심볼들을 만들어 넣는다.
    //null 심볼이나 stack 심볼, big 심볼등이 있을 수 있다
    virtual protected void AddInterpolationSymbols()
    {
        string lastResultName = _resultSymbolNames[_resultSymbolNames.Length - 1];
        string topSpiningName = _symbols[0].SymbolName;

        List<string> addedNames = new List<string>();

        //일반 추가
        if (topSpiningName == NullSymbol.EMPTY && lastResultName == NullSymbol.EMPTY)
        {
            addedNames.Add(_currentStrip.GetRandom(_column));
        }
        //널 추가 
        else if (topSpiningName != NullSymbol.EMPTY && lastResultName != NullSymbol.EMPTY)
        {
            addedNames.Add(NullSymbol.EMPTY);
        }

        int count = addedNames.Count;
        while (count-- > 0)
        {
            var symbolName = addedNames[count];
            var symbol = GetSymbol(symbolName);
            var h = symbol.Height;

            AddSymbolToHead(symbol, _symbols[0].Y + h);

            _spinDis += h;
        }
    }

    //todo
    //AddSpiningSymbols 와 중복. 리팩토링 필요
    virtual protected void AddDummySymbols()
    {
        int count = _config.DummySymbolCount;

        bool nullOrder = _symbols[0] is NullSymbol == false;

        while (count-- > 0)
        {
            var symbolName = nullOrder ? NullSymbol.EMPTY : _currentStrip.GetRandom(_column);
            var symbol = GetSymbol(symbolName);
            var h = symbol.Height;

            AddSymbolToHead(symbol, _symbols[0].Y + h);

            _spinDis += h;

            nullOrder = !nullOrder;
        }
    }

    Symbol GetSymbol(string symbolName)
    {
        Symbol symbol = GamePool.Instance.SpawnSymbol(symbolName);

        if (symbol.IsInitialized == false)
        {
            symbol.Initialize(symbolName, symbol is NullSymbol ? _config.NullSymbolSize : _config.SymbolSize, _config.DebugSymbolArea);
        }

        return symbol;
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
