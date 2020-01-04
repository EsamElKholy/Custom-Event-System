using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;

public class EventManagerEditor : EditorWindow
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

    private static EventManagerEditor Instance;
    private GameEventManager EventManager;

    private string[] TabTitles = { "Scenes", "Events", "Listeners", "References"};
    private int CurrentTab = 0;

    private Dictionary<string, string> SceneNames = new Dictionary<string, string>();
    private string[] DropdownNames;

    private Resizer resizer;
    private SearchBar searchBar;
    
    private Rect tabBar;
    private float tabBarHeight = 24;

    private SearchField searchField;
    private string searchString = "";
    private Vector2 scroll;

    private List<int> leftPanelItemsID = new List<int>();

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

    private SceneAsset toBeDeletedScene;
    private GameEvent toBeDeletedEvent;
    private GameEventCollection collectionToDeleteFrom;

    private SceneAsset pickedScene;

    //private float Cooldown = 0;
    //private float Counter = 0;

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
    Texture2D bg;
    
    private void OnEnable()
    {
        bg = MakeTex(10, 10, new Color(0.4f, 0.5f, 0.5f, 1));       

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

    private void DrawEventManagerSection()
    {
        GUILayout.BeginArea(new Rect(2, 3, position.width - 6, 27), EditorStyles.helpBox);
        GUILayout.Space(3);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Game Event Manager", GUILayout.Width(((position.width - 6) / 4)));
        EventManager = (GameEventManager)EditorGUILayout.ObjectField(EventManager, typeof(GameEventManager), false, GUILayout.Width(((position.width - 6) * 3 / 4) - 15));
        EditorGUILayout.EndHorizontal();       
        GUILayout.EndArea();
    }

    private void DrawSearchSection()
    {
        var temp = EditorStyles.helpBox.normal.background;
        EditorStyles.helpBox.normal.background = bg;


        GUILayout.BeginArea(searchBar.Area, EditorStyles.helpBox);
        EditorStyles.helpBox.normal.background = temp;
        searchBar.UpdateArea(2, 31, position.width - 6, 25);

        GUILayout.BeginVertical();

        Rect searchArea = new Rect(5, 5, position.width - 15, 18);
        Rect searchBarArea = new Rect(2, 2, position.width - 15, 18);
        GUILayout.BeginArea(searchBarArea);

        if (EventManager)
        {
            //if (Counter >= Cooldown)
            {
                //Counter = 0;
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

            //Counter += Time.fixedDeltaTime;
        }
        GUILayout.BeginHorizontal();
        DrawSearchBar(searchBarArea);
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private void DrawDataSection()
    {
        DrawLeftDataPanel();
        DrawRightDataPanel();
    }

    Vector2 leftScroll;
    List<SceneAsset> scenes = new List<SceneAsset>();
    int filter = 0;
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

                            leftPanelItemsID = new List<int>();

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

                                if (GUILayout.Button("Open Scene"))
                                {
                                    var path = AssetDatabase.GetAssetPath(scenes[i]);
                                    var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

                                    if(scene.IsValid() && scene.isLoaded)
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
                                int id = GUIUtility.GetControlID(FocusType.Passive);
                                leftPanelItemsID.Add(id);

                                y += 65;
                            }                            
                        }
                        break;
                    case 1:
                        {
                            if (EventManager)
                            {
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
                        break;
                    case 2:
                        {
                            if (EventManager)
                            {
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
                        break;
                    case 3:
                        {
                            if (EventManager)
                            {
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
                    }else if (Event.current.commandName == "ObjectSelectorClosed" && EditorGUIUtility.GetObjectPickerControlID() == scenePickerID)
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

    Vector2 rightScroll;
    Vector2 eventScroll;
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
                            break;
                        case 1:
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
                                    }else if (Event.current.commandName == "ObjectSelectorClosed")
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
                            break;
                        case 2:
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

                                    string objectText = "This Listener is on The GameObject: \"" + selectedListener.gameObject.name + "\"";
                                    GUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField("Game Object", EditorStyles.helpBox, GUILayout.Width(labelTitleWidth));
                                    EditorGUILayout.LabelField(objectText, EditorStyles.helpBox, GUILayout.Width(labelDataWidth));
                                    GUILayout.EndHorizontal();
                                    EditorStyles.helpBox.fontSize = 10;
                                    EditorStyles.helpBox.fontStyle = FontStyle.Bold;

                                    EditorGUILayout.Space();
                                    EditorGUILayout.Space();

                                    if (selectedListener.Event)
                                    {
                                        string eventText = "This Listener is Listening waiting for \"" + selectedListener.Event.name + "\"";
                                        GUILayout.BeginHorizontal();
                                        EditorGUILayout.LabelField("Event", EditorStyles.helpBox, GUILayout.Width(labelTitleWidth));
                                        EditorGUILayout.LabelField(eventText, EditorStyles.helpBox, GUILayout.Width(labelDataWidth));
                                        GUILayout.EndHorizontal();

                                        EditorGUILayout.Space();
                                        EditorGUILayout.Space();
                                    }

                                    string responseText = "This Listener Will Activate " + selectedListener.response.GetPersistentEventCount() + " When The Event is Raised.";
                                    GUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField("Response", EditorStyles.helpBox, GUILayout.Width(labelTitleWidth));
                                    EditorGUILayout.LabelField(responseText, EditorStyles.helpBox, GUILayout.Width(labelDataWidth));
                                    GUILayout.EndHorizontal();
                                }                                
                            }
                            break;
                        case 3:
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
                            break;
                        default:
                            break;
                    }
                }
            }
            GUILayout.EndArea();
        }
    }

    void DrawSearchBar(Rect area)
    {
        DoSearchField(area);
    }

    void DoSearchField(Rect rect)
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
                                if (item.Key.ToLower().Contains(result.ToLower()))
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
                    }
                    break;
                case 3:
                    {
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
                    }
                    break;
                default:
                    break;
            }
        }

        searchString = result;
    }

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
            //if ((SceneNames.Count != EventManager._Scenes.Count))
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
                
                DropdownNames = new string[SceneNames.Count];
                SceneNames.Values.CopyTo(DropdownNames, 0);
            }

        }
    }
}
