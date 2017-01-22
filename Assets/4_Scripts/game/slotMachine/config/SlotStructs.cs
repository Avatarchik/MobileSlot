using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

using lpesign;

namespace Game
{
    //------------------------------------------------------------------
    // Transition
    //------------------------------------------------------------------
    [Serializable]
    public struct Transition
    {
        [RangeAttribute(0f, 1f)]
        public float ReelStopAfterDelay;

        [RangeAttribute(0f, 1f)]
        public float PlaySymbolAfterDelay;

        [RangeAttribute(0.5f, 2f)]
        public float EachWin;
        [RangeAttribute(0.5f, 2f)]
        public float EachWinSummary;

        [RangeAttribute(1f, 2f)]
        public float EachLockReel;
        [RangeAttribute(1f, 2f)]
        public float LockReelAfterDelay;
        [RangeAttribute(1f, 2.5f)]
        public float FreeSpinTriggerDuration;
    }

    //------------------------------------------------------------------
    // SymbolDefine
    //------------------------------------------------------------------
    [System.Serializable]
    public struct SymbolDefine
    {
        public string symbolName;
        public SymbolType type;
        public Symbol prefab;
        public int buffer;

        public static bool IsScatter(SymbolType type)
        {
            return -1 != type.ToString().ToLower().IndexOf("scatter");
        }

        public static bool IsScatter(SymbolDefine symbolDefine)
        {
            return IsScatter(symbolDefine.type);
        }
    }

    //------------------------------------------------------------------
    // ScatterInfo
    //------------------------------------------------------------------
    [Serializable]
    public struct ScatterInfo
    {
        public SymbolType type;
        public int[] ableReel;
        public int maxCount;
        public int expectThreshold;
        public AudioClip expectSound;
        public AudioClip[] stopSounds;

        // int _index;

        public ScatterInfo(SymbolType scatterType, int maxCount, int expectThreshold, int[] ableReel)
        {
            this.type = scatterType;
            this.maxCount = maxCount;
            this.expectThreshold = expectThreshold;
            this.expectSound = null;
            this.ableReel = ableReel;
            this.stopSounds = new AudioClip[ableReel.Length];
        }

        // public void Reset()
        // {
        //     _index = 0;
        // }

        public bool CheckScattered(Reel reel, out AudioClip stopSound)
        {
            // Debug.Log("reel: " + reel.Column + " check ableReel: " + string.Join(",", ableReel.Select(x => x.ToString()).ToArray()));

            // if (Array.IndexOf(ableReel, reel.Column) == -1 ||
            //     reel.ContainsByName(symbolName) == false)
            // {
            //     stopSound = null;
            //     Debug.Log("no scatterd: " + Array.IndexOf(ableReel, reel.Column) + " : " + reel.ContainsByName(symbolName));
            //     return false;
            // }

            // Debug.Log(symbolName + " scattered");
            // stopSound = stopSounds[_index++];
            stopSound = null;
            return true;
        }
    }
}
