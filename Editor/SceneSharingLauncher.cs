using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// SceneSharing Manager, to perform an Update to each SceneSharing object at each frame

[InitializeOnLoad]
class SceneSharingLauncher
{
    static List<SceneSharing> scenesList = new List<SceneSharing>();
    static SceneSharingLauncher()
    {
        EditorApplication.update += SceneSharingLauncher.Update;
    }

    public static void Update()
    {
        foreach (SceneSharing s in scenesList)
            s.UpdateMe();
    }
    
    public static void addScene(SceneSharing s)
    {
        scenesList.Add(s);
        s.Init(new Preset());
    }
}