using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ReelContainer : MonoBehaviour
{
    public event Action OnReelStopComplete;

    SlotConfig _relativeConfig;
    
    List<Reel> _reels;

    Transform _tf;

    void Awake()
    {
        _tf = transform;

        Reel[] tempReels = GetComponentsInChildren<Reel>();
        if( tempReels != null && tempReels.Length > 0)
        {
            foreach( var r in tempReels )
            {
                Destroy( r.gameObject );
            }
        }
    }

    public void Initialize( SlotConfig config )
    {
        _relativeConfig = config;

        CreateReels();
    }

    void CreateReels()
    {
        if( _reels != null ) return;

        int count = _relativeConfig.Column;

        _reels = new List<Reel>(count);

        for (var i = 0; i < count; ++i)
        {
            Reel reel = Instantiate( _relativeConfig.ReelPrefab ) as Reel;
            reel.Column = i;

            reel.OnStop += OnStopListener;
            
            reel.transform.SetParent(_tf );
            reel.transform.localPosition = Vector3.right * _relativeConfig.ReelSpace * i;
            reel.Initialize( _relativeConfig );

            _reels.Add( reel );
        }
    }

    public void Spin()
    {
        for( var i = 0; i < _relativeConfig.Column; ++i )
        {
            _reels[i].Spin();
        }
    }

    public void ReceivedSymbol( ResDTO.Spin.Payout.SpinInfo spinInfo )
    {
        
        for( var i = 0; i < _relativeConfig.Column; ++i )
        {
            _reels[i].ReceivedSymbol( spinInfo );
        }
    }

    void OnStopListener( Reel reel )
    {
        Debug.Log("reel " + reel.Column + " stopped.");

        if( false )
        {
            ReelAllCompleted();
        }
    }

    void ReelAllCompleted()
    {
        if( OnReelStopComplete != null ) OnReelStopComplete();
    }
}
