using UnityEngine;
using UnityEngine.EventSystems;

namespace DC
{
    public class UISingleton<T> : UIBehaviour where T : UIBehaviour
    {
        protected static T instance;

        protected override void Awake()
        {
            if (instance == null || instance == this)
            {
                instance = this as T;
                OnAwake();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnAwake() { }

        public static T Instance
        {
            get {
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();

                    if (instance == null)
                    {
                        Debug.LogError("An instance of " + typeof(T) +
                           " is needed in the scene, but there is none.");
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// Create instance from prefab if it was not found in the scene
        /// </summary>
        /// <param name="prefab"></param>
        public static void FindOrCreateInstance(T prefab)
        {
            instance = (instance ?? FindObjectOfType<T>()) ?? Instantiate(prefab, Vector3.zero, Quaternion.identity);
        }

        public static GameObject GameObject { get { return Instance.gameObject; } }

        public static Transform Transform { get { return Instance.transform; } }

        protected override void OnDestroy()
        {
            if (this == instance)
            {
                instance = null;
            }
        }
    }
}