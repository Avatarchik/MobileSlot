using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SmartToggleUI : Toggle
{
    Transform _targetTF;
    Vector3 _originScale;
    protected override void Start()
    {
        base.Start();

        toggleTransition = ToggleTransition.None;

        _targetTF = targetGraphic.transform; ;
        _originScale = targetGraphic.transform.localScale;

        onValueChanged.AddListener(DisableTargetGraphic);
    }

    void DisableTargetGraphic(bool b)
    {
        if (b) _targetTF.localScale = Vector3.zero;
        else _targetTF.localScale = _originScale;
    }
}