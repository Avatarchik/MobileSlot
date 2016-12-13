using UnityEngine;
using System.Collections;
using lpesign;

using DG.Tweening;

public class GameManager : Singleton<GameManager>
{
	public SoundPlayer soundPlayer;
    public SceneLoader loader;
    override protected void Awake()
    {
        base.Awake();

        //general
        DOTween.Init(recycleAllByDefault: false, useSafeMode: false, logBehaviour: LogBehaviour.ErrorsOnly);
        DOTween.showUnityEditorReport = true; //default false
        DOTween.timeScale = 1f;
        DOTween.SetTweensCapacity(200, 50);

        //applied to newly
        DOTween.defaultAutoPlay = AutoPlay.None;
        DOTween.defaultEaseType = Ease.Linear;
    }

	//런타임 시작 시 처음 한번 실행 된다.(awake 이후)
	[RuntimeInitializeOnLoadMethodAttribute]
    public static void GameInitialize()
    {
		SceneLoader.CheckScene();
    }

    public void GameLoad(GameItemDTO data)
    {
        loader.Load("Game" + data.ID.ToString("00"));
    }

    public void GameReady()
    {
        loader.IsGameReady = true;
    }

    void OnApplicationQuit()
    {
        Debug.Log("Application Quit!");

		GameServerCommunicator.Dispose();
    }

    void OnApplicationFocus(bool focusStatus)
    {
        //Debug.Log("OnApplicationFocus: " + focusStatus );
    }

    void OnApplicationPause(bool pauseStatus)
    {
        // if( pauseStatus ) Debug.Log("Application pause");
        // else Debug.Log("Application resume");
    }
}
