using UnityEngine;
using System.Collections;

public class Payline : MonoBehaviour
{
    [SerializeField]
    int _lineIndex;

    public int LineIndex { get { return _lineIndex; } }

    SpriteRenderer _renderer;

    void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();

        if (_renderer != null)
        {
            _renderer.sortingLayerName = Layers.Sorting.PAYLINE;
            _renderer.sortingOrder = _lineIndex;
        }
    }

    void Start()
    {
        Hide();
    }

    public void Show()
    {
        _renderer.enabled = true;
    }

    public void Hide()
    {
        _renderer.enabled = false;
    }
}
