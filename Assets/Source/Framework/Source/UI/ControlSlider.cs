using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DC
{
	public class ControlSlider : Slider
	{
		[SerializeField]
		public Text valueText;

		public Action<Slider, float> onSliderValueChanged;

		protected override void Awake()
		{
			base.Awake();
			this.onValueChanged.AddListener((value) => {
				if (onSliderValueChanged != null)
					onSliderValueChanged.Invoke(this, value);
				valueText.text = this.value.ToString();
			});
		}
	}
}