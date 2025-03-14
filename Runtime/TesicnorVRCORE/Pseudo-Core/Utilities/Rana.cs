using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Rana : VRCollider, AnchorInterface
{ 
    #region PARAMETERS
    [Header("El mosqueton de la rana")]
    public Transform mosqueton;

    [Header("El origen del cable")]
    public Transform cableOrigen;

    [Header("El holder del mosqueton")]
    public Transform mosqueton_holder;

    [Header("El holder del origen del cable")]
    public Transform cableOrigen_holder;

    [Header("El holder de la rana")]
    public Transform rana_holder;

    [Header("El target del LineRenderer")]
    public Transform cable_target;

    [Header("El origen del cable")]
    public Transform cable_origen;

    [Header("El arnes")]
    public Transform arnes;
    #endregion

    #region FUNCTIONS

    private void Start()
    {
        release = releaseType.holder;
        holder = rana_holder;
    }

    WaitForEndOfFrame frame = new WaitForEndOfFrame();
    public void Update()
    {
            mosqueton.position = mosqueton_holder.position;
            cableOrigen.position = cableOrigen_holder.position;

            mosqueton.rotation = mosqueton_holder.rotation;
            cableOrigen.rotation = cableOrigen_holder.rotation;

            //this.transform.localPosition = new Vector3(this.transform.localPosition.x, 0, this.transform.localPosition.z);

            if(!this.isGrabbed() && target && target.conditionCompleted) this.transform.position = new Vector3(this.transform.position.x, rana_holder.position.y, this.transform.position.z);
            else { this.transform.position = rana_holder.position; this.transform.rotation = rana_holder.rotation; }

            SetLineRenderer();
    }

    public void SetLineRenderer()
    {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, cable_origen.position);
        lineRenderer.SetPosition(1, cable_target.position);
    }

    public bool IsAnchored()
    {
        return !this.isGrabbed() && target && target.conditionCompleted;
    }

    public void AnchorIt()
    {
        throw new System.NotImplementedException();
    }

    public void ReleaseIt()
    {
        throw new System.NotImplementedException();
    }
    #endregion
}
