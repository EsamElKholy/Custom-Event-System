using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;

public partial class EventManagerEditor : EditorWindow
{
    public class SearchBar
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

                if (searchString.Length == 0 && CurrentTab == 1)
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
                            PrefabNames.Add(guid, EventManager._Prefabs[i].name);
                        }
                    }
                }

                SceneDropdownNames = new string[SceneNames.Count];
                SceneNames.Values.CopyTo(SceneDropdownNames, 0);

                scenes = new List<SceneAsset>();
                for (int i = 0; i < SceneDropdownNames.Length; i++)
                {
                    for (int j = 0; j < EventManager._Scenes.Count; j++)
                    {
                        if (EventManager._Scenes[j] && EventManager._Scenes[j].name.Equals(SceneDropdownNames[i]))
                        {
                            scenes.Add(EventManager._Scenes[j]);

                            break;
                        }
                    }
                }

                PrefabDropdownNames = new string[PrefabNames.Count];
                PrefabNames.Values.CopyTo(PrefabDropdownNames, 0);

                prefabs = new List<GameObject>();
                for (int i = 0; i < PrefabDropdownNames.Length; i++)
                {
                    for (int j = 0; j < EventManager._Prefabs.Count; j++)
                    {
                        if (EventManager._Prefabs[j] && EventManager._Prefabs[j].name.Equals(PrefabDropdownNames[i]))
                        {
                            prefabs.Add(EventManager._Prefabs[j]);

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
                        if (result.Length > 0)
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
                                    PrefabNames.Add(guid, EventManager._Prefabs[i].name);
                                }
                            }

                            Dictionary<string, string> prefabs = new Dictionary<string, string>();
                            foreach (var item in PrefabNames)
                            {
                                if (item.Value.ToLower().Contains(result.ToLower()))
                                {
                                    if (prefabs.ContainsKey(item.Key) == false)
                                    {
                                        prefabs.Add(item.Key, item.Value);
                                    }
                                }
                            }

                            PrefabNames = new Dictionary<string, string>();
                            foreach (var item in prefabs)
                            {
                                if (PrefabNames.ContainsKey(item.Key) == false)
                                {
                                    PrefabNames.Add(item.Key, item.Value);
                                }
                            }
                        }
                    }
                    break;
                case 2:
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
                case 3:
                    {
                        RefreshSceneNames(true);
                        RefreshPrefabNames(true);
                        List<string> selectedScenes = new List<string>();
                        List<string> selectedPrefabs = new List<string>();

                        for (int i = 0; i < SceneDropdownNames.Length; i++)
                        {
                            int layer = 1 << i;

                            if ((sceneFilter & layer) != 0)
                            {
                                selectedScenes.Add(SceneDropdownNames[i]);
                            }
                        }

                        for (int i = 0; i < PrefabDropdownNames.Length; i++)
                        {
                            int layer = 1 << i;

                            if ((prefabFilter & layer) != 0)
                            {
                                selectedPrefabs.Add(PrefabDropdownNames[i]);
                            }
                        }

                        {
                            if (EventManager)
                            {
                                EventManager.FindAllListeners(selectedScenes, selectedPrefabs);

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
                        RefreshPrefabNames(false);
                    }
                    break;
                case 4:
                    {
                        RefreshSceneNames(true);
                        RefreshPrefabNames(true);

                        List<string> selectedScenes = new List<string>();
                        List<string> selectedPrefabs = new List<string>();

                        for (int i = 0; i < SceneDropdownNames.Length; i++)
                        {
                            int layer = 1 << i;

                            if ((sceneFilter & layer) != 0)
                            {
                                selectedScenes.Add(SceneDropdownNames[i]);
                            }
                        }

                        for (int i = 0; i < PrefabDropdownNames.Length; i++)
                        {
                            int layer = 1 << i;

                            if ((prefabFilter & layer) != 0)
                            {
                                selectedPrefabs.Add(PrefabDropdownNames[i]);
                            }
                        }

                        {
                            if (EventManager)
                            {
                                EventManager.FindAllReferences(selectedScenes, selectedPrefabs);

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
                        RefreshPrefabNames(false);
                    }
                    break;
                default:
                    break;
            }
        }

        searchString = result;
    }
}
