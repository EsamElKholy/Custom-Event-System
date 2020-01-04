﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SceneEvents : IComparer<SceneEvents>
{
    [SerializeField]
    public SceneAsset scene;

    public HashSet<CustomEvent> AllEvents = new HashSet<CustomEvent>();
    public List<EventReference> AllEventReferences = new List<EventReference>();
    public List<GameEventListener> AllListeners = new List<GameEventListener>();

    public int Compare(SceneEvents x, SceneEvents y)
    {
        if (x.scene == null || y.scene == null)
        {
            return 0;
        }

        return x.scene.name.CompareTo(y.scene.name);
    }
}

[System.Serializable]
public class EventData
{
    public CustomEvent Event;
    public SceneAsset Owner;
    public string Name;
    public List<EventReference> References = new List<EventReference>();
    public List<GameEventListener> Listeners = new List<GameEventListener>();

    public EventData(string name, CustomEvent e)
    {
        Name = name;
        Event = e;
    }
}

public struct SceneStatistics
{
    public string NumberOfEvents;
    public string NumberOfListeners;
    public string NumberOfReferences;
}

public class EventReference
{
    public MonoBehaviour Reference;
    public List<string> ReferenceNames = new List<string>();
    public List<FieldInfo> Fields = new List<FieldInfo>();
    public List<CustomEvent> Events = new List<CustomEvent>();
}

[CreateAssetMenu(menuName = "Game Events/Game Event Manager", fileName = "New Game Event Manager")]
public class GameEventManager : ScriptableObject
{
    //public List<SceneEvents> Scenes = new List<SceneEvents>();

    public List<SceneAsset> _Scenes = new List<SceneAsset>();
    public Dictionary<string, EventData> Events = new Dictionary<string, EventData>();
    public List<GameEventListener> Listeners = new List<GameEventListener>();
    public List<EventReference> References = new List<EventReference>();

    //public SceneEvents CurrentScene;
    // private int LastCount;

    public void RefreshSceneList()
    {

    }

    public void CreateNewEvent()
    {

    }

    public void FindAllEvents()
    {
        var result = AssetDatabase.FindAssets("t:CustomEvent");

        for (int i = 0; i < result.Length; i++)
        {
            var e = (CustomEvent)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(result[i]), typeof(CustomEvent));

            if (Events.ContainsKey(e.name) == false)
            {
                Events.Add(e.name, new EventData(e.name, e));
            }
        }
    }

    public void FindEvent()
    {

    }

    public void LoadEventData()
    {

    }

    public void FindAllListeners(List<string> scenes)
    {
        Listeners = new List<GameEventListener>();

        if (scenes.Count > 0)
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                if (EditorSceneManager.GetSceneAt(i) != activeScene)
                {
                    EditorSceneManager.SaveScene(EditorSceneManager.GetSceneAt(i));
                    EditorSceneManager.CloseScene(EditorSceneManager.GetSceneAt(i), true);
                    EditorSceneManager.SetActiveScene(activeScene);
                }
            }

            for (int i = 0; i < scenes.Count; i++)
            {
                for (int j = 0; j < _Scenes.Count; j++)
                {
                    if (_Scenes[j].name.Equals(scenes[i]))
                    {
                        {
                            EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(_Scenes[j]), OpenSceneMode.Additive);
                        }
                    }
                }
            }

            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                var scene = EditorSceneManager.GetSceneAt(i);

                if (scene.IsValid() && scene.isLoaded)
                {
                    var allObjects = scene.GetRootGameObjects();

                    for (int j = 0; j < allObjects.Length; j++)
                    {
                        var obj = allObjects[j];

                        var comps = obj.GetComponentsInChildren<GameEventListener>(true);

                        Listeners.AddRange(comps);
                    }
                }
            }
        }
    }

    public void RemoveScene(SceneAsset scene)
    {
        if (scene)
        {
            int index = -1;

            for (int i = 0; i < _Scenes.Count; i++)
            {
                var guid1 = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_Scenes[i]));
                var guid2 = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(scene));

                if (guid1.Equals(guid2))
                {
                    index = i;

                    break;
                }
            }

            if (index >= 0)
            {
                _Scenes.RemoveAt(index);
            }
        }
    }

    public void FindListener()
    {

    }

    public void FindAllReferences(List<string> scenes)
    {
        References = new List<EventReference>();

        if (scenes.Count > 0)
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                if (EditorSceneManager.GetSceneAt(i) != activeScene)
                {
                    EditorSceneManager.SaveScene(EditorSceneManager.GetSceneAt(i));
                    EditorSceneManager.CloseScene(EditorSceneManager.GetSceneAt(i), true);
                    EditorSceneManager.SetActiveScene(activeScene);
                }
            }

            for (int i = 0; i < scenes.Count; i++)
            {
                for (int j = 0; j < _Scenes.Count; j++)
                {
                    if (_Scenes[j].name.Equals(scenes[i]))
                    {
                        {
                            EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(_Scenes[j]), OpenSceneMode.Additive);
                        }
                    }
                }
            }

            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                var scene = EditorSceneManager.GetSceneAt(i);

                if (scene.IsValid() && scene.isLoaded)
                {
                    var allObjects = scene.GetRootGameObjects();

                    for (int j = 0; j < allObjects.Length; j++)
                    {
                        var obj = allObjects[j];

                        var comps = obj.GetComponentsInChildren<MonoBehaviour>(true);

                        foreach (var comp in comps)
                        {
                            FieldInfo[] fields = comp.GetType().GetFields();

                            var eventReference = new EventReference();
                            eventReference.Reference = comp;
                            bool add = false;
                            foreach (var field in fields)
                            {
                                EventAttribute attrib = Attribute.GetCustomAttribute(field, typeof(EventAttribute)) as EventAttribute;

                                if (attrib != null)
                                {
                                    eventReference.Events.Add(field.GetValue(comp) as CustomEvent);
                                    eventReference.ReferenceNames.Add(field.Name);
                                    eventReference.Fields.Add(field);
                                    add = true;
                                }
                            }

                            if (add)
                            {
                                References.Add(eventReference);
                            }
                        }
                    }
                }
            }
        }
    }

    public void RefreshReference(EventReference er)
    {
        if (er != null)
        {
            er.ReferenceNames = new List<string>();
            er.Events = new List<CustomEvent>();
            er.Fields = new List<FieldInfo>();
            {
                FieldInfo[] fields = er.Reference.GetType().GetFields();

                foreach (var field in fields)
                {
                    EventAttribute attrib = Attribute.GetCustomAttribute(field, typeof(EventAttribute)) as EventAttribute;

                    if (attrib != null)
                    {
                        er.Events.Add(field.GetValue(er.Reference) as CustomEvent);
                        er.ReferenceNames.Add(field.Name);
                        er.Fields.Add(field);
                    }
                }                
            }
        }
    }

    public List<MonoBehaviour> FindReference(CustomEvent e)
    {
        List<MonoBehaviour> result = new List<MonoBehaviour>();

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);

            var allObjects = scene.GetRootGameObjects();

            for (int j = 0; j < allObjects.Length; j++)
            {
                var obj = allObjects[j];

                var comps = obj.GetComponentsInChildren<MonoBehaviour>(true);

                foreach (var comp in comps)
                {
                    FieldInfo[] fields = comp.GetType().GetFields();

                    foreach (var field in fields)
                    {
                        EventAttribute attrib = Attribute.GetCustomAttribute(field, typeof(EventAttribute)) as EventAttribute;

                        if (attrib != null)
                        {
                            if (field.GetValue(comp) as CustomEvent != null && (field.GetValue(comp) as CustomEvent).name.Equals(e.name))
                            {
                                result.Add(comp);
                            }
                        }
                    }
                }
            }
        }

        return result;
    }

    public SceneStatistics GetSceneStatistics(SceneAsset s)
    {
        SceneStatistics result = new SceneStatistics();

        if (s == null)
        {
            return result;
        }

        for (int j = 0; j < _Scenes.Count; j++)
        {
            var guid1 = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_Scenes[j]));
            var guid2 = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(s));

            if (guid1.Equals(guid2))
            {
                {
                    EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(_Scenes[j]), OpenSceneMode.Additive);
                }
            }
        }

        var scene = EditorSceneManager.GetSceneByPath(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(s)));       

        if (scene.IsValid() && scene.isLoaded)
        {
            var allObjects = scene.GetRootGameObjects();

            for (int j = 0; j < allObjects.Length; j++)
            {
                var obj = allObjects[j];

                var comps = obj.GetComponentsInChildren<GameEventListener>(true);

                result.NumberOfListeners = comps.Length.ToString() + " Listeners";
                int eventCount = 0;
                int referenceCount = 0;
                foreach (var comp in comps)
                {
                    FieldInfo[] fields = comp.GetType().GetFields();

                    foreach (var field in fields)
                    {
                        EventAttribute attrib = Attribute.GetCustomAttribute(field, typeof(EventAttribute)) as EventAttribute;

                        if (attrib != null)
                        {
                            referenceCount++;
                            if (field.GetValue(comp) as CustomEvent != null)
                            {
                                eventCount++;
                            }
                        }
                    }
                }

                result.NumberOfEvents = eventCount + " Events";
                result.NumberOfReferences = referenceCount + " Event References";
            }
        }

        return result;
    }
}