using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShiningSevens : SlotMachine
{
    override protected void Awake()
    {
        SlotInfo info = new SlotInfo();
        info.Row = 3;
        info.Column = 3;
        info.SymbolRect = new Rect(0f, 0f, 2.1f, 1.1f);
        info.ReelRect = new Rect(0f, 0f, 2.1f, 2.5f);
        info.ReelGap = 30;
        info.Strips = new ShiningSevensReelStrips();

        SlotInfo.Main = info;
        SlotInfo.Port = 13100;
        SlotInfo.Version = "0.0.1";

        base.Awake();
    }
}
