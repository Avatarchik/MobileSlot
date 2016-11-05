using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ReelStrips
{
    protected Dictionary<string,string> _symbolmap;
    protected string[,] _startSymbolName;

    public ReelStrips()
    {

    }
    
    public void AddSymbolToMap(string serverName, string clientName)
    {
        if (_symbolmap == null) _symbolmap = new Dictionary<string, string>();

        _symbolmap[serverName] = clientName;
    }

    public string GetSymbolName(string serverName)
    {
        if (_symbolmap.ContainsKey(serverName)) return _symbolmap[serverName];
        else return string.Empty;
    }

    public string GetStartSymbolAt( int col, int row )
    {
        return _startSymbolName[col,row];
    }
}
