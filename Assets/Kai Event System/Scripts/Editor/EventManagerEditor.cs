using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;

namespace KAI
{
    public partial class EventManagerEditor : EditorWindow
    {      
        private static EventManagerEditor Instance;
        private GameEventManager EventManager;

        private string[] TabTitles = { "Scenes", "Events", "Listeners", "References"};
        private int CurrentTab = 0;

        private Dictionary<string, string> SceneNames = new Dictionary<string, string>();
        private string[] DropdownNames;

    
        private Rect tabBar;
        private float tabBarHeight = 24;    

        private string rightPanelTitle;
        private bool showRightPanel;

        private SceneAsset selectedScene;
        private EventData selectedEvent;
        private GameEventListener selectedListener;
        private EventReference selectedReference;

        private int eventPickerID = -1;
        private int scenePickerID = -1;

        private GameEvent pickedEvent;
        private GameEventCollection gec;
        private SceneAsset pickedScene;

        private SceneAsset toBeDeletedScene;
        private GameEvent toBeDeletedEvent;
        private GameEventCollection collectionToDeleteFrom;
    
        private Vector2 leftScroll;
        private Vector2 rightScroll;
        private Vector2 eventScroll;

        private List<SceneAsset> scenes = new List<SceneAsset>();
        private int filter = 0;

        [MenuItem("Custom Tools/Event Manager")]
        public static void OpenWindow()
        {
            if (Instance == null)
            {
                Instance = GetWindow<EventManagerEditor>();
                Instance.minSize = new Vector2(640, 480);
            }
            else
            {
                EditorWindow.FocusWindowIfItsOpen<EventManagerEditor>();
            }

            Instance.titleContent = new GUIContent("Event Manager");
        }
    
        private void OnEnable()
        {
            resizer = new Resizer(new Rect(0, 57, 5, position.height * 2), 0.5f, new GUIStyle());

            resizer.Style.normal.background = EditorGUIUtility.Load("icons/d_AvatarBlendBackground.png") as Texture2D;

            searchBar = new SearchBar();
            searchBar.Area = new Rect(2, 60, position.width - 6, 25);
        }

        private void OnInspectorUpdate()
        {
            EditorStyles.helpBox.fontSize = 12;
            EditorStyles.toolbar.fontSize = 12;

            if (gec && pickedEvent)
            {
                gec.Events.Add(pickedEvent);
                pickedEvent = null;
                gec = null;
                searchString = "";

                GUI.FocusControl("");
            }

            if (pickedScene)
            {
                if (EventManager)
                {
                    bool alreadyThere = false;
                    for (int i = 0; i < EventManager._Scenes.Count; i++)
                    {
                        var guid1 = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(EventManager._Scenes[i]));
                        var guid2 = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(pickedScene));
                        if (guid1.Equals(guid2))
                        {
                            alreadyThere = true;
                            break;
                        }
                    }

                    if (!alreadyThere)
                    {
                        EventManager._Scenes.Add(pickedScene);
                    }

                    pickedScene = null;

                    GUI.FocusControl("");

                    searchString = "";
                }
            }

            if (collectionToDeleteFrom && toBeDeletedEvent)
            {
                collectionToDeleteFrom.RemoveEvent(toBeDeletedEvent);

                toBeDeletedEvent = null;
                collectionToDeleteFrom = null;

                GUI.FocusControl("");

                searchString = "";
            }

            if (toBeDeletedScene)
            {
                if (EventManager)
                {
                    EventManager.RemoveScene(toBeDeletedScene);
                    toBeDeletedScene = null;

                    GUI.FocusControl("");

                    searchString = "";
                }
            }

            if (CurrentTab == 1)
            {
                if (EventManager)
                {
                    EventManager.FindAllEvents();
                }
            }
        }

        private void OnGUI()
        {
            DrawEventManagerSection();

            EditorGUILayout.Space();

            DrawSearchSection();

            DrawDataSection();

            DrawResizer();

            ProcessEvents(Event.current);

            if (GUI.changed) Repaint();
        }
        
        private void DrawDataSection()
        {
            DrawLeftDataPanel();
            DrawRightDataPanel();
        }  
    }
}