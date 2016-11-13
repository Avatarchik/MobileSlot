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
    Tweener _spinTween;
    int _spinCount;
    float _spinDis;
    string[] _resultSymbolNames;

    void Awake()
    {
        _symbols = new List<Symbol>();
        _symbolContainer = transform.Find("symbols");

        if (_expectObject != null) _expectObject.SetActive(false);
    }

    public void Initialize(SlotConfig config)
    {
        _config = config;
        _symbolNecessaryCount = _config.Row + _config.DummySymbolCount * 2;

        CreateStartSymbols();
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
                LogError(sname + " was null");
                continue;
            }

            AddSymbolToTail(symbol);
        }

        AlignSymbols();
    }

    public void AlignSymbols()
    {
        _symbolContainer.localPosition = Vector3.zero;

        //최종 결과로 3개의 심볼이 릴에 보여진다면 (예를들어 5 by 3 Slot) 3개의 심볼 위아래로 최소 1 이상의 여유 심볼이 필요하다.
        //전달 받은 symbosl 파라메터는 여유 심볼이 포함되어 있는 배열이므로 결과 심볼이 릴에 제대로 보이게 시작 위치를 조정해야한다.
        //각 심볼은 크기가 다를 수 있으니 ( NullSymbol 의 경우 높이가 0이거나 타 심볼의 반이하일 수 있다 ) 실제 심볼 크기를 계산한다.

        Symbol firstSymbol = _symbols[_config.DummySymbolCount];

        var ypos = 0f;

        for (var i = 0; i < _config.DummySymbolCount; ++i)
        {
            ypos += _symbols[i].Height;

            //첫번째 심볼이 Null이라면 위치 보정이 필요하다
            if (firstSymbol is NullSymbol)
            {
                float nullSymbolOffesetY = (_config.ReelSize.height - (_config.SymbolSize.height + _config.NullSymbolSize.height * 2)) * 0.5f;
                ypos -= nullSymbolOffesetY;
            }
        }

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
        _resultSymbolNames = null;

        SpinReel();
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

        AddSpiningSymbols(_config.SpiningSymbolCount);

        if (_spinCount == 1) TweenFirst();
        else TweenLinear();
    }

    void AddSpiningSymbols(int count)
    {
        _spinDis = 0;

        Symbol topSymbol = _symbols[0];

        bool nullOrder = topSymbol is NullSymbol == false;

        while (count-- > 0)
        {
            var symbolName = nullOrder ? NullSymbol.EMPTY : _currentStrip.GetRandom(_column);
            var symbol = GetSymbol(symbolName);
            var h = symbol.Height;
            var ypos = topSymbol.Y + h;

            AddSymbolToHead(symbol, ypos);

            topSymbol = symbol;
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
        var duration = _spinDis / _config.SpinSpeedPerSec;
        var tgPos = _symbolContainer.position - new Vector3(0f, _spinDis, 0f);
        _spinTween = _symbolContainer.DOMove(tgPos, duration);
        _spinTween.SetEase(Ease.Linear);
        _spinTween.OnComplete(TweenLinearComplete);
    }

    void TweenLinear()
    {
        var duration = _spinDis / _config.SpinSpeedPerSec;
        var tgPos = _symbolContainer.position - new Vector3(0f, _spinDis, 0f);
        _spinTween = _symbolContainer.DOMove(tgPos, duration);
        _spinTween.SetEase(Ease.Linear);
        _spinTween.OnComplete(TweenLinearComplete);
    }

    void TweenLinearComplete()
    {
        RemoveSymbolsExceptNecessary();

        if (_spinCount == SPIN_COUNT_LIMIT)
        {
            ServerTooLate();
        }
        else if (_spinCount > _config.SpinCountThreshold && _resultSymbolNames != null)
        {
            FinishSpinReel();
        }
        else
        {
            SpinReel();
        }
    }

    void ServerTooLate()
    {
        //데이터 어디갔니?
        //뭔가 문제가 일어남. 설정한 최대 스핀이 돌동안 서버로부터 응답이 안왔음
    }

    void FinishSpinReel()
    {
        AddSpiningSymbols(_column * _config.IncreaseCount);
        AddInterpolationSymbols();
        AddResultSymbols();

        TweenFinish();
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
            var ypos = _symbols[0].Y + h;

            AddSymbolToHead(symbol, ypos);

            _spinDis += h;
        }
    }

    void TweenFinish()
    {
        var duration = _spinDis / _config.SpinSpeedPerSec;
        var tgPos = _symbolContainer.position - new Vector3(0f, _spinDis, 0f);
        _spinTween = _symbolContainer.DOMove(tgPos, duration);
        _spinTween.SetEase(Ease.Linear);
        _spinTween.OnComplete(TweenFinishComplete);
    }

    void TweenFinishComplete()
    {
        Log("Tween finish");
        RemoveSymbolsExceptNecessary();
        AlignSymbols();
    }

    Symbol GetSymbol(string symbolName)
    {
        Symbol symbol = GamePool.Instance.SpawnSymbol(symbolName);

        if (symbol.IsInitialized == false)
        {
            symbol.Initialize(symbolName, symbol is NullSymbol ? _config.NullSymbolSize : _config.SymbolSize);
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

    void Log(object message)
    {
        if (_column != 0) return;
        Debug.Log("[Reel" + _column + "]" + message.ToString());
    }

    void LogError(object message)
    {
        Debug.LogError("[Reel_" + _column + "]" + message.ToString());
    }
}
