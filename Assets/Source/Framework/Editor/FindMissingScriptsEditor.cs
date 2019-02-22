using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DC;

public class FindMissingScriptsEditor : Editor
{
	private static int missingCount = -1;

	private const string SCENES_FOLDER_PATH = "Scenes";

	private const string MENU_ITEM_PATH = "Tools/Find Missing Scripts";

	[MenuItem(MENU_ITEM_PATH + "/Find Missing Scripts In Scenes")]
	public static void OnFindMissingScriptsInScenesClick()
	{
		FindMissingScripts(true, false);
	}

	[MenuItem(MENU_ITEM_PATH + "/Find Missing Scripts In Prefabs")]
	public static void OnFindMissingScriptsInPrefabsClick()
	{
		FindMissingScripts(false, true);
	}

	[MenuItem(MENU_ITEM_PATH + "/Find Missing Scripts In Project")]
	public static void OnFindMissingScriptsInProjectClick()
	{
		FindMissingScripts(true, true);
	}

	[MenuItem(MENU_ITEM_PATH + "/Clear Progress Bar")]
	public static void ClearProgressBar()
    {
        EditorUtility.ClearProgressBar();
    }

	private static bool FindMissingScripts(Action findMissingScripts, string missingCountString, string noMissingFoundString, out string messageLine)
	{
		missingCount = 0;
		findMissingScripts();
		if (missingCount > 0)
		{
			messageLine = string.Format(missingCountString, missingCount);
			Debug.LogWarning("FindMissingScriptsEditor: " + messageLine);
		}
		else
		{
			messageLine = noMissingFoundString;
			Debug.Log("FindMissingScriptsEditor: " + messageLine);
		}
		return missingCount > 0;
	}

	private static void FindMissingScripts(bool searchScenes, bool searchPrefabs)
	{
		string message = "";
		string messageLine;
		bool missingScriptsFound = false;

		if (searchScenes)
		{
			string scenesSuffix = (EditorApplication.isPlaying ? "current scene." : "scenes from Assets/" + SCENES_FOLDER_PATH + ".");
			FindMissingScripts(FindMissingScriptsInScenes,
				"Found {0} objects with missing scripts in " + scenesSuffix,
				"No missing scripts in " + scenesSuffix, out messageLine);
			missingScriptsFound = missingScriptsFound | missingCount > 0;
			message += messageLine + "\n";
		}
		if (searchPrefabs)
		{
			FindMissingScripts(FindMissingScriptsInPrefabs, 
				"Found {0} prefabs with missing scripts in Assets.", 
				"No missing scripts in prefabs.", out messageLine);
			missingScriptsFound = missingScriptsFound | missingCount > 0;
			message += messageLine + "\n";
		}
		if (missingScriptsFound)
		{
			message += "\nPress OK to see the full list in Console window";
		}

		GC.Collect();

		EditorUtility.ClearProgressBar();

		EditorUtility.DisplayDialog("Find Missing Scripts", message, "OK");
	}

	private static void FindMissingScriptsInScene(Scene scene)
	{
		if (!scene.IsValid())
			return;
		List<GameObject> rootGameObjects = new List<GameObject>();
		scene.GetRootGameObjects(rootGameObjects);
		for (int i = 0; i < rootGameObjects.Count; i++)
		{
			if (EditorUtility.DisplayCancelableProgressBar("Processing Scene " + scene.name + " object: " + i + "/" + rootGameObjects.Count, rootGameObjects[i].name, (float)i / (float)rootGameObjects.Count))
				break;
			FindMissingComponentsInGameObject(rootGameObjects[i], "(Scene: " + scene.name + ")");
		}
	}

	private static void FindMissingScriptsInScenes()
	{
		EditorUtility.DisplayProgressBar("Searching scenes", "", 0.0f);

		Scene currentScene = EditorSceneManager.GetActiveScene();
		string currentScenePath = currentScene.path;

		if (EditorApplication.isPlaying)
		{
			FindMissingScriptsInScene(currentScene);
			FindMissingScriptsInScene(SceneUtil.GetDontDestroyOnLoadScene());
		}
		else
		{
			string[] scenePaths = System.IO.Directory.GetFiles(Path.Combine(Application.dataPath, SCENES_FOLDER_PATH), "*.unity", System.IO.SearchOption.AllDirectories);
			foreach (var scenePath in scenePaths)
			{
				Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
				FindMissingScriptsInScene(scene);
			}
			EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
		}
	}

	private static void FindMissingScriptsInPrefabs()
	{
		EditorUtility.DisplayProgressBar("Searching Prefabs", "", 0.0f);

		string[] files = System.IO.Directory.GetFiles(Application.dataPath, "*.prefab", System.IO.SearchOption.AllDirectories);
		EditorUtility.DisplayCancelableProgressBar("Searching Prefabs", "Found " + files.Length + " prefabs", 0.0f);

		for (int i = 0; i < files.Length; i++)
		{
			string prefabPath = files[i].Replace(Application.dataPath, "Assets");
			if (EditorUtility.DisplayCancelableProgressBar("Processing Prefabs " + i + "/" + files.Length, prefabPath, (float)i / (float)files.Length))
				break;

			GameObject go = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;

			if (go != null)
			{
				FindMissingComponentsInGameObject(go, "(Prefab)");
				go = null;
				EditorUtility.UnloadUnusedAssetsImmediate(true);
			}
		}

		EditorUtility.DisplayProgressBar("Cleanup", "Cleaning up", 1.0f);

		EditorUtility.UnloadUnusedAssetsImmediate(true);
	}

    private static void FindMissingComponentsInGameObject(GameObject go, string suffix = "")
    {
        Component[] components = go.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] == null)
            {
                missingCount++;
                Transform t = go.transform;

                string componentPath = go.name;
                while (t.parent != null)
                {
                    componentPath = t.parent.name + "/" + componentPath;
                    t = t.parent;
                }
				Debug.LogWarning("FindMissingScriptsEditor: The referenced script on this Behaviour (" + componentPath + ") is missing! " + suffix, go);
			}
        }

        foreach (Transform child in go.transform)
        {
            FindMissingComponentsInGameObject(child.gameObject, suffix);
        }
    }
}
