using UnityEngine;
using System.Collections;

namespace Game
{
    public enum SymbolType
    {
        Low,
        Middle,
        High,
        Wild,
        FreeSpinScatter,
        ProgressiveScatter,
        Empty
    }

    [System.Serializable]
    public struct SymbolDefine
    {
        public string serverName;
        public SymbolType symbolType;
        public Symbol prefab;
        public int buffer;
    }
}