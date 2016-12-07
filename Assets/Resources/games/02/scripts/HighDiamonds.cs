using UnityEngine;
using System.Collections;

public class HighDiamonds : MonoBehaviour
{
    public const string S0 = "S0";
    public const string W0 = "W0";
    public const string J0 = "J0";
    public const string J1 = "J1";
    public const string J2 = "J2";
    public const string H0 = "H0";
    public const string H1 = "H1";
    public const string M0 = "M0";
    public const string M1 = "M1";
    public const string M2 = "M2";
    public const string L0 = "L0";
    public const string L1 = "L1";
    public const string L2 = "L2";

    public SlotConfig mainSlotConfig;

    SlotMachine _machine;

    void Awake()
    {
        //------------------------------------------------------------------------------------
        // define Common info
        //------------------------------------------------------------------------------------

        SlotConfig.CommonConfig commonInfo = new SlotConfig.CommonConfig()
        {
            ID = 2,
            Host = "182.252.135.251",
            Port = 13500,
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
                PaylineNum = 25
            },

            DebugSymbolArea = false,
            DebugTestSpin = true
        };

        //slot setting
        mainSlotConfig = new SlotConfig();
        mainSlotConfig.COMMON = commonInfo;

        //base
        mainSlotConfig.Row = 3;
        mainSlotConfig.Column = 5;

        //symbol
        mainSlotConfig.SymbolSize = new Size2D(1.04f, 0.68f);
        mainSlotConfig.NullSymbolSize = default(Size2D);

        //freespin
        mainSlotConfig.TriggerType = SlotConfig.FreeSpinTriggerType.Select;
        mainSlotConfig.RetriggerType = SlotConfig.FreeSpinRetriggerType.Add;

        //reel
        mainSlotConfig.ReelPrefab = Resources.Load<Reel>("games/" + commonInfo.ID.ToString("00") + "/prefabs/Reel");
        mainSlotConfig.ReelSize = new Size2D(1.04f, 2.04f);
        mainSlotConfig.ReelSpace = 1.46f;
        mainSlotConfig.ReelGap = 0.42f;

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
        nameMap.AddSymbolToMap("S0", S0);
        nameMap.AddSymbolToMap("W0", W0);

        nameMap.AddSymbolToMap("B0", J0);
        nameMap.AddSymbolToMap("B1", J1);
        nameMap.AddSymbolToMap("B2", J2);

        nameMap.AddSymbolToMap("H0", H0);
        nameMap.AddSymbolToMap("H1", H1);

        nameMap.AddSymbolToMap("M0", M0);
        nameMap.AddSymbolToMap("M1", M1);
        nameMap.AddSymbolToMap("M2", M2);

        nameMap.AddSymbolToMap("L0", L0);
        nameMap.AddSymbolToMap("L1", L1);
        nameMap.AddSymbolToMap("L2", L2);
        mainSlotConfig.NameMap = nameMap;

        //startSymbol
        mainSlotConfig.SetStartSymbols(new string[,]
        {
            { L0, J0, H0, S0, L0 },
            { M0, M2, J1, M2, M0 },
            { M1, S0, L2, J2, M1 },
            { M0, M2, J1, M2, M0 },
            { L0, J0, H0, S0, L0 }
        });

        //paylineTable
        int[][] paylineTable =
        {
            new int[] {1, 1, 1, 1, 1},
            new int[] {0, 0, 0, 0, 0},
            new int[] {2, 2, 2, 2, 2},
            new int[] {0, 1, 2, 1, 0},
            new int[] {2, 1, 0, 1, 2},

            new int[] {1, 0, 0, 0, 1},
            new int[] {1, 2, 2, 2, 1},
            new int[] {0, 0, 1, 2, 2},
            new int[] {2, 2, 1, 0, 0},
            new int[] {1, 2, 1, 0, 1},

            new int[] {1, 0, 1, 2, 1},
            new int[] {0, 1, 1, 1, 0},
            new int[] {2, 1, 1, 1, 2},
            new int[] {0, 1, 0, 1, 0},
            new int[] {2, 1, 2, 1, 2},

            new int[] {1, 1, 0, 1, 1},
            new int[] {1, 1, 2, 1, 1},
            new int[] {0, 0, 2, 0, 0},
            new int[] {2, 2, 0, 2, 2},
            new int[] {0, 2, 2, 2, 0},

            new int[] {2, 0, 0, 0, 2},
            new int[] {1, 2, 0, 2, 1},
            new int[] {1, 0, 2, 0, 1},
            new int[] {0, 2, 0, 2, 0},
            new int[] {2, 0, 2, 0, 2},
        };
        mainSlotConfig.paylineTable = paylineTable;


        //strips
        //todo
        //릴스트립도 가변배열로 고쳐야함
        mainSlotConfig.NormalStrip = new ReelStrip(new string[][]
        {
            new string[] {J0,H0,L1,M1,W0,M0,S0,L2,M1,L0,J0,H1,L0,W0,M2,L1,L2,M0,L1,L2},
            new string[] {H0,L1,M1,M0,W0,L2,M1,J1,L0,H1,L0,M2,L1,L2,M0,J1,L1,L2},
            new string[] {H0,L1,M1,W0,W0,W0,M0,S0,L2,M1,L0,H1,L0,S0,M2,L1,J2,L2,M0,L1,L2},
            new string[] {H0,L1,M1,M0,W0,L2,M1,J1,L0,H1,L0,M2,L1,L2,M0,J1,L1,L2},
            new string[] {J0,H0,L1,M1,W0,M0,S0,L2,M1,L0,J0,H1,L0,W0,M2,L1,L2,M0,L1,L2}
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
