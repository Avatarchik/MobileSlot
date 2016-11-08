using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DG.Tweening;

public class Reel : MonoBehaviour
{
    static private bool USE_LOG;

    [SerializeField]
    GameObject _expectObject;

    int _column;

    SlotConfig _config;

    List<Symbol> _symbols;
    Transform _symbolContainer;

    void Awake()
    {
        _symbols = new List<Symbol>();
        _symbolContainer = transform.Find("symbols");

        if (_expectObject != null) _expectObject.SetActive(false);
    }

    public void Initialize(SlotConfig config)
    {
        _config = config;

        CreateStartSymbols();
    }

    void CreateStartSymbols()
    {
        var count = _config.Row + _config.DummySymbolCount * 2;
        Symbol[] symbols = new Symbol[count];

        for (var i = 0; i < count; ++i)
        {
            var sname = _config.GetStartSymbolAt(_column, i);
            var symbol = GetSymbol(sname);

            if (symbol == null)
            {
                LogError(sname + " was null");
                continue;
            }

            symbols[i] = symbol;
        }

        SetStartSymbols(symbols);
    }

    public void SetStartSymbols(Symbol[] symbols)
    {
        _symbolContainer.localPosition = Vector3.zero;

        //최종 결과로 3개의 심볼이 릴에 보여진다면 (예를들어 5 by 3 Slot) 3개의 심볼 위아래로 최소 1 이상의 여유 심볼이 필요하다.
        //전달 받은 symbosl 파라메터는 여유 심볼이 포함되어 있는 배열이므로 결과 심볼이 릴에 제대로 보이게 시작 위치를 조정해야한다.
        //각 심볼은 크기가 다를 수 있으니 ( NullSymbol 의 경우 높이가 0이거나 타 심볼의 반이하일 수 있다 ) 실제 심볼 크기를 계산한다.

        Symbol firstSymbol = symbols[_config.DummySymbolCount];

        var ypos = 0f;

        for (var i = 0; i < _config.DummySymbolCount; ++i)
        {
            ypos += symbols[i].Height;

            //첫번째 심볼이 Null이라면 위치 보정이 필요하다
            if (firstSymbol is NullSymbol)
            {
                float nullSymbolOffesetY = (_config.ReelSize.height - (_config.SymbolSize.height + _config.NullSymbolSize.height * 2)) * 0.5f;
                ypos -= nullSymbolOffesetY;
            }
        }

        for (var i = 0; i < symbols.Length; ++i)
        {
            var symbol = symbols[i];

            symbol.SetParent(_symbolContainer, ypos);
            _symbols.Add(symbol);

            ypos -= symbol.Height;
        }
    }

    Tweener _spinTween;
    int _spinCount;

    public void Spin()
    {
        //Log("Spin");

        _spinCount = 20;

        SpinLinear();
    }

    void SpinLinear()
    {
        //Log("SpinReel");
        AddSpiningSymbolAtFirst();

        Symbol lastSymbol = _symbols.Last();
        var speedPerSecond = 15f;
        var dis = lastSymbol.Height;
        var duration = dis / speedPerSecond;
        var tgPos = _symbolContainer.position + Vector3.down * dis;
        _spinTween = _symbolContainer.DOMove(tgPos, duration);
        _spinTween.SetEase(Ease.Linear);
        _spinTween.OnComplete(SpinTweenComplte);
    }

    void SpinTweenComplte()
    {
        //Log("SpinEnd");

        RemoveLastSpiningSymbol();
        --_spinCount;
        if (_spinCount > 0) SpinLinear();
        else SpinComplete();
    }

    void SpinComplete()
    {
        //Log("SpinComplete");
    }

    void AddSpiningSymbolAtFirst()
    {
        Symbol firstSymbol = _symbols.First();

        string addedSymbolName;
        if( firstSymbol is NullSymbol )
        {
            addedSymbolName = _config.NormalStrip.GetRandom(_column);
        }
        else
        {
            addedSymbolName = NullSymbol.EMPTY;
        }

        Symbol addedSymbol = GetSymbol(addedSymbolName);

        var ypos = firstSymbol.Y + addedSymbol.Height;
        addedSymbol.SetParent(_symbolContainer, ypos, true);

        // Log("before > " + GetAddedSymbolNames());
        _symbols.Insert(0, addedSymbol);
        // Log("after > " + GetAddedSymbolNames());
    }

    void RemoveLastSpiningSymbol()
    {
        Symbol lastSymbol = _symbols.Last();

        //Log("remove symbolname: " + lastSymbol.SymbolName);

        _symbols.RemoveAt( _symbols.Count-1 );
        GamePool.Instance.DespawnSymbol( lastSymbol );
    }

    Symbol GetSymbol(string symbolName)
    {
        Symbol symbol = GamePool.Instance.SpawnSymbol(symbolName);

        if (symbol.Initialized == false)
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
        Debug.LogError("[Reel" + _column + "]" + message.ToString());
    }
}
