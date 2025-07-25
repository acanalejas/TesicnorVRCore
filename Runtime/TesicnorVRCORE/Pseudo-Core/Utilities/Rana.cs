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

    [Header("El eje sobre el que se mueve la rana")]
    public axis Axis;
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

        if (!this.isGrabbed() && target && target.conditionCompleted) 
        {
            switch (Axis)
            {
                case axis.x:
                    this.transform.position = new Vector3(this.rana_holder.position.x, this.transform.position.y, this.transform.position.z);
                    break;
                case axis.y:
                    this.transform.position = new Vector3(this.transform.position.x, rana_holder.position.y, this.transform.position.z);
                    break;
                case axis.z:
                    this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, this.rana_holder.position.z);
                    break;
            }
             
        
        }
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

    public void CheckDistance()
    {
        throw new System.NotImplementedException();
    }

    public void EnableWarning()
    {
        throw new System.NotImplementedException();
    }

    public void DisableWarning()
    {
        throw new System.NotImplementedException();
    }

    public void AnchorIt(GameObject _anchor)
    {
        throw new System.NotImplementedException();
    }

    public void ReleaseIt(GameObject _anchor)
    {
        throw new System.NotImplementedException();
    }
    #endregion
}
