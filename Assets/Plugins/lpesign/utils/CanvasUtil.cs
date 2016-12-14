using UnityEngine;
using UnityEngine.UI;

namespace lpesign
{
    public class CanvasUtil
    {
        static public void CanvasSetting(Canvas canvas, float referenceWidth, float referenceHeight, float pixelPerUnit = 100f)
        {
            if (canvas == null) return;

            Camera cam = canvas.worldCamera;
            if (cam == null) return;

            // cam.orthographicSize = GlobalConfig.ReferenceHeight * 0.5f / GlobalConfig.PixelPerUnit;
            cam.orthographicSize = referenceHeight * 0.5f / pixelPerUnit;

            CanvasScaler canvasScaler = canvas.GetComponent<CanvasScaler>();
            if (canvasScaler != null)
            {
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.matchWidthOrHeight = 1f;
                // canvasScaler.referenceResolution = new Vector2(GlobalConfig.ReferenceWidth, GlobalConfig.ReferenceHeight);
                canvasScaler.referenceResolution = new Vector2(referenceWidth, referenceHeight);
            }
        }
    }
}
