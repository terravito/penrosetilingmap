using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(PentaCoord))]
public class PentaCoordUIE : PropertyDrawer
{
    private const float SubLabelSpacing = 4;
    private const float BottomSpacing = 2;

    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
    {
        pos.height -= BottomSpacing;
        label = EditorGUI.BeginProperty(pos, label, prop);
        var contentRect = EditorGUI.PrefixLabel(pos, GUIUtility.GetControlID(FocusType.Passive), label);
        var labels = new[] { new GUIContent("U"), new GUIContent("V")};
        var properties = new[] { prop.FindPropertyRelative("u"), prop.FindPropertyRelative("v") };
        DrawMultiplePropertyFields(contentRect, labels, properties);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label) + BottomSpacing;
    }


    private static void DrawMultiplePropertyFields(Rect pos, GUIContent[] subLabels, SerializedProperty[] props)
    {
        // backup gui settings
        var indent = EditorGUI.indentLevel;
        var labelWidth = EditorGUIUtility.labelWidth;

        // draw properties
        var propsCount = props.Length;
        var width = (pos.width - (propsCount - 1) * SubLabelSpacing) / propsCount;
        var contentPos = new Rect(pos.x, pos.y, width, pos.height);
        EditorGUI.indentLevel = 0;
        for (var i = 0; i < propsCount; i++)
        {
            EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(subLabels[i]).x;
            EditorGUI.PropertyField(contentPos, props[i], subLabels[i]);
            contentPos.x += width + SubLabelSpacing;
        }

        // restore gui settings
        EditorGUIUtility.labelWidth = labelWidth;
        EditorGUI.indentLevel = indent;
    }
}
