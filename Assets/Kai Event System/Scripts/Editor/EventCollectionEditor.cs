using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KAI
{
    [CustomEditor(typeof(GameEventCollection))]
    public class EventCollectionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GameEventCollection e = target as GameEventCollection;

            EditorGUILayout.Space();
            e.UseInEditMode = GUILayout.Toggle(e.UseInEditMode, "Use In Editor Mode");
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