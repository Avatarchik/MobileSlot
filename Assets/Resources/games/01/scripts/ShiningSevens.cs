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

    public const string L0 = EmptySymbol.EMPTY;
    public SlotConfig mainSlotConfig;

    SlotMachine _machine;

    void Awake()
    {
        //------------------------------------------------------------------------------------
        // define Common info
        //------------------------------------------------------------------------------------

        SlotConfig.CommonConfig commonInfo = new SlotConfig.CommonConfig()
        {
            ID = 1,
            Host = "182.252.135.251",
            Port = 13100,
            Version = "0.0.1",

            Betting = new SlotBetting()
            {
                BetTable = new double[]
                {
                    100,200,500,1000,2000,
                    5000,10000,20000,50000,100000,
                    200000,300000,500000,1000000,2000000,
                    3000000,4000000,5000000,10000000,20000000
                },
                PaylineNum = 5
            },

            DebugSymbolArea = false,
            DebugTestSpin = true
        };

        //slot setting
        mainSlotConfig = new SlotConfig();
        mainSlotConfig.COMMON = commonInfo;

        //base
        mainSlotConfig.Row = 3;
        mainSlotConfig.Column = 3;

        //symbol
        mainSlotConfig.SymbolSize = new Size2D(2.1f, 1.1f);
        mainSlotConfig.NullSymbolSize = new Size2D(2.1f, 0.3f);

        //freespin
        mainSlotConfig.UseFreeSpin = false;

        //reel
        mainSlotConfig.ReelPrefab = Resources.Load<Reel>("games/" + commonInfo.ID.ToString("00") + "/prefabs/Reel");
        mainSlotConfig.ReelSize = new Size2D(2.1f, 2.5f);
        mainSlotConfig.ReelSpace = 2.56f;
        mainSlotConfig.ReelGap = 0.3f;

        //spin
        mainSlotConfig.MarginSymbolCount = 1;
        mainSlotConfig.SpiningSymbolCount = 5;
        mainSlotConfig.IncreaseCount = 5;
        mainSlotConfig.SpinCountThreshold = 5;
        mainSlotConfig.SpinSpeedPerSec = 15f;
        mainSlotConfig.DelayEachReel = 0.1f;
        mainSlotConfig.tweenFirstBackInfo = new MoveTweenInfo(0.2f, 0.2f);
        mainSlotConfig.tweenLastBackInfo = new MoveTweenInfo(0.2f, 0.3f);

        //transition
        mainSlotConfig.transition = new Transition()
        {
            ReelStopCompleteAfterDealy = 0.5f,
            PlayAllSymbols_WinBalance = 0,
            EachWin = 1f,
            EachWinSummary = 1f,
            EachLockReel = 0.2f,
            LockReel_BonusSpin = 1f,
            FreeSpinTriggerDuration = 1f
        };

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
        mainSlotConfig.NameMap = nameMap;

        //startSymbol
        mainSlotConfig.SetStartSymbols(new string[,]
        {
            { L0, SR, L0, SG, L0 },
            { BB, L0, W0, L0, BG },
            { L0, SB, L0, SG, L0 }
        });

        //paylineTable
        int[][] paylineTable =
        {
            new int[] {1,1,1},
            new int[] {0,0,0},
            new int[] {2,2,2},
            new int[] {0,1,2},
            new int[] {2,1,0}
        };
        mainSlotConfig.paylineTable = paylineTable;


        //strips
        //todo
        //릴스트립도 가변배열로 고쳐야함
        mainSlotConfig.NormalStrip = new ReelStrip(new string[][]
        {
            new string[] {SG,BG,SB,BR,SB,W0,SG,BR,SB,BR,SG,BR,BG,SB,W0},
            new string[] {SG,BR,SB,BR,SB,W0,SG,BG,SB,BG,SG,BG,BR,SB,BG},
            new string[] {SG,BG,W0,BR,SB,SR,SB,BR,SB,SG,BR,BG,W0,BR,BR}
        }, ReelStrip.ReelStripType.USE_NULL);
    }

    void Start()
    {
        _machine = FindObjectOfType<SlotMachine>();
        if (_machine == null) Debug.LogError("Can't find SlotMachine.");

        _machine.Run(mainSlotConfig);

        if (mainSlotConfig.COMMON.DebugTestSpin)
        {
            gameObject.AddComponent<DebugHelper>();
        }
    }
}