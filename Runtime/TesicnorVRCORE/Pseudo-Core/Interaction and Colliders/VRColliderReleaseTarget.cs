using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class VRColliderReleaseTarget : MonoBehaviour
{
    #region PARAMETERS
     public bool conditionCompleted;

    /// <summary>
    /// Indica si queremos que se desactive el objeto al llegar al target
    /// </summary>
    [Header("Indica si queremos que se desactive el objeto al llegar al target")]
    public bool DisableWhenRelease = true;

    [Header("Queremos que no se pueda volver a agarrar?")]
    public bool MakeUngrababble = false;

    [Header("Se puede soltar ya el objeto aqui?")]
    public bool canReleaseObject = false;

    [Header("Sigue al target despues de soltar?")]
    public bool seeksTarget = true;

    [Header("Puede cancelarse la condicion?")]
    public bool canBeCanceled = false;

    [Header("Necesita estar agarrado?")]
    public bool needsGrabbing = false;

    [Header("OPCIONAL: El Gameobject al que se adhiere")]
    public Transform attachHolder;

    [Header("Evento para cuando se el objeto llega al target")]
    public UnityEvent OnTargetReached;

    [Header("Evento para cuando se retira el objeto")]
    public UnityEvent OnTargetRelease;

    private bool wasUsingGravity;
    private bool wasKinematic;

    protected VRCollider attachedCollider;
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
    public virtual void CheckVRCollider(GameObject go)
    {
        if (go.GetComponent<VRCollider>())
        {
            VRCollider collider = go.GetComponent<VRCollider>();
            if(isGoodTarget(go) && canReleaseObject && collider.hasTarget && (!needsGrabbing || (needsGrabbing && collider.isGrabbed())))
            {
                conditionCompleted = true;
                OnTargetReached.Invoke();
                if (collider.targetSound != null) {collider.targetSound.loop = false; collider.targetSound.Play();}
                if (collider.GetGrippingHand()) collider.GetGrippingHand().Release();
                if (collider.DropTeleport)
                {
                    //collider.Release();
                    if (DisableWhenRelease)
                    {
                        collider.gameObject.SetActive(false);
                    }
                    if (MakeUngrababble)
                    {
                        collider.enabled = false;
                        go.GetComponent<Collider>().enabled = false;
                        collider.SetGrabbable(false);
                    }
                    if (seeksTarget)
                    {
                        AttachObject(collider);
                    }
                    
                    collider.onTargetReached.Invoke(this.gameObject);
                }
            }
        }
    }

    public virtual void AttachObject(VRCollider collider)
    {
        collider.transform.parent = attachHolder == null ? this.transform : attachHolder;
        collider.transform.localPosition = Vector3.zero;
        collider.transform.localRotation = Quaternion.identity;
        attachedCollider = collider;
        collider.target = this;

        if (collider.GetComponent<Rigidbody>())
        {
            Rigidbody rb = collider.GetComponent<Rigidbody>();

            wasKinematic = rb.isKinematic;
            wasUsingGravity = rb.useGravity;

            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    protected virtual bool isGoodTarget(GameObject go)
    {
        if (go.GetComponent<VRCollider>() == null) return false;

        VRCollider coll = go.GetComponent<VRCollider>();
        return (coll.hasMultipleTargets && coll.targets.Contains(this)) || (!coll.hasMultipleTargets && coll.target == this); 
    }

    public virtual void CheckVRColliderExit(GameObject go)
    {
        if(go == attachedCollider.gameObject) DeattachObject(attachedCollider);
    }

    public bool isAttachedObject(VRCollider _col)
    {
        return attachedCollider == _col;
    }

    public bool GoodDistanceToDettach()
    {
        Vector3 point1 = this.GetComponent<Collider>().ClosestPointOnBounds(attachedCollider.transform.position);
        Vector3 point2 = attachedCollider.GetComponent<Collider>().ClosestPointOnBounds(this.transform.position);

        return Vector3.Distance(point1, point2) > 0.15f;
    }

    public virtual void DeattachObject(VRCollider collider)
    {
        if (attachedCollider && isGoodTarget(attachedCollider.gameObject))
        {
            if (canBeCanceled) conditionCompleted = false;

            OnTargetRelease.Invoke();
            attachedCollider.OnTargetReleased.Invoke();

            if (attachedCollider.gameObject.GetComponent<Rigidbody>())
            {
                attachedCollider.gameObject.GetComponent<Rigidbody>().useGravity = wasUsingGravity;
                attachedCollider.gameObject.GetComponent<Rigidbody>().isKinematic = wasKinematic;
            }
        }
    }

    #region Trigger
    //protected virtual void OnTriggerEnter(Collider other)
    //{
    //    if(!conditionCompleted) CheckVRCollider(other.gameObject);
    //}

    private void OnTriggerStay(Collider other)
    {
        if (!conditionCompleted && other != null && other.gameObject != null) CheckVRCollider(other.gameObject);
    }
    protected virtual void OnTriggerExit(Collider other)
    {
        if(conditionCompleted) CheckVRColliderExit(other.gameObject);
    }
    #endregion
    #endregion
}
