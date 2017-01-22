using UnityEngine;
using System.Collections.Generic;

namespace Game
{

    public class EmptiableReel : Reel
    {
        [SerializeField]
        float _emptySymbolYOffset = float.NaN;

        override protected float GetStartSymbolPos()
        {
            if (float.IsNaN(_emptySymbolYOffset))
            {
                _emptySymbolYOffset = (_config.MainMachine.ReelSize.height - (_config.MainMachine.SymbolSize.height + _config.MainMachine.NullSymbolSize.height * 2)) * 0.5f;
            }

            var res = base.GetStartSymbolPos();

            Symbol firstSymbol = _symbols[_config.MainMachine.MarginSymbolCount];
            if (firstSymbol is EmptySymbol) res -= _emptySymbolYOffset;

            return res;
        }

        override protected void AddInterpolationSymbols()
        {
            string lastResultName = _receivedSymbolNames[_receivedSymbolNames.Length - 1];
            string topSpiningName = _symbols[0].SymbolName;

            //일반 추가
            if (topSpiningName == EmptySymbol.EMPTY && lastResultName == EmptySymbol.EMPTY)
            {
                AddSpiningSymbol(_currentStrips.GetRandom(_column));
            }
            //널 추가 
            else if (topSpiningName != EmptySymbol.EMPTY && lastResultName != EmptySymbol.EMPTY)
            {
                AddSpiningSymbol(EmptySymbol.EMPTY);
            }
        }

        override protected void ComposeLastSpiningSymbols()
        {
            base.ComposeLastSpiningSymbols();

            if (_lastSymbolNames[0] == EmptySymbol.EMPTY && _receivedSymbolNames[0] != EmptySymbol.EMPTY)
            {
                _spinDis += _emptySymbolYOffset;
            }
            else if (_lastSymbolNames[0] != EmptySymbol.EMPTY && _receivedSymbolNames[0] == EmptySymbol.EMPTY)
            {
                _spinDis -= _emptySymbolYOffset;
            }
        }

        override protected string GetSpiningSymbolName()
        {
            if (_symbols[0] is EmptySymbol == false) return EmptySymbol.EMPTY;
            else return _currentStrips.GetRandom(_column);
        }
    }
}
