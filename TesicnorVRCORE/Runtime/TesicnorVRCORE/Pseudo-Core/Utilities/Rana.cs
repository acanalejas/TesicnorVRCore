using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Rana : VRCollider
{ 
    #region PARAMETERS
    [Header("El mosqueton de la rana")]
    [SerializeField] Transform mosqueton;

    [Header("El origen del cable")]
    [SerializeField] Transform cableOrigen;

    [Header("El holder del mosqueton")]
    [SerializeField] Transform mosqueton_holder;

    [Header("El holder del origen del cable")]
    [SerializeField] Transform cableOrigen_holder;

    [Header("El holder de la rana")]
    [SerializeField] Transform rana_holder;

    [Header("El target del LineRenderer")]
    [SerializeField] Transform cable_target;

    [Header("El origen del cable")]
    [SerializeField] Transform cable_origen;

    [Header("El arnes")]
    [SerializeField] Transform arnes;
    #endregion

    #region FUNCTIONS

    private void Start()
    {
    }

    WaitForEndOfFrame frame = new WaitForEndOfFrame();
    public void Update()
    {
            mosqueton.position = mosqueton_holder.position;
            cableOrigen.position = cableOrigen_holder.position;

            mosqueton.rotation = mosqueton_holder.rotation;
            cableOrigen.rotation = cableOrigen_holder.rotation;

            //this.transform.localPosition = new Vector3(this.transform.localPosition.x, 0, this.transform.localPosition.z);

            if(!this.isGrabbed()) this.transform.position = new Vector3(this.transform.position.x, rana_holder.position.y, this.transform.position.z);

            SetLineRenderer();
    }

    public void SetLineRenderer()
    {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, cable_origen.position);
        lineRenderer.SetPosition(1, cable_target.position);
    }
    #endregion
}
