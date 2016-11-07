using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class Reel : MonoBehaviour
{
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
            var sname = _config.Strips.GetStartSymbolAt(_column, i);
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

    Symbol GetSymbol(string symbolName)
    {
        Symbol symbol = GamePool.Instance.SpawnSymbol(symbolName);

        if (symbol.Initialized == false)
        {
            symbol.Initialize(symbolName, symbol is NullSymbol ? _config.NullSymbolSize : _config.SymbolSize);
        }

        return symbol;
    }

    public void SetStartSymbols(Symbol[] symbols)
    {
        _symbolContainer.localPosition = Vector3.zero;

        //null인경우
        //0.55, -0.55,-0.85,-1.95, -2.25
        //0.3, 0,-1.1.-1.4,-2.5

        float ypos = 0;

        //dubbyCount 만큼 심볼크기를 계산해 시작위치를 설정한다.

        for (var i = 0; i < _config.DummySymbolCount; ++i)
        {
            ypos += symbols[i].Height;

            // if (symbols[0] is NullSymbol)
            // {
            //     // ypos = -(_relativeConfig.SymbolSize.height + _relativeConfig.NullSymbolSize.height )* 0.5f;
            //     ypos = -(_config.ReelSize.height - (_config.SymbolSize.height + _config.NullSymbolSize.height * 2)) * 0.5f;
            // }
        }

        for (var i = 0; i < symbols.Length; ++i)
        {
            var symbol = symbols[i];

            symbol.SetParent(_symbolContainer);
            symbol.transform.localPosition = new Vector3(0f, ypos, 0f);

            _symbols.Add(symbol);

            ypos -= symbol.Height;
        }
    }


    Tweener _spinTween;
    int _spinCount;

    public void Spin()
    {
        Log("Spin");

        _spinCount = 5;

        SpinReel();
    }

    void SpinReel()
    {
        var duration = 0.5f;
        var tgPos = _symbolContainer.position + Vector3.down * 1.5f;
        _spinTween = _symbolContainer.DOMove(tgPos, duration);
        _spinTween.SetEase(Ease.Linear);
        _spinTween.OnComplete(SpinEnd);
    }

    void SpinEnd()
    {
        --_spinCount;

        if (_spinCount > 0) SpinReel();
        else Log("SpinEnd");
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
        Debug.Log("[Reel" + _column + "]" + message.ToString());
    }

    void LogError(object message)
    {
        Debug.LogError("[Reel" + _column + "]" + message.ToString());
    }
}
