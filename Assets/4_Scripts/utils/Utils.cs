using UnityEngine;
using UnityEngine.UI;

public class CanvasUtil
{
    static public void CanvasSetting(Canvas canvas)
    {
        if (canvas == null) return;

        Camera cam = canvas.worldCamera;
        if (cam == null) return;

        cam.orthographicSize = GlobalConfig.ReferenceHeight * 0.5f / GlobalConfig.PixelPerUnit;

        CanvasScaler canvasScaler = canvas.GetComponent<CanvasScaler>();
        if (canvasScaler != null)
        {
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.matchWidthOrHeight = 0f;
            canvasScaler.referenceResolution = new Vector2(GlobalConfig.ReferenceWidth, GlobalConfig.ReferenceHeight);
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


class ColorHSV : System.Object
{
    //ref http://stackoverflow.com/questions/359612/how-to-change-rgb-color-to-hsv
    
    float _h;//색상 0~360
    float _s;//채도 0~100
    float _v;//명도 0~100
    float _a;

    public ColorHSV(float h, float s, float v, float a = 1.0f )
    {
        _h = h;
        _s = s;
        _v = v;
        _a = a;
    }

    public ColorHSV(Color color)
    {
        float min = Mathf.Min(Mathf.Min(color.r, color.g), color.b);
        float max = Mathf.Max(Mathf.Max(color.r, color.g), color.b);
        float delta = max - min;

        _v = max;

        if (!Mathf.Approximately(max, 0))
        {
            _s = delta / max;
        }
        else
        {
            _s = 0;
            _h = -1;
            return;
        }

        if (Mathf.Approximately(min, max))
        {
            _v = max;
            _s = 0;
            _h = -1;
            return;
        }

        if (color.r == max)
        {
            _h = (color.g - color.b) / delta;
        }
        else if (color.g == max)
        {
            _h = 2 + (color.b - color.r) / delta;
        }
        else
        {
            _h = 4 + (color.r - color.g) / delta;
        }

        _h *= 60;
        if (_h < 0)
        {
            _h += 360;
        }
    }

    public Color ToColor()
    {
        if (_s == 0)
        {
            return new Color(_v, _v, _v, _a);
        }
        
        float sector = _h / 60;

        int i;
        i = (int)Mathf.Floor(sector);

        float f = sector - i;
        float v = _v;
        float p = v * (1 - _s);
        float q = v * (1 - _s * f);
        float t = v * (1 - _s * (1 - f));

        Color color = new Color(0, 0, 0, _a);
        switch (i)
        {
            case 0:
                color.r = v;
                color.g = t;
                color.b = p;
                break;
            case 1:
                color.r = q;
                color.g = v;
                color.b = p;
                break;
            case 2:
                color.r = p;
                color.g = v;
                color.b = t;
                break;
            case 3:
                color.r = p;
                color.g = q;
                color.b = v;
                break;
            case 4:
                color.r = t;
                color.g = p;
                color.b = v;
                break;
            default:
                color.r = v;
                color.g = p;
                color.b = q;
                break;
        }
        return color;
    }

    public static Color GetRandomColor(float h, float s, float v, float a )
    {
        ColorHSV col = new ColorHSV(h, s, v, a );
        return col.ToColor();
    }

    public static Color GetRandomColor(float h, float a = 1.0f )
    {
        return GetRandomColor(h,1.0f,1.0f, a );
    }
}