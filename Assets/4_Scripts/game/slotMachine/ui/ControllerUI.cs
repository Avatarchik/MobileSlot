using UnityEngine;
using UnityEngine.UI;

using System;

public class ControllerUI : AbstractSlotMachineUIModule
{
    public Button btnPaytable;
    public Button btnBetDecrease;
    public Button btnBetIncrease;
    public Toggle btnFast;
    public Toggle btnAuto;
    public Button btnSpin;
    public Button btnStop;

    SlotModel _model;
    SlotBetting _betting;

    void Awake()
    {
        if (btnStop != null) btnStop.image.enabled = false;
    }

    override public void Init(SlotMachineUI slotUI)
    {
        base.Init(slotUI);

        _model = SlotModel.Instance;
        _model.OnUpdateAutoSpinCount += (count) =>
        {
            UpdateAutoButton(_model.IsAutoSpin);
        };


        _betting = _ui.SlotMachine.Config.COMMON.Betting;
        _betting.OnUpdateLineBetIndex += () =>
        {
            UpdateButtonState();
        };


        btnPaytable.onClick.AddListener(() =>
        {
            _ui.OpenPaytable();
        });

        btnSpin.onClick.AddListener(() =>
        {
            _ui.SlotMachine.TrySpin();
        });

        if (btnStop != null)
        {
            btnStop.onClick.AddListener(() =>
            {
                _ui.SlotMachine.TrySpin();
            });
        }

        btnBetDecrease.onClick.AddListener(() =>
        {
            _betting.Decrease();
        });

        btnBetIncrease.onClick.AddListener(() =>
        {
            _betting.Increase();
        });

        btnFast.onValueChanged.AddListener((b) =>
        {
            _model.IsFastSpin = b;
        });

        btnAuto.onValueChanged.AddListener((b) =>
        {
            if (b) _model.StartAutoSpin(5);
            else _model.StopAutoSpin();
        });

        UpdateButtonState();
    }

    void OnUpdateAutoSpinCountListener(int remainCount)
    {
        btnAuto.isOn = _model.IsAutoSpin;
    }

    void UpdateAutoButton(bool isAuto)
    {
        btnAuto.isOn = isAuto;
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

    public void Spin()
    {
        btnSpin.interactable = false;
        // if( btnStop != null ) btnStop.image.enabled = false;
    }

    public void ReceivedSymbol()
    {
        btnSpin.interactable = true;

        if (btnStop != null)
        {
            btnSpin.image.enabled = false;
            btnStop.image.enabled = true;
        }
    }

    public void StopSpin()
    {
        btnSpin.interactable = false;

        btnSpin.image.enabled = true;
        if (btnStop != null) btnStop.image.enabled = false;
    }

    public void ReelStopComplete()
    {
        btnSpin.interactable = true;

        btnSpin.image.enabled = true;
        if (btnStop != null) btnStop.image.enabled = false;
    }
}
