using UnityEngine;
using System.Collections;

namespace MustHave.DesignPatterns
{
    public class PersistentSingleton<T> : Singleton<T> where T : MonoBehaviour
    {
        protected override void Awake()
        {
            if (_instance == null || _instance == this)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
                OnAwake();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
    } 
}
