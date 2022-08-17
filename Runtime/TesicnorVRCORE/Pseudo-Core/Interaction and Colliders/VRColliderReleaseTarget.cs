using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class VRColliderReleaseTarget : MonoBehaviour
{
    #region PARAMETERS
    [HideInInspector] public bool conditionCompleted;

    /// <summary>
    /// Indica si queremos que se desactive el objeto al llegar al target
    /// </summary>
    [Header("Indica si queremos que se desactive el objeto al llegar al target")]
    public bool DisableWhenRelease = true;

    [Header("Se puede soltar ya el objeto aqui?")]
    public bool canReleaseObject = false;

    [Header("Sigue al target despues de soltar?")]
    public bool seeksTarget = true;

    [Header("Puede cancelarse la condicion?")]
    public bool canBeCanceled = false;

    [Header("Necesita estar agarrado?")]
    public bool needsGrabbing = false;
    #endregion
    #region FUNCTIONS
    private void Awake()
    {
        SetCollider();
    }

    /// <summary>
    /// Setea los valores deseados del collider y el rigidbody en el Awake
    /// </summary>
    private void SetCollider()
    {
        GetComponent<BoxCollider>().isTrigger = true;
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Rigidbody>().isKinematic = true;
    }

    /// <summary>
    /// Comprueba si el objeto que entra en el trigger es el que debe
    /// soltarse aqui
    /// </summary>
    /// <param name="go">gameobject que colisiona con este objeto</param>
    public void CheckVRCollider(GameObject go)
    {
        if (go.GetComponent<VRCollider>())
        {
            VRCollider collider = go.GetComponent<VRCollider>();
            if(collider.target == this && canReleaseObject && (!needsGrabbing || (needsGrabbing && collider.isGrabbed())))
            {
                conditionCompleted = true;
                if (collider.DropTeleport)
                {
                    if(collider.GetGrippingHand())collider.GetGrippingHand().Release();
                    collider.Release();
                    collider.SetGrabbable(false);
                    if (DisableWhenRelease)
                    {
                        collider.gameObject.SetActive(false);
                    }
                    if (seeksTarget)
                    {
                        collider.transform.parent = this.transform;
                    }
                }
            }
        }
    }

    public void CheckVRColliderExit(GameObject go)
    {
        if (go.GetComponent<VRCollider>())
        {
            if(go.GetComponent<VRCollider>().target == this)
            {
                if(canBeCanceled) conditionCompleted = false;
            }
        }
    }

    #region Trigger
    private void OnTriggerEnter(Collider other)
    {
        if(!conditionCompleted) CheckVRCollider(other.gameObject);
    }
    private void OnTriggerExit(Collider other)
    {
        CheckVRColliderExit(other.gameObject);
    }
    #endregion
    #endregion
}
