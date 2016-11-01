using UnityEngine;
using System.Collections;

public class ResponseDTO
{
    public class LoginDTO
    {
        public int levelPercent { get; set; }
        public int level { get; set; }
        public int gameLevel { get; set; }
        public int min_line_bet { get; set; }
        public int gameLevelPercent { get; set; }
        public int last_line_bet { get; set; }
        public int max_line_bet { get; set; }

        public double balance { get; set; }
        // public float jackpotPool;
    }

    public class SpinDTO
    {

    }
}


public class ReqDTO
{
    public class LoginData : DTO
    {
        public int userID { get; set; }
        public string signedRequest { get; set; }
    }

    public class SpinData : DTO
    {
        public double lineBet { get; set; }
    }
}

public class DTO
{

}