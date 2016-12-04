using UnityEngine;
using UnityEngine.UI;
using lpesign;
using System.Collections;

public class Lobby : SingletonSimple<Lobby>
{
	private LobbyData _data;
	public LobbyData Data{ get{ return _data;}}

	override protected void Awake()
	{
		base.Awake();

		_data = new LobbyData();
		_data.SetTestDatas();

		CanvasUtil.CanvasSetting( GetComponent<Canvas>());
	}

	void Start()
	{
		Debug.Log("lobby start");
		Application.targetFrameRate  = GlobalConfig.TargetFrameRate;
		
		if( Screen.width != GlobalConfig.ReferenceWidth || Screen.height != GlobalConfig.ReferenceHeight )
		{
			Screen.SetResolution( Screen.width, Screen.width * GlobalConfig.ReferenceHeight / GlobalConfig.ReferenceWidth, false );
		}

		Debug.Log("lobby start ok ");
	}
}
