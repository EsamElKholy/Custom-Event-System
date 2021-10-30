using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;

public partial class EventManagerEditor : EditorWindow
{
    private void DrawEventManagerSection()
    {
        GUILayout.BeginArea(new Rect(2, 3, position.width - 6, 27), EditorStyles.helpBox);
        GUILayout.Space(1);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Game Event Manager", EditorStyles.helpBox, GUILayout.Width(((position.width - 6) / 4)));
        EventManager = (GameEventManager)EditorGUILayout.ObjectField(EventManager, typeof(GameEventManager), false, GUILayout.Width(((position.width - 6) * 3 / 4) - 15));
        EditorGUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
}