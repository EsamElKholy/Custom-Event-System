using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    [HideInInspector]
    public List<SceneAsset> _Scenes = new List<SceneAsset>();
    [HideInInspector]
    public Dictionary<string, EventData> Events = new Dictionary<string, EventData>();
    [HideInInspector]
    public List<GameEventListener> Listeners = new List<GameEventListener>();
    [HideInInspector]
    public List<EventReference> References = new List<EventReference>();

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

    public void FindAllListeners(List<string> scenes)
    {
        Listeners = new List<GameEventListener>();

        if (scenes.Count > 0)
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                if (EditorSceneManager.GetSceneAt(i) != activeScene && EditorSceneManager.GetSceneAt(i).isLoaded)
                {
                    if (EditorSceneManager.GetSceneAt(i).isLoaded)
                    {
                        EditorSceneManager.SaveScene(EditorSceneManager.GetSceneAt(i));
                    }
                    else
                    {
                        EditorSceneManager.LoadScene(EditorSceneManager.GetSceneAt(i).path, LoadSceneMode.Additive);
                    }

                    EditorSceneManager.SetActiveScene(activeScene);
                }
            }

            for (int i = 0; i < scenes.Count; i++)
            {
                for (int j = 0; j < _Scenes.Count; j++)
                {
                    var guid1 = AssetDatabase.AssetPathToGUID(scenes[i]);
                    var guid2 = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_Scenes[j]));

                    {
                        if (guid1.Equals(guid2))
                        {
                            var scene = EditorSceneManager.OpenScene(scenes[i], OpenSceneMode.Additive);

                            //for (int k = 0; k < scenes.Count; k++)
                            {
                                if (scene.IsValid() && scene.isLoaded)
                                {
                                    var allObjects = scene.GetRootGameObjects();

                                    for (int h = 0; h < allObjects.Length; h++)
                                    {
                                        var obj = allObjects[h];

                                        var comps = obj.GetComponentsInChildren<GameEventListener>(true);

                                        Listeners.AddRange(comps);
                                    }
                                }
                            }
                        }
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
                var temp = EditorSceneManager.GetSceneByPath(AssetDatabase.GetAssetPath(scene));
                {
                    if (temp.IsValid() && temp.isLoaded)
                    {
                        EditorSceneManager.SaveScene(temp);
                    }

                    if (temp.IsValid())
                    {
                        EditorSceneManager.CloseScene(temp, true);
                    }
                }

                _Scenes.RemoveAt(index);
            }
        }
    }

    public void FindAllReferences(List<string> scenes)
    {
        References = new List<EventReference>();

        if (scenes.Count > 0)
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                if (EditorSceneManager.GetSceneAt(i) != activeScene && EditorSceneManager.GetSceneAt(i).isLoaded)
                {
                    if (EditorSceneManager.GetSceneAt(i).isLoaded)
                    {
                        EditorSceneManager.SaveScene(EditorSceneManager.GetSceneAt(i));
                    }
                    else
                    {
                        EditorSceneManager.LoadScene(EditorSceneManager.GetSceneAt(i).path, LoadSceneMode.Additive);
                    }

                    EditorSceneManager.SetActiveScene(activeScene);
                }
            }

            for (int i = 0; i < scenes.Count; i++)
            {
                for (int j = 0; j < _Scenes.Count; j++)
                {
                    {
                        var guid1 = AssetDatabase.AssetPathToGUID(scenes[i]);
                        var guid2 = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_Scenes[j]));

                        if (guid1.Equals(guid2))
                        {
                            var scene = EditorSceneManager.OpenScene(scenes[i], OpenSceneMode.Additive);

                            {
                                if (scene.IsValid() && scene.isLoaded)
                                {
                                    var allObjects = scene.GetRootGameObjects();

                                    for (int k = 0; k < allObjects.Length; k++)
                                    {
                                        var obj = allObjects[k];

                                        var comps = obj.GetComponentsInChildren<MonoBehaviour>(true);

                                        foreach (var comp in comps)
                                        {
                                            FieldInfo[] fields = comp.GetType().GetFields();

                                            var eventReference = new EventReference();
                                            eventReference.Reference = comp;
                                            bool add = false;
                                            foreach (var field in fields)
                                            {
                                                CustomEventAttribute attrib = Attribute.GetCustomAttribute(field, typeof(CustomEventAttribute)) as CustomEventAttribute;

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
                    CustomEventAttribute attrib = Attribute.GetCustomAttribute(field, typeof(CustomEventAttribute)) as CustomEventAttribute;

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
                        CustomEventAttribute attrib = Attribute.GetCustomAttribute(field, typeof(CustomEventAttribute)) as CustomEventAttribute;

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

        var scene = EditorSceneManager.GetSceneByPath(AssetDatabase.GetAssetPath(s));

        if (scene.IsValid() && !scene.isLoaded)
        {
            EditorSceneManager.LoadScene(scene.path);
        }

        if (scene.IsValid() && scene.isLoaded)
        {
            var allObjects = scene.GetRootGameObjects();
            int eventCount = 0;
            int referenceCount = 0;
            int listenerCount = 0;
            Dictionary<string, CustomEvent> events = new Dictionary<string, CustomEvent>();

            for (int j = 0; j < allObjects.Length; j++)
            {
                var obj = allObjects[j];

                var listenerComps = obj.GetComponentsInChildren<GameEventListener>(true);
                var AllComps = obj.GetComponentsInChildren<MonoBehaviour>(true);
                listenerCount += listenerComps.Length;

                foreach (var comp in AllComps)
                {
                    FieldInfo[] fields = comp.GetType().GetFields();

                    foreach (var field in fields)
                    {
                        CustomEventAttribute attrib = Attribute.GetCustomAttribute(field, typeof(CustomEventAttribute)) as CustomEventAttribute;

                        if (attrib != null)
                        {
                            referenceCount++;
                            if (field.GetValue(comp) as CustomEvent != null)
                            {
                                if (events.ContainsKey((field.GetValue(comp) as CustomEvent).GetInstanceID().ToString()) == false)
                                {
                                    events.Add((field.GetValue(comp) as CustomEvent).GetInstanceID().ToString(), field.GetValue(comp) as CustomEvent);
                                }
                                //eventCount++;
                            }
                        }
                    }
                }
            }

            result.NumberOfListeners = listenerCount.ToString() + " Listeners";
            result.NumberOfEvents = events.Count.ToString() + " Events";
            result.NumberOfReferences = referenceCount.ToString() + " Event References";
        }

        return result;
    }
}