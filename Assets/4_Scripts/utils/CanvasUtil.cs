using UnityEngine;
using UnityEngine.UI;

public class CanvasUtil
{
    static public void CanvasSetting(Canvas canvas)
    {
        if (canvas == null) return;

        Camera cam = canvas.worldCamera;
        if (cam == null) return;

        cam.orthographicSize = Config.ReferenceHeight * 0.5f / Config.PixelPerUnit;

        CanvasScaler canvasScaler = canvas.GetComponent<CanvasScaler>();
        if (canvasScaler != null)
        {
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.matchWidthOrHeight = 0f;
            canvasScaler.referenceResolution = new Vector2(Config.ReferenceWidth, Config.ReferenceHeight);
        }
    }
}

public class GameObjectUtil
{

    static public GameObject Create(string name, Transform parent = null)
    {
        GameObject go = new GameObject(name);

        if (parent != null)
        {
            go.transform.SetParent(parent);
        }

        return go;
    }

    static public T Create<T>(string name, Transform parent = null) where T : Component
    {
        return Create(name, parent).AddComponent<T>();
    }

    static public Transform CreateEmptyContainer(string name, Transform parent = null)
    {
        return Create(name, parent).transform;
    }
}