using UnityEngine;
using System;
using System.Collections;
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
    public class SoundGroup : SoundSchema, IEnumerable
    {
        static public string SEPARATOR = "/";
        public enum PlayType
        {
            Random,
            Order,
            CHOOSE
        }

        public PlayType type;
        [SerializeField]
        List<SoundSchema> _sounds;

        int _count;
        int _orderInex;
        List<SoundSchema> _organizedSounds;

        public List<SoundSchema> Sounds { get { return _organizedSounds; } }

        override public AudioClip Clip
        {
            get
            {
                if (_organizedSounds == null || _count == 0) return null;

                SoundSchema schema = null;

                if (_count == 1)
                {
                    schema = _organizedSounds[0];
                }
                else
                {
                    switch (type)
                    {
                        case PlayType.CHOOSE:
                            //recommend Choose Method
                            schema = _organizedSounds[0];
                            break;

                        case PlayType.Random:
                            var ranIndex = (int)UnityEngine.Random.Range(0, _count);
                            schema = _organizedSounds[ranIndex];
                            break;

                        case PlayType.Order:
                            schema = _organizedSounds[_orderInex];

                            Debug.Log("order: " + _orderInex + ", name: " + schema.Name + ",( group:" + Name + ")");

                            if (++_orderInex >= _count) _orderInex = 0;
                            break;
                    }
                }

                if (schema == null) return null;
                else return schema.Clip;
            }
        }

        public SoundGroup(string name) : base(name)
        {
            this.type = PlayType.CHOOSE;
            this._sounds = new List<SoundSchema>();
        }

        public SoundGroup(string name, PlayType type, int count) : base(name)
        {
            this.type = type;
            this._sounds = new List<SoundSchema>();
            for (var i = 0; i < count; ++i)
            {
                Add(new SoundSchema(i.ToString()));
            }
        }

        public SoundGroup(string name, PlayType type, string[] soundNames) : base(name)
        {
            this.type = type;
            this._sounds = new List<SoundSchema>();
            Add(soundNames);
        }

        public void Add(string[] soundNames)
        {
            for (var i = 0; i < soundNames.Length; ++i)
            {
                Add(soundNames[i]);
            }
        }

        public void Add(string soundName)
        {
            Add(new SoundSchema(soundName));
        }

        public void Add(SoundSchema schema)
        {
            _sounds.Add(schema);
        }

        public void Organize()
        {
            _orderInex = 0;
            _organizedSounds = new List<SoundSchema>();

            foreach (var sound in _sounds)
            {
                if (sound.Clip == null) continue;

                _organizedSounds.Add(sound);
            }

            _count = _organizedSounds.Count;
        }

        public AudioClip Choose(string name)
        {
            for (var i = 0; i < _count; ++i)
            {
                var sound = _organizedSounds[i];
                if (sound.Name == name) return sound.Clip;
            }

            return null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<SoundSchema> GetEnumerator()
        {
            for (int i = 0; i < _sounds.Count; ++i)
            {
                yield return _sounds[i];
            }
        }
    }

    public class SoundList : MonoBehaviour
    {
        public SoundGroup basic;
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
            basic = new SoundGroup("Basic");
            groups = new List<SoundGroup>();
        }

        public void Clear()
        {
            basic = new SoundGroup("Basic");
            groups.Clear();
        }
    }
}