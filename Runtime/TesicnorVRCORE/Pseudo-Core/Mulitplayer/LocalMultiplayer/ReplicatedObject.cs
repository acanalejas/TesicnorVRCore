using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

[DisallowMultipleComponent]
[SerializeField]
public class ReplicatedObject : MonoBehaviour
{
    #region PARAMETERS
    public GameObjectData this_data = new GameObjectData();
    GameObject[] children;

    public Transform this_transform;

    public bool insidePlayer = false;

    public System.Action replicate;

    public GameObjectData last_data = new GameObjectData();
    public GameObjectData last_data2 = new GameObjectData();

    public GameObjectData[] last_datas = new GameObjectData[30];
    List<GameObjectData> children_data = new List<GameObjectData>();
    #endregion

    #region FUNCTIONS
    public void Awake()
    {
        GetChildren();
        InitializeDataBuffer();
        SetGameObjectData();
    }

    public void Start()
    {
        SetGameObjectData();
    }

    public void Update()
    {
        GetChildren();
        SetGameObjectData();
    }
    private void GetChildren()
    {
        Transform[] _children = this.transform.GetComponentsInChildren<Transform>();

        List<GameObject> _childrenList = new List<GameObject>();
        foreach(var child in _children)
        {
            if (child.gameObject != this.gameObject) _childrenList.Add(child.gameObject);
        }

        children = _childrenList.ToArray();
    }

    public void SetGameObjectData()
    {
        if (isDataRepeated(MultiplayerManager.fromGameObject(this.gameObject))) return;
        this_data = MultiplayerManager.fromGameObject(this.gameObject);
        this_data.Children = "";
        if(children.Length > 0)
        {
            for(int i = 0; i < children.Length; i++)
            {
                GameObjectData child_data = MultiplayerManager.fromGameObject(children[i], insidePlayer);
                this_data.Children += JsonUtility.ToJson(child_data,true) + MultiplayerManager.childSeparator;
            }
        }
    }

    bool isDataRepeated(GameObjectData input)
    {
        foreach(var data in last_datas)
        {
            if (MultiplayerManager.EqualsGameObjectData(data, input)) return true;
        }
        return false;
    }

    void InitializeDataBuffer()
    {
        for(int i = 0; i < last_datas.Length; i++)
        {
            last_datas[i] = new GameObjectData();
        }
    }

    void AddDataToBuffer(GameObjectData input)
    {
        for (int i = last_datas.Length - 2; i >= 0; i--)
        {
            last_datas[i] = last_datas[i + 1];
        }
        last_datas[last_datas.Length - 1] = input;
    }

    Vector3 lastPosition;
    Vector3 lastRotation;
    Vector3 lastScale;
    public void Replicate(GameObjectData input)
    {
        if (MultiplayerManager.EqualsGameObjectData(this_data, input) || isDataRepeated(input)) return;
        else this_data = input;
        try
        {
            //if (Vector3.Distance(lastPosition, this.transform.position) > 0.001f
            //    && Vector3.Distance(lastRotation, this.transform.rotation.eulerAngles) > 0.001f
            //    && Vector3.Distance(lastScale, this.transform.localScale) > 0.001f) return;

            if (MultiplayerManager.EqualsGameObjectData(this_data, last_data)) throw new UnityException("Not valid to replicate due its the same");
            if (this_transform == null) return;
            if (Vector3.Distance(this.transform.position, MultiplayerManager.Instance.vt3_FromString(input.Position)) > 0.0001f)
            { this_transform.localPosition = MultiplayerManager.Instance.vt3_FromString(input.Position); }
            if (Vector3.Distance(this.transform.rotation.eulerAngles, MultiplayerManager.quat_FromString(input.Rotation).eulerAngles) > 0.0001f)
            { this_transform.localRotation = MultiplayerManager.quat_FromString(input.Rotation); }
            if (Vector3.Distance(this.transform.localScale, MultiplayerManager.Instance.vt3_FromString(input.Scale)) > 0.0001f)
            { this_transform.localScale = MultiplayerManager.Instance.vt3_FromString(input.Scale); }
            lastPosition = this.transform.position;
            lastRotation = this.transform.rotation.eulerAngles;
            lastScale = this.transform.localScale;

            string _parentID = "null";
            try
            {
                if (transform.parent)
                {
                    UniqueID pID = this.transform.parent.GetComponent<UniqueID>();
                    int pID_int = Mathf.Abs(pID.ID);
                    
                    _parentID = pID_int.ToString();
                }
            }
            catch { Debug.LogError("No se pudo conseguir la ID del padre"); }

            if (_parentID != input.ParentID)
            {
                if (input.ParentID == "null") { transform.parent = null; }
                else
                {
                    int _id = 0; int.TryParse(input.ParentID, out _id);

                    GameObject newParent = UniqueIDManager.Instance.GetGameObjectByID(_id);
                    if (newParent != null)
                        transform.parent = newParent.transform;
                }
            }
            if(input.Children.Length > 0 && children != null)
            if (children.Length > 0 && MultiplayerManager.Instance)
            {
                    string[] children_str = input.Children.Split(MultiplayerManager.childSeparator);
                    children_data.Clear();
                    for(int i = 0; i < children_str.Length; i++)
                    {
                        GameObjectData _children_data = new GameObjectData();
                        if (children_str[i] != null && children_str[i].Length > 0)
                        {
                            _children_data = JsonUtility.FromJson<GameObjectData>(children_str[i]);
                            children_data.Add(_children_data);
                        }
                    }
                    List<GameObjectData> notReplicated = new List<GameObjectData>();
                foreach (var child in children_data)
                {
                        bool replicated = false;
                        foreach (var _child in children)
                        {
                            if (child.Name == _child.name && _child.transform)
                            {
                                if (child.Position.Length > 0 && child.Position != "")
                                    _child.transform.localPosition = MultiplayerManager.Instance.vt3_FromString(child.Position);
                                if (child.Rotation.Length > 0 && child.Rotation != "")
                                    _child.transform.localRotation = MultiplayerManager.quat_FromString(child.Rotation);
                                if (child.Scale.Length > 0 && child.Scale != "")
                                    _child.transform.localScale = MultiplayerManager.Instance.vt3_FromString(child.Scale);
                                replicated = true;
                            }
                        }
                        if (!replicated)
                        {
                            int childID = 0; int.TryParse(child.ID, out childID);
                            int parentID = 0; int.TryParse(child.ParentID, out parentID);

                            //childID = Mathf.Abs(childID);
                            //parentID = Mathf.Abs(parentID);

                            Debug.Log("Parent id is : " + parentID + "\n Child id is : " + childID);

                            GameObject childGO = UniqueIDManager.Instance.GetGameObjectByID(childID);
                            GameObject parentGO = UniqueIDManager.Instance.GetGameObjectByID(parentID);

                            if (childGO == null || parentGO == null) { Debug.LogError("Couldn't find the parent or the child" + "\n Parent value is : " + parentGO + "\n Child value is : " + childGO); break; }

                            childGO.transform.parent = parentGO.transform;

                            childGO.transform.localPosition = MultiplayerManager.Instance.vt3_FromString(child.Position);
                            childGO.transform.localRotation = MultiplayerManager.quat_FromString(child.Rotation);
                            childGO.transform.localScale = MultiplayerManager.Instance.vt3_FromString(child.Scale);
                        }
                    }
            }
        }   
        catch(UnityException e)
        {
            Debug.LogError("Failed to replicate, probably invalid GameObjectData  " + e);
        }
        last_data2 = last_data;
        last_data = input;
        AddDataToBuffer(input);
    }
    #endregion
}

#if UNITY_EDITOR
[CustomEditor(typeof(ReplicatedObject), true)]
public class ReplicatedObjectEditor: Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        SearchReplicatedAttribute();
    }

    void SearchReplicatedAttribute()
    {
        TypeCache.MethodCollection mc = TypeCache.GetMethodsWithAttribute(typeof(ReplicatedAttribute));
        TypeCache.FieldInfoCollection fic = TypeCache.GetFieldsWithAttribute(typeof(ReplicatedAttribute));

        Dictionary<MethodInfo, bool> md = new Dictionary<MethodInfo, bool>();
        Dictionary<FieldInfo, bool> fd = new Dictionary<FieldInfo, bool>();

        TypeCache.TypeCollection tc = TypeCache.GetTypesDerivedFrom(typeof(ReplicatedObject));

        //Checks if the methods are not in the correct class
        if(mc.Count > 0)
        {
            foreach(var method in mc)
            {
                bool derivado = false;
                foreach(var type in tc)
                {
                    if (method.DeclaringType.Name == type.Name) derivado = true;

                }

                md.Add(method, derivado);

                if (!derivado) Debug.LogException(new UnityException("Only classes derived from ReplicatedObject can use the ReplicatedAttribute"));
            }
        }

        //Checks if the fields are not in the correct class
        if(fic.Count > 0)
        {
            foreach(var field in fic)
            {
                bool derivado = false;
                foreach(var type in tc)
                {
                    if (field.DeclaringType.Name == type.Name) derivado = true;
                }

                fd.Add(field, derivado);

                if (!derivado) { Debug.LogException(new UnityException("Only classes derived from ReplicatedObject can use the ReplicatedAttribute")); }
                
            }
        }

        foreach(var method in md)
        {
            if (!method.Value) continue;

            if(target.GetType().Name == method.Key.DeclaringType.Name)
            {
                string path = OverrideCode.GetTypePath(target);
                Debug.Log(path);
                string fieldName = "f" + method.Key.Name;
                string fieldType = "System.Action";
                string fieldValue = "";

                //FileStream fs = OverrideCode.BothStream(path);
                //OverrideCode.AddCodeToMethod(fs, "Awake", "replicate += ()=>{" + method.Key.Name + "()" + "};", method.Key.DeclaringType.Name);
                //fs.Close();
            }
        }

        foreach(var field in fic)
        {
            field.GetCustomAttribute(typeof(ReplicatedAttribute));
        }
    }
}
#endif
