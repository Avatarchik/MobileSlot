﻿using UnityEngine;
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
        public const string REEL_COVER = "ReelCover";
        public const string DEFAULT = "Default";
        public const string FOREGROUND = "GameForeGround";
        public const string PAYLINE = "Payline";
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