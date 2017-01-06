using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class User
{
    public int Level { get; set; }
    public int LevelPercent { get; set; }
    public int GameLevel { get; set; }
    public int GameLevelPercent { get; set; }
    public double Balance { get; set; }

    public void Update(ResDTO dto)
    {
        Balance = dto.balance;
        Level = dto.level;
        LevelPercent = dto.levelPercent == null ? 0 : dto.levelPercent.Value;
        GameLevel = dto.gameLevel;
        GameLevelPercent = dto.gameLevelPercent;
    }

    public void Reset()
    {
        Balance = 0;
    }
}
