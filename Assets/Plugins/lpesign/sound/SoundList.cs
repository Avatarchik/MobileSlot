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
        public SoundSchema(string name)
        {
            this.name = name;
        }
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
        public SoundSchema[] sounds;

        public SoundGroup(string groupName)
        {
            this.name = groupName;
        }

        public SoundGroup(string groupName, int count)
        {
            this.name = groupName;

            sounds = new SoundSchema[ count ];
            for( var i = 0; i < count; ++i )
            {
                sounds[i] = new SoundSchema( i.ToString());
            }
        }

        public SoundGroup(string groupName, SoundSchema[] sounds)
        {
            this.name = groupName;
            this.sounds = sounds;
        }
    }

    public class SoundList : MonoBehaviour
    {
        public bool Initialized;
        public SoundSchema[] basic;
        public List<SoundGroup> groups;

        public void Initialize()
        {
            if (Initialized) return;
            CreateDefaultList();
            Initialized = true;
        }

        virtual public void CreateDefaultList()
        {
            basic = new SoundSchema[0];
            groups = new List<SoundGroup>();
        }

        public void Clear()
        {
            Initialized = false;
            basic = null;
            groups = null;
        }
    }
}