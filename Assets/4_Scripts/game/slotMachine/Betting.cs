using UnityEngine;
using System.Collections;
using System.Linq;

[System.Serializable]
public class SlotBetting
{
    [SerializeField]
    double _lastLineBet;
    [SerializeField]
    double _minLineBet;
    [SerializeField]
    double _maxLineBet;
    [SerializeField]
    double[] _betTable;
	public double[] BetTable{ set{ _betTable = value; }}

    public void Init(double min, double max, double last)
    {
		if( _betTable == null || _betTable.Length == 0 ) Debug.LogError( "Invalid BetTable" );
		if( min >= max ) Debug.LogError( "min is bigger than max" );

		_minLineBet = min;
		_maxLineBet = max;
		_lastLineBet = last;

        // 배팅 테이블 오른차순 정렬
        _betTable = _betTable.OrderBy(b => b).ToArray();
        // Debug.Log( string.Join( ",", BetTable.Select( p => p.ToString()).ToArray() ));

        //최소 최대 라인벳에 맞게 베팅 테이블 을 수정한다.
        CheckMinBet();
        CheckMaxBet();
    }

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
}
