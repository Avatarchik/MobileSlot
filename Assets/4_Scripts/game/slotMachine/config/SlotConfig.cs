﻿using UnityEngine;
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

        //------------------------------------------------------------------
        //Slot 공통 설정
        //------------------------------------------------------------------

        public int ID;
        public string Host;
        public int Port;
        public string Version;
        public SlotBetting Betting;
        public bool DebugSymbolArea;//심볼 영역을 표시할지 여부
        public bool DebugTestSpin;//심볼 영역을 표시할지 여부

        [SerializeField]
        List<MachineConfig> _machienList;
        public MachineConfig Main
        {
            get
            {
                if (_machienList.Count == 0) return null;
                else return _machienList[0];
            }
        }

        public void AddMachine(MachineConfig machine)
        {
            _machienList.Add(machine);
        }

        //------------------------------------------------------------------
        //게임 개별 머신
        //------------------------------------------------------------------
        [Serializable]
        public class MachineConfig
        {
            [Header("Base")]
            public int Row;
            public int Column;

            [Header("Symbol")]
            public Size2D SymbolSize;
            public Size2D NullSymbolSize;

            [Header("FreeSpin")]
            public bool UseFreeSpin;
            public FreeSpinTriggerType TriggerType;
            public FreeSpinRetriggerType RetriggerType;

            public int[][] paylineTable;

            [Header("Scatter")]
            public List<ScatterInfo> scatters;

            [Header("Reel")]
            public Reel ReelPrefab;
            public Size2D ReelSize;
            public float ReelSpace;
            public float ReelGap;


            [Header("Spin")]
            public int MarginSymbolCount;//릴 위아래 여유 심볼 수
            public int IncreaseCount;//다음 릴로 갈 수록 더 생겨야할 심볼 수
            public int SpiningSymbolCount;//스핀 한 세트 당 심볼 수
            public int SpinCountThreshold;//서버가 응답이 빠르더라도 최소한 돌아야할 스핀 세트 수 
            public float SpinSpeedPerSec;//스핀 초당 속도
            public float DelayEachReel;//각 릴 사이의 스핀 시작 딜레이
            public MoveTweenInfo tweenFirstBackInfo;//첫번재 스핀에 정보
            public MoveTweenInfo tweenLastBackInfo;//마지막 스핀이 정보

            [Header("Transition")]
            public Transition transition;


            [Header("NameMap")]
            public SymbolNameMap NameMap;

            //reelStrip
            ReelStrip _normalStrip;
            ReelStrip _freeStrip;

            public ReelStrip NormalStrip
            {
                set { _normalStrip = value; }
                get { return _normalStrip; }
            }

            public ReelStrip FreeStrip
            {
                set { _freeStrip = value; }
                get { return _freeStrip ?? _normalStrip; }
            }

            //startSymbols
            string[,] _startSymbolNames;
            public void SetStartSymbols(string[,] startSymbolNames)
            {
                _startSymbolNames = startSymbolNames;
            }

            public string GetStartSymbolAt(int col, int row)
            {
                return _startSymbolNames[col, row];
            }

            public void AddSccaterInfo(string symbolname, int limit, int[] ableReel)
            {
                if (scatters == null) scatters = new List<ScatterInfo>();

                var info = new ScatterInfo(symbolname, limit, ableReel);
                scatters.Add(info);
            }
        }
    }

    //todo
    //symbol 정의를 내릴때 포함시키자
    [Serializable]
    public class ScatterInfo
    {
        public string symbolName;
        public int limit;
        public int[] ableReel;
        public AudioClip[] stopSounds;

        int _index;

        public ScatterInfo(string symbolname, int limit, int[] ableReel)
        {
            this.symbolName = symbolname;
            this.limit = limit;
            this.ableReel = ableReel;
            this.stopSounds = new AudioClip[ableReel.Length];

            Reset();
        }

        public void Reset()
        {
            _index = 0;
        }

        public bool CheckScattered(Reel reel, out AudioClip stopSound)
        {
            Debug.Log("reel: " + reel.Column + " check ableReel: " + string.Join(",", ableReel.Select(x => x.ToString()).ToArray()));

            if (Array.IndexOf(ableReel, reel.Column) == -1 ||
                reel.ContainsByName(symbolName) == false)
            {
                stopSound = null;
                Debug.Log("no scatterd: " + Array.IndexOf(ableReel, reel.Column) + " : " + reel.ContainsByName(symbolName));
                return false;
            }

            Debug.Log(symbolName + " scattered");
            stopSound = stopSounds[_index++];
            return true;
        }
    }

    public class SymbolNameMap
    {
        Dictionary<string, string> _symbolmap;
        public SymbolNameMap()
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
    }

    public class ReelStrip
    {
        public enum ReelStripType
        {
            NORMAL,
            USE_NULL,
            STACK
        }

        public ReelStripType type;

        string[][] _strip;

        public ReelStrip(string[][] strip, ReelStripType type = ReelStripType.NORMAL)
        {
            _strip = strip;
            this.type = type;
        }

        public string GetRandom(int column)
        {
            var reel = _strip[column];
            int leng = reel.Length;
            int randomIndex = UnityEngine.Random.Range(0, leng);
            return reel[randomIndex];
        }
    }

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


        [SerializeField]
        double[] _betTable;

        [SerializeField]
        double _minLineBet;

        [SerializeField]
        double _maxLineBet;

        [SerializeField]
        double _lastLineBet;

        [SerializeField]
        int _currentLinBetIndex;

        #region Property

        public double[] BetTable { set { _betTable = value; } }

        public int PaylineNum { get; set; }

        public int LineBetIndex
        {
            get { return _currentLinBetIndex; }
            set
            {
                _currentLinBetIndex = value;

                if (_currentLinBetIndex < 0) _currentLinBetIndex = 0;
                else if (_currentLinBetIndex >= _betTable.Length) _currentLinBetIndex = _betTable.Length - 1;

                if (OnUpdateLineBetIndex != null) OnUpdateLineBetIndex();
            }
        }

        public double LineBet
        {
            get { return _betTable[_currentLinBetIndex]; }
        }

        public double TotalBet
        {
            get { return LineBet * PaylineNum; }
        }

        public bool IsFirstBet
        {
            get { return _currentLinBetIndex <= 0; }
        }

        public bool IsLastBet
        {
            get { return _currentLinBetIndex >= _betTable.Length - 1; }
        }

        #endregion


        #region API
        public void Decrease()
        {
            if (IsFirstBet) return;

            LineBetIndex = _currentLinBetIndex - 1;
        }

        public void Increase()
        {
            if (IsLastBet) return;

            LineBetIndex = _currentLinBetIndex + 1;
        }

        public void Init(double min, double max, double last)
        {
            if (_betTable == null || _betTable.Length == 0) Debug.LogError("Invalid BetTable");
            if (min >= max) Debug.LogError("min is bigger than max");

            _minLineBet = min;
            _maxLineBet = max;
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
        #endregion


        void CheckMinBet()
        {
            var deleteCount = 0;
            for (var i = 0; i < _betTable.Length; ++i)
            {
                if (_minLineBet <= _betTable[i]) break;

                ++deleteCount;
            }
            if (deleteCount < _betTable.Length) _betTable = _betTable.Slice(deleteCount, _betTable.Length);
        }

        void CheckMaxBet()
        {
            if (_maxLineBet <= _minLineBet) return;

            var endIndex = 0;
            for (var i = 0; i < _betTable.Length; ++i)
            {
                if (_maxLineBet < _betTable[i]) break;

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