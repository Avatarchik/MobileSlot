using UnityEngine;
using System.Collections;

public class ShiningSevensSymbol : Symbol
{
    override protected void Awake()
    {
        base.Awake();
        _debugDisplayArea = false;
    }
}
