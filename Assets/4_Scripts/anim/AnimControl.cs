using UnityEngine;


public abstract class AnimControl : MonoBehaviour
{
    virtual public void Init()
    {

    }

    virtual public void PlayAnimation(string animName, bool loop = false, int layerIndex = 0)
    {
    }

    virtual public bool HasAnim(string animName, int layerIndex = 0)
    {
        return false;
    }

    virtual public void CurrentAnimationTimeScale(float timeScale)
    {

    }

    virtual public float CurrentAnimationDuration
    {
        get
        {
            return 0f;
        }
    }
}
