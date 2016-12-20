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
        protected AudioClip _clip;

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

        [SerializeField]
        SoundSchema[] _sounds;

        List<SoundSchema> _organizedSounds;

        public List<SoundSchema> Sounds { get { return _organizedSounds; } }

        override public AudioClip Clip
        {
            get
            {
                if (_organizedSounds == null || _organizedSounds.Count == 0) return null;

                Debug.Log( _name + " count: " + _organizedSounds.Count);

                SoundSchema schema = null;

                if (_organizedSounds.Count == 1)
                {
                    schema = _organizedSounds[0];
                }

                schema = _organizedSounds[0];

                if (schema == null) return null;
                else return schema.Clip;
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
            _sounds = new SoundSchema[count];
            for (var i = 0; i < count; ++i)
            {
                _sounds[i] = new SoundSchema(i.ToString());
            }
        }

        public SoundGroup(string name, string[] childs) : base(name)
        {
            _sounds = new SoundSchema[childs.Length];
            for (var i = 0; i < childs.Length; ++i)
            {
                _sounds[i] = new SoundSchema(childs[i]);
            }
        }

        public SoundGroup(string name, SoundSchema[] sounds) : base(name)
        {
            this._sounds = sounds;
        }

        public void Organize()
        {
            _organizedSounds = new List<SoundSchema>();

            foreach (var sound in _sounds)
            {
                if (sound.Clip == null) continue;

                _organizedSounds.Add(sound);
            }
        }
    }

    public class SoundList : MonoBehaviour
    {
        public List<SoundSchema> basic;
        public List<SoundGroup> groups;

        public void Organize()
        {
            foreach (var soundGroup in groups)
            {
                soundGroup.Organize();
            }
        }

        virtual public void CreateDefaultList()
        {
            basic = new List<SoundSchema>();
            groups = new List<SoundGroup>();
        }

        public void Clear()
        {
            basic.Clear();
            groups.Clear();
        }
    }
}