using UnityEngine;
using System.Collections;
using System;

using lpesign;
namespace Game
{
    public class SlotSoundPlayer : SingletonSimple<SlotSoundPlayer>
    {
        /*
        public class SlotSoundList
        {
            [Header("Group")]
            public AudioClipGroup spin;
            public AudioClipGroup spinStop;
            public AudioClipGroup scatterStop;
            public AudioClipGroup progressiveWin;
        }
        */

        SoundPlayer _player;

        void Start()
        {
            _player = FindObjectOfType<SoundPlayer>();
            if (_player == null) throw new NullReferenceException("SoundPlayer cannot be null");
        }

        public void Spin()
        {
            // _player.PlaySFX(spins[0]);
        }

        public void FreeSpin()
        {
            // _player.PlaySFX(spins[0]);
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


