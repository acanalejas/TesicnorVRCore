using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SpawnedObjectData
{
    public string type;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
}

public class SpawnedObjectsDataSave : MonoBehaviour
{
    #region SINGLETON

    private static SpawnedObjectsDataSave instance;
    public static SpawnedObjectsDataSave Instance { get { return instance; } }

    void CheckSingleton()
    {
        if (!instance) instance = this;
        else Destroy(this);
    }

    #endregion

    #region PARAMETERS
    [Header("La lista de key y objeto a colocar")]
    public Dictionary<string, SO_ARItem> objects = new Dictionary<string, SO_ARItem>();

    public string[] Keys; public SO_ARItem[] Values;

    public UnityEngine.Events.UnityEvent<SO_ARItem> OnDataLoadAndSpawned;
     public UnityEngine.Events.UnityEvent OnDataLoadAndEmpty;

    public string SceneKey = "ARScene";

    private static string CreatedObjects = "";

    private char separator = '¿';
    #endregion

    #region METHODS

    private void Awake()
    {
        CheckSingleton();
    }

    private void Start()
    {
        for(int i = 0; i < Keys.Length; i++)
        {
            objects.Add(Keys[i], Values[i]);
        }
        LoadObjects();
    }

    public void AddObject(string Type, GameObject _object)
    {
        SpawnedObjectData newData = new SpawnedObjectData();
        newData.type = Type;
        newData.position = _object.transform.position;
        newData.rotation = _object.transform.rotation;
        newData.scale = _object.transform.localScale;

        string data = JsonUtility.ToJson(newData);
        string previousData = PlayerPrefs.HasKey(SceneKey) ? PlayerPrefs.GetString(SceneKey) : "";

        string totalData = previousData != "" ? previousData + separator.ToString() + data : data;

        PlayerPrefs.SetString(totalData, SceneKey);
        PlayerPrefs.Save();

        string _totalData = CreatedObjects != "" ? CreatedObjects + separator.ToString() + data : data;
        CreatedObjects = _totalData;

        Debug.Log("Objeto creado guardado " + totalData);
    }

    SO_ARItem typeObj = null;
    public void LoadObjects()
    {
        string savedData = PlayerPrefs.HasKey(SceneKey) ? PlayerPrefs.GetString(SceneKey) : "";
        if (savedData == "" && CreatedObjects != "") savedData = CreatedObjects;

        Debug.Log("Saved data is " + savedData);

        if (savedData.Contains(separator))
        {
            string[] jsons = savedData.Split(separator);

            if (jsons.Length > 0 && savedData != "")
            {
                foreach (var json in jsons)
                {
                    SpawnedObjectData data = JsonUtility.FromJson<SpawnedObjectData>(json);

                    if (objects.TryGetValue(data.type, out typeObj))
                    {
                        GameObject result = GameObject.Instantiate(typeObj.prefab, data.position, data.rotation);
                        result.transform.localScale = data.scale;
                    }
                }
                OnDataLoadAndSpawned.Invoke(typeObj);
            }
        }
        else if(savedData != "")
        {
            SpawnedObjectData data = JsonUtility.FromJson<SpawnedObjectData>(savedData);

            if (objects.TryGetValue(data.type, out typeObj))
            {
                GameObject result = GameObject.Instantiate(typeObj.prefab, data.position, data.rotation);
                result.transform.localScale = data.scale;
            }

            OnDataLoadAndSpawned.Invoke(typeObj);
        }
            
        else OnDataLoadAndEmpty.Invoke();
    }

    public void RemoveData()
    {
        PlayerPrefs.SetString(SceneKey, "");
        CreatedObjects = "";
    }

    #endregion
}
