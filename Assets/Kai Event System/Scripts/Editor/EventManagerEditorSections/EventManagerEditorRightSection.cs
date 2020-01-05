using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;

public partial class EventManagerEditor : EditorWindow
{
    private void DrawRightDataPanel()
    {
        if (showRightPanel && EventManager)
        {
            //////////
            ///TitleArea///
            //////////
            Rect titleBar = new Rect((position.width * resizer.SizeRatio) + resizer.Area.width + 1, 60, position.width - (position.width * resizer.SizeRatio) - 10, 25);
            GUILayout.BeginArea(titleBar, EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(rightPanelTitle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            //////////
            ///Body///
            //////////
            Rect rightDataArea = new Rect(titleBar.x, titleBar.y + titleBar.height, position.width - (position.width * resizer.SizeRatio) - 10, position.height - 60 - titleBar.height - 3);
            GUILayout.BeginArea(rightDataArea, EditorStyles.helpBox);
            using (var v = new EditorGUILayout.VerticalScope())
            {
                using (var scrollView = new EditorGUILayout.ScrollViewScope(rightScroll, GUILayout.Width(rightDataArea.width - 5), GUILayout.Height(rightDataArea.height - 1)))
                {
                    rightScroll = scrollView.scrollPosition;

                    switch (CurrentTab)
                    {
                        case 0:
                            {
                                DrawRightSceneSection(rightDataArea);
                            }
                            break;
                        case 1:
                            {
                                DrawRightEventSection(rightDataArea);
                            }
                            break;
                        case 2:
                            {
                                DrawRightListenerSection(rightDataArea);
                            }
                            break;
                        case 3:
                            {
                                DrawRightReferenceSection(rightDataArea);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            GUILayout.EndArea();
        }
    }

    private void DrawRightSceneSection(Rect rightDataArea)
    {
        SceneStatistics statistics = EventManager.GetSceneStatistics(selectedScene);
        float labelTitleWidth = rightDataArea.width / 4;
        float labelDataWidth = (rightDataArea.width * 3 / 4) - 20;
        string eventsText = "This scene is using " + statistics.NumberOfEvents + " referenced across all scripts in the scene.";
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Event Details", EditorStyles.helpBox, GUILayout.Width(labelTitleWidth));
        EditorGUILayout.LabelField(eventsText, EditorStyles.helpBox, GUILayout.Width(labelDataWidth));
        GUILayout.EndHorizontal();
        EditorStyles.helpBox.fontSize = 12;
        EditorStyles.helpBox.fontStyle = FontStyle.Bold;

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        string listenersText = "This scene is using " + statistics.NumberOfListeners;
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Event Listener Details", EditorStyles.helpBox, GUILayout.Width(labelTitleWidth));
        EditorGUILayout.LabelField(listenersText, EditorStyles.helpBox, GUILayout.Width(labelDataWidth));
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        string referencesText = "This scene is using " + statistics.NumberOfReferences + " across all scripts in the scene.";
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Event Reference Details", EditorStyles.helpBox, GUILayout.Width(labelTitleWidth));
        EditorGUILayout.LabelField(referencesText, EditorStyles.helpBox, GUILayout.Width(labelDataWidth));
        GUILayout.EndHorizontal();
    }

    private void DrawRightEventSection(Rect rightDataArea)
    {
        var refs = EventManager.FindReference(selectedEvent.Event);
        float singleH = 25;
        float nh = rightDataArea.height;
        float h = (nh / 2 + 50) + ((selectedEvent.Event.listeners.Count + refs.Count) * 1 * singleH);
        if (selectedEvent.Event.Type == CustomEventType.Game_Event_Collection)
        {
            var eventCol = (GameEventCollection)selectedEvent.Event;
            h = (nh / 2 + 120) + ((eventCol.Events.Count + selectedEvent.Event.listeners.Count + refs.Count) * 1 * singleH);
        }
        float nw = rightDataArea.width;

        if (rightDataArea.height + 5 < h)
        {
            float dt = Mathf.Abs((rightDataArea.height) - (h));
            nh += dt - nh / 2 + 40;
            nw -= 23;
        }
        else
        {
            nh -= 15 + nh / 2 + 30;
            nw -= 8;
        }

        float labelTitleWidth = rightDataArea.width / 4;
        float labelHeight = 25;
        float labelDataWidth = (nw * 3 / 4) - 20;

        GUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Event Name", EditorStyles.helpBox, GUILayout.Width(labelTitleWidth), GUILayout.Height(labelHeight));
        EditorGUILayout.LabelField(selectedEvent.Name, EditorStyles.helpBox, GUILayout.Width(labelDataWidth), GUILayout.Height(labelHeight));
        GUILayout.EndHorizontal();
        EditorStyles.helpBox.fontSize = 10;
        EditorStyles.helpBox.fontStyle = FontStyle.Bold;

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Event Description", EditorStyles.helpBox, GUILayout.Width(labelTitleWidth), GUILayout.Height(50));
        selectedEvent.Event.Description = EditorGUILayout.TextArea(selectedEvent.Event.Description, EditorStyles.textArea, GUILayout.Width(labelDataWidth), GUILayout.Height(50));
        GUILayout.EndHorizontal();

        Undo.RecordObject(selectedEvent.Event, "Changed description");

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Event Type", EditorStyles.helpBox, GUILayout.Width(labelTitleWidth), GUILayout.Height(labelHeight));
        EditorGUILayout.LabelField(selectedEvent.Event.Type.ToString(), EditorStyles.helpBox, GUILayout.Width(labelDataWidth), GUILayout.Height(labelHeight));
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        float y = 0;
        if (selectedEvent.Event.Type == CustomEventType.Game_Event_Collection)
        {
            EditorStyles.toolbar.fontSize = 10;
            y = rightDataArea.height / 2 - 50;
            var eventsCol = (GameEventCollection)selectedEvent.Event;
            GUILayout.BeginArea(new Rect(0, y - 30, rightDataArea.width, 25));
            GUILayout.Label("Events (" + eventsCol.Events.Count + " Events)", EditorStyles.toolbar, GUILayout.Width(nw));
            GUILayout.EndArea();
            for (int i = 0; i < eventsCol.Events.Count; i++)
            {
                Rect listenerArea = new Rect(0, y, nw, 25);
                EditorGUILayout.Space();

                if (eventsCol.Events[i] == null)
                {
                    continue;
                }
                EditorStyles.helpBox.alignment = TextAnchor.MiddleCenter;
                EditorStyles.helpBox.padding.top = 2;

                GUILayout.BeginArea(listenerArea, EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(eventsCol.Events[i].name, EditorStyles.helpBox, GUILayout.Width(listenerArea.width / 4));
                eventsCol.Events[i] = (GameEvent)EditorGUILayout.ObjectField(eventsCol.Events[i], typeof(GameEvent), true, GUILayout.Width((listenerArea.width * 3 / 4) - 40));
                if (GUILayout.Button("X", EditorStyles.toolbarButton))
                {
                    toBeDeletedEvent = eventsCol.Events[i];
                    collectionToDeleteFrom = eventsCol;
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.EndArea();

                y += 32;
            }

            bool showPicker = false;

            GUILayout.BeginArea(new Rect(0, y + 5, nw, 20));
            if (GUILayout.Button("Add Event To Collection"))
            {
                showPicker = true;
            }
            GUILayout.EndArea();

            if (showPicker)
            {
                eventPickerID = EditorGUIUtility.GetControlID(FocusType.Keyboard);

                EditorGUIUtility.ShowObjectPicker<GameEvent>(null, false, "", eventPickerID);
            }

            if (Event.current.commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID() == eventPickerID)
            {
                pickedEvent = (GameEvent)EditorGUIUtility.GetObjectPickerObject();

                eventPickerID = -1;
            }
            else if (Event.current.commandName == "ObjectSelectorClosed")
            {
                gec = eventsCol;
            }
        }

        GUIContent listenerTitle = new GUIContent();
        listenerTitle.text = "Listeners (" + selectedEvent.Event.listeners.Count + " Listeners)";
        EditorStyles.toolbar.fontSize = 13;
        if (y == 0)
        {
            y = rightDataArea.height / 2 - 50;
            GUILayout.BeginArea(new Rect(0, y - 30, rightDataArea.width, 25));
            GUILayout.Label(listenerTitle, EditorStyles.toolbar, GUILayout.Width(nw));
            GUILayout.EndArea();
        }
        else
        {
            GUILayout.BeginArea(new Rect(0, y + 35, rightDataArea.width, 25));
            GUILayout.Label(listenerTitle, EditorStyles.toolbar, GUILayout.Width(nw));
            GUILayout.EndArea();
            y += 65;
        }

        for (int i = 0; i < selectedEvent.Event.listeners.Count; i++)
        {
            if (selectedEvent.Event.listeners[i] == null)
            {
                selectedEvent.Event.listeners.RemoveAt(i);
                i--;
            }
        }

        GUILayout.Label("", GUILayout.Width(rightDataArea.width - 30), GUILayout.Height(nh));
        for (int i = 0; i < selectedEvent.Event.listeners.Count; i++)
        {
            Rect listenerArea = new Rect(0, y, nw, 25);
            EditorGUILayout.Space();
            if (selectedEvent.Event == null || selectedEvent.Event.listeners[i] == null)
            {
                continue;
            }
            EditorStyles.helpBox.alignment = TextAnchor.MiddleCenter;
            EditorStyles.helpBox.padding.top = 2;

            GUILayout.BeginArea(listenerArea, EditorStyles.helpBox);
            GUILayout.BeginHorizontal();

            GUILayout.Label(selectedEvent.Event.listeners[i].name, EditorStyles.helpBox, GUILayout.Width(listenerArea.width / 4));
            selectedEvent.Event.listeners[i] = (GameEventListener)EditorGUILayout.ObjectField(selectedEvent.Event.listeners[i], typeof(GameEventListener), true, GUILayout.Width((listenerArea.width * 3 / 4) - 20));
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            y += 32;
        }
        GUILayout.BeginArea(new Rect(0, y + 10, rightDataArea.width, 25));
        GUILayout.Label("References (" + refs.Count + " References)", EditorStyles.toolbar, GUILayout.Width(nw));
        GUILayout.EndArea();
        y += 40;

        EditorStyles.toolbar.fontSize = 10;

        foreach (var item in refs)
        {
            var reference = item;
            Rect listenerArea = new Rect(0, y, nw, 25);
            EditorGUILayout.Space();
            if (reference == null)
            {
                continue;
            }
            EditorStyles.helpBox.alignment = TextAnchor.MiddleCenter;
            EditorStyles.helpBox.padding.top = 2;

            GUILayout.BeginArea(listenerArea, EditorStyles.helpBox);
            GUILayout.BeginHorizontal();

            GUILayout.Label(reference.name, EditorStyles.helpBox, GUILayout.Width(listenerArea.width / 4));
            reference = (MonoBehaviour)EditorGUILayout.ObjectField(item, typeof(MonoBehaviour), true, GUILayout.Width((listenerArea.width * 3 / 4) - 20));
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            y += 32;
        }
    }

    private void DrawRightListenerSection(Rect rightDataArea)
    {
        float labelTitleWidth = rightDataArea.width / 4;
        float labelDataWidth = (rightDataArea.width * 3 / 4) - 20;

        if (selectedListener)
        {
            string sceneText = "This Listener is in The Scene: \"" + selectedListener.gameObject.scene.path + "\"";
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Scene", EditorStyles.helpBox, GUILayout.Width(labelTitleWidth));
            EditorGUILayout.LabelField(sceneText, EditorStyles.helpBox, GUILayout.Width(labelDataWidth));
            GUILayout.EndHorizontal();
            EditorStyles.helpBox.fontSize = 10;
            EditorStyles.helpBox.fontStyle = FontStyle.Bold;

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            string objectText = "This listener is on the GameObject: \"" + selectedListener.gameObject.name + "\"";
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Game Object", EditorStyles.helpBox, GUILayout.Width(labelTitleWidth));
            EditorGUILayout.LabelField(objectText, EditorStyles.helpBox, GUILayout.Width(labelDataWidth));
            GUILayout.EndHorizontal();
            EditorStyles.helpBox.fontSize = 10;
            EditorStyles.helpBox.fontStyle = FontStyle.Bold;

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Event Reference", EditorStyles.helpBox, GUILayout.Width(labelTitleWidth));
            selectedListener.Event = (CustomEvent)EditorGUILayout.ObjectField(selectedListener.Event, typeof(CustomEvent), false, GUILayout.Width(labelDataWidth));
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            string responseText = "This listener Will activate " + selectedListener.response.GetPersistentEventCount() + " response when the event is raised.";
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Response", EditorStyles.helpBox, GUILayout.Width(labelTitleWidth));
            EditorGUILayout.LabelField(responseText, EditorStyles.helpBox, GUILayout.Width(labelDataWidth));
            GUILayout.EndHorizontal();
        }
    }

    private void DrawRightReferenceSection(Rect rightDataArea)
    {
        if (selectedReference != null && selectedReference.Reference)
        {
            EventManager.RefreshReference(selectedReference);
            float singleH = 25;
            float nh = rightDataArea.height;
            float h = (nh / 2 + 180) + ((selectedReference.ReferenceNames.Count) * 1 * singleH);

            float nw = rightDataArea.width;
            if (rightDataArea.height < h)
            {
                float dt = Mathf.Abs((rightDataArea.height) - (h));
                nh += dt - nh / 2;
                nw -= 23;
            }
            else
            {
                nh -= 15 + nh / 2;
                nw -= 8;
            }
            EditorGUILayout.BeginVertical();

            float y = 0;

            float labelTitleWidth = nw / 4;
            float labelDataWidth = (nw * 3 / 4) - 20;

            string sceneText = "This Reference is in The Scene: \"" + selectedReference.Reference.gameObject.scene.path + "\"";
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Scene", EditorStyles.helpBox, GUILayout.Width(labelTitleWidth), GUILayout.Height(50));
            EditorGUILayout.LabelField(sceneText, EditorStyles.helpBox, GUILayout.Width(labelDataWidth), GUILayout.Height(50));
            GUILayout.EndHorizontal();
            EditorStyles.helpBox.fontSize = 10;
            EditorStyles.helpBox.fontStyle = FontStyle.Bold;

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            string objectText = "This Reference is on The Game Object: \"" + selectedReference.Reference.gameObject.name + "\"";
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Game Object", EditorStyles.helpBox, GUILayout.Width(labelTitleWidth), GUILayout.Height(50));
            EditorGUILayout.LabelField(objectText, EditorStyles.helpBox, GUILayout.Width(labelDataWidth), GUILayout.Height(50));
            GUILayout.EndHorizontal();
            EditorStyles.helpBox.fontSize = 10;
            EditorStyles.helpBox.fontStyle = FontStyle.Bold;

            GUIContent listenerTitle = new GUIContent();
            listenerTitle.text = "Fields (" + selectedReference.ReferenceNames.Count + " Fields)";
            EditorStyles.toolbar.fontSize = 10;
            if (y == 0)
            {
                y = rightDataArea.height / 2 - 50;
                GUILayout.BeginArea(new Rect(0, y, nw, 25));
                GUILayout.Label(listenerTitle, EditorStyles.toolbar, GUILayout.Width(nw));
                GUILayout.EndArea();
                y += 30;
            }

            GUILayout.Label("", GUILayout.Width(rightDataArea.width - 30), GUILayout.Height(nh));
            for (int i = 0; i < selectedReference.ReferenceNames.Count; i++)
            {
                Rect listenerArea = new Rect(0, y, nw, 25);
                EditorGUILayout.Space();

                EditorStyles.helpBox.alignment = TextAnchor.MiddleCenter;
                EditorStyles.helpBox.padding.top = 2;

                GUILayout.BeginArea(listenerArea, EditorStyles.helpBox);
                GUILayout.BeginHorizontal();

                EditorStyles.toolbar.fontSize = 10;
                EditorStyles.toolbar.padding.top = 2;
                GUILayout.Label("Field Name: " + selectedReference.ReferenceNames[i], EditorStyles.toolbar, GUILayout.Width(listenerArea.width / 3));
                selectedReference.Events[i] = (CustomEvent)EditorGUILayout.ObjectField(selectedReference.Events[i], typeof(CustomEvent), true, GUILayout.Width((listenerArea.width * 2 / 3) - 20));
                selectedReference.Fields[i].SetValue(selectedReference.Reference, selectedReference.Events[i]);
                GUILayout.EndHorizontal();
                GUILayout.EndArea();
                y += 32;
            }

            EditorGUILayout.EndVertical();
        }
    }
}
