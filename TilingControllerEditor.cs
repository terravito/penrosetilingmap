using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TilingController))]
public class TilingControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TilingController controller = (TilingController)target;
        if (GUILayout.Button("Set Prefab Mode"))
            controller.SetPrefabMode();
        if (GUILayout.Button("Clear Prefab Mode"))
            controller.ClearPrefabMode();
        if (GUILayout.Button("Enable Tiles"))
            controller.EnableTiles();
        if (GUILayout.Button("Initialize Active Tiles"))
            controller.InitializeActiveTiles();
        if (GUILayout.Button("Cascade Pentagrid"))
            controller.CascadePentagrid();
        DrawDefaultInspector();
    }
}
