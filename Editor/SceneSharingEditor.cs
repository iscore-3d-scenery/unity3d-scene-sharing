using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SceneSharing))]
[CanEditMultipleObjects]
public class SceneSharingEditor : Editor {

    SerializedProperty exportPath, sender, receiver, isSharing;
    bool showFileExportMenu = true;
    bool showNetworkSharingMenu = true;

    void OnEnable()
    {
        exportPath = serializedObject.FindProperty("exportPath");
        sender = serializedObject.FindProperty("sender");
        receiver = serializedObject.FindProperty("receiver");
        isSharing = serializedObject.FindProperty("isSharing");

        SceneSharingLauncher.addScene((SceneSharing) serializedObject.targetObject);
    }

    public override void OnInspectorGUI()
    {
        showFileExportMenu = EditorGUILayout.Foldout(showFileExportMenu, "File exportation");
        if (showFileExportMenu)
        {
            EditorGUILayout.PropertyField(exportPath);
            if (GUILayout.Button("Select File"))
            {
                exportPath.stringValue = EditorUtility.SaveFilePanel("Export file", Application.dataPath, "export", "gltf");
            }
        }

        showNetworkSharingMenu = EditorGUILayout.Foldout(showNetworkSharingMenu, "Network sharing");
        if (showNetworkSharingMenu)
        {
            EditorGUILayout.PropertyField(sender);
            EditorGUILayout.PropertyField(receiver);
            EditorGUILayout.PropertyField(isSharing);

            EditorGUILayout.LabelField("");
            EditorGUILayout.LabelField("Server " + receiver.stringValue + " not responding !");
            EditorGUILayout.LabelField("No data received");
            if(isSharing.boolValue)
                EditorGUILayout.LabelField("Sending data to the server...");
        }

        serializedObject.ApplyModifiedProperties();
    }

    public void OnInspectorUpdate()
    {
        serializedObject.Update();
        //this.Repaint();
    }
}
