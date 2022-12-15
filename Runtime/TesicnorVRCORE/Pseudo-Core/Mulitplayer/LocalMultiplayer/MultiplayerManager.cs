using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Net.NetworkInformation;
using System.Net;

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
public class MultiplayerManager : MonoBehaviour
{
    #region SINGLETON
    private static MultiplayerManager instance;
    public static MultiplayerManager Instance { get { return instance; } }

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
    public static string IP { get {

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
            return "http://localhost:8080/";
        } }

    public static int Players { get { return players; } }
    private static int players;

    private static char separator { get { return '|'; } }

    private ReplicatedObject[] allReplicated;
    #endregion

    #region FUNCTIONS

    private void Awake()
    {
        CheckSingleton();
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
            components_str.Add(comp.name);
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
    public static Vector3 vt3_FromString(string input)
    {
        string[] splitted = input.Split(separator);
        float x; float.TryParse(splitted[0], out x);
        float y; float.TryParse(splitted[1], out y);
        float z; float.TryParse(splitted[2], out z);

        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Creates a Quaternion from a string
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static Quaternion quat_FromString(string input)
    {
        return Quaternion.Euler(vt3_FromString(input));
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
        string[] datas = input.Split(separator);
        foreach(var data in datas)
        {
            GameObjectData _data = JsonUtility.FromJson<GameObjectData>(data);
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
        allReplicated = GameObject.FindObjectsOfType<ReplicatedObject>();

        GameObjectData[] allData = allData_god(input);

        foreach(var data in allData)
        {
            foreach(var rep in allReplicated)
            {
                if (rep.name == data.Name) rep.Replicate(data);
            }
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
                result += Json_FromGameObjectData(obj.this_data);
            }
        }

        return result;
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
}
#endif
