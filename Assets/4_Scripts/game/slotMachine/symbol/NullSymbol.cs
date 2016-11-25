using UnityEngine;
using System.Collections;

public class NullSymbol : Symbol
{
    public const string EMPTY = "EM";


    override public void SetState(SymbolState nextState, bool useOverlap = true)
    {
        if (useOverlap == false && _currentState == nextState) return;

        _currentState = nextState;

        //do nothing
    }
}
