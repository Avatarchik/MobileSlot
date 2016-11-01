using UnityEngine;
using System.Collections;

public class ShiningSevens : SlotMachine
{
	override protected void Awake()
	{
		base.Awake();

		_slotInfo.Version = "0.0.1";
		_slotInfo.Port = 13100;
		_slotInfo.Row = 3;
		_slotInfo.Column = 3;
	}
}
