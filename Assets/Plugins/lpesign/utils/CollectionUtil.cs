using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace lpesign
{
    public class CollectionUtil
    {

    }

    static public class CollectionExtension
    {
        /// <summary>
        /// 배열??slice
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

        public static string ToEachString<T>(this IEnumerable<T> list)
        {
            return string.Join(",", list.Select(s => s.ToString()).ToArray());
        }

        public static int IndexOf(this System.Array lst, object obj)
        {
            return System.Array.IndexOf(lst, obj);
        }

        public static int IndexOf<T>(this T[] lst, T obj)
        {
            return System.Array.IndexOf(lst, obj);
        }

        public static bool IsEmpty(this IEnumerable lst)
        {
            if (lst is IList)
            {
                return (lst as IList).Count == 0;
            }
            else
            {
                return !lst.GetEnumerator().MoveNext();
            }
        }
    }
}
