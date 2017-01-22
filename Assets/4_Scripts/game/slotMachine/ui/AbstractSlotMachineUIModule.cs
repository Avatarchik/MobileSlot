using UnityEngine;
using System.Collections;

namespace Game
{
    public abstract class AbstractSlotMachineUIModule : MonoBehaviour
    {
        protected SlotMachineUI _ui;
        protected SlotMachine _machine;
        protected SlotConfig _slotConfig;
        protected SlotBetting _betting;
        protected SlotModel _model;

        virtual public void Init(SlotMachineUI slotUI)
        {
            _model = SlotModel.Instance;

            _slotConfig = _model.SlotConfig;
            _betting = _slotConfig.Betting;

            _ui = slotUI;
            _machine = _ui.SlotMachine;
        }
    }
}