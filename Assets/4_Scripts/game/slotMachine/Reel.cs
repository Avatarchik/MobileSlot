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
    List<Symbol> _symbols;
    Transform _symbolContainer;
    ReelStrip _currentStrip;
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

    public void Spin()
    {
        //Log("Spin");

        _spinCount = 0;
        _currentStrip = _config.NormalStrip;
        _resultSymbolNames = null;

        SpinReel();
    }

    public void ReceivedSymbol( ResDTO.Spin.Payout.SpinInfo spinInfo )
    {
        _resultSymbolNames = spinInfo.GetReelData( _column, _config.Row );
    }

    void SpinReel()
    {
        ++_spinCount;

        AddSpiningSymbols( _config.SpiningSymbolCount );

        if( _spinCount == 1 ) TweenFirst();
        else TweenLinear();
    }

    void AddSpiningSymbols( int count )
    {
        _spinDis = 0;

        Symbol topSymbol = _symbols[0];
        bool nullOrder = topSymbol is NullSymbol == false;

        while (count-- > 0)
        {
            var symbolName = nullOrder ? NullSymbol.EMPTY : _currentStrip.GetRandom(_column);
            var symbol = GetSymbol(symbolName);
            var h = symbol.Height;

            symbol.SetParent(_symbolContainer, topSymbol.Y + h, true);
            _symbols.Insert(0, symbol);
            topSymbol = symbol;

            _spinDis += h;

            nullOrder = !nullOrder;
        }
    }

    

    void RemoveSpiningSymbols()
    {
        int count = _config.SpiningSymbolCount;
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
        TweenLinear();
    }

    void TweenLinear()
    {
        //Log("SpinReel");
        
        var duration = _spinDis / _config.SpinSpeedPerSec;
        var tgPos = _symbolContainer.position - new Vector3(0f, _spinDis, 0f);
        _spinTween = _symbolContainer.DOMove(tgPos, duration);
        _spinTween.SetEase(Ease.Linear);
        _spinTween.OnComplete(TweenLinearComplete);
    }

    void TweenLinearComplete()
    {
        RemoveSpiningSymbols();
        
        if( _spinCount == SPIN_COUNT_LIMIT )
        {
            ServerTooLate();
        }
        else if( _spinCount > _config.SpinCountThreshold && _resultSymbolNames != null )
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
        Log("FinishSpinReel");

        AddSpiningSymbols( _column * 3 );
        AddResultSymbols();

        TweenFirst();
    }

    void AddResultSymbols()
    {
        // _resultSymbolNames

        Symbol topSymbol = _symbols[0];

        //제일위가 null 이고 결과 마지막이 null 인 경우
        //제일위가 null 이고 결과 마지막이 null 아닌 경우
        //제일위가 null 아니고 결과 마지막이 null 인경우
        //제일위가 null 아니고 결과 마지막이 null 아닌 경우

        var count = _resultSymbolNames.Length;

        while( count-- > 0 )
        {

        }
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
