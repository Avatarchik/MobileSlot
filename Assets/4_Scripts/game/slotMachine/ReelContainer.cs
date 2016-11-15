using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ReelContainer : MonoBehaviour
{
    public event Action OnReelStopComplete;

    SlotConfig _config;
    
    List<Reel> _reels;

    Transform _tf;

    int _nextStopIndex;

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
        _config = config;

        CreateReels();
    }

    void CreateReels()
    {
        if( _reels != null ) return;

        int count = _config.Column;

        _reels = new List<Reel>(count);

        for (var i = 0; i < count; ++i)
        {
            Reel reel = Instantiate( _config.ReelPrefab ) as Reel;
            reel.Column = i;

            reel.OnStop += OnStopListener;
            
            reel.transform.SetParent(_tf );
            reel.transform.localPosition = Vector3.right * _config.ReelSpace * i;
            reel.Initialize( _config );

            _reels.Add( reel );
        }
    }

    public void Spin()
    {
        _nextStopIndex = 0;

        for( var i = 0; i < _config.Column; ++i )
        {
            _reels[i].Spin();
        }
    }

    public void ReceivedSymbol( ResDTO.Spin.Payout.SpinInfo spinInfo )
    {
        
        for( var i = 0; i < _config.Column; ++i )
        {
            _reels[i].ReceivedSymbol( spinInfo );
        }
    }

    
    void OnStopListener( Reel reel )
    {
        ++_nextStopIndex;

        if( _nextStopIndex < _config.Column )
        {
            CheckNextReel();
        }
        else
        {
            ReelAllCompleted();
        }
    }

    void CheckNextReel()
    {
        //다음 릴이 lock 이 걸렸는지
        //고조를 해야 하는지 등등 처리
        Debug.Log("checkNextReel");
    }

    void ReelAllCompleted()
    {
        if( OnReelStopComplete != null ) OnReelStopComplete();
    }
}
