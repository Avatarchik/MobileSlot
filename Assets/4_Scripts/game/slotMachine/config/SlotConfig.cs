﻿using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

using lpesign;

namespace Game
{
    //------------------------------------------------------------------
    // SlotConfig
    //------------------------------------------------------------------
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
        public string host;
        public int port;
        public int accessID;
        public string ver;

        public bool Jackpot;
        public bool DebugSymbolArea;//심볼 영역을 표시할지 여부
        public bool DebugTestSpin;//테스트 스핀 핫 키 여부

        public SlotBetting Betting;

        [SerializeField]
        List<MachineConfig> _machineList;

        public List<MachineConfig> MachineList { get { return _machineList; } }
        public MachineConfig MainMachine
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
    }

    //------------------------------------------------------------------
    // MachineConfig
    //------------------------------------------------------------------
    [System.Serializable]
    public class MachineConfig
    {
        public SlotConfig RelativeSlotConfig { get; set; }
        public MachineConfig(SlotConfig relativeSlotConfig)
        {
            RelativeSlotConfig = relativeSlotConfig;
        }

        //------------------------------------------------------------
        //base
        //------------------------------------------------------------
        public int row;
        public int column;

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
        public int MarginSymbolCount;//릴 위아래 여유 심볼 수
        public int MarginSymbolCountServer;//서버에서 주는 릴 위아래 여유 심볼 수


        //------------------------------------------------------------
        //symbol
        //------------------------------------------------------------
        public Size2D SymbolSize;
        public bool useEmpty;
        public Size2D blankSymbolSize;

        [SerializeField]
        List<SymbolDefine> _symbolDefineList;

        
        //------------------------------------------------------------
        //spin
        //------------------------------------------------------------
        public float SpinSpeedPerSec;//스핀 초당 속도
        public int IncreaseCount;//다음 릴로 갈 수록 더 생겨야할 심볼 수
        public int SpinningSymbolCount;//스핀 한 세트 당 심볼 수
        public int SpinCountThreshold;//서버가 응답이 빠르더라도 최소한 돌아야할 스핀 세트 수 
        public float DelayEachSpin;//각 릴 사이의 스핀 시작 딜레이
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
        public ReelStripsBundle reelStripsBundle;

        public SymbolType TopChildSymbolType{ get;set;}

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

            var path = string.Empty;

            if (type == SymbolType.Blank) path = "games/common/prefabs/EM";
            else path = "games/" + ConvertUtil.ToDigit(RelativeSlotConfig.ID) + "/prefabs/symbols/" + symbolName;

            define.prefab = Resources.Load<Symbol>(path);

            define.buffer = buffer;

            _symbolDefineList.Add(define);
        }

        public string[] GetSymbolNames()
        {
            if (_symbolDefineList == null || _symbolDefineList.Count == 0) return null;

            return _symbolDefineList.Select(s => s.symbolName).ToArray();
        }

        //------------------------------------------------------------
        //startsymbols
        //------------------------------------------------------------
        [SerializeField]
        SymbolNames[] _startSymbols;
        public void SetStartSymbols(string[][] startSymbolNames)
        {
            var len = startSymbolNames.Length;
            _startSymbols = new SymbolNames[len];

            for (var i = 0; i < startSymbolNames.Length; ++i)
            {
                string[] symbolNames = startSymbolNames[i];
                SymbolNames symbolSet = new SymbolNames(symbolNames);
                _startSymbols[i] = symbolSet;
            }
        }

        public SymbolNames GetStartSymbolNames(int col)
        {
            return _startSymbols[col];
        }

        //------------------------------------------------------------
        //scatter
        //------------------------------------------------------------
        [SerializeField]
        List<ScatterInfo> _scatters;
        public List<ScatterInfo> ScatterInfos { get { return _scatters; } }

        public void ClearScatterInfo()
        {
            if (_scatters != null) _scatters.Clear();
        }

        public void AddScatterInfo(ScatterInfo info)
        {
            if (_scatters == null) _scatters = new List<ScatterInfo>();

            _scatters.Add(info);
        }
    }


    //------------------------------------------------------------------
    //SlotBetting
    //------------------------------------------------------------------
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
            if (min >= max) Debug.LogError("min must be bigger than max");

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
