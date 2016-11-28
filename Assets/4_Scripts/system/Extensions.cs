using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public static class ExtensionMethods
{
    /// <summary>
    /// 배열을 slice
    /// </summary>
    public static T[] Slice<T>(this T[] arr, int indexFrom, int indexTo)
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
    public static bool HasParameterOfType(this Animator self, string name, AnimatorControllerParameterType type)
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
}