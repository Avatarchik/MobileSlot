using UnityEngine;
using System.Collections;
using lpesign;

using DG.Tweening;

public class GameManager : Singleton<GameManager>
{
	public SceneLoader loader;

	override protected void Awake()
	{
		base.Awake();

		//general
		DOTween.Init( recycleAllByDefault: false, useSafeMode: false, logBehaviour: LogBehaviour.ErrorsOnly );
		DOTween.showUnityEditorReport = true; //default false
		DOTween.timeScale = 1f;
		DOTween.SetTweensCapacity( 200,50 );

		//applied to newly
		DOTween.defaultAutoPlay = AutoPlay.None;
		DOTween.defaultEaseType = Ease.Linear;
	}

	public void GameLoad( GameItemDTO data )
	{
		loader.Load( "Game" + data.ID.ToString("00") );
	}

	public void GameReady()
	{
		loader.IsGameReady = true;
	}

	void OnApplicationQuit() {
        Debug.Log("Application Quit!");
    }

	void OnApplicationFocus( bool focusStatus )
	{
		//Debug.Log("OnApplicationFocus: " + focusStatus );
	}

	void OnApplicationPause( bool pauseStatus )
	{
		// if( pauseStatus ) Debug.Log("Application pause");
		// else Debug.Log("Application resume");
	}
}
