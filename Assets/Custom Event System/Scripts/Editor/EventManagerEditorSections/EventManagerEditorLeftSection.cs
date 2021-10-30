using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;

public partial class EventManagerEditor : EditorWindow
{
    private void DrawLeftDataPanel()
    {
        tabBar = new Rect(2, 60, position.width * resizer.SizeRatio - 3, tabBarHeight);
        GUILayout.BeginArea(tabBar, EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        int tab = -1;
        tab = GUILayout.Toolbar(CurrentTab, TabTitles);

        if (tab != CurrentTab)
        {
            searchString = "";
            showRightPanel = false;
            filter = 0;
            GUI.FocusControl("");
        }

        CurrentTab = tab;

        EditorGUILayout.EndHorizontal();
        GUILayout.EndArea();
        Rect leftDataArea = new Rect(tabBar.x, tabBar.y + tabBar.height, (position.width * resizer.SizeRatio - 3), position.height - 60 - tabBar.height - 3);

        if (CurrentTab == 0)
        {
            leftDataArea.height -= 25;
        }
        GUILayout.BeginArea(leftDataArea, EditorStyles.helpBox);
        using (var v = new EditorGUILayout.VerticalScope())
        {
            using (var scrollView = new EditorGUILayout.ScrollViewScope(leftScroll, GUILayout.Width(leftDataArea.width - 5), GUILayout.Height(leftDataArea.height - 1)))
            {
                leftScroll = scrollView.scrollPosition;

                switch (CurrentTab)
                {
                    case 0:
                        {
                            DrawLeftSceneSection(leftDataArea);
                        }
                        break;
                    case 1:
                        {
                            DrawLeftEventSection(leftDataArea);
                        }
                        break;
                    case 2:
                        {
                            DrawLeftListenerSection(leftDataArea);
                        }
                        break;
                    case 3:
                        {
                            DrawLeftReferenceSection(leftDataArea);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        GUILayout.EndArea();

        switch (CurrentTab)
        {
            case 0:
                {
                    GUILayout.BeginArea(new Rect(leftDataArea.x, leftDataArea.height + leftDataArea.y + 3, leftDataArea.width, 25));
                    if (GUILayout.Button("Add Scene"))
                    {
                        scenePickerID = EditorGUIUtility.GetControlID(FocusType.Keyboard);

                        EditorGUIUtility.ShowObjectPicker<SceneAsset>(null, false, "", scenePickerID);
                    }

                    if (Event.current.commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID() == scenePickerID)
                    {
                        scenePickerID = -1;
                    }
                    else if (Event.current.commandName == "ObjectSelectorClosed" && EditorGUIUtility.GetObjectPickerControlID() == scenePickerID)
                    {
                        pickedScene = (SceneAsset)EditorGUIUtility.GetObjectPickerObject();
                    }

                    GUILayout.EndArea();
                }
                break;
            default:
                break;
        }
    }

    private void DrawLeftSceneSection(Rect leftDataArea)
    {
        if (selectedScene && showRightPanel == false)
        {
            var path = AssetDatabase.GetAssetPath(selectedScene);
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

            if (scene.IsValid() && scene.isLoaded)
            {
                // Right Panel
                showRightPanel = true;
                rightPanelTitle = "Scene: \"" + scene.path + "\" Statistics";
            }
        }

        EditorGUILayout.Space();
        float y = 0;
        float singleH = 55;
        float h = scenes.Count * 1 * singleH;
        float nh = leftDataArea.height;
        float nw = leftDataArea.width;
        if (leftDataArea.height < h)
        {
            float dt = Mathf.Abs(leftDataArea.height - (h));
            nh += dt;
            nw -= 25;
        }
        else
        {
            nh -= 15;
            nw -= 8;
        }

        GUILayout.Label("", GUILayout.Width(leftDataArea.width - 29), GUILayout.Height(nh));

        for (int i = 0; i < scenes.Count * 1; i++)
        {
            GUILayout.BeginArea(new Rect(0, y, nw, 55), EditorStyles.helpBox);
            GUILayout.Space(3);
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(scenes[i].name, EditorStyles.label, GUILayout.Width(leftDataArea.width / 3 - 15));

            scenes[i] = (SceneAsset)EditorGUILayout.ObjectField(scenes[i], typeof(SceneAsset), false, GUILayout.Width((leftDataArea.width * 2 / 3) - 37));

            if (GUILayout.Button("X", EditorStyles.toolbarButton))
            {
                toBeDeletedScene = scenes[i];
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(3);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Open and Show Details"))
            {
                var path = AssetDatabase.GetAssetPath(scenes[i]);
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

                if (scene.IsValid() && scene.isLoaded)
                {
                    // Right Panel
                    showRightPanel = true;
                    selectedScene = scenes[i];
                    rightPanelTitle = "Scene: \"" + scene.path + "\" Statistics";
                }
            }
            GUILayout.Space(2);

            if (GUILayout.Button("Make Active"))
            {
                var path = AssetDatabase.GetAssetPath(scenes[i]);
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

                if (scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.SetActiveScene(scene);
                }
            }
            GUILayout.Space(2);

            if (GUILayout.Button("CloseScene"))
            {
                var path = AssetDatabase.GetAssetPath(scenes[i]);
                var scene = EditorSceneManager.GetSceneByPath(path);

                selectedScene = null;
                showRightPanel = false;
                rightPanelTitle = "";

                if (scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.SaveScene(scene);
                }

                if (scene.IsValid())
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.EndArea();

            y += 65;
        }
    }

    private void DrawLeftEventSection(Rect leftDataArea)
    {
        if (EventManager)
        {
            if (EventManager && EventManager.Events.Count == 0)
            {
                EventManager.FindAllEvents();
            }

            if (selectedEvent != null && selectedEvent.Event && showRightPanel == false)
            {
                GUI.FocusControl("");
                showRightPanel = true;
                rightPanelTitle = "Event: " + selectedEvent.Name + " Details";
            }

            float y = 0;
            float singleH = 30;
            float h = EventManager.Events.Count * singleH;
            float nh = leftDataArea.height;
            float nw = leftDataArea.width;
            float nm = 0;
            if (leftDataArea.height < h)
            {
                float dt = Mathf.Abs(leftDataArea.height - (h));
                nh += dt + 40;
                nw -= 30;
                nm = -30;
            }
            else
            {
                nh -= 30;
                nw -= 8;
                nm = -10;
            }

            if (GUILayout.Button("Find All Events in Project"))
            {
                if (EventManager)
                {
                    searchString = "";
                    EventManager.FindAllEvents();
                }
            }

            GUILayout.Label("", GUILayout.Width(leftDataArea.width - 29), GUILayout.Height(nh));

            if (EventManager)
            {
                foreach (var e in EventManager.Events)
                {
                    if (e.Value != null && e.Value.Event)
                    {
                        GUILayout.BeginArea(new Rect(0, y + 27, nw, 30), EditorStyles.helpBox);
                        GUILayout.Space(3);
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(e.Value.Event.name, GUILayout.Width(leftDataArea.width / 4 - 15)))
                        {
                            // Right Panel
                            GUI.FocusControl("");
                            selectedEvent = e.Value;
                            showRightPanel = true;
                            rightPanelTitle = "Event: " + selectedEvent.Name + " Details";
                        }
                        EditorGUILayout.ObjectField(e.Value.Event, typeof(CustomEvent), false, GUILayout.Width((leftDataArea.width * 3 / 4) - nm));
                        EditorGUILayout.EndHorizontal();
                        GUILayout.EndArea();
                        y += 32;
                    }
                }
            }
        }
    }

    private void DrawLeftListenerSection(Rect leftDataArea)
    {
        if (EventManager)
        {
            if (selectedListener && showRightPanel == false)
            {
                showRightPanel = true;
                rightPanelTitle = "Event Listener: " + selectedListener.name + " Details";
            }

            float y = 0;
            float singleH = 30;
            float h = EventManager.Listeners.Count * singleH;
            float nh = leftDataArea.height;
            float nw = leftDataArea.width;
            if (leftDataArea.height < h)
            {
                float dt = Mathf.Abs(leftDataArea.height - (h));
                nh += dt;
                nw -= 30;
            }
            else
            {
                nh -= 27;
                nw -= 8;
            }

            EditorGUILayout.BeginHorizontal();
            bool clicked = GUILayout.Button("Find Listeners in", EditorStyles.toolbarButton);
            GUILayout.Space(2);
            RefreshSceneNames(true);
            filter = EditorGUILayout.MaskField(filter, DropdownNames, EditorStyles.toolbarPopup);
            List<string> selectedScenes = new List<string>();

            for (int i = 0; i < DropdownNames.Length; i++)
            {
                int layer = 1 << i;

                if ((filter & layer) != 0)
                {
                    selectedScenes.Add(DropdownNames[i]);
                }
            }

            if (clicked)
            {
                if (EventManager)
                {
                    showRightPanel = false;
                    selectedListener = null;
                    searchString = "";
                    EventManager.FindAllListeners(selectedScenes);
                }
            }
            RefreshSceneNames(false);

            EditorGUILayout.EndHorizontal();

            GUILayout.Label("", GUILayout.Width(leftDataArea.width - 29), GUILayout.Height(nh));

            if (EventManager)
            {
                foreach (var l in EventManager.Listeners)
                {
                    if (l)
                    {
                        var listener = l;
                        GUILayout.BeginArea(new Rect(0, y + 27, nw, 30), EditorStyles.helpBox);
                        GUILayout.Space(3);
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(listener.name, GUILayout.Width(leftDataArea.width / 4 - 15)))
                        {
                            // Right Panel
                            showRightPanel = true;
                            rightPanelTitle = "Event Listener: " + listener.name + " Details";
                            selectedListener = listener;
                        }
                        listener = (GameEventListener)EditorGUILayout.ObjectField(listener, typeof(GameEventListener), false, GUILayout.Width((leftDataArea.width * 3 / 4) - 23));
                        EditorGUILayout.EndHorizontal();
                        GUILayout.EndArea();
                        y += 32;
                    }
                }
            }
        }
    }

    private void DrawLeftReferenceSection(Rect leftDataArea)
    {
        if (EventManager)
        {
            if (selectedReference != null && selectedReference.Reference && showRightPanel == false)
            {
                showRightPanel = true;
                rightPanelTitle = "Event Reference: " + selectedReference.Reference.name + " Details";
            }

            float y = 0;
            float singleH = 30;
            float h = EventManager.References.Count * singleH;
            float nh = leftDataArea.height;
            float nw = leftDataArea.width;
            if (leftDataArea.height < h)
            {
                float dt = Mathf.Abs(leftDataArea.height - (h));
                nh += dt;
                nw -= 30;
            }
            else
            {
                nh -= 27;
                nw -= 8;
            }

            EditorGUILayout.BeginHorizontal();
            bool clicked = GUILayout.Button("Find References in", EditorStyles.toolbarButton);
            GUILayout.Space(2);
            RefreshSceneNames(true);
            filter = EditorGUILayout.MaskField(filter, DropdownNames, EditorStyles.toolbarPopup);

            List<string> selectedScenes = new List<string>();

            for (int i = 0; i < DropdownNames.Length; i++)
            {
                int layer = 1 << i;

                if ((filter & layer) != 0)
                {
                    selectedScenes.Add(DropdownNames[i]);
                }
            }

            if (clicked)
            {
                if (EventManager)
                {
                    showRightPanel = false;
                    selectedReference = null;
                    searchString = "";
                    EventManager.FindAllReferences(selectedScenes);
                }
            }
            RefreshSceneNames(false);

            EditorGUILayout.EndHorizontal();

            GUILayout.Label("", GUILayout.Width(leftDataArea.width - 29), GUILayout.Height(nh));

            if (EventManager)
            {
                foreach (var r in EventManager.References)
                {
                    if (r.Reference)
                    {
                        var reference = r;
                        GUILayout.BeginArea(new Rect(0, y + 27, nw, 30), EditorStyles.helpBox);
                        GUILayout.Space(3);
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(reference.Reference.name, GUILayout.Width(leftDataArea.width / 4 - 15)))
                        {
                            // Right Panel
                            showRightPanel = true;
                            rightPanelTitle = "Event Reference: " + reference.Reference.name + " Details";
                            selectedReference = reference;
                        }
                        reference.Reference = (MonoBehaviour)EditorGUILayout.ObjectField(reference.Reference, typeof(MonoBehaviour), false, GUILayout.Width((leftDataArea.width * 3 / 4) - 23));
                        EditorGUILayout.EndHorizontal();
                        GUILayout.EndArea();
                        y += 32;
                    }
                }
            }
        }
    }
}