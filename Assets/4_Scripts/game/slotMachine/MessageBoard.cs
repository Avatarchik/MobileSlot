using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MessageBoard : MonoBehaviour
{
    const string GAME_INIT = "PRESS SPIN TO PLAY";
    const string SPIN_START = "PLAYING {0} LINES FOR {1} COINS. GOOD LUCK!";
    const string FREE_SPIN_TRIGGER = "SPINS REFILLS 7 FREE GAMES AGAIN!";
    const string FREE_SPIN_START = "FREE SPIN PLAY. GOOD LUCK!";
    const string FREE_SPIN_LAST = "LAST FREE SPIN!";
    const string FREE_SPINNING = "{0} FREE SPIN(S) REMAINING";
    const string PAYLINE_WIN = "LINE {0} PAYS {1} COINS";
    const string PAYLINE_WIN_ALL = "YOU'VE WON {0} LINE(S) {1} COINS";

    const string FREE_SPIN_END = "CONGRATULATIONS!";
    const string WHEEL_TRIGGER = "YOU’VE WON WHEEL BONUS GAMES!";
    const string WHEEL_READY = "CLICK TO SPIN THE WHEEL";
    const string ALLWAY_WIN = "SCATTER PAYS <font color='#FFFF00'>|payout|</font> COINS!";
    const string FIRE_OF_KINDS = "5 OF A KIND";
    const string JACKPOT = "J A C K P O T";
    const string JACKPOT_GRAND = "GRAND JACKPOT";
    const string JACKPOT_MEGA = "MEGA JACKPOT";
    const string JACKPOT_MAJOR = "MAJOR JACKPOT";
    const string JACKPOT_MINOR = "MINOR JACKPOT";

    Text _txt;
    SlotConfig _config;
    SlotModel _model;

    void Awake()
    {
        _txt = GetComponentInChildren<Text>();
    }

    public void Initialize(SlotMachine slot)
    {
        _config = slot.Config;
        _model = SlotModel.Instance;

        WriteBoard(GAME_INIT);
    }

    public void Spin()
    {
        WriteBoard(string.Format(SPIN_START, _config.paylineTable.Length, _config.COMMON.Betting.TotalBet.ToBalance()));
    }

    public void FreeSpin()
    {
        var remain = _model.FreeSpinRemain;
        var count = _model.FreeSpinCurrentCount;

        if (count == 1) WriteBoard(FREE_SPIN_START);
        else if (remain == 0) WriteBoard(FREE_SPIN_LAST);
        else WriteBoard(string.Format(FREE_SPINNING, remain));
    }

    public void PlayAllWin(WinItemList info)
    {
        WriteBoard(string.Format(PAYLINE_WIN_ALL, info.PaylineItemCount, info.Payout.ToBalance()));
    }

    public void PlayEachWin(WinItemList.Item item)
    {
        WriteBoard(string.Format(PAYLINE_WIN, item.PaylineIndex + 1, item.Payout.ToBalance()));
    }

    public void FreeSpinTrigger()
    {
        WriteBoard(string.Format(FREE_SPIN_TRIGGER, _model.FreeSpinTotal));
    }

    void WriteBoard(string msg)
    {
        _txt.text = msg;
    }
}
