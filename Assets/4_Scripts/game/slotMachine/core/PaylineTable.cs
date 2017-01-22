using UnityEngine;
using System;
using System.Collections.Generic;

namespace Game
{
    //------------------------------------------------------------------
    //PaylineTable
    //------------------------------------------------------------------
    [Serializable]
    public class PaylineTable
    {
        [SerializeField]
        List<Payline> _table;

        public int Count { get { return _table.Count; } }

        public PaylineTable(int length)
        {
            _table = new List<Payline>();
        }

        public PaylineTable(int[][] tableArr)
        {
            _table = new List<Payline>();

            var length = tableArr.Length;
            for (var i = 0; i < length; ++i)
            {
                int[] rows = tableArr[i];
                AddPayline(rows);
            }
        }

        public Payline GetPayline(int line)
        {
            return _table[line];
        }

        public void AddPayline(int[] rows)
        {
            AddPayline(new Payline(rows));
        }

        public void AddPayline(Payline payline)
        {
            _table.Add(payline);
        }

        [Serializable]
        public class Payline
        {
            public int[] rows;

            public Payline(int[] rows)
            {
                this.rows = rows;
            }
        }
    }
}