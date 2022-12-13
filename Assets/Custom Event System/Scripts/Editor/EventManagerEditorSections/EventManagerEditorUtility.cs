using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;

public partial class EventManagerEditor : EditorWindow
{
    public class Resizer
    {
        public Rect Area;
        public float SizeRatio;
        public bool IsResizing;
        public GUIStyle Style;

        public Resizer(Rect area, float ratio, GUIStyle style)
        {
            Area = area;
            SizeRatio = ratio;
            Style = style;
        }
    }

    private Resizer resizer;

    private void DrawResizer()
    {
        resizer.Area.x = (position.width * resizer.SizeRatio);

        GUILayout.BeginArea(resizer.Area, resizer.Style);
        GUILayout.EndArea();

        EditorGUIUtility.AddCursorRect(resizer.Area, MouseCursor.ResizeHorizontal);
    }

    private void DrawSeparator(float width)
    {
        string sep = "";

        for (int j = 0; j < width; j++)
        {
            sep += '-';
        }

        EditorGUILayout.LabelField(sep, GUILayout.Width(width));
    }

    private void ProcessEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0 && resizer.Area.Contains(e.mousePosition))
                {
                    resizer.IsResizing = true;
                }
                break;

            case EventType.MouseUp:
                resizer.IsResizing = false;
                break;
        }

        Resize(e);
    }

    private void Resize(Event e)
    {
        if (resizer.IsResizing)
        {
            resizer.SizeRatio = e.mousePosition.x / position.width;
            if (resizer.SizeRatio < 0)
            {
                resizer.SizeRatio = 0.01f;
            }

            if (resizer.SizeRatio > 1)
            {
                resizer.SizeRatio = 0.95f;
            }
            Repaint();
        }
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];

        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();

        return result;
    }

    private void RefreshSceneNames(bool path)
    {
        if (EventManager)
        {
            {
                {
                    SceneNames = new Dictionary<string, string>();

                    for (int i = 0; i < EventManager._Scenes.Count; i++)
                    {
                        if (EventManager._Scenes[i] == null)
                        {
                            continue;
                        }

                        var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(EventManager._Scenes[i]));

                        if (SceneNames.ContainsKey(guid) == false)
                        {
                            if (path)
                            {
                                SceneNames.Add(guid, AssetDatabase.GetAssetPath(EventManager._Scenes[i]));
                            }
                            else
                            {
                                SceneNames.Add(guid, EventManager._Scenes[i].name);
                            }
                        }
                    }
                }

                SceneDropdownNames = new string[SceneNames.Count];
                SceneNames.Values.CopyTo(SceneDropdownNames, 0);
            }
        }
    }

    private void RefreshPrefabNames(bool path)
    {
        if (EventManager)
        {
            {
                {
                    PrefabNames = new Dictionary<string, string>();

                    for (int i = 0; i < EventManager._Prefabs.Count; i++)
                    {
                        if (EventManager._Prefabs[i] == null)
                        {
                            continue;
                        }

                        var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(EventManager._Prefabs[i]));

                        if (PrefabNames.ContainsKey(guid) == false)
                        {
                            if (path)
                            {
                                PrefabNames.Add(guid, AssetDatabase.GetAssetPath(EventManager._Prefabs[i]));
                            }
                            else
                            {
                                PrefabNames.Add(guid, EventManager._Prefabs[i].name);
                            }
                        }
                    }
                }

                PrefabDropdownNames = new string[PrefabNames.Count];
                PrefabNames.Values.CopyTo(PrefabDropdownNames, 0);
            }
        }
    }
}