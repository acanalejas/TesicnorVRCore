using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRSlidingReleaseTarget : VRColliderReleaseTarget
{
    #region PARAMETERS
    [Header("El eje LOCAL sobre el que va a deslizar")]
    [SerializeField] protected axis Axis = axis.x;

    [Header("Funciona a traves de las dimensiones del collider?")]
    [SerializeField] protected bool bUsesCollider = true;

    [Header("La velocidad a la que se desliza el objeto")]
    [SerializeField] protected float SlideSpeed = 5;

    [Header("El offset que se aplica al posicionamiento en la dirección elegida")]
    [SerializeField] protected float PositioningOffset = 0;

    private Collider col;
    #endregion

    #region METHODS
    public override void AttachObject(VRCollider collider)
    {
        base.AttachObject(collider);
        StartCoroutine(nameof(SlideObjectCoroutine));
    }
    public override void DeattachObject(VRCollider collider)
    {
        base.DeattachObject(collider);
        StopCoroutine(nameof(SlideObjectCoroutine));
    }

    WaitForEndOfFrame frame = new WaitForEndOfFrame();
    IEnumerator SlideObjectCoroutine()
    {
        col = this.GetComponent<Collider>();
        while (true)
        {
            SlideObject();
            yield return frame;
        }
    }
    protected virtual void SlideObject()
    {
        if (!attachedCollider) return;
        float extent = Axis == axis.x ? col.bounds.extents.x : Axis == axis.y ? col.bounds.extents.y : col.bounds.extents.z;

        attachedCollider.transform.position = Vector3.Lerp(attachedCollider.transform.position,
            Axis == axis.x ? new Vector3(TesicnorPlayer.Instance.Camera_GO.position.x + PositioningOffset, attachedCollider.transform.position.y, attachedCollider.transform.position.z) :
            Axis == axis.y ? new Vector3(attachedCollider.transform.position.x, TesicnorPlayer.Instance.Camera_GO.position.y + PositioningOffset, attachedCollider.transform.position.z) :
            new Vector3(attachedCollider.transform.position.x, attachedCollider.transform.position.y, TesicnorPlayer.Instance.Camera_GO.position.z + PositioningOffset), Time.deltaTime * SlideSpeed);

        if (!bUsesCollider) return;

        float clampedCoordinate = 0;
        float minCoordinate = 0;
        float maxCoordinate = 0;
        switch (Axis)
        {
            case axis.x:
                minCoordinate = this.transform.position.x - extent;
                maxCoordinate = this.transform.position.x + extent;

                clampedCoordinate = Mathf.Clamp(attachedCollider.transform.position.x, minCoordinate, maxCoordinate);
                attachedCollider.transform.position = new Vector3(clampedCoordinate, attachedCollider.transform.position.y, attachedCollider.transform.position.z);
                break;
            case axis.y:
                minCoordinate = this.transform.position.y - extent;
                maxCoordinate = this.transform.position.y + extent;

                clampedCoordinate = Mathf.Clamp(attachedCollider.transform.position.y, minCoordinate, maxCoordinate);
                attachedCollider.transform.position = new Vector3(attachedCollider.transform.position.x, clampedCoordinate, attachedCollider.transform.position.z);
                break;
            case axis.z:
                minCoordinate = this.transform.position.z - extent;
                maxCoordinate = this.transform.position.z + extent;

                clampedCoordinate = Mathf.Clamp(attachedCollider.transform.position.z, minCoordinate, maxCoordinate);
                attachedCollider.transform.position = new Vector3(attachedCollider.transform.position.x, attachedCollider.transform.position.y, clampedCoordinate);
                break;
        }
    }
    #endregion
}
