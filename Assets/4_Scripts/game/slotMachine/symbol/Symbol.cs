﻿using UnityEngine;
using System.Collections;

using DG.Tweening;

public abstract class Symbol : MonoBehaviour
{
    public enum SymbolState
    {
        Null,
        Idle,
        Spin,
        Stop,
        Scatter,
        Trigger,
        Win,
        Lock
    }

    public Size2D Area { get; private set; }
    public string SymbolName { get; private set; }
    public bool IsInitialized { get; private set; }

    protected SymbolState _currentState;

    Transform _tf;
    Transform _content;
    SpriteRenderer _sprite;

    AnimControl _anim;
    Sequence _fallbackAnim;
    Reel _relativeReel;

    virtual protected void Awake()
    {
        _tf = transform;
        _content = _tf.Find("content");
        _sprite = _tf.Find("content/sprite").GetComponent<SpriteRenderer>();
        if (_sprite != null) _sprite.sortingLayerName = Layers.Sorting.SYMBOL;

        _anim = GetComponentInChildren<AnimControl>();
    }

    virtual public void Initialize(string symbolName, SlotConfig config)
    {
        Initialize(symbolName, config.SymbolSize, config.DebugSymbolArea);
    }

    protected void Initialize(string symbolName, Size2D areaSize, bool dipslayArea = false)
    {
        if (IsInitialized) return;

        IsInitialized = true;

        Area = areaSize;
        SymbolName = symbolName;

        _content.localPosition = new Vector3(Width * 0.5f, Height * -0.5f, 0f);

        if (dipslayArea) CreateDisplayArea();
    }

    SpriteRenderer CreateDisplayArea()
    {
        var go = new GameObject("debugArea");
        go.transform.SetParent(_content);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = new Vector3(Area.width, Area.height, 0f);
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sortingLayerName = Layers.Sorting.SYMBOL;
        renderer.sortingOrder = -1;
        renderer.sprite = Resources.Load<Sprite>("textures/Square");

        renderer.color = ColorHSV.GetRandomColor(Random.Range(0.0f, 360f), 0.75f);
        return renderer;
    }

    void BringToFront()
    {
        _sprite.sortingLayerName = Layers.Sorting.WIN;
    }

    void BackFromFront()
    {
        _sprite.sortingLayerName = Layers.Sorting.SYMBOL;
    }

    public void SetParent(Reel reel, float ypos, bool asFirst = false)
    {
        _relativeReel = reel;
        _tf.SetParent(_relativeReel.SymbolContainer);
        Y = ypos;

        if (asFirst) _tf.SetAsFirstSibling();
    }

    virtual public void SetState(SymbolState nextState, bool useOverlap = true)
    {
        if (useOverlap == false && _currentState == nextState) return;

        _currentState = nextState;

        switch (_currentState)
        {
            case SymbolState.Null:
                Reset();
                break;

            case SymbolState.Idle:
                Idle();
                break;

            case SymbolState.Spin:
                Spin();
                break;

            case SymbolState.Stop:
                Stop();
                break;

            case SymbolState.Scatter:
                Scatter();
                break;

            case SymbolState.Trigger:
                Trigger();
                break;

            case SymbolState.Win:
                Win();
                break;

            case SymbolState.Lock:
                Lock();
                break;
        }
    }

    void Reset()
    {
        if (_fallbackAnim != null)
        {
            _fallbackAnim.Kill();
            _fallbackAnim = null;
        }

        _relativeReel = null;

        //stop anim
        // _content scale alpha ...etc reset
        // _sprite scale alpha ...etc reset
        // reset blur
    }

    void Idle()
    {

    }

    void Spin()
    {
        _sprite.sortingLayerName = Layers.Sorting.SYMBOL;
        _content.localScale = Vector3.one;
    }

    void Stop()
    {

    }

    void Scatter()
    {

    }

    void Trigger()
    {

    }

    void Win()
    {
        if (PlayAnimation("Win") == false) MotionScale();
    }

    void Lock()
    {

    }

    protected bool PlayAnimation(string animName, bool loop = true, int layerIndex = 0)
    {
        if (_anim == null || _anim.HasAnim(animName) == false) return false;

        _anim.PlayAnimation(animName);

        return true;
    }

    void MotionScale(float fromScale = 1.0f, float toScale = 1.25f, float duration = 0.3f, float interval = 0.2f)
    {
        _content.localScale = Vector3.one * fromScale;

        if (_fallbackAnim != null && _fallbackAnim.IsPlaying()) _fallbackAnim.Kill();

        _fallbackAnim = DOTween.Sequence();

        _fallbackAnim.Append(_content.DOScale(toScale, duration)
                                     .SetEase(Ease.OutCubic)
                                     .OnStart(BringToFront));

        _fallbackAnim.AppendInterval(interval);

        _fallbackAnim.Append(_content.DOScale(fromScale, duration)
                                     .SetEase(Ease.InCubic)
                                     .OnStart(BackFromFront));

        _fallbackAnim.Play();
    }

    public void Clear()
    {
        SetState(SymbolState.Null);
        GamePool.Instance.DespawnSymbol(this);
    }

    public float Width
    {
        get { return Area.width; }
    }

    public float Height
    {
        get { return Area.height; }
    }

    public float Y
    {
        get { return _tf.localPosition.y; }
        set { _tf.localPosition = new Vector3(0f, value, 0f); }
    }
}
