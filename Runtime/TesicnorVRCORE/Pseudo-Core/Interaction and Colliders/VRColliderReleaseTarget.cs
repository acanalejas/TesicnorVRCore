using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

    private bool wasUsingGravity;
    private bool wasKinematic;
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
            if(isGoodTarget(go) && canReleaseObject && collider.hasTarget && (!needsGrabbing || (needsGrabbing && collider.isGrabbed())))
            {
                conditionCompleted = true;
                if (collider.targetSound != null) {collider.targetSound.loop = false; collider.targetSound.Play();}
                if (collider.DropTeleport)
                {
                    if(collider.GetGrippingHand())collider.GetGrippingHand().Release();
                    //collider.Release();
                    collider.SetGrabbable(false);
                    collider.onTargetReached.Invoke(this.gameObject);
                    if (DisableWhenRelease)
                    {
                        collider.gameObject.SetActive(false);
                    }
                    if (seeksTarget)
                    {
                        collider.transform.parent = this.transform;

                        if (collider.GetComponent<Rigidbody>())
                        {
                            Rigidbody rb = collider.GetComponent<Rigidbody>();

                            wasKinematic = rb.isKinematic;
                            wasUsingGravity = rb.useGravity;

                            rb.useGravity = false;
                            rb.isKinematic = true;
                        }
                    }
                }
            }
        }
    }

    bool isGoodTarget(GameObject go)
    {
        if (go.GetComponent<VRCollider>() == null) return false;

        VRCollider coll = go.GetComponent<VRCollider>();
        return (coll.hasMultipleTargets && coll.targets.Contains(this)) || (!coll.hasMultipleTargets && coll.target == this); 
    }

    public void CheckVRColliderExit(GameObject go)
    {
        if (go.GetComponent<VRCollider>())
        {
            if(isGoodTarget(go))
            {
                if(canBeCanceled) conditionCompleted = false;

                if (go.GetComponent<Rigidbody>())
                {
                    go.GetComponent<Rigidbody>().useGravity = wasUsingGravity;
                    go.GetComponent<Rigidbody>().isKinematic = wasKinematic;
                }
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
