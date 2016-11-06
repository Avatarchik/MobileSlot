using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ReelContainer : MonoBehaviour
{
    public float reelSpace;

    SlotConfig _relativeConfig;

    List<Reel> _reels;

    Transform _tf;

    void Awake()
    {
        _tf = transform;
    }

    public void Initialize( SlotConfig config )
    {
        _relativeConfig = config;

        CreateReels();
    }

    public void CreateReels()
    {
        if( _reels != null ) return;

        int count = _relativeConfig.Column;

        _reels = new List<Reel>(count);

        for (var i = 0; i < count; ++i)
        {
            Reel reel = Instantiate( _relativeConfig.ReelPrefab ) as Reel;
            reel.Column = i;
            reel.transform.SetParent(_tf);
            reel.transform.localPosition = Vector3.right * reelSpace * i;
            reel.CreateStartSymbols();
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
