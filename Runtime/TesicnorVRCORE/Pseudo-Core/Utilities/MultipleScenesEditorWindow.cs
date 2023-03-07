using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
public struct ModifiedComponent
{
    public System.Type type;
    public System.Reflection.PropertyInfo[] properties;
    public string gameObjectName;
    public System.Object[] fieldsValues;

    public System.Reflection.FieldInfo[] fields;
    public System.Object[] propertiesValues;
}
#endif

public class MultipleScenesEditorWindow : EditorWindow 
{
#if UNITY_EDITOR
    #region FIELDS
    GameObject selected;
    public static string ScenesFolderPath = "Assets/Scenes";
    public static string SearchString = "";

    static ModifiedComponent modifiedComponent;
    static ModifiedComponent initialComponent;
    static ModifiedComponent differences;
    #endregion

    #region METHODS
    [MenuItem("Window/Multiple scene editor")]
    public static void ShowWindow()
    {
        EditorWindow window = EditorWindow.GetWindow<MultipleScenesEditorWindow>();

        if (!window) EditorWindow.CreateWindow<MultipleScenesEditorWindow>();

        window.ShowUtility();
    }

    private void OnGUI()
    {
        //TOP 20 px
        DisplaySearchText();
        //First window width quarter
        SearchAllScenes();
        DisplayScenesInfo();

        //Second half window width
        SetObjectToModify();
        DisplayApplyButton();
    }

    private void DisplaySearchText()
    {
        SearchString = GUILayout.TextArea(SearchString, EditorStyles.textArea);
    }

    private void DisplayApplyButton()
    {
        GUILayout.BeginArea(new Rect(this.position.width - this.position.width / 4, this.position.height - 20, this.position.width / 4, 20));

        bool apply = GUILayout.Button("Apply on all scenes");

        if (apply && selected) ApplyOnAllScenes();

        GUILayout.EndArea();
    }

    int componentSelected = 0;
    Vector2 scrollPosition2;
    private void SetObjectToModify()
    {
        if (Selection.gameObjects.Length <= 0) return;

       // if (selected == null) return;

        Debug.Log("Displaying obejct");

        GUILayout.BeginArea(new Rect(this.position.width / 2, 20, this.position.width / 2, this.position.height));

        selected = Selection.gameObjects[0];

        List<GUIContent> contents = new List<GUIContent>();

        Component[] components = selected.GetComponents<Component>();

        foreach(var comp in components)
        {
            GUIContent content = new GUIContent();
            content.text = comp.GetType().Name;
            contents.Add(content);
        }

        componentSelected = EditorGUILayout.Popup(componentSelected, contents.ToArray());

        GUILayout.Space(10);

        scrollPosition2 = GUILayout.BeginScrollView(scrollPosition2);
        if (componentSelected < components.Length)
        {
            Editor editor = Editor.CreateEditor(components[componentSelected]);
            editor.OnInspectorGUI();

            System.Reflection.PropertyInfo[] properties = components[componentSelected].GetType().GetProperties();
            System.Reflection.FieldInfo[] fields = components[componentSelected].GetType().GetFields();

            foreach(var field in fields)
            {
                Debug.Log(field.Name);
            }

            ModifiedComponent component = new ModifiedComponent();
            component.gameObjectName = selected.name;
            component.properties = properties;
            component.fields = fields;
            component.type = components[componentSelected].GetType();
            List<System.Object> values = new List<System.Object>();
            List<System.Object> properties_values = new List<object>();

            if(fields.Length > 0)
            foreach (var field in fields)
            {
                    try
                    {
                        values.Add(field.GetValue(components[componentSelected]));
                    }
                    catch
                    {
                        values.Add(null);
                    }
            }

            foreach(var property in properties)
            {
                try
                {
                    properties_values.Add(property.GetValue(components[componentSelected]));
                }
                catch
                {
                    properties_values.Add(null);
                }
            }
            component.fieldsValues = values.ToArray();
            component.propertiesValues = properties_values.ToArray();

            modifiedComponent = component;

            if (modifiedComponent.type != initialComponent.type || initialComponent.Equals(new ModifiedComponent()))
            {
                initialComponent = component;
            }

            differences.type = component.type;
            differences.gameObjectName = component.gameObjectName;
            differences.fields = component.fields;
            differences.properties = component.properties;

            List<object> list = new List<object>();
            List<object> _list = new List<object>();
            if(component.fields.Length > 0)
            for (int i = 0; i < component.fields.Length; i++)
            {
                    if (modifiedComponent.fieldsValues[i] == initialComponent.fieldsValues[i]) list.Add(null);
                    else list.Add(modifiedComponent.fieldsValues[i]);
            }
            for(int i = 0; i < component.properties.Length; i++)
            {
                if (modifiedComponent.propertiesValues[i] == initialComponent.propertiesValues[i]) _list.Add(null);
                else _list.Add(modifiedComponent.propertiesValues[i]);
            }
            differences.fieldsValues = list.ToArray();
            differences.propertiesValues = _list.ToArray();
        }
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    Scene currentScene;
    private void ApplyOnAllScenes()
    {
        currentScene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Scenes paths count : " + scenesPaths.Count);
        for(int i = 0; i < scenesPaths.Count; i++)
        {
            if (enabledScenes[i])
            {
                Scene scene = EditorSceneManager.OpenScene(scenesPaths[i], OpenSceneMode.Single);
                EditorSceneManager.SetActiveScene(scene);
                GameObject go = GameObject.Find(differences.gameObjectName);
                Debug.Log("Scene opened : " + scene.name);
                if (!go) continue;

                Component comp = go.GetComponent(differences.type.Name);
                if (!comp) continue;
                if(differences.fields.Length > 0)
                for(int j = 0; j < differences.fields.Length; j++)
                {
                    if (differences.fieldsValues[j] != null)
                        {
                            try { differences.fields[j].SetValue(comp, differences.fieldsValues[j]); }
                            catch { }
                            Debug.Log(differences.fields[j].Name);
                        }                    }
                for(int j = 0; j < differences.properties.Length; j++)
                {
                    if (differences.propertiesValues[j] != null)
                    {
                        try
                        {
                            differences.properties[j].SetValue(comp, differences.propertiesValues[j]);
                        }
                        catch { }
                        Debug.Log(differences.properties[j].Name);
                    }
                }
                Scene _scene = EditorSceneManager.GetActiveScene();
                EditorSceneManager.MarkSceneDirty(_scene);
                EditorSceneManager.SaveScene(_scene, "", false);
                EditorSceneManager.SaveOpenScenes();
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            }
        }
        
    }

    Vector2 scrollPosition = Vector2.zero;
    private void DisplayScenesInfo()
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true, GUILayout.Width(this.position.width/3));

        for (int i = 0; i < enabledScenes.Count; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Scene : " + allScenes[i].name, EditorStyles.boldLabel);
            enabledScenes[i] = GUILayout.Toggle(enabledScenes[i], enabledScenes[i]?"Enabled":"Disabled", GUILayout.Width(this.position.width/3));
            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
    }
    List<SceneAsset> allScenes = new List<SceneAsset>();
    List<string> scenesPaths = new List<string>();
    List<bool> enabledScenes = new List<bool>();
    string oldString = "";
    private void SearchAllScenes()
    {
        if (SearchString.Length <= 0 || oldString == SearchString) return;
        allScenes.Clear();
        enabledScenes.Clear();

        string[] allPaths = AssetDatabase.GetAllAssetPaths();

        scenesPaths.Clear();

        foreach(string path in allPaths)
        {
            string[] splitted = path.Split(".");
            if (splitted.Length == 2)
            {
                if (splitted[1] == "unity" && splitted[0].Contains(SearchString)) { scenesPaths.Add(path); enabledScenes.Add(false); Debug.Log(splitted[0]); }
            }
        }
        
        foreach(var path in scenesPaths)
        {
            SceneAsset scene = AssetDatabase.LoadAssetAtPath(path, typeof(SceneAsset)) as SceneAsset;
            allScenes.Add(scene);   
        }
        oldString = SearchString;
    }
    #endregion
#endif
}
