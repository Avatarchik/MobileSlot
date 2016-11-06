﻿using UnityEngine;
using System;
using System.Collections;

[Serializable]
public struct Size2D
{
    public float width;
    public float height;

    public Size2D(float w, float h)
    {
        width = w;
        height = h;
    }
}