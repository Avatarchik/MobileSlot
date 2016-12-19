using UnityEngine;
using System.Collections.Generic;

using lpesign;

namespace Game
{
    public class SlotSoundList : SoundList
    {
        //todo
        //현재 씬의 슬롯 config 을 얻어와야 한다.
        override public void CreateDefaultList()
        {
            int columnCount = 3;
            int progressiveCount = 5;
            bool useFreespin = false;

            basic = new SoundSchema[]
            {
                new SoundSchema("INTRO"),
                new SoundSchema("BGM"),
                new SoundSchema("BGM_FREE"),

                new SoundSchema("BTN_SPIN"),
                new SoundSchema("BTN_FAST"),
                new SoundSchema("BTN_DECREASE"),
                new SoundSchema("BTN_INCREASE"),
                new SoundSchema("BTN_COMMON"),

                new SoundSchema("FREESPIN_TRIGGER"),
                new SoundSchema("FREESPIN_READY"),
                new SoundSchema("FREESPIN_READY_LOOP"),
                new SoundSchema("FREESPIN_NUMBERING"),
                new SoundSchema("FREESPIN_COMPLETE"),
                new SoundSchema("FREESPIN_CONGRATULATION")
            };

            groups = new List<SoundGroup>()
            {
                new SoundGroup("SPIN",columnCount),
                new SoundGroup("REEL_STOP",columnCount),
                new SoundGroup("FREE_REEL_STOP",columnCount),
                new SoundGroup("SCATTER_STOP",columnCount),
                new SoundGroup("EXPECT", columnCount ),
                new SoundGroup("WIN_PROGRESSIVE", progressiveCount),
                new SoundGroup("WIN_NORMAL", new SoundSchema[]
                {
                    new SoundSchema("DEFAULT"),
                    new SoundSchema("END"),
                    new SoundSchema("JACKPOT"),
                    new SoundSchema("MEGAWIN"),
                    new SoundSchema("BIGWIN"),
                    new SoundSchema("JMB_END"),
                })
            };
        }
    }
}
