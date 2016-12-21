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
            Order,
            CHOOSE
        }

        public PlayType type;
        [SerializeField]
        SoundSchema[] _sounds;

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

        public AudioClip Choose(string name)
        {
            for (var i = 0; i < _count; ++i)
            {
                var sound = _organizedSounds[i];
                if (sound.Name == name) return sound.Clip;
            }

            return null;
        }

        public SoundGroup(string name) : base(name)
        {

        }

        public SoundGroup(string name, PlayType type, int count) : base(name)
        {
            this.type = type;
            _sounds = new SoundSchema[count];
            for (var i = 0; i < count; ++i)
            {
                _sounds[i] = new SoundSchema(i.ToString());
            }
        }

        public SoundGroup(string name, PlayType type, string[] childs) : base(name)
        {
            this.type = type;
            _sounds = new SoundSchema[childs.Length];
            for (var i = 0; i < childs.Length; ++i)
            {
                _sounds[i] = new SoundSchema(childs[i]);
            }
        }

        public SoundGroup(string name, PlayType type, SoundSchema[] sounds) : base(name)
        {
            this.type = type;
            this._sounds = sounds;
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