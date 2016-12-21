using UnityEngine;
using System;
using System.Collections.Generic;

using lpesign;

namespace Game
{
    public class SlotSoundPlayer : SoundList
    {
        const string INTRO = "INTRO";
        const string BGM = "BGM";
        const string WIN = "WIN";
        const string WIN_END = "WIN_END";

        const string BGM_FREE = "BGM_FREE";
        const string FREESPIN_TRIGGER = "FREESPIN_TRIGGER";
        const string FREESPIN_READY = "FREESPIN_READY";
        const string FREESPIN_READY_LOOP = "FREESPIN_READY_LOOP";
        const string FREESPIN_NUMBERING = "FREESPIN_NUMBERING";
        const string FREESPIN_COMPLETE = "FREESPIN_COMPLETE";
        const string FREESPIN_CONGRATULATION = "FREESPIN_CONGRATULATION";


        //GROUP
        const string SPIN = "SPIN";
        const string REEL_STOP = "REEL_STOP";

        const string BTN = "BTN";
        const string BTN_SPIN = "SPIN";
        const string BTN_FAST = "FAST";
        const string BTN_DECREASE = "DECREASE";
        const string BTN_INCREASE = "INCREASE";
        const string BTN_COMMON = "COMMON";

        const string SPECIAL_WIN = "SPECIAL_WIN";
        const string SPECIAL_WIN_JACKPOT = "JACKPOT";
        const string SPECIAL_WIN_MEGAWIN = "MEGAWIN";
        const string SPECIAL_WIN_BIGWIN = "BIGWIN";

        const string EXPECT = "EXPECT";
        const string SCATTER_STOP = "SCATTER_STOP";

        const string FREE_REEL_STOP = "FREE_REEL_STOP";
        const string PROGRESSIVE_WIN = "PROGRESSIVE_WIN";

        public static SlotSoundPlayer Instance;
        SoundPlayer _player;

        void Awake()
        {
            Instance = this;
        }

        public void Initialize()
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
            basic.Add(new SoundSchema(INTRO));
            basic.Add(new SoundSchema(BGM));
            basic.Add(new SoundSchema(WIN));
            basic.Add(new SoundSchema(WIN_END));

            //create Group
            groups.Add(new SoundGroup(SPIN, SoundGroup.PlayType.Order, columnCount));
            groups.Add(new SoundGroup(REEL_STOP, SoundGroup.PlayType.Order, columnCount));
            groups.Add(new SoundGroup(BTN, SoundGroup.PlayType.CHOOSE, new string[] { BTN_SPIN, BTN_FAST, BTN_DECREASE, BTN_INCREASE, BTN_COMMON }));
            groups.Add(new SoundGroup(SPECIAL_WIN, SoundGroup.PlayType.CHOOSE, new string[] { SPECIAL_WIN_JACKPOT, SPECIAL_WIN_MEGAWIN, SPECIAL_WIN_BIGWIN }));

            //todo
            //SCATTER 종류와 수 타입을 config 에서 얻어와 돌려야 한다
            string scatterName = "DIA";
            int scatterableReelCount = 5;
            int scatterLimitCount = 7;
            groups.Add(new SoundGroup(scatterName, SoundGroup.PlayType.Order, scatterableReelCount));
            groups.Add(new SoundGroup(scatterName + "_STOP", SoundGroup.PlayType.CHOOSE, scatterLimitCount));

            //optional
            if (useFreespin)
            {
                basic.Add(new SoundSchema(BGM_FREE));
                basic.Add(new SoundSchema(FREESPIN_TRIGGER));
                basic.Add(new SoundSchema(FREESPIN_READY));
                basic.Add(new SoundSchema(FREESPIN_READY_LOOP));
                basic.Add(new SoundSchema(FREESPIN_NUMBERING));
                basic.Add(new SoundSchema(FREESPIN_COMPLETE));
                basic.Add(new SoundSchema(FREESPIN_CONGRATULATION));

                groups.Add(new SoundGroup(FREE_REEL_STOP, SoundGroup.PlayType.Order, columnCount));
            }

            if (useProgressive)
            {
                groups.Add(new SoundGroup(PROGRESSIVE_WIN, SoundGroup.PlayType.CHOOSE, progressiveCount));
            }
        }

        public void PlayBGM()
        {
            _player.PlayBGM(BGM);
        }

        AudioSource _spinChannel;

        public void Spin()
        {
            _spinChannel = _player.PlaySFX(SPIN);
        }

        public void StopSpin()
        {
            if (_spinChannel != null) _spinChannel.Stop();
        }

        public void Expect()
        {
            StopSpin();
        }

        public void ReelStop()
        {
            _player.PlaySFX(REEL_STOP);

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
