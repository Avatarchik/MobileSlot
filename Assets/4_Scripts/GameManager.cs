using UnityEngine;
using System.Collections;
using lpesign;


public class GameManager : Singleton<GameManager>
{
	public SceneLoader loader;

	public void GameLoad( GameItemDTO data )
	{
		loader.Load( "Game" + data.ID.ToString("00") );
	}

	public void GameReady()
	{
		loader.IsGameReady = true;
	}

	void OnApplicationQuit() {
        Debug.Log("Application ending after " + Time.time + " seconds");
    }

	void OnApplicationFocus( bool focusStatus )
	{
		//Debug.Log("OnApplicationFocus: " + focusStatus );
	}

	void OnApplicationPause( bool pauseStatus )
	{
		if( pauseStatus ) Debug.Log("Application pause");
		else Debug.Log("Application resume");
	}
}
