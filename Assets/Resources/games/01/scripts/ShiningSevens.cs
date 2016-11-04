using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShiningSevens : SlotMachine
{
    public const string WILD = "W0";
    public const string SR = "H0";
    public const string SG = "H1";
    public const string SB = "H2";

    public const string BB = "M0";
    public const string BG = "M1";
    public const string BR = "M2";

    public const string L0 = "L0";

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

        var map = new Dictionary<string, string>();
        map["W0"] = WILD;
        
        map["H0"] = SR;
        map["H1"] = SG;
        map["H2"] = SB;
        map["M0"] = BB;
        map["M1"] = BG;
        map["M2"] = BR;

        map["L0"] = L0;

        SlotInfo.Main = info;


        base.Awake();
    }


}
