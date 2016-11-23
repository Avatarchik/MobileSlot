using UnityEngine;
using UnityEditor.Animations;
using System.Collections.Generic;

public class AnimControlAnimator : AnimControl
{
    Animator mAnim;

    Dictionary<string, AnimationClip> mMap;

    AnimationClip mCurrentClip;

    void Awake()
    {
        mMap = new Dictionary<string, AnimationClip>();

        mAnim = GetComponent<Animator>();

        AnimatorController ac = mAnim.runtimeAnimatorController as AnimatorController;
        ChildAnimatorState[] stateArr = ac.layers[0].stateMachine.states;

        foreach (var child in stateArr)
        {
            if (child.state == null) continue;
            var clip = child.state.motion as AnimationClip;

            mMap.Add(child.state.name, clip);
        }
    }

    override public void Init()
    {

    }

    override public void PlayAnimation(string animName, bool loop = false, int layerIndex = 0)
    {
        if (HasAnim(animName, layerIndex) == false)
        {
            Debug.LogErrorFormat("'{0}' anim invalid in '{1}'", animName, gameObject.name);
            return;
        }

        mAnim.Play(animName, layerIndex);
        mCurrentClip = mMap[animName];

        //Debug.Log("state:" + animName + ", clip:" + CurrentAnimationClipName + ", t: " + CurrentAnimationDuration);
    }

    override public bool HasAnim(string animName, int layerIndex = 0)
    {
        return mMap.ContainsKey(animName);
        /*
        var stateID = Animator.StringToHash(animName);
        return mAnim.HasState(0, stateID);
        */
    }

    string CurrentAnimationClipName
    {
        get
        {
            if (mCurrentClip == null) return "null";
            else return mCurrentClip.name;
        }
    }

    override public void CurrentAnimationTimeScale(float timeScale)
    {
        mAnim.speed = timeScale;
    }

    override public float CurrentAnimationDuration
    {
        get
        {
            if (mCurrentClip == null) return 0f;
            else return mCurrentClip.length;
        }
    }
}
