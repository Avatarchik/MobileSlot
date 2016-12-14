using UnityEngine;
using System.Collections;
using System;

using lpesign;
namespace Game
{
    public class SlotSoundPlayer : SingletonSimple<SlotSoundPlayer>
    {
        SoundPlayer _player;

        void Start()
        {
            _player = FindObjectOfType<SoundPlayer>();
            if (_player == null) throw new NullReferenceException("SoundPlayer cannot be null");
        }

        public void PlayFreeSpinTrigger()
        {

        }

        public void StopFreeSpinTrigger()
        {

        }

        public void PlayFreeSpinReady()
        {

        }

        public void StopFreeSpinReady()
        {

        }
    }
}


