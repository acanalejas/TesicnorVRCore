using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Net.NetworkInformation;
using System.Net;
using System;
using System.Reflection;
using System.IO;

public struct GameObjectData{
    //For transform, all measures in world space
    public string Position;
    public string Rotation;
    public string Scale;

    //For general
    public string Name;
    public string[] Components;
    public GameObjectData[] Children;
}

public struct MultiplayerClientData
{
    public int ID;
    public string URL;
}

public struct ActionData
{
    public string objectID;
    public string TypeName;
    public string ActionName;
}


[RequireComponent(typeof(UnityMainThreadInvoker))]
[RequireComponent(typeof(UniqueIDManager))]
[DisallowMultipleComponent]
public class MultiplayerManager : MonoBehaviour
{
    #region SINGLETON
    private static MultiplayerManager instance;
    public static MultiplayerManager Instance { get { return instance; } }

    public delegate void replicateMethod(Type classType, string methodName, int uniqueID);

    replicateMethod replicate = ReplicateMethod;

    static void ReplicateMethod(Type classType, string methodName, int UniqueID)
    {
        string toAdd = classType.Name + ParamsSeparator + methodName + ParamsSeparator + UniqueID.ToString();
        OnReplication += toAdd + MethodsSeparator;
    }

    void CheckSingleton()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }
    #endregion

    #region PARAMETERS
    public enum MultiplayerMode { P2P, Server}
    [SerializeField, HideInInspector]
    public MultiplayerMode mode;

    [SerializeField, HideInInspector]
    public static int Port = 8080;

    /// <summary>
    /// IP of this shit
    /// </summary>
    public static string IP { get { return LocalIP(); } }

    public static int Players { get { return players; } }
    private static int players;

    private static char separator { get { return '|'; } }
    private static char jsonSeparator { get { return '?'; } }

    private ReplicatedObject[] allReplicated;

    public static string OnReplication = "";

    private static char MethodsNJsonSeparator { get { return '%'; } }
    private static char MethodsSeparator { get { return '/'; } }
    private static char ParamsSeparator { get { return '!'; } }

    private MethodInfo[] replicatedMethods;

    public List<ActionData> actionsData = new List<ActionData>();
    #endregion

    #region FUNCTIONS

    private void Awake()
    {
        if (UniqueIDManager.Instance == null) this.gameObject.AddComponent<UniqueIDManager>();
        CheckSingleton();
        allReplicated = GameObject.FindObjectsOfType<ReplicatedObject>(true);
    }

    private void Start()
    {
#if UNITY_EDITOR
        replicatedMethods = GetAllReplicatedMethods();
#endif
    }

    public static string LocalIP()
    {
        var entry = Dns.GetHostEntry(Dns.GetHostName());

        var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

        foreach (var network in networkInterfaces)
        {
            IPInterfaceProperties properties = network.GetIPProperties();
            if (network.OperationalStatus != OperationalStatus.Up)
                continue;

            foreach (var address in properties.UnicastAddresses)
            {
                if (address.Address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                    continue;
                if (IPAddress.IsLoopback(address.Address))
                    continue;

                return address.Address.ToString();
            }
        }
        return "localhost";
    }

    /// <summary>
    /// Stores the data of a GameObject in a GameObjectData struct
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    public static GameObjectData fromGameObject(GameObject go)
    {
        GameObjectData data = new GameObjectData();

        data.Name = go.name;

        data.Position = str_fromVector3(go.transform.position);
        data.Rotation = str_fromQuaternion(go.transform.rotation);
        data.Scale = str_fromVector3(go.transform.localScale);

        Component[] components = go.GetComponents<Component>();
        List<string> components_str = new List<string>();
        foreach(var comp in components)
        {
            components_str.Add(comp.ToString());
        }
        data.Components = components_str.ToArray();

        return data;
    }

    /// <summary>
    /// Gets a Json string from a GameObjectData struct
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string Json_FromGameObjectData(GameObjectData input)
    {
        string json = JsonUtility.ToJson(input);
        return json;
    }

    /// <summary>
    /// Gets a GameObjectData struct from a Json string
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static GameObjectData GOD_FromJson(string json)
    {
        GameObjectData data = JsonUtility.FromJson<GameObjectData>(json);

        return data;
    }

    /// <summary>
    /// Creates a string from a Vector3
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string str_fromVector3(Vector3 input)
    {
        float x = input.x;
        float y = input.y;
        float z = input.z;

        return x.ToString() + separator.ToString() + y.ToString() + separator.ToString() + z.ToString();
    }

    /// <summary>
    /// Creates a string from a Quaternion
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string str_fromQuaternion(Quaternion input)
    {
        Vector3 angles = input.eulerAngles;
        return str_fromVector3(angles);
    }

    /// <summary>
    /// Creates a Vector3 from a string.
    /// Format of the string (float x|float y| float z)
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public Vector3 vt3_FromString(string input)
    {
        float x = 0;
        float y = 0;
        float z = 0;
        string[] splitted = input.Split(separator);

        float.TryParse(splitted[0], out x);
        float.TryParse(splitted[1], out y);
        float.TryParse(splitted[2], out z);

        Debug.Log("Vector from string " + x.ToString() + y.ToString() + z.ToString());
        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Creates a Quaternion from a string
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static Quaternion quat_FromString(string input)
    {
        Quaternion result = Quaternion.identity;
        try { result = Quaternion.Euler(Instance.vt3_FromString(input)); }
        catch { Debug.LogError("Error parsing from string to Quaternion"); }
        return result;
    }

    /// <summary>
    /// Creates an unique Json string from an array of GameObjectData
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string allData_str(GameObjectData[] input)
    {
        string result = "";
        int index = 0;
        foreach(var data in input)
        {
            string data_str = JsonUtility.ToJson(data);
            data_str += index < input.Length - 1 ? separator : "";
            result += data_str;
            index++;
        }

        return result;
    }

    /// <summary>
    /// Gets all GameObjectData structs from a single string
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static GameObjectData[] allData_god(string input)
    {
        List<GameObjectData> result = new List<GameObjectData>();
        if (input == null || input.Length == 0 || input == "") return null;
        string[] datas = input.Split(MethodsNJsonSeparator)[0].Split(jsonSeparator);
        foreach(var data in datas)
        {
            if (data == "" || data == null || data.Length <= 0) break;
            Debug.Log("Data splitted is : " + data);
            GameObjectData _data = JsonUtility.FromJson<GameObjectData>(data);

            if(data != null)
                result.Add(_data);
        }

        return result.ToArray();
    }

    /// <summary>
    /// Find all the replicated objects and applys the incoming modification from the other device
    /// </summary>
    /// <param name="input"></param>
    public void FindReplicatedGameObjects(string input)
    {
        try
        {
            string jsonObjects = input.Split(MethodsNJsonSeparator.ToString())[0];
            GameObjectData[] allData = allData_god(input);
            Debug.Log("Json parsed & AllDataLength : " + allData.Length);
            Debug.Log("All replicated length : " + allReplicated.Length);

            if (allData.Length > 0)
            {
                foreach (var data in allData)
                {
                    foreach (var rep in allReplicated)
                    {
                        if (rep.this_data.Name == data.Name) rep.Replicate(data);
                    }
                }
            }
        }
        catch(MissingReferenceException e)
        {
            Debug.LogError("Invalid GameObject to replicate");
        }
        try
        {
            string methods = input;
            try
            {
                methods = methods.Split(MethodsNJsonSeparator.ToString())[1];
            }
            catch
            {
                Debug.LogError("Error at spliting the string");
                return;
            }
            if (methods.Length <= 0 || methods == "" || methods == null) return;
            string[] methodsJson = methods.Split(jsonSeparator.ToString());

            foreach(var method in methodsJson)
            {
                Debug.Log("Before parsing to struct");
                ActionData data = JsonUtility.FromJson<ActionData>(method);
                Debug.Log("After parsing to struct");
                int id = 0; int.TryParse(data.objectID, out id);
                Debug.Log("After parsing string id : " + id);
                if (UniqueIDManager.Instance == null) Debug.Log("UniqueIDManager null value");
                GameObject go = UniqueIDManager.Instance.GetGameObjectByID(id);
                Debug.Log("After Getting the GO by ID");
                Component comp = go.GetComponent(data.TypeName);
                Debug.Log("After getting the component");
                MonoBehaviour mono = comp as MonoBehaviour;
                Debug.Log("After parsing component to MonoBehaviour");
                Debug.Log("Before Invoking the action");
                mono.Invoke("F" + data.ActionName, 0);
                
            }
        }
        catch
        {
            Debug.LogError("Couldn't replicate actions");
        }
    }

    public string FindReplicatedGameObjects_str()
    {
        allReplicated = GameObject.FindObjectsOfType<ReplicatedObject>();

        string result = "";
        if(allReplicated.Length > 0)
        {
            foreach(var obj in allReplicated)
            {
                result += Json_FromGameObjectData(obj.this_data) + jsonSeparator.ToString();
            }
        }

        result += MethodsNJsonSeparator.ToString() + FindReplicatedFunctions_str();
        return result;
    }

    public string FindReplicatedFunctions_str()
    {
        if (actionsData.Count <= 0) return "";

        string result = "";

        foreach(var data in actionsData)
        {
            result += JsonUtility.ToJson(data) + jsonSeparator.ToString();
        }

        return result;
    }

    public bool isValidString(string data)
    {
        if (data == null) return false;
        if (data.Length <= 0 || data == "") return false;

        if (!data.Contains(jsonSeparator) && !data.Contains(separator)) return false;

        if (data.Contains(jsonSeparator))
        {
            string[] splitted = data.Split(jsonSeparator);
            try
            {
                JsonUtility.FromJson<GameObjectData>(splitted[0]);
            }
            catch
            {
                return false;
            }
        }
        if (!data.Contains(jsonSeparator))
        {
            try
            {
                JsonUtility.FromJson<GameObjectData>(data);
            }
            catch
            {
                return false;
            }
        }

        return true;
    }

    Action toAdd;
    MethodInfo[] GetAllReplicatedMethods()
    {

        var methods = TypeCache.GetFieldsWithAttribute(typeof(ReplicatedAttribute));
        List<MethodInfo> allinfo = new List<MethodInfo>();

        
        foreach(var method in methods)
        {
            Type _type = method.DeclaringType;
            Debug.Log("Declaring type is : " + _type.FullName);
            UnityEngine.Object[] gos = GameObject.FindObjectsOfType(_type);
            Debug.Log("Objects with the type : " + gos.Length);
            
            foreach(var go in gos)
            {
                Component _go = go as Component;
                Debug.Log("Component type is : " + _go.GetType());
                MonoBehaviour mono = go as MonoBehaviour;

                toAdd = () =>
                {
                    ActionData adata = new ActionData();
                    adata.ActionName = method.Name;
                    adata.TypeName = _type.FullName;
                    adata.objectID = UniqueIDManager.Instance.GetIDFromGameObject(_go.gameObject).ToString();
                    this.actionsData.Add(adata);
                };

                Action ac = method.GetValue(_go) as Action;
                ac += toAdd;
                object obj = ac.Clone();

                

                Debug.Log(ac.GetInvocationList().Length);
                method.SetValue(_go, obj);
            }
        }

        return allinfo.ToArray();
    }
    
    private void AddOnInvokeReplicated()
    {
        /*if (replicatedMethods.Length <= 0) return;

        foreach(var _method in replicatedMethods)
        {
            Debug.Log(_method.DeclaringType);
            
        }*/
        
    }
    #endregion
}

#if UNITY_EDITOR
[CustomEditor(typeof(MultiplayerManager), true)]
[CanEditMultipleObjects]
[DisallowMultipleComponent]
public class MultiplayerManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var manager = target as MultiplayerManager;

        //Parameters
        GUILayout.Space(10);
        GUILayout.Label("Modo de multijugador", EditorStyles.boldLabel);
        manager.mode = (MultiplayerManager.MultiplayerMode)EditorGUILayout.EnumPopup(manager.mode);

        GUILayout.Space(10);
        GUILayout.Label("Puerto que usaremos para la comunicación", EditorStyles.boldLabel);
        MultiplayerManager.Port = EditorGUILayout.IntField(MultiplayerManager.Port);
    }

    private void CheckActionWrapper(MultiplayerManager manager)
    {

    }
}
#endif

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
public class ReplicatedAttribute : Attribute
{
    public ReplicatedAttribute(string filePath, string actionName, string className)
    {
        FileStream fs = OverrideCode.BothStream(filePath);
        OverrideCode.AddMethod(fs, "F" + actionName, actionName + "(); \n MultiplayerManager.Instance.actionsData.RemoveAt(MultiplayerManager.Instance.actionsData.Count);", className);
        fs.Close();
        Debug.Log("Constructing Replicated Attribute");
    }
}


#region FOR ID MANAGEMENT
[DisallowMultipleComponent]
public class UniqueIDManager : MonoBehaviour
{
    private static UniqueIDManager instance;
    public static UniqueIDManager Instance { get { return instance; } set { instance = value; } }

    private void CheckSingleton()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    public class UniqueID : MonoBehaviour
    {
        public int ID { get { return  id;} }
        private int id;

        public void SetID(int _id)
        {
            id = _id;
        }
    }

    List<UniqueID> allIDs = new List<UniqueID>();

    private void Awake()
    {
        CheckSingleton();

        GameObject[] gameObjects = GameObject.FindObjectsOfType<GameObject>(true);
        int index = 0;
        foreach(GameObject gameObject in gameObjects)
        {
            UniqueID _id = gameObject.AddComponent<UniqueID>();
            _id.SetID(index);
            allIDs.Add(_id);
            index++;
        }
    }

    public GameObject GetGameObjectByID(int id)
    {
        GameObject result = null;
        foreach(UniqueID _id in allIDs)
        {
            if (id == _id.ID) result = _id.gameObject;
        }

        return result;
    }

    public int GetIDFromGameObject(GameObject gameObject)
    {
        if(gameObject.GetComponent<UniqueID>() != null) return gameObject.GetComponent<UniqueID>().ID;

        return -1;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(UniqueIDManager), true)]
public class UniqueIDManagerEditor: Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        target.hideFlags = HideFlags.HideInInspector;
    }
}
#endif
#endregion