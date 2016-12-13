using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SlotTopUI : AbstractSlotMachineUIModule
{
	public Button btnLobby;

	void Awake()
	{
		if( btnLobby == null ) Debug.LogError("btnLobby must exist");

		btnLobby.onClick.AddListener( ()=> GameManager.Instance.GoToLobby() );
	}
}
