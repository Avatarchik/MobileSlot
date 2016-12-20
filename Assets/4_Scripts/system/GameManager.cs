using UnityEngine;
using System.Collections;
using lpesign;

using DG.Tweening;

public class GameManager : Singleton<GameManager>
{
    static public string SC_LOBBY = "Lobby";
    static public string SC_GAME01 = "Game01";
    static public string SC_GAME02 = "Game02";

    public SoundPlayer soundPlayer;
    public SceneLoader loader;

    bool _isLobby;

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

    //처음 시작 시 한번 실행(awake 이후 )
    [RuntimeInitializeOnLoadMethodAttribute]
    public static void GameInitialize()
    {
        Application.targetFrameRate = GlobalConfig.TargetFrameRate;

        if (Screen.width != GlobalConfig.ReferenceWidth || Screen.height != GlobalConfig.ReferenceHeight)
        {
            Screen.SetResolution(Screen.width, Screen.width * GlobalConfig.ReferenceHeight / GlobalConfig.ReferenceWidth, false);
        }

        SceneLoader.CheckScene( new string[] { SC_LOBBY, SC_GAME01, SC_GAME02 } );
    }

    void Start()
    {
        _isLobby = SceneLoader.CurrentScene.name == SC_LOBBY;
    }

    public void GameLoad(GameItemDTO data)
    {
        if( _isLobby == false ) return;

        loader.Load("Game" + data.ID.ToString("00"));

        _isLobby = false;
    }

    public void SceneReady()
    {
        loader.IsSceneReady = true;
    }

    public void GoToLobby()
    {
        if( _isLobby ) return;

        loader.Load(SC_LOBBY, ()=> _isLobby = true );
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
