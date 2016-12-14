using UnityEngine;
using System.Collections;

namespace Game
{
    public class EmptySymbol : Symbol
    {
        public const string EMPTY = "EM";

        override public void Initialize(string symbolName, SlotConfig config)
        {
            Initialize(symbolName, config.NullSymbolSize, config.COMMON.DebugSymbolArea);
        }

        override public void SetState(SymbolState nextState, bool useOverlap = true)
        {
            if (useOverlap == false && _currentState == nextState) return;

            _currentState = nextState;
        }
    }
}
