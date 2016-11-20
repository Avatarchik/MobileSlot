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
    public SlotBetting betting;

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

        //spin
        config.DummySymbolCount = 1;
        config.SpiningSymbolCount = 5;
        config.IncreaseCount = 5;
        config.SpinCountThreshold = 4;
        config.SpinSpeedPerSec = 15f;
        config.DelayEachReel = 0f;
        config.tweenFirstBackInfo = new MoveTweenInfo(0.2f, 0.2f);
        config.tweenLastBackInfo = new MoveTweenInfo(0.2f, 0.3f);


        //debug
        config.DebugSymbolArea = false;

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

        //startSymbol
        config.SetStartSymbols(new string[,]
        {
            { L0, SR, L0, SG, L0 },
            { BB, L0, W0, L0, BG },
            { L0, SB, L0, SG, L0 }
        });

        /*
        Config.payLineTable = [
				[1, 1, 1],
				[0, 0, 0],
				[2, 2, 2],
				[0, 1, 2],
				[2, 1, 0,]
			];
            */

        //strips
        config.NormalStrip = new ReelStrip(new string[,]
        {
            {SG,BG,SB,BR,SB,W0,SG,BR,SB,BR,SG,BR,BG,SB,W0},
            {SG,BR,SB,BR,SB,W0,SG,BG,SB,BG,SG,BG,BR,SB,BG},
            {SG,BG,W0,BR,SB,SR,SB,BR,SB,SG,BR,BG,W0,BR,BR}
        }, ReelStrip.ReelStripType.USE_NULL);

        if (betting == null) betting = new SlotBetting();


        //betting
        betting.BetTable = new double[]
        {
            100,200,500,1000,2000,
            5000,10000,20000,50000,100000,
            200000,300000,500000,1000000,2000000,
            3000000,4000000,5000000,10000000,20000000
        };
        SlotConfig.Betting = betting;

        _machine = FindObjectOfType<SlotMachine>();
        _machine.Config = config;
    }

    void Start()
    {
        _machine.Run();
    }
}