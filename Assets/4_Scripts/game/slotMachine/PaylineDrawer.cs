using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PaylineDrawer : MonoBehaviour
{
    public Payline[] _paylines;
    public List<Payline> _drawnLines;
    void Awake()
    {
        _paylines = GetComponentsInChildren<Payline>().OrderBy(p => p.LineIndex).ToArray();
		foreach( var p in _paylines ) p.Hide();

        _drawnLines = new List<Payline>();
    }

    public void DrawAll(List<WinPayInfo> winInfos)
    {
        var count = winInfos.Count;
        for (var i = 0; i < count; ++i)
        {
            DrawLine(winInfos[i], true);
        }
    }

    void DrawLine(WinPayInfo info, bool drawOver = false)
    {
        if (info.PaylineIndex == null) return;

        if (drawOver == false) Clear();

        var payline = GetLine(info.PaylineIndex.Value);
        payline.Show();

        _drawnLines.Add(payline);
    }

    Payline GetLine(int idx)
    {
        if (idx < 0 || idx >= _paylines.Length) return null;
        else return _paylines[idx];
    }

    public void Clear()
    {
        var count = _drawnLines.Count;
        while (count-- > 0) _drawnLines[count].Hide();
        _drawnLines.Clear();
    }
}
