using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using SimpleJSON;
using System.IO;

[ExecuteInEditMode]
public class SceneSharing : MonoBehaviour{
    GameObject[] sceneObjects;
    Scene activeScene;

    GameObject exporterGO;
    SceneToGlTFWiz exporter;
    Preset preset;

    bool init = false;

    public string exportPath;
    public string sender;
    public string receiver;
    public bool isSharing = false; 

    void UpdateScene()
    {
        activeScene = SceneManager.GetActiveScene();
        sceneObjects = activeScene.GetRootGameObjects();

        //ExportScene();
        Debug.Log("The scene has been successfully changed !");

    }

    void ExportScene()
    {
        Debug.Log("Exporting Scene...");

        Object[] previousSelection = Selection.objects;
        Selection.objects = sceneObjects;
        //preset.Load(activeScene.path);
        if(exportPath != string.Empty)
            exporter.ExportCoroutine(exportPath, preset, false, true, true);

        Selection.objects = previousSelection;
        Debug.Log(activeScene.path);
        Debug.Log("Exporting done");
    } 

    // Use this for initialization
    void Start ()
    {
        init = true;
        Debug.Log("SceneSharing Initialization...");

        UpdateScene();
        exporterGO = new GameObject(); 
        exporter = exporterGO.AddComponent<SceneToGlTFWiz>();
        Debug.Log(exporter.ToString());      
    }

    public void Init(Preset p)
    {
        preset = p;
    }
	
	// Update is called once per frame
	public void UpdateMe ()
    {
        if (!init)
            Start();
                 
        if (activeScene.path != SceneManager.GetActiveScene().path)
            UpdateScene();

        if (isSharing)
        {
            Debug.Log("Sharing...");
            ExportScene();
        }
    }
     
    void OnDestroy()
    {
        Debug.Log("Destroying...");
        GameObject.DestroyImmediate(exporterGO);
        exporter = null;
        preset = null;
    }
}
