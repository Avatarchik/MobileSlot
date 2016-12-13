using UnityEngine;
using System.Collections;

using lpesign;

[RequireComponent(typeof( SoundPlayer ))]
public class SoundManager : SingletonSimple<SoundManager>
{
	SoundPlayer _player;
	override protected void Awake()
	{
		base.Awake();
		_player = GetComponent<SoundPlayer>();
	}

	public void PlayFreeSpinTrigger()
	{

	}

	public void StopFreeSpinTrigger()
	{
		
	}

	public void PlayFreeSpinReady()
	{

	}

	public void StopFreeSpinReady()
	{

	}
}
