using UnityEngine;
using System;
using System.Collections;
using lpesign;

namespace Game
{

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
        public WinType WinType { get; private set; }
        public float WinMultiplier { get; private set; }

        public double TotalPayout { get { return _accumulatedPayout + SpinDTO.payouts.totalPayout; } }

        public ResDTO.Spin SpinDTO { get; private set; }

        public bool IsJMBWin
        {
            get
            {
                if (IsFreeSpinTrigger || IsFreeSpinning) return false;
                else return WinType == WinType.BIGWIN || WinType == WinType.MEGAWIN || WinType == WinType.JACPOT;
            }
        }

        public bool HasNextSpin
        {
            get
            {
                return SpinDTO.payouts.Next() != null;
            }
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

        double _accumulatedPayout;
        int _remainAutoCount;
        int _spinCount;

        SlotBetting _betting;
        SlotConfig _config;


        public void Initialize(SlotMachine slot, ResDTO.Login dto)
        {
            if (Owner == null) Owner = new User();
            else Owner.Reset();

            _config = slot.Config;
            _betting = _config.Betting;

            Reset();
            SetLoginData(dto);
        }

        public void AccumulatePayout(double payout)
        {
            _accumulatedPayout += payout;
            Debug.Log("AccumulatePayout" + _accumulatedPayout);
        }

        public void Reset()
        {
            _accumulatedPayout = 0;
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

        public void SetFreeSpinData(ResDTO.Spin dto)
        {
            SetSpinData(dto);

            FreeSpinCurrentCount = 0;
            FreeSpinAddedCount = dto.payouts.SpinCount;
            FreeSpinTotal = FreeSpinAddedCount;
        }

        WinType GetWinType()
        {
            if (SpinDTO.payouts.totalPayout == 0f) return WinType.LOSE;
            else if (SpinDTO.payouts.isBigWin) return WinType.BIGWIN;
            else if (SpinDTO.payouts.isMegaWin) return WinType.MEGAWIN;
            else if (SpinDTO.payouts.isJackpot) return WinType.JACPOT;
            else return WinType.NORMAL;
        }

        // public bool IsFreeSpinTrigger { get; private set; }
        // public bool IsFreeSpinReTrigger { get { return IsFreeSpinning && IsFreeSpinTrigger; } }

        public ResDTO.Spin.Payout.SpinInfo NextSpin()
        {
            _lastSpinInfo = SpinDTO.payouts.MoveNext();

            if (_lastSpinInfo == null) throw new System.NullReferenceException("SpinInfo can't be null");

            IsFreeSpinTrigger = _lastSpinInfo.IsFreeSpinTrigger;

            if (IsFreeSpinTrigger)
            {
                if (IsFreeSpinReTrigger)
                {
                    switch (_config.MainMachine.RetriggerType)
                    {
                        case FreeSpinRetriggerType.Add:
                            FreeSpinAddedCount = _lastSpinInfo.freeSpinCount;
                            break;

                        case FreeSpinRetriggerType.Refill:
                            FreeSpinAddedCount = _lastSpinInfo.freeSpinCount - FreeSpinRemain;
                            break;

                        case FreeSpinRetriggerType.None:
                            IsFreeSpinTrigger = false;
                            break;
                    }

                    FreeSpinTotal += FreeSpinAddedCount;
                }
                else
                {
                    FreeSpinCurrentCount = 0;
                    FreeSpinAddedCount = _lastSpinInfo.freeSpinCount;
                    FreeSpinTotal = FreeSpinAddedCount;
                }
            }

            WinType = GetWinType();

            return _lastSpinInfo;
        }

        public ResDTO.Spin.Payout.SpinInfo UseFreeSpin()
        {
            IsFreeSpinTrigger = false;
            IsFreeSpinning = true;
            ++FreeSpinCurrentCount;
            return NextSpin();
        }

        public void FreeSpinEnd()
        {
            IsFreeSpinning = false;
            FreeSpinCurrentCount = 0;
            FreeSpinAddedCount = 0;
            FreeSpinTotal = 0;
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
}
