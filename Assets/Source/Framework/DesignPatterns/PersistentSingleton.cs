using UnityEngine;
using System.Collections;

namespace DC
{
	public class PersistentSingleton<T> : Singleton<T> where T : MonoBehaviour
	{
		protected override void Awake()
		{
			if (instance == null || instance == this)
			{
				instance = this as T;
				DontDestroyOnLoad(gameObject);
				OnAwake();
			}
			else if (instance != this)
			{
				Destroy(gameObject);
			}
		}
	}
}