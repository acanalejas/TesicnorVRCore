using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReleaseHelmet : VR_Task
{
    #region PARAMETERS
    [Header("El componente VRCollider del casco")]
    public VRCollider helmetCollider;

    [Header("El VRColliderReleaseTarget donde debemos llevar el casco")]
    public VRColliderReleaseTarget helmetColliderTarget;
    #endregion

    #region FUNCTIONS
    private void Awake()
    {
        helmetCollider.target.canReleaseObject = false;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        helmetCollider.target.canReleaseObject = true;
    }
    public override void myUpdate()
    {
        base.myUpdate();
        if (isCompleted()) CompleteTask();
        if (isFailed()) FailTask();
    }

    bool isCompleted()
    {
        bool result = !helmetCollider.isGrabbed() && helmetColliderTarget.conditionCompleted;

        return result;
    }

    bool isFailed()
    {
        bool result = !helmetCollider.isGrabbed() && !helmetColliderTarget.conditionCompleted;

        return result;
    }
    #endregion
}
