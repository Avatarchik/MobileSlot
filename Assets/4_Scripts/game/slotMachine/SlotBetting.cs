using UnityEngine;
using System.Collections;
using System;
using System.Linq;

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
