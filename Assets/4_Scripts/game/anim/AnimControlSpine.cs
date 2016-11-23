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

    override public void Init()
    {
        _skeletonAnim = GetComponentInChildren<SkeletonAnimation>();

        _skeletonState = _skeletonAnim.state;
        _skeletonState.Event += OnEvent;
        _skeletonState.Complete += OnComplete;

        _skeletonData = _skeletonState.Data.SkeletonData;
    }

    void OnComplete(Spine.TrackEntry trackEntry)
    {

    }

    void OnEvent(Spine.TrackEntry trackEntry, Spine.Event e)
    {

    }

    void OnAnimSound(int i, float f, string s)
    {
        //play sound
    }

    override public void PlayAnimation(string animName, bool loop = false, int layerIndex = 0)
    {
        Spine.Animation animation = _skeletonData.FindAnimation(animName);
        if (animation == null)
        {
            Debug.LogErrorFormat("'{0}' anim invalid in '{1}'", animName, gameObject.name);
            return;
        }

        _skeletonState.SetAnimation(layerIndex, animation, loop);
    }

    override public bool HasAnim(string animName, int layerIndex = 0)
    {
        return _skeletonData.FindAnimation(animName) == null ? false : true;
    }

    TrackEntry GetCurrent(int layerIndex = 0)
    {
        return _skeletonState.GetCurrent(layerIndex);
    }

    override public void CurrentAnimationTimeScale(float timeScale)
    {
        GetCurrent(0).TimeScale = timeScale;
    }

    override public float CurrentAnimationDuration
    {
        get
        {
            return GetCurrent(0).Animation.Duration;
        }
    }
}
