using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;


public class InfoViewUI : MonoBehaviour
{
    public Text txtLine;
    public Text txtBet;
    public Text txtTotalBet;
    public Text txtWin;

    SlotBetting _betting;

    double _lineBet;
    double _totalBet;
    double _win;

    void Awake()
    {
        txtLine.text = "";
        txtBet.text = "";
        txtTotalBet.text = "";
        txtWin.text = "";
    }

    public void Init(SlotMachine slot)
    {
        _betting = slot.Config.COMMON.Betting;
        _betting.OnUpdateLineBetIndex += OnUpdateLineBetHandler;

        SetLineNum(slot.Config.paylineTable.Length);
        SetWin(0);

        OnUpdateLineBetHandler();
    }

    void OnUpdateLineBetHandler()
    {
        SetLineBet(_betting.LineBet);
        SetTotalBet(_betting.TotalBet);
    }

    public void SetWin(double win, float duration = 0f)
    {
        if (win == _win || duration == 0f) UpdateWin(win);
        else DOTween.To(() => _win, x => UpdateWin(x), win, duration).Play();
    }

    public void AddWin(WinBalanceInfo info)
    {
        SetWin(_win + info.balance, info.duration);
    }

    void UpdateWin(double win)
    {
        _win = win;
        txtWin.text = _win.ToBalance();
    }

    void SetLineBet(double bet, float duration = 0.2f)
    {
        if (_lineBet == 0 || bet <= _lineBet || duration == 0f) UpdateLineBet(bet);
        else DOTween.To(() => _lineBet, x => UpdateLineBet(x), bet, duration).Play();
    }

    void UpdateLineBet(double bet)
    {
        _lineBet = bet;
        txtBet.text = bet.ToBalance();
    }

    void SetTotalBet(double bet, float duration = 0.2f)
    {
        if (_totalBet == 0 || bet <= _totalBet || duration == 0f) UpdateTotalBet(bet);
        else DOTween.To(() => _totalBet, x => UpdateTotalBet(x), bet, duration).Play();
    }

    void UpdateTotalBet(double bet)
    {
        _totalBet = bet;
        txtTotalBet.text = bet.ToBalance();
    }

    void SetLineNum(int num)
    {
        txtLine.text = num.ToString();
    }
}