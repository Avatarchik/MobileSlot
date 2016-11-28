using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PaylineDrawer : MonoBehaviour
{
    [SerializeField]
    Payline[] _paylines;
    List<Payline> _drawnLines;

    void Awake()
    {
        _paylines = GetComponentsInChildren<Payline>().OrderBy(p => p.Index).ToArray();
        _drawnLines = new List<Payline>();
    }

    public void DrawAll(WinItemList winInfo)
    {
        int count = winInfo.ItemCount;
        for (var i = 0; i < count; ++i)
        {
            DrawLine(winInfo.GetItem(i), true);
        }
    }

    public void DrawLine(WinItemList.Item item, bool drawOver = false)
    {
        if (item.PaylineIndex == null) return;

        if (drawOver == false) Clear();

        var payline = GetLine(item.PaylineIndex.Value);
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
