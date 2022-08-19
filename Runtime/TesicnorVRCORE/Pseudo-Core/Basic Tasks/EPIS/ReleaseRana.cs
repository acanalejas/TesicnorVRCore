using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReleaseRana : VR_Task
{
    #region PARAMETERS
    [Header("El collider de la rana")]
    public VRCollider rana_collider;

    [Header("El target de la rana")]
    public VRColliderReleaseTarget rana_target;
    #endregion

    #region FUNCTIONS

    public override void OnEnable()
    {
        base.OnEnable();

        rana_collider.target.canReleaseObject = true;
    }

    public override void myUpdate()
    {
        base.myUpdate();

        if (isCompleted())
        {
            rana_collider.SetGrabbable(false);
            CompleteTask();
        }

        if (isFailed()) FailTask();
    }

    private bool isCompleted()
    {
        if (!rana_collider.isGrabbed() && rana_target.conditionCompleted) return true;

        return false;
    }

    private bool isFailed()
    {
        if (!rana_collider.isGrabbed() && !rana_target.conditionCompleted) return true;

        return false;
    }
    #endregion
}
