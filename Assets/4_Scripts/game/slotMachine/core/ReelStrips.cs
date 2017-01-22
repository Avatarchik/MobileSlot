using UnityEngine;
using System;
using System.Collections.Generic;

using lpesign;

namespace Game
{
    //------------------------------------------------------------------
    //ReelStripsBundle
    //------------------------------------------------------------------
    [Serializable]
    public class ReelStripsBundle
    {
        [Serializable]
        public class ReelStripsMap : SerializableDictionaryBase<Group, ReelStrips> {}
        public enum Group
        {
            Default,
            Free,
            A,
            B,
            C
        }

        [SerializeField]
        ReelStripsMap _map;

        [SerializeField]
        Group _currentGroup;

        [SerializeField]
        List<Group> _containsGroup;

        public ReelStripsBundle()
        {
            _map = new ReelStripsMap();
            _containsGroup = new List<Group>();
        }

        public ReelStripsBundle(string[][] defaultstrips, ReelStrips.Type stripsType = ReelStrips.Type.Normal) : base()
        {
            AddStrips(Group.Default, defaultstrips, stripsType);
        }

        public void AddStrips(Group groupName, string[][] symbols, ReelStrips.Type stripsType = ReelStrips.Type.Normal)
        {
            ReelStrips reelStrips = new ReelStrips(symbols, stripsType);
            _map[groupName] = reelStrips;

            _containsGroup.Add(groupName);
        }

        public void RemoveStrips(Group groupName)
        {
            if (_map.ContainsKey(groupName) == false) return;

            _map.Remove(groupName);
            _containsGroup.Remove(groupName);
        }

        public void SelectStrip(Group groupName)
        {
            _currentGroup = groupName;
        }

        public ReelStrips GetStrips()
        {
            return _map[_currentGroup];
        }

        public ReelStrips GetStrips(Group groupName)
        {
            SelectStrip(groupName);
            return GetStrips();
        }
    }
    //------------------------------------------------------------------
    //ReelStrips
    //------------------------------------------------------------------
    [Serializable]
    public class ReelStrips
    {
        public enum Type
        {
            Normal,
            Stack
        }

        public Type type;

        [SerializeField]
        SymbolNames[] _names;

        public ReelStrips(string[][] symbols, Type type = Type.Normal)
        {
            this.type = type;

            var count = symbols.Length;
            _names = new SymbolNames[count];
            for (var i = 0; i < count; ++i)
            {
                var strip = new SymbolNames(symbols[i]);
                _names[i] = strip;
            }
        }

        public string GetRandom(int column)
        {
            var strip = GetStripAt(column);

            switch (type)
            {
                case Type.Normal:
                    //customize
                    break;

                case Type.Stack:
                    //customize
                    break;
            }

            return strip.GetRandom();
        }

        public SymbolNames GetStripAt(int column)
        {
            return _names[column];
        }

        public int Length { get { return _names.Length; } }

    }

    [Serializable]
    public class SymbolNames
    {
        [SerializeField]
        string[] _names;

        public SymbolNames(int count)
        {
            Value = new string[count];
        }

        public SymbolNames(string[] symbolNames)
        {
            Value = symbolNames;
        }

        public string GetRandom()
        {
            int randomIndex = UnityEngine.Random.Range(0, _names.Length);
            return _names[randomIndex];
        }


        public string[] Value
        {
            get { return _names; }
            set { _names = value; }
        }

        public int Length { get { return _names.Length; } }

        public string this[int i]
        {
            get { return _names[i]; }
            set { _names[i] = value; }
        }
    }
}
