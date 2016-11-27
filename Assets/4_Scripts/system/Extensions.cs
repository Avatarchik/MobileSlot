using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public static class ExtensionMethods
{
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

    public static string ToBalance( this double self )
    {
        return self.ToString("#,##0");
    }
}