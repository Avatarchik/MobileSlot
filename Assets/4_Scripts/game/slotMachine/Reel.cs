using UnityEngine;
using System.Collections;

public class Reel : MonoBehaviour
{
    // public int column{get;set;}

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

    public string[,] startSymbolName;

    Transform _symbolContainer;
    void Awake()
    {
        _symbolContainer = transform.Find("symbols");
    }

    IEnumerator Start()
    {
        ReelStrips strips = SlotConfig.Main.Strips;
        int row = SlotConfig.Main.Row;
        string[] names = new string[row];

        for (var r = 0; r < row; ++r)
        {
            var name = strips.GetStartSymbolAt(_column, r);
            names[r] = name;
        }

        yield return new WaitForSeconds(0.5f);
        
        SetSymbols(names);
    }

    public void SetSymbols(string[] names)
    {
        float ypos = 0;

        for (var i = 0; i < names.Length; ++i)
        {
            var symbolName = names[i];
            var pos = Vector3.down * 0.2f * i;
            var symbol = GamePool.Instance.SpawnSymbol(symbolName);

            if( symbol == null )
            {
                Debug.Log( symbolName + " was null" );
                continue;
            }

            symbol.SetParent( _symbolContainer );
            symbol.transform.localPosition = new Vector3(0f,ypos,0f);
            // ypos += symbol.Height;
            ypos -= SlotConfig.Main.SymbolRect.height;
        }
    }
}
