using UnityEngine;
using System.Collections;

namespace MustHave.DesignPatterns
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T _instance;

        protected virtual void Awake()
        {
            if (_instance == null || _instance == this)
            {
                _instance = this as T;
                OnAwake();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnAwake() { }

        public static T Instance
        {
            get {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();

                    if (_instance == null)
                    {
                        Debug.LogError("An instance of " + typeof(T) +
                           " is needed in the scene, but there is none.");
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Create instance from prefab if it was not found in the scene
        /// </summary>
        /// <param name="prefab"></param>
        public static void FindOrCreateInstance(T prefab)
        {
            _instance = (_instance ?? FindObjectOfType<T>()) ?? Instantiate(prefab, Vector3.zero, Quaternion.identity);
        }

        public static GameObject GameObject { get { return Instance.gameObject; } }

        public static Transform Transform { get { return Instance.transform; } }

        protected virtual void OnDestroy()
        {
            if (this == _instance)
            {
                _instance = null;
            }
        }
    } 
}
