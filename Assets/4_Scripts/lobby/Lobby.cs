using UnityEngine;
using UnityEngine.UI;
using lpesign;
using System.Collections;

public class Lobby : SingletonSimple<Lobby>
{
    private LobbyData _data;
    public LobbyData Data { get { return _data; } }

    override protected void Awake()
    {
        base.Awake();

        _data = new LobbyData();
        _data.SetTestDatas();

        GetComponent<Canvas>().SetReferenceSize(GlobalConfig.ReferenceWidth, GlobalConfig.ReferenceHeight, GlobalConfig.PixelPerUnit);
    }

    void Start()
    {
        Debug.Log("lobby start");

        GameManager.Instance.SceneReady();
    }
}
