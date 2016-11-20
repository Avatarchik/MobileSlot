using UnityEngine;
using UnityEditor;

using System.Collections.Generic;


public class DTO
{

}

public class ResDTO
{
    public double balance;
    public int level;
    public int levelPercent;
    public int gameLevel;
    public int gameLevelPercent;

    public class Login : ResDTO
    {
        public int min_line_bet;
        public int last_line_bet;
        public int max_line_bet;

        public double? jackpotPool;
    }

    public class Spin : ResDTO
    {
        public double level_up_bonus;
        public string freeSpinKey;
        public double? jackpotPool;
        public int winID;

        public Payout payouts;

        public class Payout
        {
            public float lineBet;
            public double totalPayout;
            public float multipleWin;
            public bool isMegaWin;
            public bool isBigWin;
            public bool isJackpot;
            public Queue<SpinInfo> spins;

            public SpinInfo Next()
            {
                if (spins != null || spins.Count > 0) return spins.Dequeue();

                return null;
            }

            public class SpinInfo
            {
                public string[] reel;
                public int freeSpinCount;
                public double totalPayout;
                public bool isNormal;
                public int[] fixedreel;
                public Payline[] payLines;

                public bool IsFreeSpinTrigger
                {
                    get { return freeSpinCount > 0; }
                }

                public string[] GetReelData(int column, int rowCount)
                {
                    var startIndex = column * rowCount;
                    string[] res = new string[rowCount];
                    for (var i = 0; i < rowCount; ++i)
                    {
                        res[i] = reel[startIndex + i];
                    }
                    return res;
                }

                public class Payline
                {
                    public double payout;
                    public int matches;
                    public int line;
                    public bool isJackpot;
                }
            }
        }
    }
}


public class ReqDTO
{
    public class Login : DTO
    {
        public int userID { get; set; }
        public string signedRequest { get; set; }
    }

    public class Spin : DTO
    {
        public double lineBet { get; set; }
    }
}