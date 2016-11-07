using UnityEngine;

namespace lpesign
{
    /// <summary>
    /// 씬이 변해도 지워지지 않는 싱글톤. 단 부모가 있다면 지워진다.
    /// 이후에 만들어진 객체는 제거되고 첫 생성된 객체만 유지한다.
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        protected static T _instance;
        static public T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();

                    if (_instance == null)
                    {
                        GameObject go = new GameObject(typeof(T).Name);
                        _instance = go.AddComponent<T>();
                    }
                }

                return _instance;
            }
        }

        public static T SetParent(Transform parent)
        {
            if (parent != null)
            {
                Transform me = Instance.transform;
                me.SetParent(parent);
            }

            return Instance;
        }

        public static T SetParent(GameObject go)
        {
            return SetParent(go.transform);
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                if( transform.parent == null ) DontDestroyOnLoad(gameObject);
            }
            else
            {
                if (_instance != this)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
