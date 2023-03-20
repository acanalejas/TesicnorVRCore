using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif


#if UNITY_EDITOR
public struct ModifiedComponent
{
    public System.Type type;
    public System.Reflection.PropertyInfo[] properties;
    public string gameObjectName;
    public System.Object[] fieldsValues;

    public System.Reflection.FieldInfo[] fields;
    public System.Object[] propertiesValues;

    public System.Reflection.FieldInfo[] private_fields;
    public System.Object[] privateFieldValues;
}
#endif

#if UNITY_EDITOR
public class MultipleScenesEditorWindow : EditorWindow 
{
    #region FIELDS
    GameObject selected;
    public static string ScenesFolderPath = "Assets/Scenes";
    public static string SearchString = "";

    //El estado actual del componente seleccionado
    static ModifiedComponent modifiedComponent;
    //El estado del componente seleccionado antes de realizarle ningun cambio
    static ModifiedComponent initialComponent;
    //Las diferencias entre los estados anteriores
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

    /// <summary>
    /// Muestra el texto que se usará para la búsqueda
    /// </summary>
    private void DisplaySearchText()
    {
        SearchString = GUILayout.TextArea(SearchString, EditorStyles.textArea);
    }

    /// <summary>
    /// Muestra el botón que se usará para aplicar los cambios
    /// </summary>
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
        //Si no hay ningun gameobject seleccionado no nos interesa seguir
        if (Selection.gameObjects.Length <= 0) return;

        //Empieza el area de la ventana en el que se enseñará el componente seleccionado
        GUILayout.BeginArea(new Rect(this.position.width / 2, 20, this.position.width / 2, this.position.height));

        selected = Selection.gameObjects[0];

        List<GUIContent> contents = new List<GUIContent>();

        //Recoge sus componentes y setea el nombre como contenido para el popup donde elegiremos el componente a modificar
        Component[] components = selected.GetComponents<Component>();
        foreach(var comp in components)
        {
            GUIContent content = new GUIContent();
            content.text = comp.GetType().Name;
            contents.Add(content);
        }

        //Crea el popup para elegir componente
        componentSelected = EditorGUILayout.Popup(componentSelected, contents.ToArray());

        GUILayout.Space(10);

        //Crea una zona de scroll por si el contenido se nos va de madre poder recorrerlo igualmente
        scrollPosition2 = GUILayout.BeginScrollView(scrollPosition2);
        //En caso de que el componente seleccionado esté dentro del array de componentes (Por seguridad)
        if (componentSelected < components.Length)
        {
            //Enseña el editor del componente seleccionado para poder modificar sus valores
            Editor editor = Editor.CreateEditor(components[componentSelected]);
            editor.OnInspectorGUI();

            //Recoge las propiedades, y los campos publicos y privados
            System.Reflection.PropertyInfo[] properties = components[componentSelected].GetType().GetProperties();
            System.Reflection.FieldInfo[] fields = components[componentSelected].GetType().GetFields();
            System.Reflection.FieldInfo[] privateFields = components[componentSelected].GetType().GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            //Crea una estructura para guardar los datos del componente y lo rellena con los campos y propiedades, ademas del nombre del gameobject que contiene el componente y el tipo del componente
            ModifiedComponent component = new ModifiedComponent();
            component.gameObjectName = selected.name;
            component.properties = properties;
            component.fields = fields;
            component.private_fields = privateFields;
            component.type = components[componentSelected].GetType();
            //Instancia las listas para contener los valores de las propiedades y los campos
            List<System.Object> values = new List<System.Object>();
            List<System.Object> properties_values = new List<object>();
            List<System.Object> private_values = new List<object>();

            //Recoge el valor de cada campo publico y lo añade a la lista correspondiente
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

            //Recoge cada valor de cada campo privado y lo añade a la lista correspondiente
            if(privateFields.Length > 0)    
            foreach(var field in privateFields)
            {
                try { private_values.Add(field.GetValue(components[componentSelected])); }
                catch { private_values.Add(null); }
            }

            //Recoge el valor de cada propiedad y lo añade a la lista correspondiente
            if(properties.Length > 0)
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
            //Aplica las listas recientemente rellenadas para la estructura de datos del componente
            component.fieldsValues = values.ToArray();
            component.propertiesValues = properties_values.ToArray();
            component.privateFieldValues = private_values.ToArray();

            modifiedComponent = component;

            //En caso de que no haya componente seleccionado, o se seleccione uno nuevo, se establece la estructura actual de datos como la inicial
            if (modifiedComponent.type != initialComponent.type || initialComponent.Equals(new ModifiedComponent()))
            {
                initialComponent = component;
            }
            
            //Rellenamos valores de la estructura de diferencias entre el estado inicial y el final
            differences.type = component.type;
            differences.gameObjectName = component.gameObjectName;
            differences.fields = component.fields;
            differences.private_fields = component.private_fields;
            differences.properties = component.properties;

            //Listas para los valores de las propiedades y los campos
            List<object> list = new List<object>();
            List<object> _list = new List<object>();
            List<object> privateList = new List<object>();

            //Se rellenan las listas de valores con cada valor solo en caso de que no coindidan con el estado inicial
            //En caso de coincidir se añade un valor nulo para saber que ese no debe tocarse
            //Asi evitamos cambios indeseados como en el transform en caso de no querer mover el objeto, o variables que no queremos aplicar en el resto de escenas
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
            for(int i = 0; i < component.private_fields.Length; i++)
            {
                if (modifiedComponent.privateFieldValues[i] == initialComponent.privateFieldValues[i]) privateList.Add(null);
                else privateList.Add(modifiedComponent.privateFieldValues[i]);
            }
            //Se aplican los valores a la estructura de datos de las diferencias
            differences.fieldsValues = list.ToArray();
            differences.propertiesValues = _list.ToArray();
            differences.privateFieldValues = privateList.ToArray();
        }
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    Scene currentScene;
    /// <summary>
    /// Aplica los cambios en el componente seleccionado en todas las escenas que queramos
    /// </summary>
    private void ApplyOnAllScenes()
    {
        //Recoge la escena actual y le guarda los cambios
        currentScene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.SaveOpenScenes();

        //Recorre el bucle por cada escena seleccionada
        if(scenesPaths.Count > 0)
        for(int i = 0; i < scenesPaths.Count; i++)
        {
            //Comprueba si la escena está activada para aplicar los cambios
            if (enabledScenes[i])
            {
                //Abre la escena correspondiente segun el orden, la establece como activa y recoge todos los GameObjects dentro de esta
                Scene scene = EditorSceneManager.OpenScene(scenesPaths[i], OpenSceneMode.Single);
                EditorSceneManager.SetActiveScene(scene);
                Transform[] gos = GameObject.FindObjectsOfType<Transform>(true);
                GameObject go = null;

                //Busca si hay algún gameobject que coincida con el nombre del que deseamos, y si es asi, lo asignamos.
                //Se usa este método mas "a lo bruto" en vez de "GameObject.Find" ya que esta última ignora los objetos inactivos
                //y nos interesa que aunque esté desactivado se le apliquen los cambios
                foreach(var _go in gos)
                {
                    if (_go.name == differences.gameObjectName) go = _go.gameObject;
                }

                //En caso de no encontrar ningun gameobject coincidente pasar a la siguiente escena
                if (!go) continue;

                //Recoge el componente del tipo especificado en la estructura
                Component comp = go.GetComponent(differences.type.Name);
                //En caso de no encontrarlo lo añade al gameobject
                if (!comp) { comp = go.AddComponent(differences.type); }
                //Recorre cada lista de valores tanto de campos y propiedades, e intenta setearles un valor en caso de que el valor de la lista sea null.
                if(differences.fields.Length > 0)
                for(int j = 0; j < differences.fields.Length; j++)
                {
                    if (differences.fieldsValues[j] != null)
                        {
                            try { differences.fields[j].SetValue(comp, differences.fieldsValues[j]); }
                            catch { }
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
                    }
                }
                for(int j = 0; j < differences.private_fields.Length; j++)
                {
                    if (differences.privateFieldValues[j] != null)
                    {
                        try
                        {
                            differences.private_fields[j].SetValue(comp, differences.privateFieldValues[j]);
                        }
                        catch { }
                    }
                }
                //Guarda los cambios en la escena antes de pasar a la siguiente
                Scene _scene = EditorSceneManager.GetActiveScene();
                EditorSceneManager.MarkSceneDirty(_scene);
                EditorSceneManager.SaveScene(_scene, "", false);
                EditorSceneManager.SaveOpenScenes();
            }
        }
        
    }

    Vector2 scrollPosition = Vector2.zero;
    /// <summary>
    /// Enseña las escenas buscadas en la ventana del editor
    /// </summary>
    private void DisplayScenesInfo()
    {
        //Nos interesa que se pueda hacer scroll en esta zona, ya que la lista se nos puede ir de madre
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

    /// <summary>
    /// Busca las escenas dentro de todos los assets y las guarda en las listas que tenemos justo encima
    /// </summary>
    private void SearchAllScenes()
    {
        //Si no se busca nada, o no cambia nada, no nos interesa seguir
        if (SearchString.Length <= 0 || oldString == SearchString) return;
        //Vacia las listas
        allScenes.Clear();
        enabledScenes.Clear();

        //Recoge los directorios de todos los assets del proyecto
        string[] allPaths = AssetDatabase.GetAllAssetPaths();

        scenesPaths.Clear();

        foreach(string path in allPaths)
        {
            //Separa los directorios por el punto, para asi tener el directorio en un lado, y el tipo de archivo por otro
            string[] splitted = path.Split(".");
            //En caso de tener formato de archivo, busca si el archivo es tipo .unity para saber si es una escena, y la añade a la lista correspondiente, y le crea un bool asociado
            //Que es el que nos dice si aplicaremos cambios en esta escena o no
            if (splitted.Length == 2)
            {
                if (splitted[1] == "unity" && splitted[0].Contains(SearchString)) { scenesPaths.Add(path); enabledScenes.Add(true); Debug.Log(splitted[0]); }
            }
        }
        
        //Guarda los SceneAssets cargados desde los directorios elegidos
        foreach(var path in scenesPaths)
        {
            SceneAsset scene = AssetDatabase.LoadAssetAtPath(path, typeof(SceneAsset)) as SceneAsset;
            allScenes.Add(scene);   
        }
        oldString = SearchString;
    }
    #endregion
}
#endif
