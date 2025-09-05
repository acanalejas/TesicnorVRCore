using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Rana : Anchor
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

    float targetExtent;
    #endregion

    #region FUNCTIONS

    private void Start()
    {
        release = releaseType.holder;
        holder = rana_holder;

        targetExtent = Axis == axis.x ? target.GetComponent<Collider>().bounds.extents.x : Axis == axis.y ? target.GetComponent<Collider>().bounds.extents.y : target.GetComponent<Collider>().bounds.extents.z;
<<<<<<< HEAD

        globalCenter = target.transform.TransformPoint(target.GetComponent<Collider>().bounds.center);
=======
>>>>>>> parent of a7f87c9 (Update Rana.cs)
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
                    this.transform.position = new Vector3(Mathf.Clamp(this.rana_holder.position.x, target.transform.position.x - targetExtent, target.transform.position.x + targetExtent)
                        , this.transform.position.y
                        , this.transform.position.z);
                    break;
                case axis.y:
                    this.transform.position = new Vector3(this.transform.position.x
                        , Mathf.Clamp(this.rana_holder.position.y, target.transform.position.y - targetExtent, target.transform.position.y + targetExtent)
                        , this.transform.position.z);
                    break;
                case axis.z:
                    this.transform.position = new Vector3(this.transform.position.x
                        , this.transform.position.y
                        , Mathf.Clamp(this.rana_holder.position.z, target.transform.position.z - targetExtent, target.transform.position.z + targetExtent));
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
