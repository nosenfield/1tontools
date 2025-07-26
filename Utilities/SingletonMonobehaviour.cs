using Sirenix.OdinInspector;
using UnityEngine;

namespace OneTon.Singletons
{
    public class SingletonMonobehaviour<T> : MonoBehaviour where T : SingletonMonobehaviour<T>
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject obj = new GameObject(typeof(T).FullName + " SingletonMonobehaviour");
                    DontDestroyOnLoad(obj);
                    instance = obj.AddComponent<T>();
                }

                return instance;
            }
        }

        protected virtual void Awake()
        {
            if (instance != null && instance != (T)this)
            {
                Destroy((T)this);
            }
            else
            {
                instance = (T)this;
            }
        }
    }
}
