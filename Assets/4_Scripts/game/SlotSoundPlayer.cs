using UnityEngine;
using System.Collections;
using System;

using lpesign;
namespace Game
{
    public class SlotSoundPlayer : SingletonSimple<SlotSoundPlayer>
    {
        public AudioClip[] basicClips;
        public SoundPlayer.SoundCategory[] categoryList;


        public AudioClip BGM;
        public AudioClip BGM_FREE;

        public AudioClip[] spins;

        public AudioClip[] spinStop;

        public AudioClip[] scatterStops;

        public AudioClip win;
        public AudioClip winEnd;

        public AudioClip[] progressiveWin;

        public AudioClip freeSpinTrigger;
        public AudioClip freeSpinReady;
        public AudioClip freeSpinReadyLoop;
        public AudioClip freeSpinNumbering;
        public AudioClip freeSpinComplete;
        public AudioClip freeSpinCongratulation;

        public AudioClip btnSpin;
        public AudioClip btnFast;
        public AudioClip btnDecrease;
        public AudioClip btnIncrease;
        public AudioClip btnCommon;

        public AudioClip bigWin;
        public AudioClip megaWin;
        public AudioClip jackpot;

        public AudioClip intro;

        SoundPlayer _player;

        void Start()
        {
            _player = FindObjectOfType<SoundPlayer>();
            if (_player == null) throw new NullReferenceException("SoundPlayer cannot be null");
        }

        public void Spin()
        {
            _player.PlaySFX(spins[0]);
        }

        public void FreeSpin()
        {
            _player.PlaySFX(spins[0]);
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


