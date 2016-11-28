using UnityEngine;
using System.Collections;

public abstract class ActiveItem : MonoBehaviour
{
    enum State
    {
        Default,
        Active,
        Deactive
    }

    [SerializeField]
    int _index;
    public int Index { get { return _index; } }
    protected Animator _anim;

    State _currentState;

    void Awake()
    {
        _anim = GetComponent<Animator>();
    }

    void UpdateStatus(State state)
    {
        if( _currentState == state ) return;

        _currentState = state;

        if (_anim == null) return;

        var name = _currentState.ToString();
        if (_anim.HasParameterOfType(name, AnimatorControllerParameterType.Trigger) == false) return;

        _anim.SetTrigger(name);
    }

    public void Default()
    {
        UpdateStatus(State.Default);
    }

    public void Active()
    {
        UpdateStatus(State.Active);
    }

    public void Deactive()
    {
        UpdateStatus(State.Deactive);
    }
}
