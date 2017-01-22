﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/*
<require ver>
5.4 이상

< namingConvention >
private,protected 멤버는 _로 시작
public 멤버는 소문자로 시작
Method 와 Property 는 대문자

Event 는 On 접두사를 붙이자
Event 에 추가된 Delegate 는 Listener 접미사를 붙이자

Coroutine 에서 사용되는 IEnumerator 는 Routine 접미사를 붙이자

//todo
머신 이하 모듈들 통합.
config.Main 접근 조금 더 보기 좋게
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
        public const string CABINET = "Cabinet";
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

    static public bool IsDebug()
    {
        // if( Application.isEditor ) return true;
        var res = false;
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        res = true;
#endif
        return res;
    }
}