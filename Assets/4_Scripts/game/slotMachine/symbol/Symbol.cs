using UnityEngine;
using System.Collections;

public abstract class Symbol : MonoBehaviour
{
    public Size2D Area { get; private set; }
    public string SymbolName { get; private set; }
    public bool Initialized { get; private set; }

    Transform _tf;
    Transform _content;

    void Awake()
    {
        _tf = transform;
        _content = _tf.Find("content");
    }

    public void Initialize(string symbolName, Size2D areaSize)
    {
        Area = areaSize;
        SymbolName = symbolName;
        Initialized = true;
    }

    void Start()
    {
        _content.localPosition = new Vector3(Width * 0.5f, Height * -0.5f, 0f);
    }

    public void SetParent(Transform parent, float ypos, bool asFirst = false )
    {
        _tf.SetParent(parent);
        _tf.localPosition = new Vector3(0f, ypos, 0f);
        if( asFirst ) _tf.SetAsFirstSibling();
    }

    public void Clear()
    {
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
    }
}
