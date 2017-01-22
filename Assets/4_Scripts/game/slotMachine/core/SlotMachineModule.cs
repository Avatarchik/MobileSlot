using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public abstract class SlotMachineModule : MonoBehaviour
    {
        // protected SlotMachineUI _ui;
        // protected SlotMachine _machine;
        // protected SlotConfig _slotConfig;
        // protected SlotBetting _betting;
        // protected SlotModel _model;

        protected SlotMachine _machine;
        protected MachineConfig _machineConfig;

        virtual public void Initialize(SlotMachine relativeMachine)
        {
            _machine = relativeMachine;
            _machineConfig = _machine.Config;

            // _model = SlotModel.Instance;

            // _slotConfig = _model.SlotConfig;
            // _betting = _slotConfig.Betting;

            // _ui = slotUI;
            // _machine = _ui.SlotMachine;
        }
    }
}
