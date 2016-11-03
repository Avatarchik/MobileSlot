using UnityEngine;
using System.Collections;

public class ShiningSevens : SlotMachine
{
    override protected void Awake()
    {
        SlotInfo.Port = 13100;
        SlotInfo.Version = "0.0.1";

        SlotInfo info = new SlotInfo();

        info.Row = 3;
        info.Column = 3;

        info.SymbolRect = new Rect(0f, 0f, 5.1f, 1.1f);
        info.ReelRect = new Rect(0f, 0f, 2.1f, 2.5f);
        info.ReelGap = 30;

        SlotInfo.Main = info;


        base.Awake();
    }
}
