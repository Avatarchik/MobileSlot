using UnityEngine;
using System;
using System.Collections.Generic;

using lpesign;

namespace Game
{
    public class SlotSoundList : SoundList
    {
        public static SlotSoundList Instance;
        SoundPlayer _player;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            _player = FindObjectOfType<SoundPlayer>();
            if (_player == null) throw new NullReferenceException("SoundPlayer cannot be null");

            _player.SetSoundList(this);
        }

        override public void CreateDefaultList()
        {
            base.CreateDefaultList();

            //todo
            //현재 씬의 슬롯 config 을 얻어와서 설정에 따라 스마트하게 되어야 한다.
            int columnCount = 5;
            bool useFreespin = false;

            bool useProgressive = false;
            int progressiveCount = 5;

            //create sound
            basic.Add(new SoundSchema("INTRO"));
            basic.Add(new SoundSchema("BGM"));
            basic.Add(new SoundSchema("WIN"));
            basic.Add(new SoundSchema("WIN_END"));

            //create Group
            groups.Add(new SoundGroup("SPIN", columnCount));
            groups.Add(new SoundGroup("REEL_STOP", columnCount));
            groups.Add(new SoundGroup("BTN", new string[] { "SPIN", "FAST", "DECREASE", "DECREASE", "COMMON" }));
            groups.Add(new SoundGroup("SPECIAL_WIN", new string[] { "JACKPOT", "MEGAWIN", "BIGWIN" }));

            //todo
            //SCATTER 종류와 수 타입을 config 에서 얻어와 돌려야 한다
            groups.Add(new SoundGroup("EXPECT", columnCount));
            groups.Add(new SoundGroup("SCATTER_STOP", columnCount));

            //optional
            if (useFreespin)
            {
                basic.Add(new SoundSchema("BGM_FREE"));
                basic.Add(new SoundSchema("FREESPIN_TRIGGER"));
                basic.Add(new SoundSchema("FREESPIN_READY"));
                basic.Add(new SoundSchema("FREESPIN_READY_LOOP"));
                basic.Add(new SoundSchema("FREESPIN_NUMBERING"));
                basic.Add(new SoundSchema("FREESPIN_COMPLETE"));
                basic.Add(new SoundSchema("FREESPIN_CONGRATULATION"));

                groups.Add(new SoundGroup("FREE_REEL_STOP", columnCount));
            }

            if (useProgressive)
            {
                groups.Add(new SoundGroup("PROGRESSIVE_STOP", progressiveCount));
                groups.Add(new SoundGroup("PROGRESSIVE_WIN", progressiveCount));
            }
        }

        public void BGM()
        {
            _player.PlayBGM("BGM");
        }

        public void Spin()
        {
            _player.PlaySFX("SPIN");
        }

        public void ReelStop()
        {
            _player.PlaySFX("REEL_STOP");

            /*
            var tgSounds: Vector.<SoundSchema>;
			if( mModel.isFreeSpin )
			{
				tgSounds = STOP_SOUNDS;
			}
			else
			{
				tgSounds = STOP_SOUNDS;
			}
			
			if( tgSounds == null || tgSounds.length == 0 ) return;
			
			var column: int = reel.column;
			if( column >= tgSounds.length ) column = tgSounds.length - 1;
			var schema: SoundSchema = tgSounds[ column ];
			SoundPlayer.sfx.play( schema );
            */
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
