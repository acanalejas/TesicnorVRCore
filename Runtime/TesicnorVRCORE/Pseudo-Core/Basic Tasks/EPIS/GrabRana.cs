using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabRana : VR_Task
{
    #region PARAMETERS
    [Header("El VRCollider de la rana")]
    public VRCollider rana_collider;
    #endregion

    #region FUNCTIONS
    private void Awake()
    {
        rana_collider.SetGrabbable(false);
        rana_collider.target.canReleaseObject = false;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        rana_collider.SetGrabbable(true);
    }
    public override void myUpdate()
    {
        base.myUpdate();

        if (isCompleted()) CompleteTask();
    }

    private bool isCompleted()
    {
        if (rana_collider.isGrabbed()) return true;

        return false;
    }
    #endregion
}
