using UnityEngine;
using System.Collections;

using Spine;
using Spine.Unity;
using Spine.Unity.Modules;

public class AnimControlSpine : AnimControl
{

    SkeletonAnimation _skeletonAnim;
    Spine.AnimationState _skeletonState;
    SkeletonData _skeletonData;
    SkeletonGhost _ghost;
    SkeletonRagdoll2D _ragdoll;

    override public void Init()
    {
        _skeletonAnim = GetComponentInChildren<SkeletonAnimation>();

        _skeletonState = _skeletonAnim.state;
        _skeletonState.Event += OnEvent;
        _skeletonState.Complete += OnComplete;

        _skeletonData = _skeletonState.Data.SkeletonData;

        _ghost = GetComponentInChildren<SkeletonGhost>();
        _ragdoll = GetComponentInChildren<SkeletonRagdoll2D>();
    }

    void OnComplete(Spine.TrackEntry trackEntry)
    {

    }

    void OnEvent(Spine.TrackEntry trackEntry, Spine.Event e)
    {
    }

    void OnAnimVX(int i, float f, string s)
    {
        //Controller.AddForceX(mFacing * f);
    }

    void OnAnimVY(int i, float f, string s)
    {
        //Controller.AddForceY(f);
    }

    void OnAnimGhosting(int i, float f, string s)
    {
        //GhostMode(i == 1 ? true : false, s, f);
    }

    void OnAnimStep(int i, float f, string s)
    {
        // Platform platform = Controller.State.StandingPlatform;
        //if (platform != null) platform.Tread(transform.position);
    }

    void OnAnimSound(int i, float f, string s)
    {
        // SoundManager.Instance.PlaySFX(s, 1f, 1, transform.position);
    }

    void OnAnimEffect(int i, float f, string s)
    {
        
    }

    override public void PlayAnimation(string animName, bool loop, int layerIndex = 0 )
    {
        Spine.Animation animation = _skeletonData.FindAnimation(animName);
        if (animation == null)
        {
            Debug.LogErrorFormat("'{0}' anim invalid in '{1}'", animName, gameObject.name );
            return;
        }

        _skeletonState.SetAnimation(layerIndex, animation, loop);
    }

    override public bool HasAnim(string animName, int layerIndex = 0 )
    {
        return _skeletonData.FindAnimation(animName) == null ? false : true;
    }

    TrackEntry GetCurrent(int layerIndex = 0)
    {
        return _skeletonState.GetCurrent(layerIndex);
    }

    override public void CurrentAnimationTimeScale( float timeScale )
    {
        GetCurrent (0).TimeScale = timeScale;
    }

    override public float CurrentAnimationDuration
    {
        get
        {
            return GetCurrent(0).Animation.Duration;
        }
    }
}
