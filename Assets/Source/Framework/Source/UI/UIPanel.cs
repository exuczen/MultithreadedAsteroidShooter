using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DC
{
	public class UIPanel : UIBehaviour
	{
		public bool IsShown { get { return gameObject.activeSelf; } }

		public virtual void Show()
		{
			gameObject.SetActive(true);
		}

		public virtual void Hide()
		{
			gameObject.SetActive(false);
		}
	}
}
