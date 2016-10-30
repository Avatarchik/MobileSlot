using UnityEngine;
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
}
