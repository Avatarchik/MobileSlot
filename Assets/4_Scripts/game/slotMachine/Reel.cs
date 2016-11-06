using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Reel : MonoBehaviour
{
    [SerializeField]
    GameObject _expectObject;

    int _column;

    SlotConfig _config;

    List<Symbol> _symbols;
    Transform _symbolContainer;

    void Awake()
    {
        _symbols = new List<Symbol>();
        _symbolContainer = transform.Find("symbols");

        if( _expectObject != null ) _expectObject.SetActive( false );
    }

    public void Initialize(SlotConfig config)
    {
        _config = config;

        CreateStartSymbols();
    }

    void CreateStartSymbols()
    {
        string[] names = new string[_config.Row];

        for (var r = 0; r < _config.Row; ++r)
        {
            var name = _config.Strips.GetStartSymbolAt(_column, r);
            names[r] = name;
        }

        SetSymbols(names);
    }

    public void SetSymbols(string[] names)
    {
        //null인경우
        //0.55, -0.55,-0.85,-1.95, -2.25
        //0.3, 0,-1.1.-1.4,-2.5

        float ypos = 0;

        if( names[0] == NullSymbol.EMPTY )
        {
            // ypos = -(_relativeConfig.SymbolSize.height + _relativeConfig.NullSymbolSize.height )* 0.5f;
            ypos = -( _config.ReelSize.height - (_config.SymbolSize.height + _config.NullSymbolSize.height * 2)) * 0.5f;
        }

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

            if( symbol is NullSymbol ) symbol.Area = _config.NullSymbolSize;
            else symbol.Area = _config.SymbolSize;

            _symbols.Add( symbol );
            
            ypos -= symbol.Height;
        }
    }

    public void Spin()
    {
        Debug.Log("Spin");
    }

    public int Column
    {
        get { return _column; }
        set
        {
            _column = value;
            gameObject.name = "Reel" + _column;
        }
    }
}
