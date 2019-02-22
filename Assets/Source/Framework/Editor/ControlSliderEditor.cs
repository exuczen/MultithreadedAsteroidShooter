using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;
using DC;

[CustomEditor(typeof(ControlSlider))]
public class ControlSliderEditor : Editor
{
	public override void OnInspectorGUI()
	{
		ControlSlider myTarget = (ControlSlider)target;

		// Show default inspector property editor
		DrawDefaultInspector();
	}
}