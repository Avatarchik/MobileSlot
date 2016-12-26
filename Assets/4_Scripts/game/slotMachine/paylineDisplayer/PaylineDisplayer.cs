using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Game
{
    public class PaylineDisplayer : MonoBehaviour
    {
        [SerializeField]
        PaylineRenderer[] _paylines;
        List<PaylineRenderer> _renderers;

        void Awake()
        {
            _paylines = GetComponentsInChildren<PaylineRenderer>().OrderBy(p => p.Index).ToArray();
            _renderers = new List<PaylineRenderer>();
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

            _renderers.Add(payline);
        }

        PaylineRenderer GetLine(int idx)
        {
            if (idx < 0 || idx >= _paylines.Length) return null;
            else return _paylines[idx];
        }

        public void Clear()
        {
            var count = _renderers.Count;
            while (count-- > 0) _renderers[count].Hide();
            _renderers.Clear();
        }
    }
}

