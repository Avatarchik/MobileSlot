using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Game
{
    public class WinTableModule : MonoBehaviour
    {
        [SerializeField]
        WinTableItem[] _items;
        void Awake()
        {
            _items = GetComponentsInChildren<WinTableItem>().OrderBy(p => p.Index).ToArray();
        }

        public void PlayAllWin(WinItemList info)
        {
            ActiveWins(info.WinTablesIndices);

        }

        public void PlayEachWin(WinItemList.Item item)
        {
            ActiveWin(item.WinTable.Value);
        }

        void ActiveWin(int index)
        {
            for (var i = 0; i < _items.Length; ++i)
            {
                var item = _items[i];
                if (i == index) item.Active();
                else item.Deactive();
            }
        }

        void ActiveWins(List<int> indices)
        {
            for (var i = 0; i < _items.Length; ++i)
            {
                var item = _items[i];
                if (indices.Contains(i)) item.Active();
                else item.Deactive();
            }
        }

        public void Clear()
        {
            for (var i = 0; i < _items.Length; ++i)
            {
                _items[i].Default();
            }
        }
    }
}