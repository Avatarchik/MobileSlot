using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/*
< namingConvention >
Event 는 On 접두사를 붙이자
Event 에 추가된 Delegate 는 Handler 접미사를 붙이자

*/

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
        public const string PAYLINE = "Payline";
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



/// <summary>
/// 슬롯 게임에 대한 설정
/// 한 슬롯 게임 내부에 다수의 머신이 존재할 수 있다.
/// 모든 머신의 공통적인 설정과 개별 설정을 분리해서 정의한다.
/// </summary>
[Serializable]
public class SlotConfig
{
    public enum FreeSpinRetriggerType
    {
        Add,
        Rollback,
    }

    //------------------------------------------------------------------
    //게임 속 머신 공통 설정
    //------------------------------------------------------------------
    [Serializable]
    public class CommonConfig
    {
        public int ID;
        public string Host;
        public int Port;
        public string Version;
        public SlotBetting Betting;
    }

    //------------------------------------------------------------------
    //게임 속 머신 개별
    //------------------------------------------------------------------
    [Header("Global")]
    public CommonConfig COMMON;

    [Header("Base")]
    public int Row;
    public int Column;

    [Header("Symbol")]
    public Size2D SymbolSize;
    public Size2D NullSymbolSize;

    [Header("Options")]
    public FreeSpinRetriggerType RetriggerType;

    [Header("PaylineTable")]
    public int[][] paylineTable;

    [Header("Reel")]
    public Reel ReelPrefab;
    public Size2D ReelSize;
    public float ReelSpace;
    public float ReelGap;


    [Header("Spin")]
    public int DummySymbolCount;//릴 위아래 위치할 심볼 수
    public int IncreaseCount;//다음 릴로 갈 수록 더 생겨야할 심볼 수
    public int SpiningSymbolCount;//스핀 한 세트 당 심볼 수
    public int SpinCountThreshold;//서버가 응답이 빠르더라도 최소한 돌아야할 스핀 세트 수 
    public float SpinSpeedPerSec;//스핀 초당 속도
    public float DelayEachReel;//각 릴 사이의 스핀 시작 딜레이
    public MoveTweenInfo tweenFirstBackInfo;//첫번재 스핀에 정보
    public MoveTweenInfo tweenLastBackInfo;//마지막 스핀이 정보

    [Header("Transition")]
    public Transition transition;


    [Header("NameMap")]
    public SymbolNameMap NameMap;

    [Header("Debug")]
    public bool DebugSymbolArea;//심볼 영역을 표시할지 여부

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
    public enum ReelStripType
    {
        NORMAL,
        USE_NULL,
        STACK
    }

    public ReelStripType type;

    string[,] _strip;

    public ReelStrip(string[,] strip, ReelStripType type = ReelStripType.NORMAL)
    {
        _strip = strip;
        this.type = type;
    }

    public string GetRandom(int column)
    {
        int leng = _strip.GetLength(1);
        int randomIndex = UnityEngine.Random.Range(0, leng);
        return _strip[column, randomIndex];
    }
}

[Serializable]
public class Transition
{
    public float ReelStopCompleteAfterDealy = 0.5f;
    public float PlayAllSymbols_WinBalance = 0f;
    public float EachWin = 1f;
    public float EachWinSummary = 1f;
}
