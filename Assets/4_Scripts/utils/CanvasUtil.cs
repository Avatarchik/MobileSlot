using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CanvasUtil
{
	static public void CanvasSetting( Canvas canvas )
	{
		if( canvas == null ) return;

		// Camera cam = canvas.worldCamera;
		// if( cam == null ) return;

		// cam.orthographicSize = Config.ReferenceHeight * 0.5f / Config.PixelPerUnit;

		// CanvasScaler canvasScaler = canvas.GetComponent<CanvasScaler>();
		// if( canvasScaler != null )
		// {
		// 	canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		// 	canvasScaler.matchWidthOrHeight = 0f;
		// 	canvasScaler.referenceResolution = new Vector2( Config.ReferenceWidth, Config.ReferenceHeight );
		// }
	}
}
