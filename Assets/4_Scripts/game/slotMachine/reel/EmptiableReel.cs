using UnityEngine;
using System.Collections.Generic;

namespace Game
{

    public class BlankableReel : Reel
    {
        float _emptySymbolYOffset = float.NaN;

        override protected float GetStartSymbolPos()
        {
            if (float.IsNaN(_emptySymbolYOffset))
            {
                _emptySymbolYOffset = (_machineConfig.ReelSize.height - (_machineConfig.SymbolSize.height + _machineConfig.blankSymbolSize.height * 2)) * 0.5f;
            }

            var res = base.GetStartSymbolPos();

            Symbol firstSymbol = _symbols[_machineConfig.MarginSymbolCount];
            if (firstSymbol is BlankSymbol) res -= _emptySymbolYOffset;

            return res;
        }

        override protected void AddInterpolationSymbols()
        {
            string lastResultName = _receivedSymbolNames[_receivedSymbolNames.Length - 1];
            string topSpiningName = _symbols[0].SymbolName;

            //일반 추가
            if (topSpiningName == BlankSymbol.EMPTY && lastResultName == BlankSymbol.EMPTY)
            {
                AddSpinningSymbol(_currentStrips.GetRandom(_column));
            }
            //널 추가 
            else if (topSpiningName != BlankSymbol.EMPTY && lastResultName != BlankSymbol.EMPTY)
            {
                AddSpinningSymbol(BlankSymbol.EMPTY);
            }
        }

        override protected void ComposeLastSpinningSymbols()
        {
            base.ComposeLastSpinningSymbols();

            if (_lastSymbolNames[0] == BlankSymbol.EMPTY && _receivedSymbolNames[0] != BlankSymbol.EMPTY)
            {
                _spinDis += _emptySymbolYOffset;
            }
            else if (_lastSymbolNames[0] != BlankSymbol.EMPTY && _receivedSymbolNames[0] == BlankSymbol.EMPTY)
            {
                _spinDis -= _emptySymbolYOffset;
            }
        }

        override protected string GetSpinningSymbolName()
        {
            if (_symbols[0] is BlankSymbol == false) return BlankSymbol.EMPTY;
            else return _currentStrips.GetRandom(_column);
        }
    }
}
