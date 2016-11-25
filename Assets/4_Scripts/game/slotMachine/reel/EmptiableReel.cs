using UnityEngine;
using System.Collections.Generic;

public class EmptiableReel : Reel
{
    [SerializeField]
    float _emptySymbolYOffset = float.NaN;

    override protected float GetStartSymbolPos()
    {
        if (float.IsNaN(_emptySymbolYOffset))
        {
            _emptySymbolYOffset = (_config.ReelSize.height - (_config.SymbolSize.height + _config.NullSymbolSize.height * 2)) * 0.5f;
        }

        var res = base.GetStartSymbolPos();

        Symbol firstSymbol = _symbols[_config.MarginSymbolCount];
        if (firstSymbol is EmptySymbol) res -= _emptySymbolYOffset;

        return res;
    }

    override protected void ComposeLastSpiningSymbols()
    {
        base.ComposeLastSpiningSymbols();

        if (_lastResultSymbolNames[0] == EmptySymbol.EMPTY && _receivedSymbolNames[0] != EmptySymbol.EMPTY)
        {
            _spinDis -= _emptySymbolYOffset;
        }
        else if (_lastResultSymbolNames[0] != EmptySymbol.EMPTY && _receivedSymbolNames[0] == EmptySymbol.EMPTY)
        {
            _spinDis -= _emptySymbolYOffset;
        }
    }

    override protected void AddInterpolationSymbols()
    {
        string lastResultName = _receivedSymbolNames[_receivedSymbolNames.Length - 1];
        string topSpiningName = _symbols[0].SymbolName;

        //일반 추가
        if (topSpiningName == EmptySymbol.EMPTY && lastResultName == EmptySymbol.EMPTY)
        {
            AddSpiningSymbol(_currentStrip.GetRandom(_column));
        }
        //널 추가 
        else if (topSpiningName != EmptySymbol.EMPTY && lastResultName != EmptySymbol.EMPTY)
        {
            AddSpiningSymbol(EmptySymbol.EMPTY);
        }
    }

    override protected string GetSpiningSymbolName()
    {
        if (_symbols[0] is EmptySymbol == false) return EmptySymbol.EMPTY;
        else return _currentStrip.GetRandom(_column);
    }
}
