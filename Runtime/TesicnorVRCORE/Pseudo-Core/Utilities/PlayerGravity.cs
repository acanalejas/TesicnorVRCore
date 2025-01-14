using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGravity : MonoBehaviour
{
    #region PARAMETERS
    [Header("El GameObject del cuerpo")]
    [SerializeField] public GameObject BodyGO;

    [Header("Esta activa la gravedad?")]
    public bool IsGravtyActive = true;

    [Header("Los enganches que pueden afectar a la gravedad")]
    [SerializeField] private AnchorInterface[] Anchors;

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

        BodyRB.isKinematic = true;
    }

    void TransferVelocity()
    {

    }

    private void Start()
    {
        if (!IsGravtyActive) return;
        SetupBodyGO();
        StartCoroutine(nameof(CustomUpdate));
    }

    WaitForEndOfFrame Frame = new WaitForEndOfFrame();
    IEnumerator CustomUpdate()
    {
        while (true)
        {
            if(!IsPlayerAnchored())
                BodyGO.transform.parent.position += BodyRB.velocity * Time.deltaTime;

            yield return Frame;
        }
    }

    private bool IsPlayerAnchored()
    {
        foreach(var a in Anchors)
        {
            if (a.IsAnchored()) return true;
        }
        return false;
    }
    #endregion
}
