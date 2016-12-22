using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using lpesign;

namespace Game
{
    public class SlotMachineUI : MonoBehaviour
    {
        public Text TxtBalance;

        InfoViewUI _info;
        PaytableUI _paytable;
        WinAnimatorUI _winAnimator;

        public SlotMachine SlotMachine { get; private set; }
        SlotBetting _betting;
        MessageBoardUI _messageBoard;
        ControllerUI _controller;

        double _balance;
        Tweener _tweenBalance;
        User _user;
        SlotModel _model;


        void Awake()
        {
            GetComponent<Canvas>().SetReferenceSize(GlobalConfig.ReferenceWidth, GlobalConfig.ReferenceHeight, GlobalConfig.PixelPerUnit);
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
            _betting = slot.Config.Betting;

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
            _paytable.Init(this);
        }

        void InitWinDisplayer()
        {
            _winAnimator = GetComponentInChildren<WinAnimatorUI>();
            _winAnimator.Init(this);
        }

        void InitInfo()
        {
            _info = GetComponentInChildren<InfoViewUI>();
            if (_info == null) Debug.LogError("can't find InfoViewUI");
            _info.Init(this);
        }

        void InitMessageBoard()
        {
            _messageBoard = GetComponentInChildren<MessageBoardUI>();
            if (_messageBoard) _messageBoard.Init(this);
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

            if (_messageBoard != null) _messageBoard.Spin();
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
            if (_messageBoard != null) _messageBoard.PlayAllWin(info);
        }

        public void PlayEachWin(WinItemList.Item item)
        {
            if (_messageBoard != null) _messageBoard.PlayEachWin(item);
        }

        public void TakeCoin(WinBalanceInfo info, bool IsSummary)
        {
            if (IsSummary) _info.SetWin(0);

            _info.AddWin(info);
            _winAnimator.AddWin(info);
        }

        public void SkipTakeCoin()
        {
            _winAnimator.SkipTakeCoin();
            _info.SkipTakeCoin();
        }

        public void FreeSpinTrigger()
        {
            if (_messageBoard != null) _messageBoard.FreeSpinTrigger();
        }

        public void FreeSpin()
        {
            if (_messageBoard != null) _messageBoard.FreeSpin();
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

}
