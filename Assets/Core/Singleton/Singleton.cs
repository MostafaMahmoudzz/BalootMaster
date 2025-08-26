using UnityEngine;

namespace Pebble
{
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        private static T s_instance;
        private static object s_lock = new object();
        private static bool s_applicationIsQuitting = false;

        public static T Instance
        {
            get
            {
                if (s_applicationIsQuitting)
                {
                    return null;
                }

                lock (s_lock)
                {
                    if (s_instance == null)
                    {
                        s_instance = FindObjectOfType<T>();
                        if (s_instance == null)
                        {
                            GameObject obj = new GameObject(typeof(T).Name);
                            s_instance = obj.AddComponent<T>();
                        }
                    }
                    return s_instance;
                }
            }
        }

        public virtual void Awake()
        {
            if (s_instance == null)
            {
                s_instance = this as T;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        public virtual void OnDestroy()
        {
            s_applicationIsQuitting = true;
        }
    }
}

