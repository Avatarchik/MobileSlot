using UnityEngine;
using System.Collections;

public class GameUI : MonoBehaviour
{
	void Awake()
	{
		CanvasUtil.CanvasSetting( GetComponent<Canvas>());
	}
}
