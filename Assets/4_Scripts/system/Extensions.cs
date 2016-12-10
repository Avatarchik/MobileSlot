using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public static class ExtensionMethods
{
    /// <summary>
    /// 배열을 slice
    /// </summary>
    public static T[] Slice<T>(this T[] arr,
                               int indexFrom, int indexTo)
    {
        if (indexFrom > indexTo)
        {
            throw new ArgumentOutOfRangeException("indexFrom is bigger than indexTo!");
        }

        int length = indexTo - indexFrom;
        T[] result = new T[length];
        Array.Copy(arr, indexFrom, result, 0, length);

        return result;
    }

    /// <summary>
    /// double 을 000,000,000 형태로 바꾼다
    /// </summary>
    public static string ToBalance(this double self)
    {
        return self.ToString("#,##0");
    }

    /// <summary>
    /// 애니메이터가 특정 파라메터를 포함하는지 알아낸다.
    /// </summary>
    public static bool HasParameterOfType(this Animator self,
                                          string name, AnimatorControllerParameterType type)
    {
        var parameters = self.parameters;
        foreach (var currParam in parameters)
        {
            if (currParam.type == type && currParam.name == name)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// UIGroup 컴포넌트의 alpha 를 fado
    /// </summary>
    public static IEnumerator FadeTo(this CanvasGroup self,
                                     float from, float to, float duration = 0.2f, Action cb = null)
    {
        self.alpha = from;

        float t = 0f;
        while (self.alpha != to)
        {
            self.alpha = Mathf.Lerp(from, to, t);
            t += Time.deltaTime / duration;
            yield return null;
        }

        if (cb != null) cb();
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