using UnityEngine;
using UnityEngine.EventSystems;

namespace MustHave.DesignPatterns
{
    public class UISingleton<T> : UIBehaviour where T : UIBehaviour
    {
        protected static T _instance;

        protected override void Awake()
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

        protected override void OnDestroy()
        {
            if (this == _instance)
            {
                _instance = null;
            }
        }
    }
}