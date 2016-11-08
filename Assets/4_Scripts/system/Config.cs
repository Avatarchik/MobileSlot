﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

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


    [Header("Reel")]
    public Reel ReelPrefab;
    public Size2D ReelSize;
    public float ReelSpace;
    public int DummySymbolCount;
    public float ReelGap;

    public ReelStrips Strips;
}

public class ReelStrips
{
    Dictionary<string, string> _symbolmap;

    string[,] _startSymbolNames;
    string[,] _normalStrips;


    public ReelStrips()
    {

    }

    #region SymbolNameMap
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
    #endregion

    public void SetNormalStrips( string[,] normal )
    {
        _normalStrips = normal;
    }

    #region StartSymbol
    public void SetStartSymbols(string[,] startSymbolNames)
    {
        _startSymbolNames = startSymbolNames;
    }

    public string GetStartSymbolAt(int col, int row)
    {
        return _startSymbolNames[col, row];
    }
    #endregion

    

    public string GetRandom( int column )
    {
        int leng = _normalStrips.GetLength(1);
        int randomIndex = UnityEngine.Random.Range(0,leng);
        return _normalStrips[ column,randomIndex];
    }
}
