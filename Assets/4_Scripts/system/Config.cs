using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Layers
{
    public class Physics
    {
        public const string DEFAULT = "Default";
        public const string UI = "UI";
        public const string LOADING = "Loading";
    }

    public class Sorting
    {
        public const string BACKGROUND = "GameBackground";
        public const string REEL = "Reel";
        public const string SYMBOL = "Symbol";
        public const string DEFAULT = "Default";
        public const string FOREGROUND = "GameForeGround";
        public const string WIN = "WinLayer";
        public const string TOPBOARD = "Topboard";
        public const string UI = "UI";
    }
}

public class GlobalConfig
{
    static public int TargetFrameRate = 60;
    static public int ReferenceWidth = 1334;
    static public int ReferenceHeight = 750;
    static public int PixelPerUnit = 100;
}

[Serializable]
public class SlotConfig
{
    //------------------------------------------------------------------
    //게임 전반적인 설정
    //------------------------------------------------------------------
    public enum FreeSpinRetriggerType
    {
        Add,
        Rollback,
    }


    static public int ID;
    static public string Host;
    static public int Port;
    static public string Version;

    //------------------------------------------------------------------
    //게임 내 슬롯 설정 ( 게임 속 다수의 슬롯 머신이 존재할 수 도 있다 )
    //------------------------------------------------------------------
    public int Row;
    public int Column;

    public Size2D SymbolSize;
    public Size2D NullSymbolSize;

    public FreeSpinRetriggerType RetriggerType;

    [Header("Reel")]
    public Reel ReelPrefab;
    public Size2D ReelSize;
    public float ReelSpace;
    public float ReelGap;

    public int DummySymbolCount;

    public int SpiningSymbolCount;
    public int SpinCountThreshold;
    public float SpinSpeedPerSec;

    public SymbolNameMap NameMap;

    //startSymbols
    string[,] _startSymbolNames;
    public void SetStartSymbols(string[,] startSymbolNames)
    {
        _startSymbolNames = startSymbolNames;
    }

    public string GetStartSymbolAt(int col, int row)
    {
        return _startSymbolNames[col, row];
    }

    //reelStrip
    ReelStrip _normalStrip;
    ReelStrip _freeStrip;

    public ReelStrip NormalStrip
    {
        set { _normalStrip = value; }
        get { return _normalStrip; }
    }

    public ReelStrip FreeStrip
    {
        set { _freeStrip = value; }
        get { return _freeStrip ?? _normalStrip; }
    }
}

public class SymbolNameMap
{
    Dictionary<string, string> _symbolmap;
    public SymbolNameMap()
    {

    }

    public void AddSymbolToMap(string serverName, string clientName)
    {
        if (_symbolmap == null) _symbolmap = new Dictionary<string, string>();

        _symbolmap[serverName] = clientName;
    }

    public string GetSymbolName(string serverName)
    {
        if (_symbolmap.ContainsKey(serverName)) return _symbolmap[serverName];
        else return string.Empty;
    }
}

public class ReelStrip
{

    string[,] _strip;

    public ReelStrip(string[,] strip)
    {
        _strip = strip;
    }

    public string GetRandom(int column)
    {
        int leng = _strip.GetLength(1);
        int randomIndex = UnityEngine.Random.Range(0, leng);
        return _strip[column, randomIndex];
    }
}
