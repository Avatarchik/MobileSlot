using UnityEngine;
using System.Collections.Generic;

public class AnimControlAnimator : AnimControl
{
    Animator _anim;

    Dictionary<string, AnimationClip> _map;

    AnimationClip _currentClip;

    void Awake()
    {
        _anim = GetComponent<Animator>();
        _map = new Dictionary<string, AnimationClip>();
        //todo
        //find clip map
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

        _anim.Play(animName, layerIndex);
        _currentClip = _map[animName];

        //Debug.Log("state:" + animName + ", clip:" + CurrentAnimationClipName + ", t: " + CurrentAnimationDuration);
    }

    override public bool HasAnim(string animName, int layerIndex = 0)
    {
        return _map.ContainsKey(animName);
        /*
        var stateID = Animator.StringToHash(animName);
        return mAnim.HasState(0, stateID);
        */
    }

    string CurrentAnimationClipName
    {
        get
        {
            if (_currentClip == null) return "null";
            else return _currentClip.name;
        }
    }

    override public void CurrentAnimationTimeScale(float timeScale)
    {
        _anim.speed = timeScale;
    }

    override public float CurrentAnimationDuration
    {
        get
        {
            if (_currentClip == null) return 0f;
            else return _currentClip.length;
        }
    }
}
