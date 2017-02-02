﻿using UnityEngine;
using lpesign;

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
        public const string L0 = "L0";

        override public void SettingByScript()
        {
            base.SettingByScript();
            //------------------------------------------------------------------------------------
            // define Common info
            //------------------------------------------------------------------------------------
            _slotConfig.ID = 1;
            _slotConfig.host = "182.252.135.251";
            _slotConfig.port = 13100;
            _slotConfig.accessID = 1086; //or 1;
            _slotConfig.ver = "0.0.1";
            _slotConfig.Jackpot = false;
            _slotConfig.Betting = new SlotBetting()
            {
                BetTable = new double[]
                {
                    100,200,500,1000,2000,
                    5000,10000,20000,50000,100000,
                    200000,300000,500000,1000000,2000000,
                    3000000,4000000,5000000,10000000,20000000
                },
            };
            _slotConfig.DebugSymbolArea = false;
            _slotConfig.DebugTestSpin = true;

            //------------------------------------------------------------------------------------
            // define machienConfig
            //------------------------------------------------------------------------------------
            var machine = new MachineConfig(_slotConfig);
            machine.row = 3;
            machine.column = 3;

            //freespin
            machine.UseFreeSpin = false;

            //reel
            machine.ReelPrefab = Resources.Load<Reel>("games/" + ConvertUtil.ToDigit(_slotConfig.ID) + "/prefabs/Reel");
            machine.ReelSize = new Size2D(2.1f, 2.5f);
            machine.ReelSpace = 2.56f;
            machine.ReelGap = 0.3f;

            //spin
            machine.MarginSymbolCount = 1;
            machine.SpinningSymbolCount = 5;
            machine.IncreaseCount = 5;
            machine.SpinCountThreshold = 5;
            machine.SpinSpeedPerSec = 15f;
            machine.DelayEachSpin = 0.1f;
            machine.tweenFirstBackInfo = new MoveTweenInfo(0.2f, 0.2f);
            machine.tweenLastBackInfo = new MoveTweenInfo(0.2f, 0.3f);

            //transition
            machine.transition = new Transition()
            {
                ReelStopAfterDelay = 0.5f,
                PlaySymbolAfterDelay = 0,
                EachWin = 1f,
                EachWinSummary = 1f,
                EachLockReel = 0.2f,
                LockReelAfterDelay = 1f,
                FreeSpinTriggerDuration = 1f,
                AutoSpinDelay = 0.2f
            };

            //symbol define
            machine.useEmpty = true;
            machine.SymbolSize = new Size2D(2.1f, 1.1f);
            machine.blankSymbolSize = new Size2D(2.1f, 0.3f);

            machine.ClearSymbolDefine();
            machine.TopChildSymbolType = SymbolType.Blank;
            machine.AddSymbolDefine("W0", SymbolType.Wild);

            machine.AddSymbolDefine("H0", SymbolType.High);
            machine.AddSymbolDefine("H1", SymbolType.High);
            machine.AddSymbolDefine("H2", SymbolType.High);

            machine.AddSymbolDefine("M0", SymbolType.Middle);
            machine.AddSymbolDefine("M1", SymbolType.Middle);
            machine.AddSymbolDefine("M2", SymbolType.Middle);

            machine.AddSymbolDefine("L0", SymbolType.Blank);

            //startSymbol
            machine.SetStartSymbols(new string[][]
            {
                new string[]{ L0, SR, L0, SG, L0 },
                new string[]{ BB, L0, W0, L0, BG },
                new string[]{ L0, SB, L0, SG, L0  }
            });

            //paylineTable
            PaylineTable paylineTable = new PaylineTable
            (
                new int[][]
                {
                    new int[] {1,1,1},
                    new int[] {0,0,0},
                    new int[] {2,2,2},
                    new int[] {0,1,2},
                    new int[] {2,1,0}
                }
            );

            machine.paylineTable = paylineTable;


            //reelStrip

            var stripsBundle = new ReelStripsBundle();
            stripsBundle.AddStrips(ReelStripsBundle.Group.Default, new string[][]
            {
                new string[] {SG,BG,SB,BR,SB,W0,SG,BR,SB,BR,SG,BR,BG,SB,W0},
                new string[] {SG,BR,SB,BR,SB,W0,SG,BG,SB,BG,SG,BG,BR,SB,BG},
                new string[] {SG,BG,W0,BR,SB,SR,SB,BR,SB,SG,BR,BG,W0,BR,BR}
            });
            machine.reelStripsBundle = stripsBundle;

            //register machineconfig
            _slotConfig.ClearMachines();
            _slotConfig.AddMachine(machine);
        }
    }
}