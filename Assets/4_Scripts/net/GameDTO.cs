public class DTO
{

}

public class ResDTO
{
    public class Login : DTO
    {
        public int levelPercent { get; set; }
        public int level { get; set; }
        public int gameLevel { get; set; }
        public int gameLevelPercent { get; set; }
        public double balance { get; set; }

        public int min_line_bet { get; set; }
        public int last_line_bet { get; set; }
        public int max_line_bet { get; set; }

        public double? jackpotPool { get; set; }
    }

    public class Spin : DTO
    {
        public int levelPercent { get; set; }
        public int level { get; set; }
        public int gameLevel { get; set; }
        public int gameLevelPercent { get; set; }
        public double balance { get; set; }

        public double level_up_bonus { get; set; }


        public string freeSpinKey { get; set; }
        public double? jackpotPool { get; set; }
        public int winID { get; set; }

        public Payout payouts { get; set; }

        public class Payout
        {
            public float lineBet { get; set; }
            public double totalPayout { get; set; }
            public float multipleWin { get; set; }
            public bool isMegaWin { get; set; }
            public bool isBigWin { get; set; }
            public bool isJackpot { get; set; }

            public SpinInfo[] spins{get;set;}

            // public SpinInfo[] spins { get; set; }

            public class SpinInfo
            {
                public string[] reel { get; set; } //결과 심볼들
                public int freeSpinCount { get; set; }
                public double totalPayout { get; set; }
                public bool isNormal { get; set; }
                public int[] fixedreel { get; set; }
                public Payline[] payLines { get; set; }

                public class Payline
                {
                    public double payout { get; set; }
                    public int matches { get; set; }
                    public int line { get; set; }
                    public bool isJackpot { get; set; }
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