using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameEvent))]
public class EventEditor : Editor
{
    private bool UseInEditMode;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();
        UseInEditMode = GUILayout.Toggle(UseInEditMode, "Use In Edit Mode");
        EditorGUILayout.Space();

        GameEvent e = target as GameEvent;

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
