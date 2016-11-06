using UnityEngine;
using System.Collections;

public class Symbol : MonoBehaviour
{
    public float Height{ get;set;}
    
    Transform _tf;
    Transform _content;

    void Awake()
    {
        _tf = transform;
        _content = _tf.Find("content");
    }

    void Start()
    {
        _content.localPosition = new Vector3(SlotConfig.Main.SymbolRect.width * 0.5f, SlotConfig.Main.SymbolRect.height * -0.5f, 0f);
    }

    public void SetParent( Transform parent )
    {
        _tf.SetParent( parent );
    }

    public void Clear()
    {
        GamePool.Instance.DespawnSymbol( this );
    }
}
