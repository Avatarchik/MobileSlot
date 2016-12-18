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
            public AudioClip BGM;
            public AudioClip BGM_FREE;
            public AudioClip intro;

            [Header("FreeSpin")]
            public AudioClip win;
            public AudioClip winEnd;
            public AudioClip bigWin;
            public AudioClip megaWin;
            public AudioClip jackpot;

            [Header("FreeSpin")]
            public AudioClip freeSpinTrigger;
            public AudioClip freeSpinReady;
            public AudioClip freeSpinReadyLoop;
            public AudioClip freeSpinNumbering;
            public AudioClip freeSpinComplete;
            public AudioClip freeSpinCongratulation;

            [Header("Button")]
            public AudioClip btnSpin;
            public AudioClip btnFast;
            public AudioClip btnDecrease;
            public AudioClip btnIncrease;
            public AudioClip btnCommon;

            [Header("Group")]
            public AudioClipGroup spin;
            public AudioClipGroup spinStop;
            public AudioClipGroup scatterStop;
            public AudioClipGroup progressiveWin;
        }
        */

        public SoundList soundList;

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


