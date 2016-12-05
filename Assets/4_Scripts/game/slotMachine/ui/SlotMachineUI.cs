using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class SlotMachineUI : MonoBehaviour
{
    public Text TxtBalance;

    InfoViewUI _info;
    PaytableUI _paytable;
    WinAnimatorUI _winAnimator;

    public SlotMachine SlotMachine { get; private set; }
    SlotBetting _betting;
    MessageBoardUI _board;
    ControllerUI _controller;

    double _balance;
    Tweener _tweenBalance;
    User _user;
    SlotModel _model;


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
        SlotMachine = slot;

        _model = SlotModel.Instance;
        _user = _model.Owner;
        _betting = slot.Config.COMMON.Betting;

        InitPaytable();
        InitInfo();
        InitMessageBoard();
        InitWinDisplayer();
        InitButtons();

        UpdateBalance(_user.Balance);
    }

    void InitPaytable()
    {
        _paytable = GetComponentInChildren<PaytableUI>();
        _paytable.Init( this );
    }

    void InitWinDisplayer()
    {
        _winAnimator = GetComponentInChildren<WinAnimatorUI>();
        _winAnimator.Init( this );
    }

    void InitInfo()
    {
        _info = GetComponentInChildren<InfoViewUI>();
        if (_info == null) Debug.LogError("can't find InfoViewUI");
        _info.Init(this);
    }

    void InitMessageBoard()
    {
        _board = GetComponentInChildren<MessageBoardUI>();
        if (_board) _board.Init(this);
    }

    void InitButtons()
    {
        _controller = GetComponentInChildren<ControllerUI>();
        _controller.Init(this);
    }

    public void OpenPaytable()
    {
        _paytable.Open();
    }

    public void Idle()
    {

    }

    public void Spin()
    {
        _info.SetWin(0);

        _controller.Spin();
        SetBalance(_user.Balance - _betting.TotalBet);

        if (_board != null) _board.Spin();
    }

    public void StopSpin()
    {
        _controller.StopSpin();
    }

    public void ReceivedSymbol()
    {
        _controller.ReceivedSymbol();
    }

    public void ReelStopComplete()
    {
        _controller.ReelStopComplete();
    }

    public void PlayAllWin(WinItemList info)
    {
        if (_board != null) _board.PlayAllWin(info);
    }

    public void PlayEachWin(WinItemList.Item item)
    {
        if (_board != null) _board.PlayEachWin(item);
    }

    public void TakeCoin(WinBalanceInfo info)
    {
        _info.AddWin(info);
        _winAnimator.AddWin(info);
    }

    public void SkipTakeCoin()
    {
        _winAnimator.SkipTakeCoin();
        _info.SkipTakeCoin();
    }

    public void ApplyUserBalance()
    {
        SetBalance(_user.Balance);
    }

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
