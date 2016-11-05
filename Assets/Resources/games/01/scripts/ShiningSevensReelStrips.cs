using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShiningSevensReelStrips : ReelStrips
{
    public const string W0 = "W0";
    public const string SR = "H0";
    public const string SG = "H1";
    public const string SB = "H2";

    public const string BB = "M0";
    public const string BG = "M1";
    public const string BR = "M2";

    public const string L0 = "L0";

    public ShiningSevensReelStrips()
    {
        _startSymbolName = new string[,]
        {
            { SR, L0, SG },
            { L0, W0, L0 },
            { SB, L0, SG }
        };

        _symbolmap= new Dictionary<string, string>();
        _symbolmap["W0"] = W0;
        _symbolmap["H0"] = SR;
        _symbolmap["H1"] = SG;
        _symbolmap["H2"] = SB;
        _symbolmap["M0"] = BB;
        _symbolmap["M1"] = BG;
        _symbolmap["M2"] = BR;
        _symbolmap["L0"] = L0;
    }
}
