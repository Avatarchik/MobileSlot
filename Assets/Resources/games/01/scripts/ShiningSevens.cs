using UnityEngine;

public class ShiningSevens : MonoBehaviour
{
    public const string W0 = "W0";
    public const string SR = "H0";
    public const string SG = "H1";
    public const string SB = "H2";

    public const string BB = "M0";
    public const string BG = "M1";
    public const string BR = "M2";

    public const string L0 = "L0";
    public SlotConfig slotInfo;

    SlotMachine _machine;

    void Awake()
    {
        SlotConfig config = new SlotConfig();
        config.Row = 3;
        config.Column = 3;
        config.SymbolRect = new Rect(0f, 0f, 2.1f, 1.1f);
        config.ReelRect = new Rect(0f, 0f, 2.1f, 2.5f);
        config.ReelGap = 30;

        ReelStrips reelStrips = new ReelStrips();
        reelStrips.SetStartSymbols(new string[,]
        {
            { SR, L0, SG },
            { L0, W0, L0 },
            { SB, L0, SG }
        });
        reelStrips.AddSymbolToMap("W0", W0);

        reelStrips.AddSymbolToMap("H0", SR);
        reelStrips.AddSymbolToMap("H1", SG);
        reelStrips.AddSymbolToMap("H2", SB);

        reelStrips.AddSymbolToMap("M0", BB);
        reelStrips.AddSymbolToMap("M1", BG);
        reelStrips.AddSymbolToMap("M2", BR);

        reelStrips.AddSymbolToMap("L0", L0);
        config.Strips = reelStrips;

        SlotConfig.Main = config;
        SlotConfig.Port = 13100;
        SlotConfig.Version = "0.0.1";

        _machine = FindObjectOfType<SlotMachine>();
        // _machine.
    }
}
