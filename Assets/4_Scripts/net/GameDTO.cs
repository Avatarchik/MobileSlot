//todo
//class 가 아니라 struct 도 고려
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
        public int min_line_bet { get; set; }
        public int gameLevelPercent { get; set; }
        public int last_line_bet { get; set; }
        public int max_line_bet { get; set; }

        public double balance { get; set; }
        public double? jackpotPool{get;set;}
    }

    public class Spin : DTO
    {
        public double balance { get; set; }
        
        /*
        {
            "data": {
                "levelPercent": 48,
                "jackpotPool": null,
                "freeSpinKey": null,
                "gameLevel": 29,
                "balance": 342131634187,
                "payouts": {
                    "lineBet": 1000,
                    "spins": [
                        {
                            "payLines": [],
                            "isNormal": true,
                            "freeSpinCount": 0,
                            "reel": [
                                "H2",
                                "L0",
                                "H1",
                                "L0",
                                "M1",
                                "L0",
                                "L0",
                                "M1",
                                "L0"
                            ],
                            "totalPayout": 0,
                            "fixedreel": [
                                0,
                                0,
                                0
                            ]
                        }
                    ],
                    "totalPayout": 0,
                    "multipleWin": 0,
                    "isMegaWin": false,
                    "isBigWin": false,
                    "accumulateSum": 0,
                    "isJackpot": false
                },
                "level_up_bonus": 0,
                "gameLevelPercent": 93,
                "level_up_spins": 0,
                "winID": 0,
                "level": 163
            },
            "success": true,
            "cmd": "spin"
        }
        */
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