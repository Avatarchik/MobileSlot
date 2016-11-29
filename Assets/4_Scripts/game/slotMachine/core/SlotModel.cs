using UnityEngine;
using System;
using System.Collections;
using lpesign;

public class SlotModel : SingletonSimple<SlotModel>
{

    public event Action<double> OnUpdateWinBalance;
    public event Action<int> OnUpdateAutoSpinCount;


    #region freespin property
    public bool IsFreeSpinTrigger { get; private set; }
    public bool IsFreeSpining { get; private set; }
    public int FreeSpinAddedCount { get; private set; }
    public int FreeSpinCurrentCount { get; private set; }
    public int FreeSpinTotal { get; private set; }
    public int FreeSpinRemain { get { return FreeSpinTotal - FreeSpinCurrentCount; } }
    public bool IsAutoSpin { get { return _remainAutoCount > 0; } }
    public bool IsFastSpin { get; set; }
    public SlotConfig.FreeSpinRetriggerType RetriggerType { get; set; }
    public User Owner { get; private set; }

    double _winBalance;
    public double WinBalances
    {
        get { return _winBalance; }
        private set
        {
            if (_winBalance == value) return;

            _winBalance = value;
            if (OnUpdateWinBalance != null) OnUpdateWinBalance(_winBalance);
        }
    }

    ResDTO.Spin.Payout.SpinInfo _lastSpinInfo;
    #endregion

    int _remainAutoCount;
    int _spinCount;

    ResDTO.Spin _spinDTO;

    SlotBetting _betting;

    SlotConfig _config;

    public void Reset()
    {
        if (Owner == null) Owner = new User();
        else Owner.Reset();

        _spinCount = 0;

        _spinDTO = null;

        IsFreeSpinTrigger = false;
    }

    public void Initialize(SlotMachine slot, ResDTO.Login dto)
    {
        Reset();

        _config = slot.Config;

        _betting = _config.COMMON.Betting;

        SetLoginData(dto);
    }

    public void SetLoginData(ResDTO.Login dto)
    {
        Owner.Update(dto);

        _betting.Init(dto.min_line_bet, dto.max_line_bet, dto.last_line_bet);
    }

    public void SetSpinData(ResDTO.Spin dto)
    {
        Owner.Update(dto);

        ++_spinCount;

        _spinDTO = dto;
    }

    public ResDTO.Spin.Payout.SpinInfo NextSpin()
    {
        _lastSpinInfo = _spinDTO.payouts.MoveNext();

        if (_lastSpinInfo == null) throw new System.NullReferenceException("SpinInfo can't be null");

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

    public bool HasBonusSpin
    {
        get
        {
            var next = _spinDTO.payouts.Next();
            if (next == null) return false;
            else return next.isBonusSpin;
        }
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

    public void StartAutoSpin(int count = int.MaxValue)
    {
        _remainAutoCount = count;
    }

    public void StopAutoSpin()
    {
        _remainAutoCount = 0;
    }

    public void UseAutoSpin()
    {
        if (IsAutoSpin == false) return;
        --_remainAutoCount;

        if (OnUpdateAutoSpinCount != null) OnUpdateAutoSpinCount(_remainAutoCount);
    }
}
