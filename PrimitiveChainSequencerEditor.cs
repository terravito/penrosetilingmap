using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PrimitiveChainSequencer))]
public class PrimitiveChainSequencerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PrimitiveChainSequencer sequencer = (PrimitiveChainSequencer)target;
        if (GUILayout.Button("Load All Chains in Asset Database"))
            sequencer.LoadChainsInAssetDatabase();
        if (GUILayout.Button("Sort All Chains by Length"))
            sequencer.SortAllChainsByLength();
        if (GUILayout.Button("Sort Chains by Length Up to Target Index"))
            sequencer.SortChainsByLengthUpToTarget();
        if (GUILayout.Button("Select One Chain of Target Index"))
            sequencer.SelectFirstChain();
        if (GUILayout.Button("Select All Chains of Target Index"))
            sequencer.SelectAllChains();
        if (GUILayout.Button("Count All Chains in Output"))
            sequencer.SequenceOutput();
        if (GUILayout.Button("Count All Chains in Input"))
            sequencer.SequenceInput();
        DrawDefaultInspector();
    }
}
