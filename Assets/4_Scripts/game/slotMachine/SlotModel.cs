using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using lpesign;

public class SlotModel : SingletonSimple<SlotModel>
{
    public SlotConfig.FreeSpinRetriggerType RetriggerType { get; set; }

    public event UnityAction<double> OnUpdateBalance;

    public float TotalBet { get; private set; }

    //freespin
    public bool IsFreeSpinTrigger { get; private set; }
    public bool IsFreeSpining { get; private set; }
    public int FreeSpinAddedCount { get; private set; }
    public int FreeSpinCurrentCount { get; private set; }
    public int FreeSpinTotal { get; private set; }
    public int FreeSpinRemain { get { return FreeSpinTotal - FreeSpinCurrentCount; } }

    public double Balance
    {
        get { return _balance; }
        private set
        {
            _balance = value;
            if (OnUpdateBalance != null) OnUpdateBalance(_balance);
        }
    }

    double _balance;
    int _spinCount;
    ResDTO.Spin _spinDTO;
    ResDTO.Spin.Payout.SpinInfo _lastSpinInfo;
    public ResDTO.Spin.Payout.SpinInfo LastSpinInfo{ get{ return _lastSpinInfo; }}

    public void Reset()
    {
        _spinCount = 0;
        _balance = 0;

        _spinDTO = null;

        TotalBet = 0;
        IsFreeSpinTrigger = false;
    }

    public void SetLoginData(ResDTO.Login dto)
    {
        Balance = dto.balance;

        //min, max bet 처리. last_line_bet 보다 우선한다
        //last_line_bet 처리
    }

    public void SetSpinData(ResDTO.Spin dto)
    {
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
        Balance = _spinDTO.balance;
    }
}
