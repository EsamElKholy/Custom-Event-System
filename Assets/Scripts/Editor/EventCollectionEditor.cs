using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameEventCollection))]
public class EventCollectionEditor : Editor
{
    private bool UseInEditMode;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();
        UseInEditMode = GUILayout.Toggle(UseInEditMode, "Use In Edit Mode");
        EditorGUILayout.Space();

        GameEventCollection e = target as GameEventCollection;

        if (GUILayout.Button("Raise"))
        {
            if ((UseInEditMode && !Application.isPlaying)
                || (!UseInEditMode && Application.isPlaying))
            {
                e.Raise();
            }
        }
    }
}
