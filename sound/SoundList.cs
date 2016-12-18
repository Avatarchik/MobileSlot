using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace lpesign
{
    [Serializable]
    public class SoundSchema
    {
        public string name;
        public AudioClip clip;
    }

    [Serializable]
    public class SoundGroup
    {
        public enum PlayType
        {
            RANDOM,
            Loop
        }

        public string name;
        public PlayType type;
        public List<SoundSchema> sounds;
    }

    [Serializable]
    public class SoundList : MonoBehaviour
    {
        public SoundSchema[] basic;
        public SoundGroup[] categories;
    }
}