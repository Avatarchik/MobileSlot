using UnityEngine;
using lpesign;

namespace Game
{
    [RequireComponent(typeof(SlotConfig), typeof(SlotSoundList))]
    public class SlotCreator : MonoBehaviour
    {
        protected SlotConfig _slotConfig;
        protected SlotSoundList _soundPlayer;
        protected SlotMachine _machine;

        public SlotConfig Config { get { return _slotConfig; } }
        virtual public void SettingByScript()
        {
            _slotConfig = GetComponent<SlotConfig>();
            _soundPlayer = GetComponent<SlotSoundList>();
        }
    }
}
