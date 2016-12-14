using UnityEngine;
using System.Collections;

namespace Game
{
    public abstract class AbstractSlotMachineUIModule : MonoBehaviour
    {
        protected SlotMachineUI _ui;
        protected SlotMachine _machine;
        protected SlotConfig _config;
        virtual public void Init(SlotMachineUI slotUI)
        {
            _ui = slotUI;
            _machine = _ui.SlotMachine;
            _config = _machine.Config;
        }
    }
}