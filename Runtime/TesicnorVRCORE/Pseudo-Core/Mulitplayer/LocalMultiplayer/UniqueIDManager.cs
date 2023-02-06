using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class UniqueIDManager : MonoBehaviour
{
    private static UniqueIDManager instance;
    public static UniqueIDManager Instance { get { return instance; } set { instance = value; } }

    private void CheckSingleton()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    

    List<UniqueID> allIDs = new List<UniqueID>();

    private void Awake()
    {
        CheckSingleton();
    }

    public void SetIDs()
    {
        GameObject[] gameObjects = GameObject.FindObjectsOfType<GameObject>(true);
        int index = 0;
        foreach (GameObject gameObject in gameObjects)
        {
            if (gameObject.GetComponent<ReplicatedObject>() != null)
            {
                if (gameObject.GetComponent<ReplicatedObject>().insidePlayer && !gameObject.GetComponent<UniqueID>())
                {
                    UniqueID _id_player = gameObject.AddComponent<UniqueID>();
                    _id_player.SetID(-index);
                    allIDs.Add(_id_player);
                }
                else if (!gameObject.GetComponent<UniqueID>())
                {
                    UniqueID _id = gameObject.AddComponent<UniqueID>();
                    _id.SetID(index);
                    allIDs.Add(_id);
                }
            }
            else if (!gameObject.GetComponent<UniqueID>())
            {
                UniqueID _id = gameObject.AddComponent<UniqueID>();
                _id.SetID(index);
                allIDs.Add(_id);
            }
            index++;
        }
    }

    public GameObject GetGameObjectByID(int id)
    {
        GameObject result = null;
        int searchID = id;
        if (id < 0) searchID = -id;
        foreach (UniqueID _id in allIDs)
        {
            if (searchID == _id.ID) result = _id.gameObject;
        }

        if (result == null && id < 0)
        {
            GameObject original = null;
            foreach (var _id in allIDs)
            {
                if (id == _id.ID) original = _id.gameObject;
            }

            if (original)
            {
                Debug.Log("Creating new GOS for replicating");
                GameObject newGO = GameObject.Instantiate(original, original.transform.position, original.transform.rotation);
                newGO.GetComponent<UniqueID>().SetID(-id);
                result = newGO;
            }
        }

        return result;
    }

    public int GetIDFromGameObject(GameObject gameObject)
    {
        if (gameObject.GetComponent<UniqueID>() != null) return gameObject.GetComponent<UniqueID>().ID;

        return -1;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(UniqueIDManager), true)]
public class UniqueIDManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
    public void OnEnable()
    {
        UniqueIDManager manager = target as UniqueIDManager;
        manager.SetIDs();
    }
}
#endif
