using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SlotMachineUI : MonoBehaviour
{
    public Text TxtBalance;

    [Header("Buttons")]
    public Button btnPaytable;
    public Button btnBetDecrease;
    public Button btnBetIncrease;
    public Button btnFast;
    public Button btnAuto;
    public Button btnSpin;

    SlotMachine _slot;

    InfoViewUI _info;

    void Awake()
    {
        CanvasUtil.CanvasSetting(GetComponent<Canvas>());

        _info = GetComponentInChildren<InfoViewUI>();
        if (_info == null) Debug.LogError("can't find InfoViewUI");
    }

    void Start()
    {
        SetBalance(0);
    }

    SlotBetting _betting;
    public void Initialize(SlotMachine slot)
    {
        _slot = slot;
        _betting = _slot.Config.Common.Betting;

        InitInfo();
        InitButtons();
    }

    void InitInfo()
    {
        _info.Init(_slot);
    }

    void InitButtons()
    {
        btnPaytable.onClick.AddListener(() =>
        {
            Debug.Log("show Paytable");
        });

        btnBetDecrease.onClick.AddListener(() =>
        {
            _betting.Decrease();
        });

        btnBetIncrease.onClick.AddListener(() =>
        {
            _betting.Increase();
        });

        btnFast.onClick.AddListener(() =>
        {
            //Debug.Log("fast");
        });

        btnAuto.onClick.AddListener(() =>
        {
            //Debug.Log("auto");
        });

        btnSpin.onClick.AddListener(() =>
        {
            _slot.TrySpin();
        });

        _betting.OnUpdateLineBetIndex += () =>
        {
            UpdateButtonState();
        };

        UpdateButtonState();
    }

    void UpdateButtonState()
    {
        if (_betting.IsFirstBet)
        {
            btnBetDecrease.interactable = false;
            btnBetIncrease.interactable = true;
        }
        else if (_betting.IsLastBet)
        {
            btnBetDecrease.interactable = true;
            btnBetIncrease.interactable = false;
        }
        else
        {
            btnBetDecrease.interactable = true;
            btnBetIncrease.interactable = true;
        }
    }

    public void Idle()
    {
        UpdateBalance();
    }

    public void Spin()
    {
        _info.SetWin(0);
    }

    public Coroutine AddWinBalance(WinBalanceInfo info)
    {
        return StartCoroutine(AddWinBalanceRoutine(info));
    }

    IEnumerator AddWinBalanceRoutine(WinBalanceInfo info)
    {
        _info.AddWin( info.balance, info.duration);

        yield return new WaitForSeconds( info.duration);
    }

    public void UpdateBalance()
    {
        SetBalance(SlotModel.Instance.Owner.Balance);
    }

    void SetBalance(double balance)
    {
        TxtBalance.text = balance.ToString("#,##0");
    }
}
