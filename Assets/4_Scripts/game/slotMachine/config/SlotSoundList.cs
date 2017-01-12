using UnityEngine;
using System;

using lpesign;

namespace Game
{
    public class SlotSoundList : SoundList
    {
        const string INTRO = "INTRO";
        const string BGM = "BGM";
        const string BGM_FREESPIN = "BGM_FREESPIN";

        const string WIN = "WIN";
        const string WIN_JACKPOT = "JACKPOT";
        const string WIN_MEGAWIN = "MEGAWIN";
        const string WIN_BIGWIN = "BIGWIN";
        const string WIN_END = "WIN_END";

        const string FREESPIN_TRIGGER = "FREESPIN_TRIGGER";
        const string FREESPIN_READY = "FREESPIN_READY";
        const string FREESPIN_READY_LOOP = "FREESPIN_READY_LOOP";
        const string FREESPIN_NUMBERING = "FREESPIN_NUMBERING";
        const string FREESPIN_COMPLETE = "FREESPIN_COMPLETE";
        const string FREESPIN_CONGRATULATION = "FREESPIN_CONGRATULATION";


        const string SPIN = "SPIN";
        const string SPIN_FREESPIN = "SPIN_FREESPIN";
        const string REEL_STOP = "REEL_STOP";
        const string REEL_STOP_FREESPIN = "REEL_STOP_FREESPIN";

        const string BTN = "BTN";
        const string BTN_SPIN = "SPIN";
        const string BTN_FAST = "FAST";
        const string BTN_DECREASE = "DECREASE";
        const string BTN_INCREASE = "INCREASE";
        const string BTN_COMMON = "COMMON";

        const string PROGRESSIVE_WIN = "PROGRESSIVE_WIN";

        override public void CreateDefaultList()
        {
            base.CreateDefaultList();

            var config = FindObjectOfType<SlotConfig>();
            if (config == null) throw new NullReferenceException("SlotConfig can not be null");
            var machine = config.Main;
            if (machine == null) throw new NullReferenceException("Main MachineConfig  can not be null");

            var freespin = machine.UseFreeSpin;
            var columnCount = machine.column;
            //basic

            basic.Add(INTRO);
            basic.Add(BGM);
            if (freespin) basic.Add(BGM_FREESPIN);

            basic.Add(WIN);
            basic.Add(WIN_END);

            basic.Add(WIN_BIGWIN);
            basic.Add(WIN_MEGAWIN);
            if (config.Jackpot) basic.Add(WIN_JACKPOT);


            //freespin sounds
            if (freespin)
            {
                basic.Add(FREESPIN_TRIGGER);
                basic.Add(FREESPIN_READY);
                basic.Add(FREESPIN_READY_LOOP);
                basic.Add(FREESPIN_NUMBERING);
                basic.Add(FREESPIN_COMPLETE);
                basic.Add(FREESPIN_CONGRATULATION);
            }

            //spin, stop
            groups.Add(new SoundGroup(SPIN, SoundGroup.PlayType.Order, columnCount));
            if (freespin) groups.Add(new SoundGroup(SPIN_FREESPIN, SoundGroup.PlayType.Order, columnCount));

            groups.Add(new SoundGroup(REEL_STOP, SoundGroup.PlayType.Order, columnCount));
            if (freespin) groups.Add(new SoundGroup(REEL_STOP_FREESPIN, SoundGroup.PlayType.Order, columnCount));

            //progressive win sounds
            var scatterInfo = machine.ScatterInfos;
            foreach (var s in scatterInfo)
            {
                if (s.type == SymbolType.PGSVScatter)
                {
                    groups.Add(new SoundGroup(PROGRESSIVE_WIN, SoundGroup.PlayType.CHOOSE, s.maxCount));
                    break;
                }
            }

            //button
            groups.Add(new SoundGroup(BTN, SoundGroup.PlayType.CHOOSE, new string[]
            {
                BTN_SPIN, BTN_FAST, BTN_DECREASE, BTN_INCREASE, BTN_COMMON
            }));
        }

        static private SoundPlayer _player;
        static private AudioSource _spinChannel;
        static private SlotSoundList _instance;
        void Awake()
        {
            _instance = this;
        }

        void OnDestroy()
        {
            _instance = null;
        }

        static public void Initialize()
        {
            _player = FindObjectOfType<SoundPlayer>();
            if (_player == null) throw new NullReferenceException("SoundPlayer cannot be null");

            _player.SetSoundList(_instance);
        }

        static public void PlayBGM()
        {
            _player.PlayBGM(BGM);
        }

        static public void StopBGM()
        {
            _player.StopBGM();
        }

        static public void PlaySFX(AudioClip clip, float volume = 1f)
        {
            _player.PlaySFX(clip, volume);
        }

        static public void Spin()
        {
            _spinChannel = _player.PlaySFX(SPIN);
        }

        static public void StopSpin()
        {
            if (_spinChannel != null) _spinChannel.Stop();
        }

        static public void Expect(AudioClip clip, float volume = 1f)
        {
            StopSpin();
            _player.PlaySFX(clip, volume);
        }

        static public void ReelStop()
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

        static public void FreeSpin()
        {
            // _player.PlaySFX(spins[0]);
        }

        static public void PlayFreeSpinTrigger()
        {

        }

        static public void StopFreeSpinTrigger()
        {

        }

        static public void PlayFreeSpinReady()
        {

        }

        static public void StopFreeSpinReady()
        {

        }
    }
}
