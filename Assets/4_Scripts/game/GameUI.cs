using UnityEngine;
using System.Collections;

public class GameUI : MonoBehaviour
{
	
	void Awake()
	{
		CanvasUtil.CanvasSetting( GetComponent<Canvas>());
	}

	public void Spin()
	{
		GameServerCommunicator.Instance.Spin();
	}

	public void SetData( ResponseDTO.LoginDTO data )
	{

	}
}
