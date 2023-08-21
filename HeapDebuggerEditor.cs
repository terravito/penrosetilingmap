using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HeapDebugger))]
public class NewBehaviourScript : Editor
{
    public override void OnInspectorGUI() 
    { 
        HeapDebugger heapDebugger = (HeapDebugger)target;
        if (GUILayout.Button("Execute Code"))
            heapDebugger.ExecuteCode();
        DrawDefaultInspector();
    }
}
