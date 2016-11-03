using UnityEngine;
using System.Collections;

public class Symbol : MonoBehaviour
{
    Transform _content;

    void Awake()
    {
        _content = transform.Find("content");
    }

    void Start()
    {
        _content.localPosition = new Vector3(SlotInfo.Main.SymbolRect.width * 0.5f, SlotInfo.Main.SymbolRect.height * -0.5f, 0f);
    }
}
