using UnityEditor;
using UnityEngine;

namespace KAI
{
    [CustomEditor(typeof(GameEvent))]
    public class EventEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GameEvent e = target as GameEvent;

            EditorGUILayout.Space();
            e.UseInEditMode = GUILayout.Toggle(e.UseInEditMode, "Use In Edit Mode");
            EditorGUILayout.Space();

            if (GUILayout.Button("Raise"))
            {
                if ((e.UseInEditMode && !Application.isPlaying)
                    || (!e.UseInEditMode && Application.isPlaying))
                {
                    e.Raise();
                }
            }
        }
    }
}
