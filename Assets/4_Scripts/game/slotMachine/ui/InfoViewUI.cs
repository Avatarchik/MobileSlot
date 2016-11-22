using UnityEngine;
using UnityEngine.UI;

using System.Collections;


public class InfoViewUI : MonoBehaviour
{
    public Text txtLine;
    public Text txtBet;
    public Text txtTotalBet;
    public Text txtWin;

    SlotModel _model;
    SlotBetting _betting;

    void Awake()
    {
        txtLine.text = "";
        txtBet.text = "";
        txtTotalBet.text = "";
        txtWin.text = "";
    }

    public void Init(SlotMachine slot)
    {
        _model = SlotModel.Instance;

        _betting = SlotConfig.Betting;
        _betting.OnUpdateLineBetIndex += OnUpdateLineBetHandler;

        SetLineNum(slot.Config.paylineTable.Length);
        SetWin( 0 );

        OnUpdateLineBetHandler();
    }

    void OnUpdateLineBetHandler()
    {
        SetLineBet(_betting.CurrentLineBet);
        SetTotalBet(_betting.CurrentTotalBet);
    }

    void SetLineBet(double balance, float duration = 0.5f)
    {
        txtBet.text = balance.ToString("#,##0");
    }

    void SetTotalBet(double balance, float duration = 0.5f)
    {
        txtTotalBet.text = balance.ToString("#,##0");
    }

    void SetWin(double balance, float duration = 0.5f)
    {
        txtWin.text = balance.ToString("#,##0");
    }

    void SetLineNum(int num)
    {
        txtLine.text = num.ToString();
    }
}