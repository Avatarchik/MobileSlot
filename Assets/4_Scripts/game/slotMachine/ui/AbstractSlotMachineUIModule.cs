using UnityEngine;
using System.Collections;

public abstract class AbstractSlotMachineUIModule : MonoBehaviour
{
    protected SlotMachineUI _ui;
    virtual public void Init(SlotMachineUI slotUI)
    {
        _ui = slotUI;
    }
}
