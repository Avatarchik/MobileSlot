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

	SlotModel _model;
	SlotBetting _betting;

	override public void Init( SlotMachineUI slotUI  )
	{
        base.Init( slotUI );

		_model = SlotModel.Instance;
		_model.OnUpdateAutoSpinCount += ( count )=>
        {
            UpdateAutoButton( _model.IsAutoSpin );
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

	public void UpdateAutoButton( bool isAuto )
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
}
