using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;

public partial class EventManagerEditor : EditorWindow
{    public class SearchBar
    {
        public Rect Area;

        public void UpdateArea(float x, float y, float w, float h)
        {
            if (Area == null)
            {
                Area = new Rect(x, y, w, h);
            }
            else
            {
                Area.x = x;
                Area.y = y;
                Area.width = w;
                Area.height = h;
            }
        }
    }

    private SearchBar searchBar;
    private SearchField searchField;
    private string searchString = "";

    private void DrawSearchSection()
    {
        GUILayout.BeginArea(searchBar.Area, EditorStyles.helpBox);
        searchBar.UpdateArea(2, 31, position.width - 6, 25);

        GUILayout.BeginVertical();

        Rect searchArea = new Rect(5, 5, position.width - 15, 18);
        Rect searchBarArea = new Rect(2, 2, position.width - 15, 18);
        GUILayout.BeginArea(searchBarArea);

        if (EventManager)
        {
            {
                if (searchString.Length == 0 && CurrentTab == 0)
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
                            SceneNames.Add(guid, EventManager._Scenes[i].name);
                        }
                    }
                }

                DropdownNames = new string[SceneNames.Count];
                SceneNames.Values.CopyTo(DropdownNames, 0);

                scenes = new List<SceneAsset>();
                for (int i = 0; i < DropdownNames.Length; i++)
                {
                    for (int j = 0; j < EventManager._Scenes.Count; j++)
                    {
                        if (EventManager._Scenes[j] && EventManager._Scenes[j].name.Equals(DropdownNames[i]))
                        {
                            scenes.Add(EventManager._Scenes[j]);

                            break;
                        }
                    }
                }
            }
        }
        GUILayout.Space(1);
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Type the name of the item you are looking for", EditorStyles.helpBox, GUILayout.Width(searchBarArea.width / 4));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        DrawSearchBar(new Rect(searchBarArea.width / 4 + 7, searchBarArea.y, searchBarArea.width * 3 / 4 - 5, searchBarArea.height));
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private void DrawSearchBar(Rect area)
    {
        DoSearchField(area);
    }

    private void DoSearchField(Rect rect)
    {
        if (searchField == null)
        {
            searchField = new SearchField();
        }

        var result = searchField.OnToolbarGUI(rect, searchString);

        if (EventManager == null)
        {
            return;
        }

        if (result != searchString)
        {
            switch (CurrentTab)
            {
                case 0:
                    {
                        if (result.Length > 0)
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
                                    SceneNames.Add(guid, EventManager._Scenes[i].name);
                                }
                            }

                            Dictionary<string, string> scenes = new Dictionary<string, string>();
                            foreach (var item in SceneNames)
                            {
                                if (item.Value.ToLower().Contains(result.ToLower()))
                                {
                                    if (scenes.ContainsKey(item.Key) == false)
                                    {
                                        scenes.Add(item.Key, item.Value);
                                    }
                                }
                            }

                            SceneNames = new Dictionary<string, string>();
                            foreach (var item in scenes)
                            {
                                if (SceneNames.ContainsKey(item.Key) == false)
                                {
                                    SceneNames.Add(item.Key, item.Value);
                                }
                            }
                        }
                    }
                    break;
                case 1:
                    {
                        if (EventManager)
                        {
                            EventManager.FindAllEvents();
                        }

                        Dictionary<string, EventData> results = new Dictionary<string, EventData>();
                        foreach (var item in EventManager.Events)
                        {
                            if (item.Key.ToLower().Contains(result.ToLower()))
                            {
                                results.Add(item.Key, item.Value);
                            }
                        }

                        EventManager.Events = results;
                    }
                    break;
                case 2:
                    {
                        RefreshSceneNames(true);
                        List<string> selectedScenes = new List<string>();

                        for (int i = 0; i < DropdownNames.Length; i++)
                        {
                            int layer = 1 << i;

                            if ((filter & layer) != 0)
                            {
                                selectedScenes.Add(DropdownNames[i]);
                            }
                        }

                        {
                            if (EventManager)
                            {
                                EventManager.FindAllListeners(selectedScenes);

                                List<GameEventListener> newResult = new List<GameEventListener>();

                                for (int i = 0; i < EventManager.Listeners.Count; i++)
                                {
                                    if (EventManager.Listeners[i].name.ToLower().Contains(result.ToLower()))
                                    {
                                        newResult.Add(EventManager.Listeners[i]);
                                    }
                                }

                                EventManager.Listeners = newResult;
                            }
                        }

                        RefreshSceneNames(false);
                    }
                    break;
                case 3:
                    {
                        RefreshSceneNames(true);

                        List<string> selectedScenes = new List<string>();

                        for (int i = 0; i < DropdownNames.Length; i++)
                        {
                            int layer = 1 << i;

                            if ((filter & layer) != 0)
                            {
                                selectedScenes.Add(DropdownNames[i]);
                            }
                        }

                        {
                            if (EventManager)
                            {
                                EventManager.FindAllReferences(selectedScenes);

                                List<EventReference> newResult = new List<EventReference>();

                                for (int i = 0; i < EventManager.References.Count; i++)
                                {
                                    if (EventManager.References[i].Reference.name.ToLower().Contains(result.ToLower()))
                                    {
                                        newResult.Add(EventManager.References[i]);
                                    }
                                }

                                EventManager.References = newResult;
                            }
                        }

                        RefreshSceneNames(false);
                    }
                    break;
                default:
                    break;
            }
        }

        searchString = result;
    }
}
