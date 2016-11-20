using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using lpesign;

public class SlotModel : SingletonSimple<SlotModel>
{
    public SlotConfig.FreeSpinRetriggerType RetriggerType { get; set; }

    public event UnityAction<double> OnUpdateWinBalance;

    public User Owner { get; private set; }

    public float TotalBet { get; private set; }


    #region freespin property
    public bool IsFreeSpinTrigger { get; private set; }
    public bool IsFreeSpining { get; private set; }
    public int FreeSpinAddedCount { get; private set; }
    public int FreeSpinCurrentCount { get; private set; }
    public int FreeSpinTotal { get; private set; }
    public int FreeSpinRemain { get { return FreeSpinTotal - FreeSpinCurrentCount; } }
    #endregion

    private double _winBalance;
    public double WinBalances
    {
        get { return _winBalance; }
        private set
        {
            _winBalance = value;
            if (OnUpdateWinBalance != null) OnUpdateWinBalance(_winBalance);
        }
    }

    int _spinCount;

    ResDTO.Spin _spinDTO;
    ResDTO.Spin.Payout.SpinInfo _lastSpinInfo;
    public ResDTO.Spin.Payout.SpinInfo LastSpinInfo { get { return _lastSpinInfo; } }

    public void Reset()
    {
        if (Owner == null) Owner = new User();
        else Owner.Reset();

        _spinCount = 0;

        _spinDTO = null;

        TotalBet = 0;
        IsFreeSpinTrigger = false;
    }

    public void SetLoginData(ResDTO.Login dto)
    {
        Owner.Update( dto );

        //min, max bet 처리. last_line_bet 보다 우선한다
        //last_line_bet 처리
    }

    public void SetSpinData(ResDTO.Spin dto)
    {
        Owner.Update( dto );

        ++_spinCount;

        _spinDTO = dto;

        NextSpin();
    }

    ResDTO.Spin.Payout.SpinInfo NextSpin()
    {
        _lastSpinInfo = _spinDTO.payouts.Next();

        IsFreeSpinTrigger = _lastSpinInfo.IsFreeSpinTrigger;

        if (IsFreeSpinTrigger)
        {
            if (IsFreeSpining == false)
            {
                FreeSpinCurrentCount = 0;
                FreeSpinAddedCount = _lastSpinInfo.freeSpinCount;
                FreeSpinTotal = FreeSpinAddedCount;
            }
            else
            {
                switch (RetriggerType)
                {
                    case SlotConfig.FreeSpinRetriggerType.Add:
                        FreeSpinAddedCount = _lastSpinInfo.freeSpinCount;
                        break;

                    case SlotConfig.FreeSpinRetriggerType.Rollback:
                        FreeSpinAddedCount = _lastSpinInfo.freeSpinCount - FreeSpinRemain;
                        break;
                }

                FreeSpinTotal += FreeSpinAddedCount;
            }
        }

        return _lastSpinInfo;
    }

    public ResDTO.Spin.Payout.SpinInfo UseFreeSpin()
    {
        if (FreeSpinRemain <= 0) return null;

        IsFreeSpining = true;
        ++FreeSpinCurrentCount;
        return NextSpin();
    }

    public void ApplyBalance()
    {
        Owner.Balance = _spinDTO.balance;
    }
}
