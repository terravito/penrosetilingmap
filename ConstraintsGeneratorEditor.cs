using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ConstraintsGenerator))]
public class ConstraintsGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ConstraintsGenerator generator = (ConstraintsGenerator)target;
        if (GUILayout.Button("Generate Constraints"))
            generator.GenerateContraints();
        if (GUILayout.Button("Generate Possiblity Space"))
            generator.GeneratePossibilitySpace();
        DrawDefaultInspector();
    }
}
