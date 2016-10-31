using UnityEngine;
using System.Collections;

public class SlotMachine : MonoBehaviour
{

	void Awake()
	{
		Debug.Log("game Awake");
	}

    IEnumerator Start()
    {
		Debug.Log("game Start");

		yield return new WaitForSeconds(0.5f);

		Debug.Log("game login complete");
		GameManager.Instance.GameReady();
    }
}
