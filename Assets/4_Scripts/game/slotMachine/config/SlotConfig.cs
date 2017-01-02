using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

using lpesign;

namespace Game
{
    /// <summary>
    /// 슬롯 게임에 대한 설정
    /// 한 슬롯 게임 내부에 다수의 머신이 존재할 수 있다.
    /// 모든 머신의 공통적인 설정과 개별 설정을 분리해서 정의한다.
    /// </summary>
    [Serializable]
    [ExecuteInEditMode]
    public class SlotConfig : MonoBehaviour
    {
        //------------------------------------------------------------------
        //Slot 공통 설정
        //------------------------------------------------------------------
        new public string name;
        public int ID;
        public string Host;
        public int Port;
        public string Version;

        public bool Jackpot;
        public bool DebugSymbolArea;//심볼 영역을 표시할지 여부
        public bool DebugTestSpin;//테스트 스핀 핫 키 여부

        public SlotBetting Betting;

        [SerializeField]
        List<MachineConfig> _machineList;

        public List<MachineConfig> MachineList { get { return _machineList; } }
        public MachineConfig Main
        {
            get { return GetMachineAt(0); }
        }

        public void ClearMachines()
        {
            if (_machineList == null) _machineList = new List<MachineConfig>();

            _machineList.Clear();
        }

        public void AddMachine(MachineConfig machine)
        {
            if (_machineList == null) _machineList = new List<MachineConfig>();

            _machineList.Add(machine);
        }

        public void AddMachine()
        {
            if (_machineList == null) _machineList = new List<MachineConfig>();

            var machine = new MachineConfig(this);
            _machineList.Add(machine);
        }

        public MachineConfig GetMachineAt(int index)
        {
            if (_machineList == null || _machineList.Count == 0) return null;
            else return _machineList[index];
        }

        public void RemoveMachineAt(int index)
        {
            if (_machineList == null) return;

            _machineList.RemoveAt(index);
        }

        //------------------------------------------------------------------
        //게임 개별 머신
        //------------------------------------------------------------------
        [Serializable]
        public class MachineConfig
        {
            //------------------------------------------------------------
            //base
            //------------------------------------------------------------
            public int row;
            public int column;

            //------------------------------------------------------------
            //symbol
            //------------------------------------------------------------
            public Size2D SymbolSize;
            public bool useEmpty;
            public Size2D NullSymbolSize;

            [SerializeField]
            List<SymbolDefine> _symbolDefineList;
            public List<SymbolDefine> SymbolDefineList { get { return _symbolDefineList; } }
            public void ClearSymbolDefine()
            {
                if (_symbolDefineList != null) _symbolDefineList.Clear();
            }
            public void AddSymbolDefine(string symbolName, SymbolType type)
            {
                var buffer = row * column;

                AddSymbolDefine(symbolName, type, buffer);
            }

            public void AddSymbolDefine(string symbolName, SymbolType type, int buffer)
            {
                if (_symbolDefineList == null) _symbolDefineList = new List<SymbolDefine>();

                var define = new SymbolDefine();
                define.symbolName = symbolName;
                define.type = type;
                define.prefab = type == SymbolType.Empty ? GetEmptySymbolPrefab() : GetSymbolPrefab(RelativeSlotConfig.ID, symbolName);
                define.buffer = buffer;
                _symbolDefineList.Add(define);
            }

            protected Symbol GetSymbolPrefab(int gameID, string prefabName)
            {
                return Resources.Load<Symbol>("games/" + ConvertUtil.ToDigit(gameID) + "/prefabs/symbols/" + prefabName);
            }

            protected Symbol GetEmptySymbolPrefab()
            {
                return Resources.Load<Symbol>("games/common/prefabs/EM");
            }

            //startsymbols
            public ReelSymbolSet[] startSymbolNames;
            public void SetStartSymbols(string[][] startSymbolNames)
            {
                var len = startSymbolNames.Length;
                this.startSymbolNames = new ReelSymbolSet[len];

                for (var i = 0; i < startSymbolNames.Length; ++i)
                {
                    string[] symbolNames = startSymbolNames[i];
                    ReelSymbolSet symbolSet = new ReelSymbolSet(symbolNames);
                    this.startSymbolNames[i] = symbolSet;
                }
            }

            public string GetStartSymbolAt(int col, int row)
            {
                return startSymbolNames[col].GetNameAt(row);
            }

            //scatter
            [SerializeField]
            List<ScatterInfo> _scatters;
            public List<ScatterInfo> ScatterInfos { get { return _scatters; } }

            public void ClearScatterInfo()
            {
                if (_scatters != null) _scatters.Clear();
            }

            public void AddSccaterInfo(SymbolType scatterType, int limit, int[] ableReel)
            {
                var info = new ScatterInfo(scatterType, limit, ableReel);
                AddScatterInfo(info);
            }

            public void AddScatterInfo(ScatterInfo info)
            {
                if (_scatters == null) _scatters = new List<ScatterInfo>();

                _scatters.Add(info);
            }

            //------------------------------------------------------------
            //freespin
            //------------------------------------------------------------
            public bool UseFreeSpin;
            public FreeSpinTriggerType TriggerType;
            public FreeSpinRetriggerType RetriggerType;

            //------------------------------------------------------------
            //reel
            //------------------------------------------------------------
            public Size2D ReelSize;
            public float ReelSpace;
            public float ReelGap;
            public Reel ReelPrefab;

            //------------------------------------------------------------
            //spin
            //------------------------------------------------------------
            public float SpinSpeedPerSec;//스핀 초당 속도
            public int MarginSymbolCount;//릴 위아래 여유 심볼 수
            public int IncreaseCount;//다음 릴로 갈 수록 더 생겨야할 심볼 수
            public int SpiningSymbolCount;//스핀 한 세트 당 심볼 수
            public int SpinCountThreshold;//서버가 응답이 빠르더라도 최소한 돌아야할 스핀 세트 수 
            public float DelayEachReel;//각 릴 사이의 스핀 시작 딜레이
            public MoveTweenInfo tweenFirstBackInfo;//첫번재 스핀에 정보
            public MoveTweenInfo tweenLastBackInfo;//마지막 스핀이 정보

            //------------------------------------------------------------
            //transition
            //------------------------------------------------------------
            public Transition transition;

            //------------------------------------------------------------
            //paytable
            //------------------------------------------------------------
            public PaylineTable paylineTable;

            //------------------------------------------------------------
            //reelstrip
            //------------------------------------------------------------
            public ReelStripList reelStripBundle;

            public SlotConfig RelativeSlotConfig { get; set; }

            public MachineConfig(SlotConfig relativeSlotConfig)
            {
                RelativeSlotConfig = relativeSlotConfig;
            }
        }

        //------------------------------------------------------------------
        //custom enum
        //------------------------------------------------------------------
        #region customn enum
        public enum FreeSpinTriggerType
        {
            Auto,
            Select
        }

        public enum FreeSpinRetriggerType
        {
            None,
            Add,
            Refill
        }

        public enum ExpectReelType
        {
            Null,
            BonusSpin,
            FreeSpin,
            Progressive
        }

        public enum WinType
        {
            LOSE,
            NORMAL,
            BIGWIN,
            MEGAWIN,
            JACPOT
        }
        #endregion
    }


    [Serializable]
    public class ReelSymbolSet
    {
        public string[] symbolNames;
        public ReelSymbolSet(string[] symbolNames)
        {
            this.symbolNames = symbolNames;
        }

        public string GetNameAt(int row)
        {
            return symbolNames[row];
        }
    }

    [Serializable]
    public class PaylineTable
    {
        [SerializeField]
        Payline[] _table;

        public int Length { get { return _table.Length; } }

        public PaylineTable(int length)
        {
            _table = new Payline[length];
        }

        public PaylineTable(int[][] tableArr)
        {
            var length = tableArr.Length;
            _table = new Payline[length];

            for (var i = 0; i < length; ++i)
            {
                int[] rows = tableArr[i];
                _table[i] = new Payline(rows);
            }
        }

        public Payline GetPayline(int line)
        {
            return _table[line];
        }

        public void AddPayline(int[] payline)
        {

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



    //todo
    //symbol 정의를 내릴때 포함시키자
    [Serializable]
    public struct ScatterInfo
    {
        public SymbolType type;
        public int limit;
        public int[] ableReel;
        public AudioClip[] stopSounds;

        // int _index;

        public ScatterInfo(SymbolType scatterType, int limit, int[] ableReel)
        {
            this.type = scatterType;
            this.limit = limit;
            this.ableReel = ableReel;
            this.stopSounds = new AudioClip[ableReel.Length];
        }

        public ScatterInfo(SymbolType scatterType)
        {
            this.type = scatterType;
            this.limit = 0;
            this.ableReel = new int[0];
            this.stopSounds = new AudioClip[ableReel.Length];
        }

        // public void Reset()
        // {
        //     _index = 0;
        // }

        public bool CheckScattered(Reel reel, out AudioClip stopSound)
        {
            // Debug.Log("reel: " + reel.Column + " check ableReel: " + string.Join(",", ableReel.Select(x => x.ToString()).ToArray()));

            // if (Array.IndexOf(ableReel, reel.Column) == -1 ||
            //     reel.ContainsByName(symbolName) == false)
            // {
            //     stopSound = null;
            //     Debug.Log("no scatterd: " + Array.IndexOf(ableReel, reel.Column) + " : " + reel.ContainsByName(symbolName));
            //     return false;
            // }

            // Debug.Log(symbolName + " scattered");
            // stopSound = stopSounds[_index++];
            stopSound = null;
            return true;
        }
    }

    #region ReelStirp
    [Serializable]
    public class ReelStripList
    {
        public const string DEFAULT = "default";
        public const string FREE = "free";

        [SerializeField]
        List<ReelStrips> _list;

        public ReelStripList(string[][] defaultyStrips, ReelStrips.Type type = ReelStrips.Type.NORMAL)
        {
            _list = new List<ReelStrips>();

            AddStrip(DEFAULT, defaultyStrips, type);
        }

        public void AddStrip(string name, string[][] symbols, ReelStrips.Type type = ReelStrips.Type.NORMAL)
        {
            ReelStrips reelStrips = new ReelStrips(name, symbols, type);
            _list.Add(reelStrips);
        }

        public ReelStrips GetStrips(string key = DEFAULT)
        {
            var count = _list.Count;
            for (var i = 0; i < count; ++i)
            {
                var reelStrip = _list[i];
                if (reelStrip.name == key) return reelStrip;
            }

            return null;
        }
    }

    [Serializable]
    public class ReelStrips
    {
        public enum Type
        {
            NORMAL,
            STACK
        }

        public ReelStrips.Type type;
        public string name;

        [SerializeField]
        Strip[] _strips;

        public ReelStrips(string name, string[][] symbols, ReelStrips.Type type = ReelStrips.Type.NORMAL)
        {
            this.name = name;
            this.type = type;

            var count = symbols.Length;
            _strips = new Strip[count];
            for (var i = 0; i < count; ++i)
            {
                var strip = new Strip(symbols[i]);
                _strips[i] = strip;
            }
        }

        public string GetRandom(int column)
        {
            var strip = _strips[column];

            switch (type)
            {
                case Type.NORMAL:
                    //customize
                    break;

                case Type.STACK:
                    //customize
                    break;
            }

            return strip.GetRandom();
        }
    }

    [Serializable]
    public class Strip
    {
        [SerializeField]
        string[] _strip;
        public Strip(string[] strip)
        {
            _strip = strip;
        }

        public string GetRandom()
        {
            int randomIndex = UnityEngine.Random.Range(0, _strip.Length);
            return _strip[randomIndex];
        }
    }
    #endregion

    [Serializable]
    public class Transition
    {
        public float ReelStopCompleteAfterDealy = 0.2f;
        public float PlayAllSymbols_WinBalance = 0f;
        public float EachWin = 1f;
        public float EachWinSummary = 1f;

        public float EachLockReel = 1f;
        public float LockReel_BonusSpin = 1f;
        public float FreeSpinTriggerDuration = 1f;
    }

    [System.Serializable]
    public class SlotBetting
    {
        public event Action OnUpdateLineBetIndex;

        double _min;
        double _max;
        int _currentIndex;

        [SerializeField]
        double _lastLineBet;
        [SerializeField]
        double _currentLineBet;
        [SerializeField]
        double[] _betTable;


        #region Property

        public double[] BetTable { set { _betTable = value; } }

        public int PaylineNum { get; set; }

        public int LineBetIndex
        {
            get { return _currentIndex; }
            set
            {
                _currentIndex = value;

                if (_currentIndex < 0) _currentIndex = 0;
                else if (_currentIndex >= _betTable.Length) _currentIndex = _betTable.Length - 1;

                _currentLineBet = _betTable[_currentIndex];

                if (OnUpdateLineBetIndex != null) OnUpdateLineBetIndex();
            }
        }

        public double LineBet
        {
            get { return _currentLineBet; }
        }

        public double TotalBet
        {
            get { return LineBet * PaylineNum; }
        }

        public bool IsFirstBet
        {
            get { return _currentIndex <= 0; }
        }

        public bool IsLastBet
        {
            get { return _currentIndex >= _betTable.Length - 1; }
        }

        #endregion


        #region API
        public void Decrease()
        {
            if (IsFirstBet) return;

            LineBetIndex = _currentIndex - 1;
        }

        public void Increase()
        {
            if (IsLastBet) return;

            LineBetIndex = _currentIndex + 1;
        }

        public void Init(double min, double max, double last)
        {
            if (_betTable == null || _betTable.Length == 0) Debug.LogError("Invalid BetTable");
            if (min >= max) Debug.LogError("min is bigger than max");

            _min = min;
            _max = max;

            _lastLineBet = last;

            // 배팅 테이블 오른차순 정렬
            _betTable = _betTable.OrderBy(b => b).ToArray();
            // Debug.Log( string.Join( ",", BetTable.Select( p => p.ToString()).ToArray() ));

            //최소 최대 라인벳에 맞게 베팅 테이블 을 수정한다.
            CheckMinBet();
            CheckMaxBet();

            //최초 벳 결정
            CheckFirstBet();
        }

        public void Save()
        {
            _lastLineBet = _currentLineBet;
        }
        #endregion


        void CheckMinBet()
        {
            var deleteCount = 0;
            for (var i = 0; i < _betTable.Length; ++i)
            {
                if (_min <= _betTable[i]) break;

                ++deleteCount;
            }
            if (deleteCount < _betTable.Length) _betTable = _betTable.Slice(deleteCount, _betTable.Length);
        }

        void CheckMaxBet()
        {
            if (_max <= _min) return;

            var endIndex = 0;
            for (var i = 0; i < _betTable.Length; ++i)
            {
                if (_max < _betTable[i]) break;

                ++endIndex;
            }

            if (endIndex < _betTable.Length) _betTable = _betTable.Slice(0, endIndex);
        }

        void CheckFirstBet()
        {
            if (_lastLineBet <= 0)
            {
                LineBetIndex = 0;
                return;
            }

            var betIndex = Array.IndexOf(_betTable, _lastLineBet);
            if (betIndex < 0) LineBetIndex = 0;
            else LineBetIndex = betIndex;
        }
    }
}
