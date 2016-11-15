using UnityEngine;
using System.Collections;

public abstract class Symbol : MonoBehaviour
{
    public Size2D Area { get; private set; }
    public string SymbolName { get; private set; }
    public bool IsInitialized { get; private set; }

    Transform _tf;
    Transform _content;
    SpriteRenderer _displayArea;

    virtual protected void Awake()
    {
        _tf = transform;
        _content = _tf.Find("content");
    }

    public void Initialize(string symbolName, Size2D areaSize, bool dipslayArea = false )
    {
        if (IsInitialized) return;

        IsInitialized = true;

        Area = areaSize;
        SymbolName = symbolName;

        if ( dipslayArea )
        {
            _displayArea = CreateDisplayArea();
        }
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

        renderer.color = ColorHSV.GetRandomColor(Random.Range(0.0f, 360f),0.75f);
        return renderer;
    }

    void Start()
    {
        _content.localPosition = new Vector3(Width * 0.5f, Height * -0.5f, 0f);
    }

    public void SetParent(Transform parent, float ypos, bool asFirst = false)
    {
        _tf.SetParent(parent);
        Y = ypos;

        if (asFirst) _tf.SetAsFirstSibling();
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
        set { _tf.localPosition = new Vector3(0f, value, 0f); }
    }
}
