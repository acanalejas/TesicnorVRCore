using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGravity : MonoBehaviour
{
    #region PARAMETERS
    [Header("El GameObject del cuerpo")]
    [SerializeField] private GameObject BodyGO;

    private Rigidbody BodyRB;
    private CapsuleCollider BodyColl;
    #endregion

    #region METHODS
    private void SetupBodyGO()
    {
        if(!BodyGO.GetComponent<CapsuleCollider>()) BodyColl = BodyGO.AddComponent<CapsuleCollider>();
        else BodyColl = BodyGO.GetComponent<CapsuleCollider>();
        if (!BodyGO.GetComponent<Rigidbody>()) BodyRB = BodyGO.AddComponent<Rigidbody>();
        else BodyRB = BodyGO.GetComponent<Rigidbody>();

        BodyColl.height = 1.75f;
        BodyColl.radius = 0.3f;
    }

    void TransferVelocity()
    {

    }

    private void Start()
    {
        SetupBodyGO();
    }
    #endregion
}
