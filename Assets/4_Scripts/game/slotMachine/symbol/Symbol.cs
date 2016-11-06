using UnityEngine;
using System.Collections;

public abstract class Symbol : MonoBehaviour
{
    Size2D? _symbolArea;
    Transform _tf;
    Transform _content;

    void Awake()
    {
        _tf = transform;
        _content = _tf.Find("content");
    }

    void Start()
    {
        _content.localPosition = new Vector3( Width * 0.5f, Height * -0.5f, 0f);
    }

    public void SetParent( Transform parent )
    {
        _tf.SetParent( parent );
    }

    public void Clear()
    {
        GamePool.Instance.DespawnSymbol( this );
    }

    public float Width
    {
        get{ return Area.width; }
    }

    public float Height
    {
        get{ return Area.height; }
    }

    public Size2D Area {get;set;}
}
