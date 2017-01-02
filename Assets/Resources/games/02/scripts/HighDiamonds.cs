﻿using UnityEngine;
using System.Collections.Generic;

using lpesign;

namespace Game.HighDiamonds
{
    public class HighDiamonds : SlotCreator
    {
        public const string S0 = "S0";
        public const string W0 = "W0";
        public const string B0 = "B0";
        public const string B1 = "B1";
        public const string B2 = "B2";
        public const string H0 = "H0";
        public const string H1 = "H1";
        public const string M0 = "M0";
        public const string M1 = "M1";
        public const string M2 = "M2";
        public const string L0 = "L0";
        public const string L1 = "L1";
        public const string L2 = "L2";

        override public void SettingByScript()
        {
            base.SettingByScript();
            //------------------------------------------------------------------------------------
            // define Common info
            //------------------------------------------------------------------------------------
            _slotConfig.ID = 2;
            _slotConfig.Host = "182.252.135.251";
            _slotConfig.Port = 13500;
            _slotConfig.Version = "0.0.1";
            _slotConfig.Jackpot = true;
            _slotConfig.Betting = new SlotBetting()
            {
                BetTable = new double[]
                {
                    100,200,500,1000,2000,
                    5000,10000,20000,50000,100000
                },
                PaylineNum = 25
            };
            _slotConfig.DebugSymbolArea = false;
            _slotConfig.DebugTestSpin = true;


            //------------------------------------------------------------------------------------
            // define machienConfig
            //------------------------------------------------------------------------------------
            var machine = new SlotConfig.MachineConfig(_slotConfig);

            //base
            machine.row = 3;
            machine.column = 5;

            //freespin
            machine.UseFreeSpin = true;
            machine.TriggerType = SlotConfig.FreeSpinTriggerType.Select;
            machine.RetriggerType = SlotConfig.FreeSpinRetriggerType.Add;

            //reel
            machine.ReelPrefab = Resources.Load<Reel>("games/" + _slotConfig.ID.ToString("00") + "/prefabs/Reel");
            machine.ReelSize = new Size2D(1.04f, 2.04f);
            machine.ReelSpace = 1.46f;
            machine.ReelGap = 0.42f;

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

            //paytable
            PaylineTable paylineTable = new PaylineTable
            (
                new int[][]
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
                }
            );
            machine.paylineTable = paylineTable;

            //symbol define
            machine.useEmpty = false;
            machine.SymbolSize = new Size2D(1.04f, 0.68f);
            machine.ClearSymbolDefine();
            machine.AddSymbolDefine("S0", SymbolType.FSScatter, 3);
            machine.AddSymbolDefine("W0", SymbolType.Wild);

            machine.AddSymbolDefine("B0", SymbolType.PGSVScatter);
            machine.AddSymbolDefine("B1", SymbolType.PGSVScatter);
            machine.AddSymbolDefine("B2", SymbolType.PGSVScatter);

            machine.AddSymbolDefine("H0", SymbolType.High);
            machine.AddSymbolDefine("H1", SymbolType.High);

            machine.AddSymbolDefine("M0", SymbolType.Middle);
            machine.AddSymbolDefine("M1", SymbolType.Middle);
            machine.AddSymbolDefine("M2", SymbolType.Middle);

            machine.AddSymbolDefine("L0", SymbolType.Low);
            machine.AddSymbolDefine("L1", SymbolType.Low);
            machine.AddSymbolDefine("L2", SymbolType.Low);

            //scatters
            machine.ClearScatterInfo();
            machine.AddSccaterInfo(SymbolType.FSScatter, 3, new int[] { 0, 2, 4 });
            machine.AddSccaterInfo(SymbolType.PGSVScatter, 9, new int[] { 0, 1, 2, 3, 4 });


            //startSymbol
            machine.SetStartSymbols(new string[][]
            {
                new string[]{ L0, B0, H0, S0, L0 },
                new string[]{ M0, M2, B1, M2, M0 },
                new string[]{ M1, S0, L2, B2, M1 },
                new string[]{ M0, M2, B1, M2, M0 },
                new string[]{ L0, B0, H0, S0, L0 }
            });

            //rellStrip
            machine.reelStripBundle = new ReelStripList(new string[][]
            {
                new string[] {B0,H0,L1,M1,W0,M0,S0,L2,M1,L0,B0,H1,L0,W0,M2,L1,L2,M0,L1,L2},
                new string[] {H0,L1,M1,M0,W0,L2,M1,B1,L0,H1,L0,M2,L1,L2,M0,B1,L1,L2},
                new string[] {H0,L1,M1,W0,W0,W0,M0,S0,L2,M1,L0,H1,L0,S0,M2,L1,B2,L2,M0,L1,L2},
                new string[] {H0,L1,M1,M0,W0,L2,M1,B1,L0,H1,L0,M2,L1,L2,M0,B1,L1,L2},
                new string[] {B0,H0,L1,M1,W0,M0,S0,L2,M1,L0,B0,H1,L0,W0,M2,L1,L2,M0,L1,L2}
            }, ReelStrips.Type.NORMAL);

            //register machineconfig
            _slotConfig.ClearMachines();
            _slotConfig.AddMachine(machine);
        }
    }
}
