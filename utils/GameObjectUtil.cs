using UnityEngine;
using System.Collections;

namespace lpesign
{
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

        static public void LogNullCheck(object obj)
        {
            LogNullCheck("", obj);
        }

        static public void LogNullCheck(string prefix, object obj)
        {
            if (prefix.IsNotEmpty()) Debug.Log("'" + prefix + "' is " + (obj == null ? "null" : "not null"));
            else Debug.Log(obj == null ? "null" : "not null");
        }
    }
}

