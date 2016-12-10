using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class SceneLoader : MonoBehaviour
{
    static public string SC_START = "Main";
    static public string SC_GAME01 = "Game01";
    static public string SC_GAME02 = "Game02";

    static public string[] sceneNamesInBuild;
    static public Scene LastScene { get; private set; }


    //런타임 시작 시 처음 한번 실행 된다.
    [RuntimeInitializeOnLoadMethodAttribute]
    public static void CheckScene()
    {
        Debug.Log("Check Scene...");

        try
        {
            Application.backgroundLoadingPriority = ThreadPriority.High;

            sceneNamesInBuild = new string[] { SC_START, SC_GAME01, SC_GAME02 };

            Scene startScene = SceneManager.GetActiveScene();
            string startSceneName = startScene.name;

            if (ContainsInBuild(startSceneName) == false)
            {
                Debug.LogWarning(string.Format("빌드셋팅에 포함되지 않은 '{0}' 씬으로 시작 되었습니다.", startSceneName));
            }


            LastScene = startScene;
        }
        catch (System.Exception e)
        {
            Debug.Log("ex: " + e.ToString());
        }
    }


    static public bool ContainsInBuild(string sceneName)
    {
        return System.Array.IndexOf(sceneNamesInBuild, sceneName) != -1;
    }

    //--------------------------------------------------------------------------
    // instance
    //--------------------------------------------------------------------------

    public Canvas canvas;
    public Camera cam;
    public CanvasGroup loadingScreen;

    [Header("Progress")]
    public Slider progressBar;
    public Text progressText;
    public float ProgressBarSpeed = 2f;

    public bool IsGameReady { get; set; }

    string _sceneToLoad = "";
    AsyncOperation _asyncOperation;
    float _loadingProgress;
    RectTransform _loadingScreenRtf;

    void Awake()
    {
        CanvasUtil.CanvasSetting(canvas);
    }

    void Start()
    {
        _loadingScreenRtf = loadingScreen.GetComponent<RectTransform>();
        visible = false;
    }

    void Reset()
    {
        IsGameReady = false;
        SetProgress(0f);
        _loadingProgress = 0f;
    }

    public void Load(string sceneName)
    {
        if (LastScene.name == sceneName) return;

        if (string.IsNullOrEmpty(sceneName)) _sceneToLoad = SC_START;
        else _sceneToLoad = sceneName;

        Debug.Log("load scene: " + _sceneToLoad);

        visible = true;

        Reset();

        ShowLoadingScreen();

        StartCoroutine(LoadAsync());
        StartCoroutine(AnimatingProgressBar());
    }

    void ShowLoadingScreen()
    {
        StartCoroutine(loadingScreen.FadeTo(0f, 1f));
        StartCoroutine(_loadingScreenRtf.LocalScaleTo(0f, 1f));
    }

    IEnumerator AnimatingProgressBar()
    {
        while (progressBar.value != 1f)
        {
            SetProgress(Mathf.MoveTowards(progressBar.value, _loadingProgress, Time.deltaTime * ProgressBarSpeed));
            yield return null;
        }
    }

    void SetProgress(float value)
    {
        progressBar.value = value;
        progressText.text = (value * 100).ToString("0") + " %";
    }

    IEnumerator LoadAsync()
    {
        if (ContainsInBuild(_sceneToLoad))
        {
            _asyncOperation = SceneManager.LoadSceneAsync(_sceneToLoad, LoadSceneMode.Single);
        }
        else
        {
            Debug.Log(_sceneToLoad + "는 존재하지 않는 scene 입니다. assetBundle 받아야함");
            yield break;
        }

        //게임 씬 로드. 90% 까지만 진행
        _asyncOperation.allowSceneActivation = false;
        while (_asyncOperation.progress < 0.9f)
        {
            _loadingProgress = 0.9f * Mathf.InverseLerp(0f, 0.9f, _asyncOperation.progress);
            yield return null;
        }
        _loadingProgress = 0.9f;

        //로딩 팝업 애니메이션이 완료되기 전에 씬이 로드되었다면 애니메이션 대기
        while (loadingScreen.alpha != 1)
        {
            yield return null;
        }
        _asyncOperation.allowSceneActivation = true;

        //씬 전환 이후 게임의 준비완료 신호 대기( 서버접속, 로그인, 초기화, 동적 로딩 등등)
        while (IsGameReady == false)
        {
            yield return null;
        }
        _loadingProgress = 1f;

        //ProgressBar 애니메이션 진행이 완료될때까지 기다림
        while (progressBar.value != 1)
        {
            yield return null;
        }

        StartCoroutine(LoadingComplete());
    }

    IEnumerator LoadingComplete()
    {
        Debug.Log("Game Loading Complete");
        yield return StartCoroutine(loadingScreen.FadeTo(1f, 0f));
        visible = false;
    }

    public bool visible
    {
        set
        {
            canvas.gameObject.SetActive(value);
            cam.gameObject.SetActive(value);
        }
    }
}
