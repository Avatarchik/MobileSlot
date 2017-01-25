using UnityEngine;

using System;

using DG.Tweening;

[Serializable]
public struct Size2D
{
    public float width;
    public float height;
    public float HalfWidth{get; private set;}
    public float HalfHeight{get; private set;}
    public Size2D(float w, float h)
    {
        width = w;
        height = h;

        HalfWidth = width * 0.5f;
        HalfHeight = height * 0.5f;
    }

    override public string ToString()
    {
        return string.Format("w: {0} h:{1}",width,height);
    }
}

[Serializable]
public struct MoveTweenInfo
{
    public float distance;
    public float duration;

    public MoveTweenInfo( float distance, float duration )
    {
        this.distance = distance;
        this.duration = duration;
    }
}