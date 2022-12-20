using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[SerializeField]
public class ReplicatedObject : MonoBehaviour
{
    #region PARAMETERS
    public GameObjectData this_data;
    GameObject[] children;

    public bool insidePlayer = false;
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
        if(children.Length > 0)
        {
            for(int i = 0; i < children.Length; i++)
            {
                this_data.Children[i] = MultiplayerManager.fromGameObject(children[i]);
            }
        }
    }

    public void Replicate(GameObjectData input)
    {
        this_data = input;

        try
        {
            this.transform.position = MultiplayerManager.vt3_FromString(this_data.Position);
            this.transform.rotation = MultiplayerManager.quat_FromString(this_data.Rotation);
            this.transform.localScale = MultiplayerManager.vt3_FromString(this_data.Scale);

            if (children.Length > 0)
            {
                for (int i = 0; i < children.Length; i++)
                {
                    children[i].transform.position = MultiplayerManager.vt3_FromString(this_data.Children[i].Position);
                    children[i].transform.rotation = MultiplayerManager.quat_FromString(this_data.Children[i].Rotation);
                    children[i].transform.localScale = MultiplayerManager.vt3_FromString(this_data.Children[i].Scale);
                }
            }
        }
        catch
        {
            Debug.LogWarning("Coudn't replicate because the GameObjectData given was not valid");
        }
        
    }
    #endregion
}
