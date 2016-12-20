using UnityEngine;
using System;
using System.Collections.Generic;

namespace lpesign
{
    [Serializable]
    public class SoundSchema
    {
        [SerializeField]
        protected string _name;

        [SerializeField]
        AudioClip _clip;

        public string Name { get { return _name; } }
        virtual public AudioClip Clip { get { return _clip; } }

        public SoundSchema(string name)
        {
            _name = name;
        }
    }

    [Serializable]
    public class SoundGroup : SoundSchema
    {
        static public string SEPARATOR = "/";
        public enum PlayType
        {
            Random,
            Order
        }

        public PlayType type;
        public SoundSchema[] sounds;

        override public AudioClip Clip
        {
            get
            {
                return null;
            }
        }

        public SoundGroup(string name) : base(name)
        {
            /*
            var relativeCategory = _groupMap[categoryName];
            var ranidx = (int)Random.Range(0, relativeCategory.sounds.Length);
            sound = relativeCategory.sounds[ranidx];
            return sound;
            */
        }

        public SoundGroup(string name, int count) : base(name)
        {
            sounds = new SoundSchema[count];
            for (var i = 0; i < count; ++i)
            {
                sounds[i] = new SoundSchema(i.ToString());
            }
        }

        public SoundGroup(string name, string[] childs) : base(name)
        {
            sounds = new SoundSchema[childs.Length];
            for (var i = 0; i < childs.Length; ++i)
            {
                sounds[i] = new SoundSchema(childs[i]);
            }
        }

        public SoundGroup(string name, SoundSchema[] sounds) : base(name)
        {
            this.sounds = sounds;
        }
    }

    public class SoundList : MonoBehaviour
    {
        public bool Initialized;
        public List<SoundSchema> basic;
        public List<SoundGroup> groups;

        public void Initialize()
        {
            if (Initialized) return;
            CreateDefaultList();
            Initialized = true;
        }

        virtual public void CreateDefaultList()
        {
            basic = new List<SoundSchema>();
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