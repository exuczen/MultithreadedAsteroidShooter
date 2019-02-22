// Created by Hugo "HRuivo" Ruivo, edited/adjusted by JKK
// https://hruivoportfolio.wordpress.com/2014/07/26/time-scale-controller-unity3d-editor-extension/

using UnityEditor;
using UnityEngine;

public class TimeScaleController : EditorWindow {
	private float lastTimeScale = 1f;

	/// <summary>NOTE: Unity event</summary>
	[MenuItem("Window/Time Scale Controller")]
	static void Init() {
		TimeScaleController window = (TimeScaleController)EditorWindow.GetWindow(typeof(TimeScaleController));
		window.titleContent.text = "Time Scaler";
		window.Show();
		window.minSize = new Vector2(10, 30);
		window.position = new Rect(100, 200, 560, 32);
	}

	/// <summary>NOTE: Unity event</summary>
	private void OnGUI() {
		EditorGUILayout.BeginHorizontal();
		Time.timeScale = EditorGUILayout.Slider(Time.timeScale, 0, 20);
		if (position.width > 468) {
			if (GUILayout.Button("1/4", GUILayout.Height(20), GUILayout.Width(42))) {
				Time.timeScale = 0.25f;
			}
			if (GUILayout.Button("1/2", GUILayout.Height(20), GUILayout.Width(42))) {
				Time.timeScale = 0.5f;
			}
			if (GUILayout.Button("*2", GUILayout.Height(20), GUILayout.Width(42))) {
				Time.timeScale *= 2f;
			}
			if (GUILayout.Button(":2", GUILayout.Height(20), GUILayout.Width(42))) {
				Time.timeScale /= 2f;
			}
			if (GUILayout.Button("Reset", GUILayout.Height(20), GUILayout.Width(64))) {
				Time.timeScale = 1f;
			}
			if (Time.timeScale == 0f) {
				if (GUILayout.Button("Resume", GUILayout.Height(20), GUILayout.Width(64))) {
					Time.timeScale = lastTimeScale;
				}
			} else {
				if (GUILayout.Button("Pause", GUILayout.Height(20), GUILayout.Width(64))) {
					lastTimeScale = Time.timeScale;
					Time.timeScale = 0f;
				}
			}
		}
		EditorGUILayout.EndHorizontal();
	}
}
