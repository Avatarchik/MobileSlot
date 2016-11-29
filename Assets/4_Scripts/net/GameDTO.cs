using System.Collections.Generic;


public class DTO
{

}

public class ResDTO : DTO
{
    public double balance;
    public int level;
    public int levelPercent;
    public int gameLevel;
    public int gameLevelPercent;

    public class Login : ResDTO
    {
        public int min_line_bet;
        public int max_line_bet;
        public int last_line_bet;

        public double? jackpotPool;
    }

    public class Spin : ResDTO
    {
        /*
		"jackpotPool": 1000017250,
		"last_line_bet": 1000,
		"max_line_bet": 5000000,
		"min_line_bet": 1000,
		"subjackpotPool": [
			1318593525,
			1072501000,
			1070535000,
			1030501750
		],
        */

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

            public SpinInfo MoveNext()
            {
                if (spins != null && spins.Count > 0) return spins.Dequeue();
                else return null;
            }

            public SpinInfo Next()
            {
                if (spins != null && spins.Count > 0) return spins.Peek();
                else return null;
            }

            public class SpinInfo
            {
                public string[] reel;
                public int freeSpinCount;
                public double totalPayout;
                public bool isNormal;
                public int[] fixedreel;
                public Payline[] payLines;
                public bool isBonusSpin;

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

                public Payline GetPaylineAt(int idx)
                {
                    if (idx < 0 || payLines.Length <= idx) return null;

                    return payLines[idx];
                }

                public class Payline
                {
                    public int line;
                    public int matches;
                    public double payout;
                    public int? winTable;
                    public bool isJackpot;

                    //추가되면 편리할 것 같은 것들
                    //public byte[] lineRow;//{1, 1, 1}
                    //public string[] symbolNames;
                    //public bool containWild;

                    // 라인에 매칭 되었나
                    public bool IsLineMatched { get { return line >= 0; } }
                }
            }
        }
    }
}


public class ReqDTO : DTO
{
    public class Login : ReqDTO
    {
        public int userID { get; set; }
        public string signedRequest { get; set; }
    }

    public class Spin : ReqDTO
    {
        public double lineBet { get; set; }
    }
}