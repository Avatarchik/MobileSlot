using UnityEngine;
using System;
using System.Collections;
using lpesign;

public class SlotModel : SingletonSimple<SlotModel>
{
    public event Action<int> OnUpdateAutoSpinCount;


    #region freespin property
    public bool IsFreeSpinTrigger { get; private set; }
    public bool IsFreeSpinReTrigger { get { return IsFreeSpinning && IsFreeSpinTrigger; } }

    public bool IsFreeSpinning { get; private set; }
    public int FreeSpinAddedCount { get; private set; }
    public int FreeSpinCurrentCount { get; private set; }
    public int FreeSpinTotal { get; private set; }
    public int FreeSpinRemain { get { return FreeSpinTotal - FreeSpinCurrentCount; } }
    public bool IsAutoSpin { get { return _remainAutoCount > 0; } }
    public bool IsFastSpin { get; set; }
    public User Owner { get; private set; }
    public SlotConfig.WinType WinType { get; private set; }
    public float WinMultiplier { get; private set; }

    public ResDTO.Spin SpinDTO { get; private set; }

    public bool IsJMBWin
    {
        get
        {
            return WinType == SlotConfig.WinType.BIGWIN || WinType == SlotConfig.WinType.MEGAWIN || WinType == SlotConfig.WinType.JACPOT;
        }
    }

    public bool HasNextSpin
    {
        get { return SpinDTO.payouts.Next() != null; }
    }

    public bool HasBonusSpin
    {
        get
        {
            if (HasNextSpin == false) return false;

            return SpinDTO.payouts.Next().isBonusSpin;
        }
    }

    public bool HasNudge { get; private set; }

    #endregion

    ResDTO.Spin.Payout.SpinInfo _lastSpinInfo;
    int _remainAutoCount;
    int _spinCount;

    SlotBetting _betting;

    SlotConfig _config;

    public void Reset()
    {
        if (Owner == null) Owner = new User();
        else Owner.Reset();

        _spinCount = 0;

        SpinDTO = null;

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

        SpinDTO = dto;

        WinMultiplier = SpinDTO.payouts.multipleWin;
    }

    SlotConfig.WinType GetWinType()
    {
        if (SpinDTO.payouts.totalPayout == 0f) return SlotConfig.WinType.LOSE;
        else if (IsFreeSpinTrigger || IsFreeSpinning) return SlotConfig.WinType.NORMAL;
        else if (SpinDTO.payouts.isBigWin) return SlotConfig.WinType.BIGWIN;
        else if (SpinDTO.payouts.isMegaWin) return SlotConfig.WinType.MEGAWIN;
        else if (SpinDTO.payouts.isJackpot) return SlotConfig.WinType.JACPOT;
        else return SlotConfig.WinType.NORMAL;
    }

    public ResDTO.Spin.Payout.SpinInfo NextSpin()
    {
        _lastSpinInfo = SpinDTO.payouts.MoveNext();

        if (_lastSpinInfo == null) throw new System.NullReferenceException("SpinInfo can't be null");

        IsFreeSpinTrigger = _lastSpinInfo.IsFreeSpinTrigger;

        if (IsFreeSpinTrigger)
        {
            if (IsFreeSpinning == false)
            {
                FreeSpinCurrentCount = 0;
                FreeSpinAddedCount = _lastSpinInfo.freeSpinCount;
                FreeSpinTotal = FreeSpinAddedCount;
            }
            else
            {
                switch (_config.RetriggerType)
                {
                    case SlotConfig.FreeSpinRetriggerType.Add:
                        FreeSpinAddedCount = _lastSpinInfo.freeSpinCount;
                        break;

                    case SlotConfig.FreeSpinRetriggerType.Refill:
                        FreeSpinAddedCount = _lastSpinInfo.freeSpinCount - FreeSpinRemain;
                        break;
                }

                FreeSpinTotal += FreeSpinAddedCount;
            }
        }

        WinType = GetWinType();

        return _lastSpinInfo;
    }

    public ResDTO.Spin.Payout.SpinInfo UseFreeSpin()
    {
        IsFreeSpinning = true;
        ++FreeSpinCurrentCount;
        return NextSpin();
    }

    public void ApplyBalance()
    {
        Owner.Balance = SpinDTO.balance;
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
