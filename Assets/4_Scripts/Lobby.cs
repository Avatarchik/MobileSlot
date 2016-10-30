using UnityEngine;
using UnityEngine.UI;
using lpesign;
using System.Collections;

public class Lobby : SingletonSimple<Lobby>
{
	private Camera _camera;
	private LobbyData _data;
	public LobbyData Data{ get{ return _data;}}

	override protected void Awake()
	{
		base.Awake();

		_data = new LobbyData();
		_data.SetTestDatas();
		
		_camera = GetComponentInChildren<Camera>();
		_camera.orthographicSize = Config.ReferenceHeight * 0.5f / Config.PixelPerUnit;

		var canvasScaler = GetComponent<CanvasScaler>();
		if( canvasScaler != null ) canvasScaler.referenceResolution = new Vector2( Config.ReferenceWidth, Config.ReferenceHeight );
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
