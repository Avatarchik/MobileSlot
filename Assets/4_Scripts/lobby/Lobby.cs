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
		Application.targetFrameRate  = Config.TargetFrameRate;
		
		if( Screen.width != Config.ReferenceWidth || Screen.height != Config.ReferenceHeight )
		{
			Screen.SetResolution( Screen.width, Screen.width * Config.ReferenceHeight / Config.ReferenceWidth, false );
		}
	}
}
