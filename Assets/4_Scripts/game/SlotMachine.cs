using UnityEngine;
using System.Collections;

using LitJson;

public class SlotMachine : MonoBehaviour
{
    public GameUI ui;

    void Start()
    {
        Debug.Log("game Start");

        GameServerCommunicator.Instance.OnLogin += LogimComplete;
        GameServerCommunicator.Instance.Connect("182.252.135.251", 13100);
    }

    void LogimComplete(ResponseDTO.LoginDTO data)
    {
        Debug.Log("game login complete." );
        GameManager.Instance.GameReady();

        ui.SetData( data );
    }
}
