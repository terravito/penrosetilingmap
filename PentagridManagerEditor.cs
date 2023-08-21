using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
[CustomEditor(typeof(Pentagrid))]
public class PentagridManagerEditor : Editor
{
    private Material material;
    private List<Vector3> vertexBuffer;
    private List<Color> colorBuffer;
    private Rect layoutRectangle;
    private Pentagrid pentagrid;

    void OnEnable()
    {
        // Find the "Hidden/Internal-Colored" shader, and cache it for use.
        material = new Material(Shader.Find("Hidden/Internal-Colored"));

        // Label target as pentagrid
        pentagrid = (Pentagrid)target;
    }

    public override void OnInspectorGUI()
    {
        // Begin to draw a horizontal layout, using the helpBox EditorStyle
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        // Reserve GUI space with a width and height of 350, and cache it as a rectangle.
        layoutRectangle = GUILayoutUtility.GetRect(350, 350, 350, 350);

        // Update vertex and color buffers for drawing.
        UpdateVertexBuffer();
        UpdateColorBuffer();

        if (Event.current.type == EventType.Repaint && vertexBuffer != null)
        {
            // If we are currently in the Repaint event, begin to draw a clip of the size of 
            // previously reserved rectangle, and push the current matrix for drawing.
            GUI.BeginClip(layoutRectangle);
            GL.PushMatrix();

            // Clear the current render buffer, setting a new background colour, and set our
            // material for rendering.
            GL.Clear(true, false, Color.black);
            material.SetPass(0);

            // Start drawing in OpenGL Quads, to draw the background canvas. Set the
            // colour black as the current OpenGL drawing colour, and draw a quad covering
            // the dimensions of the layoutRectangle.
            GL.Begin(GL.QUADS);
            GL.Color(new Color(0.07f,0.07f,0.07f));
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(layoutRectangle.width, 0, 0);
            GL.Vertex3(layoutRectangle.width, layoutRectangle.height, 0);
            GL.Vertex3(0, layoutRectangle.height, 0);
            GL.End();

            // Loop through vertex and color buffers and use the GL.LINES to draw a line for each pair of vertices.
            for (int i = 0; i < vertexBuffer.Count; i += 2)
            {
                GL.Begin(GL.LINES);
                GL.Color(colorBuffer[i / 2]);
                GL.Vertex3(vertexBuffer[i].x, vertexBuffer[i].y, vertexBuffer[i].z);
                GL.Vertex3(vertexBuffer[i + 1].x, vertexBuffer[i+1].y, vertexBuffer[i+1].z);
                GL.End();
            }

            // Pop the current matrix for rendering, and end the drawing clip.
            GL.PopMatrix();
            GUI.EndClip();
        }

        // End our horizontal 
        GUILayout.EndHorizontal();

        // Add a button to update the pentagrid data
        if (GUILayout.Button("Update Pentagrid"))
        {
            pentagrid.UpdatePentagridData();
        }

        // Draw the default inspector
        DrawDefaultInspector();
    }

    void UpdateVertexBuffer()
    {
        if (pentagrid != null && pentagrid.GetVertexBuffer() != null)
            vertexBuffer = Geometry.VectorListXZToRect(pentagrid.GetVertexBuffer(), layoutRectangle);
    }

    void UpdateColorBuffer()
    {
        if (pentagrid != null && pentagrid.GetColorBuffer() != null)
            colorBuffer = pentagrid.GetColorBuffer();
    }
}
