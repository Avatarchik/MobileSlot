using UnityEngine;
using System.Collections;

public class Reel : MonoBehaviour
{
    int _column;
    public int Column
    {
        get { return _column; }
        set
        {
            _column = value;
            gameObject.name = "Reel" + _column;
        }
    }

    ReelStrips _strips;
    int _row;
    Transform _symbolContainer;

    void Awake()
    {
        _symbolContainer = transform.Find("symbols");
    }

    public void Initialize(SlotConfig config)
    {
        _strips = config.Strips;
        _row = config.Row;

        CreateStartSymbols();
    }

    void CreateStartSymbols()
    {
        string[] names = new string[_row];

        for (var r = 0; r < _row; ++r)
        {
            var name = _strips.GetStartSymbolAt(_column, r);
            names[r] = name;
        }

        SetSymbols(names);
    }

    public void SetSymbols(string[] names)
    {
        float ypos = 0;

        for (var i = 0; i < names.Length; ++i)
        {
            var symbolName = names[i];
            var symbol = GamePool.Instance.SpawnSymbol(symbolName);

            if (symbol == null)
            {
                Debug.Log(symbolName + " was null");
                continue;
            }

            symbol.SetParent(_symbolContainer);
            symbol.transform.localPosition = new Vector3(0f, ypos, 0f);
            // ypos += symbol.Height;
            ypos -= 200f;
        }
    }

    public void Spin()
    {

    }
}
