using UnityEngine;
using System.Collections;

public class ConditionalHideRangeAttribute : ConditionalHideAttribute
{
	public float min;
	public float max;

	public ConditionalHideRangeAttribute(float min, float max, string conditionalSourceField, bool hideInInspector) 
		: base(conditionalSourceField, hideInInspector)
	{
		this.min = min;
		this.max = max;
	}
}