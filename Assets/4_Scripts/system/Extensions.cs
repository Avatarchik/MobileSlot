using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

//todo
//적절한 클래스로 분리하자
public static class ExtensionMethods
{
    /// <summary>
    /// double 을 000,000,000 형태로 바꾼다
    /// </summary>
    public static string ToBalance(this double self)
    {
        return self.ToString("#,##0");
    }



    /// <summary>
    /// UIGroup 컴포넌트의 alpha 를 fade
    /// </summary>
    public static IEnumerator FadeTo(this CanvasGroup self,
                                     float toA, float duration = 0.2f, Action cb = null)
    {
        float fromA = self.alpha;
        yield return self.FadeTo(fromA, toA, duration, cb);
    }
    public static IEnumerator FadeTo(this CanvasGroup self,
                                     float fromA, float toA, float duration = 0.2f, Action cb = null)
    {
        self.alpha = fromA;

        if (duration > 0)
        {
            float t = 0f;
            while (self.alpha != toA)
            {
                if (self == null) yield break;

                self.alpha = Mathf.Lerp(fromA, toA, t);
                t += Time.deltaTime / duration;
                yield return null;
            }
        }

        self.alpha = toA;

        if (cb != null) cb();

        yield break;
    }

    /// <summary>
    /// Graphic 을 상속한 컴포넌트의 alpha 를 fade
    /// </summary>
    public static IEnumerator FadeTo(this Graphic g,
                                     float toA, float duration, Action cb = null)
    {
        float fromA = g.color.a;
        yield return g.FadeTo(fromA, toA, duration, cb);
    }

    public static IEnumerator FadeTo(this Graphic g,
                                     float fromA, float toA, float duration, Action cb = null)
    {
        SetAlpha(g, fromA);

        if (duration > 0)
        {
            float t = 0f;
            while (t < 1)
            {
                if (g == null) yield break;

                SetAlpha(g, Mathf.Lerp(fromA, toA, t));
                t += Time.deltaTime / duration;
                yield return null;
            }
        }

        SetAlpha(g, toA);

        if (cb != null) cb();
        yield break;
    }

    public static IEnumerator ChangeColor(this Graphic g,
                                          Color toColor, float duration, Action cb = null)
    {
        if (duration > 0)
        {
            Color startColor = g.color;
            float t = 0f;
            while (t < 1)
            {
                if (g == null) yield break;

                Color newColor = Color.Lerp(startColor, toColor, t);
                g.color = newColor;

                t += Time.deltaTime / duration;

                yield return null;
            }
        }

        g.color = toColor;

        if (cb != null) cb();

        yield break;
    }

    public static void SetAlpha(Graphic g, float alpha)
    {
        Color color = g.color;
        color.a = alpha;
        g.color = color;
    }

    /// <summary>
    /// rebderer 을 상속한 컴포넌트의 alpha 를 fade
    /// </summary>
    public static IEnumerator FadeTo(this Renderer r,
                                    float fromA, float toA, float duration, Action cb = null)
    {
        if (duration > 0)
        {
            float t = 0f;
            while (t < 1)
            {
                if (r == null) yield break;

                SetAlpha(r, Mathf.Lerp(fromA, toA, t));
                t += Time.deltaTime / duration;
                yield return null;
            }
        }

        SetAlpha(r, toA);

        if (cb != null) cb();

        yield break;
    }

    public static void SetAlpha(Renderer r, float alpha)
    {
        if (r == null) return;
        Color color = r.material.color;
        color.a = alpha;
        r.material.color = color;
    }

    public static IEnumerator ChangeColor(this Renderer r,
                                          Color toColor, float duration, Action cb = null)
    {
        Color fromColor = r.material.color;
        yield return r.ChangeColor(fromColor, toColor, duration, cb);
    }

    public static IEnumerator ChangeColor(this Renderer r,
                                          Color fromColor, Color toColor, float duration, Action cb = null)
    {
        if (duration > 0)
        {
            float t = 0f;
            while (t < 1f)
            {
                if (r == null) yield break;

                r.material.color = Color.Lerp(fromColor, toColor, t);
                t += Time.deltaTime / duration;

                yield return null;
            }
        }

        r.material.color = toColor;

        if (cb != null) cb();

        yield break;
    }


    /// <summary>
    /// Transform 컴포넌트의 scale 를 tween
    /// </summary>
    public static IEnumerator LocalScaleTo(this Transform self,
                                           float from, float to, float duration = 0.2f, Action cb = null)
    {
        Vector3 fromScale = Vector3.one * from;
        Vector3 toScale = Vector3.one * to;
        self.localScale = fromScale;

        float t = 0f;
        while (self.localScale != toScale)
        {
            self.localScale = Vector3.Lerp(fromScale, toScale, t);
            t += Time.deltaTime / duration;
            yield return null;
        }

        if (cb != null) cb();
    }
}