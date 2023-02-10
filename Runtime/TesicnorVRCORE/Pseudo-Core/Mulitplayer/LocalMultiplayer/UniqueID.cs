using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniqueID : MonoBehaviour
{
    public int ID { get { return id; } }
    [SerializeField]private int id;

    public void SetID(int _id)
    {
        id = _id;
    }
}
