using UnityEngine;

namespace Game.ShiningSevens
{
    public class ShiningSevens : SlotCreator
    {
        public const string W0 = "W0";
        public const string SR = "H0";
        public const string SG = "H1";
        public const string SB = "H2";

        public const string BB = "M0";
        public const string BG = "M1";
        public const string BR = "M2";

        public const string L0 = EmptySymbol.EMPTY;
        override public void SettingByScript()
        {
            base.SettingByScript();
            //------------------------------------------------------------------------------------
            // define Common info
            //------------------------------------------------------------------------------------
            _slotConfig.ID = 1;
            _slotConfig.Host = "182.252.135.251";
            _slotConfig.Port = 13100;
            _slotConfig.Version = "0.0.1";
            _slotConfig.Betting = new SlotBetting()
            {
                BetTable = new double[]
                {
                    100,200,500,1000,2000,
                    5000,10000,20000,50000,100000,
                    200000,300000,500000,1000000,2000000,
                    3000000,4000000,5000000,10000000,20000000
                },
                PaylineNum = 5
            };
            _slotConfig.DebugSymbolArea = false;
            _slotConfig.DebugTestSpin = true;

            //------------------------------------------------------------------------------------
            // define machienConfig
            //------------------------------------------------------------------------------------
            var machine = new SlotConfig.MachineConfig();
            machine.Row = 3;
            machine.Column = 3;

            //symbol
            machine.SymbolSize = new Size2D(2.1f, 1.1f);
            machine.NullSymbolSize = new Size2D(2.1f, 0.3f);

            //freespin
            machine.UseFreeSpin = false;

            //reel
            machine.ReelPrefab = Resources.Load<Reel>("games/" + _slotConfig.ID.ToString("00") + "/prefabs/Reel");
            machine.ReelSize = new Size2D(2.1f, 2.5f);
            machine.ReelSpace = 2.56f;
            machine.ReelGap = 0.3f;

            //spin
            machine.MarginSymbolCount = 1;
            machine.SpiningSymbolCount = 5;
            machine.IncreaseCount = 5;
            machine.SpinCountThreshold = 5;
            machine.SpinSpeedPerSec = 15f;
            machine.DelayEachReel = 0.1f;
            machine.tweenFirstBackInfo = new MoveTweenInfo(0.2f, 0.2f);
            machine.tweenLastBackInfo = new MoveTweenInfo(0.2f, 0.3f);

            //transition
            machine.transition = new Transition()
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
            machine.NameMap = nameMap;

            //startSymbol
            machine.SetStartSymbols(new string[,]
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
            machine.paylineTable = paylineTable;


            //strips
            //todo
            //릴스트립도 가변배열로 고쳐야함
            machine.NormalStrip = new ReelStrip(new string[][]
            {
                new string[] {SG,BG,SB,BR,SB,W0,SG,BR,SB,BR,SG,BR,BG,SB,W0},
                new string[] {SG,BR,SB,BR,SB,W0,SG,BG,SB,BG,SG,BG,BR,SB,BG},
                new string[] {SG,BG,W0,BR,SB,SR,SB,BR,SB,SG,BR,BG,W0,BR,BR}
            }, ReelStrip.ReelStripType.USE_NULL);
        }
    }
}