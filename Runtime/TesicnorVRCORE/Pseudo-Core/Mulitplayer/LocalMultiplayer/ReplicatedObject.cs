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
    #endregion

    #region FUNCTIONS
    public void Awake()
    {
        GetChildren();
        SetGameObjectData();
    }

    public void Start()
    {
        SetGameObjectData();
    }

    public void Update()
    {
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
        this_data = MultiplayerManager.fromGameObject(this.gameObject);
        if (children == null || this_data.Children == null) return;
        if(children.Length > 0 && this_data.Children.Length > 0)
        {
            for(int i = 0; i < children.Length; i++)
            {
                this_data.Children[i] = MultiplayerManager.fromGameObject(children[i]);
            }
        }
    }

    Vector3 lastPosition;
    Vector3 lastRotation;
    Vector3 lastScale;
    public void Replicate(GameObjectData input)
    {
        this_data = input;
        try
        {
            //if (Vector3.Distance(lastPosition, this.transform.position) > 0.001f
            //    && Vector3.Distance(lastRotation, this.transform.rotation.eulerAngles) > 0.001f
            //    && Vector3.Distance(lastScale, this.transform.localScale) > 0.001f) return;

            if (MultiplayerManager.EqualsGameObjectData(this_data, last_data)) throw new UnityException("Not valid to replicate due its the same");
            if (this_transform == null) return;
            if (Vector3.Distance(this.transform.position, MultiplayerManager.Instance.vt3_FromString(input.Position)) > 0.001f)
            { this_transform.position = MultiplayerManager.Instance.vt3_FromString(input.Position); }
            if (Vector3.Distance(this.transform.rotation.eulerAngles, MultiplayerManager.quat_FromString(input.Rotation).eulerAngles) > 0.001f)
            { this_transform.rotation = MultiplayerManager.quat_FromString(input.Rotation); }
            if (Vector3.Distance(this.transform.localScale, MultiplayerManager.Instance.vt3_FromString(input.Scale)) > 0.001f)
            { this_transform.localScale = MultiplayerManager.Instance.vt3_FromString(input.Scale); }
            lastPosition = this.transform.position;
            lastRotation = this.transform.rotation.eulerAngles;
            lastScale = this.transform.localScale;

            string _parentID = "null";
            try
            {
                if(transform.parent)
                _parentID = this.transform.parent.GetComponent<UniqueID>().ID.ToString();
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
            /*if (children.Length > 0)
            {
                for (int i = 0; i < children.Length; i++)
                {
                    children[i].transform.position = MultiplayerManager.vt3_FromString(input.Children[i].Position);
                    children[i].transform.rotation = MultiplayerManager.quat_FromString(input.Children[i].Rotation);
                    children[i].transform.localScale = MultiplayerManager.vt3_FromString(input.Children[i].Scale);
                }
            }*/
        }
        catch(UnityException e)
        {
            Debug.LogError("Failed to replicate, probably invalid GameObjectData  " + e);
        }
        last_data = input;
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
