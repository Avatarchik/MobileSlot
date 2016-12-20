using UnityEngine;
using UnityEngine.UI;

namespace lpesign
{
    public class CanvasUtil
    {
    }

    static public class CanvasExtension
    {
        static public void SetReferenceSize(this Canvas canvas, float referenceWidth, float referenceHeight, float pixelPerUnit = 100f)
        {
            Camera cam = canvas.worldCamera;
            if (cam == null) return;

            cam.orthographicSize = referenceHeight * 0.5f / pixelPerUnit;

            CanvasScaler canvasScaler = canvas.GetComponent<CanvasScaler>();
            if (canvasScaler != null)
            {
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.matchWidthOrHeight = 1f;
                canvasScaler.referenceResolution = new Vector2(referenceWidth, referenceHeight);
            }
        }
    }
}
