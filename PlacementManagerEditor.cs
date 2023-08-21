using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TileSpawner))]
public class PlacementManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TileSpawner placementManager = (TileSpawner)target;
        if (GUILayout.Button("Build Tiling"))
            placementManager.BuildTiling();
        EditorGUILayout.Space();
        if (GUILayout.Button("Clear Tiling"))
            placementManager.DestroyTiling();
        EditorGUILayout.Space();
        DrawDefaultInspector();
    }
}
