using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

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
    SlotBetting _betting;
    InfoViewUI _info;
    MessageBoard _board;
    WinAnimatorUI _winAnimator;
    double _balance;

    User _user;

    void Awake()
    {
        CanvasUtil.CanvasSetting(GetComponent<Canvas>());
    }

    void Start()
    {
        SetBalance(0);
    }

    public void Initialize(SlotMachine slot)
    {
        _slot = slot;
        _betting = _slot.Config.COMMON.Betting;
        _user = SlotModel.Instance.Owner;

        InitInfo();
        InitBoard();
        InitWinDisplayer();
        InitButtons();

        UpdateBalance(_user.Balance);
    }

    void InitWinDisplayer()
    {
        _winAnimator = GetComponentInChildren<WinAnimatorUI>();
        _winAnimator.Init();
    }

    void InitInfo()
    {
        _info = GetComponentInChildren<InfoViewUI>();
        if (_info == null) Debug.LogError("can't find InfoViewUI");
        _info.Init(_slot);
    }

    void InitBoard()
    {
        _board = GetComponentInChildren<MessageBoard>();
        if (_board) _board.Initialize(_slot);
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

    }

    public void Spin()
    {
        _info.SetWin(0);

        SetBalance(_user.Balance - _betting.TotalBet);

        if (_board != null) _board.Spin();
    }

    public void PlayAllWin(WinItemList info)
    {
        if (_board != null) _board.PlayAllWin(info);
    }

    public void PlayEachWin(WinItemList.Item item)
    {
        if (_board != null) _board.PlayEachWin(item);
    }

    public void AddWinBalance(WinBalanceInfo info)
    {
        _info.AddWin(info);
        _winAnimator.AddWin(info);
    }

    public void ApplyUserBalance()
    {
        SetBalance(_user.Balance);
    }

    Tweener _tweenBalance;

    void SetBalance(double balance)
    {
        if (balance == _balance) return;

        if (_tweenBalance != null) _tweenBalance.Kill();

        var duration = balance > _balance ? 0.5f : 0.2f;
        _tweenBalance = DOTween.To(() => _balance, x => UpdateBalance(x), balance, duration).Play();
    }

    void UpdateBalance(double balance)
    {
        _balance = balance;
        TxtBalance.text = balance.ToBalance();
    }
}
