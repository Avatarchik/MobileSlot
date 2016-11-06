﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ReelContainer : MonoBehaviour
{
    public float reelSpace;

    List<Reel> _reels;

    Transform _tf;

    void Awake()
    {
        _tf = transform;
    }

    void Start()
    {

    }

    public void CreateReels(Reel prefab)
    {
        int count = SlotConfig.Main.Column;
        _reels = new List<Reel>(count);

        for (var i = 0; i < count; ++i)
        {
            Reel reel = Instantiate(prefab) as Reel;
            reel.Column = i;
            reel.transform.SetParent(_tf);
            reel.transform.localPosition = Vector3.right * reelSpace * i;
        }
    }

    public void Spin()
    {
        int count = SlotConfig.Main.Column;
        for( var i = 0; i < count; ++i )
        {
            _reels[i].Spin();
        }
    }
}
