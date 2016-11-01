using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

public class SceneLoader : MonoBehaviour
{
    static public string SC_START = "Main";

    static public List<string> sceneNamesInBuild;
    static public Scene LastScene { get; private set; }

    //런타임 시작 시 처음 한번 실행 된다.
    [RuntimeInitializeOnLoadMethodAttribute]
    public static void CheckScene()
    {
        Debug.Log("Check Scene...");

        Application.backgroundLoadingPriority = ThreadPriority.High;
        EditorBuildSettingsScene[] scenesInBuild = EditorBuildSettings.scenes;

        sceneNamesInBuild = new List<string>();
        foreach (var sc in scenesInBuild)
        {
            sceneNamesInBuild.Add(Path.GetFileNameWithoutExtension(sc.path));
        }

        Scene startScene = SceneManager.GetActiveScene();
        string startSceneName = startScene.name;

        if (ContainsInBuild(startSceneName) == false)
        {
            Debug.LogWarning(string.Format("빌드셋팅에 포함되지 않은 '{0}' 씬으로 시작 되었습니다.", startSceneName));
        }

        LastScene = startScene;
    }


    static public bool ContainsInBuild(string sceneName)
    {
        return sceneNamesInBuild.Contains(sceneName);
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
    float _progressValue;
    RectTransform _loadingScreenRtf;

    void Awake()
    {
        CanvasUtil.CanvasSetting( canvas );
    }

    void Start()
    {
        _loadingScreenRtf = loadingScreen.GetComponent<RectTransform>();
        visible = false;
    }

    void Reset()
    {
        IsGameReady = false;
        _progressValue = 0f;
        progressBar.value = 0f;
        progressText.text = "0%";
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
        StartCoroutine(UpdateProgressBar());
    }

    void ShowLoadingScreen()
    {
        StartCoroutine(FadeLoadingScreen(0f, 1f));
        StartCoroutine(ScaleLoadingScreen(0f, 1f));
    }

    IEnumerator FadeLoadingScreen(float from, float to, float duration = 0.2f)
    {
        loadingScreen.alpha = from;

        float t = 0f;
        while (loadingScreen.alpha != to)
        {
            loadingScreen.alpha = Mathf.Lerp(from, to, t);
            t += Time.deltaTime / duration;
            yield return null;
        }
    }
    IEnumerator ScaleLoadingScreen(float from, float to, float duration = 0.2f)
    {
        Vector3 fromScale = Vector3.one * from;
        Vector3 toScale = Vector3.one * to;
        _loadingScreenRtf.localScale = fromScale;

        float t = 0f;
        while (_loadingScreenRtf.localScale != toScale)
        {
            _loadingScreenRtf.localScale = Vector3.Lerp(fromScale, toScale, t);
            t += Time.deltaTime / duration;
            yield return null;
        }
    }

    IEnumerator UpdateProgressBar()
    {
        while (progressBar.value != 1f)
        {
            progressBar.value = Mathf.MoveTowards(progressBar.value, _progressValue, Time.deltaTime * ProgressBarSpeed);
            progressText.text = (_progressValue * 100).ToString("0") + " %";
            yield return null;
        }
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
            _progressValue = 0.9f * Mathf.InverseLerp(0f, 0.9f, _asyncOperation.progress);
            yield return null;
        }
        _progressValue = 0.9f;

        //로딩 이미지 애니메이션 대기
        while (loadingScreen.alpha != 1)
        {
            yield return null;
        }
        _asyncOperation.allowSceneActivation = true;


        //씬 전환 이후 게임의 준비완료 신호 대기
        while (IsGameReady == false)
        {
            yield return null;
        }
        _progressValue = 1f;

        //ProgressBar 진행 애니메이션 완료 대기
        while (progressBar.value != 1)
        {
            yield return null;
        }

        StartCoroutine(LoadingComplete());
    }

    IEnumerator LoadingComplete()
    {
        yield return StartCoroutine(FadeLoadingScreen(1f, 0f ));
        visible = false;
        Debug.Log("All Complete");
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
