using UnityEngine;

namespace lpesign
{
    /// <summary>
    /// 전역접근만을 위한 심플 싱글톤. 씬이 변하면 제거된다.
    /// 다수가 생성 될 경우 _instance 의 참조만 변경되고 모든 객체는 유지된다.
    /// </summary>
    public class SingletonSimple<T> : MonoBehaviour where T : Component
    {
        protected static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject(typeof(T).Name);
                        obj.hideFlags = HideFlags.HideAndDontSave;
                        _instance = obj.AddComponent<T>();
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
            _instance = this as T;
        }

        void OnDestroy()
        {
            _instance = null;
        }
    }
}


