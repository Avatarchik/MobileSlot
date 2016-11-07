using UnityEngine;

namespace lpesign
{

    /// <summary>
    /// 씬이 변해도 지워지지 않는 싱글톤. 갱신 될 수 있다.
    /// 한번 생성 된 뒤 다른 객체가 나온 경우 나중에 만들어진 객체가 우선된다.
    /// </summary>

    public class SingletonHumble<T> : MonoBehaviour where T : Component
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

        public float InitializationTime;

        protected virtual void Awake()
        {
            InitializationTime = Time.time;

            T[] check = FindObjectsOfType<T>();
            foreach (T searched in check)
            {
                if (searched != this)
                {
                    if (searched.GetComponent<SingletonHumble<T>>().InitializationTime < InitializationTime)
                    {
                        Destroy(searched.gameObject);
                    }
                }
            }

            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
    }
}