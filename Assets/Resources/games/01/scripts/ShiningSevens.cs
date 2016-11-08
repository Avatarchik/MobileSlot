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

    public const string L0 = NullSymbol.EMPTY;
    public SlotConfig config;

    SlotMachine _machine;

    void Awake()
    {
        SlotConfig.ID = 1;
        SlotConfig.Host = "182.252.135.251";
        SlotConfig.Port = 13100;
        SlotConfig.Version = "0.0.1";

        //slot setting
        if (config == null) config = new SlotConfig();

        config.Row = 3;
        config.Column = 3;

        //symbol
        config.SymbolSize = new Size2D(2.1f, 1.1f);
        config.NullSymbolSize = new Size2D(2.1f, 0.3f);

        //reel
        config.ReelPrefab = Resources.Load<Reel>("games/" + SlotConfig.ID.ToString("00") + "/prefabs/Reel");
        config.ReelSize = new Size2D(2.1f, 2.5f);
        config.ReelSpace = 2.56f;
        config.ReelGap = 0.3f;
        config.DummySymbolCount = 1;

        //symbolNameMap

        SymbolNameMap nameMap = new SymbolNameMap();
        nameMap.AddSymbolToMap("W0", W0);

        nameMap.AddSymbolToMap("H0", SR);
        nameMap.AddSymbolToMap("H1", SG);
        nameMap.AddSymbolToMap("H2", SB);

        nameMap.AddSymbolToMap("M0", BB);
        nameMap.AddSymbolToMap("M1", BG);
        nameMap.AddSymbolToMap("M2", BR);

        nameMap.AddSymbolToMap("L0", L0);
        config.NameMap = nameMap;

        //strips setting
        ReelStrip reelStrips = new ReelStrip();
        reelStrips.SetStartSymbols(new string[,]
        {
            { L0, SR, L0, SG, L0 },
            { BB, L0, W0, L0, BG },
            { L0, SB, L0, SG, L0 }
        });

        reelStrips.SetNormalStrips(new string[,]
        {
            {SG,BG,SB,BR,SB,W0,SG,BR,SB,BR,SG,BR,BG,SB,W0},
            {SG,BR,SB,BR,SB,W0,SG,BG,SB,BG,SG,BG,BR,SB,BG},
            {SG,BG,W0,BR,SB,SR,SB,BR,SB,SG,BR,BG,W0,BR,BR}
        });

        config.Strips = reelStrips;

        _machine = FindObjectOfType<SlotMachine>();
        _machine.Config = config;
    }

    void Start()
    {
        _machine.Run();
    }
}