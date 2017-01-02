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
        FSScatter,//FreeSpinScatter
        PGSVScatter,//ProgressiveScatter
        Empty
    }

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
}