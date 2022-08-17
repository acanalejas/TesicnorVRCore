using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReleaseArnes : VR_Task 
{ 
    #region PARAMETERS
    [Header("El componente VRCollider del arnes")]
    [SerializeField] VRCollider arnesCollider;

    [Header("El VRColliderReleaseTarget al que debemos llevar el arnes")]
    [SerializeField] VRColliderReleaseTarget arnesColliderTarget;
    #endregion

    #region FUNCTIONS

    public override void OnEnable()
    {
        base.OnEnable();
        arnesColliderTarget.canReleaseObject = true;
    }
    public override void myUpdate()
    {
        base.myUpdate();

        if (isCompleted()) CompleteTask();
        if (isFailed()) FailTask();
    }

    bool isCompleted()
    {
        return !arnesCollider.isGrabbed() && arnesColliderTarget.conditionCompleted;
    }
    bool isFailed()
    {
        return !arnesCollider.isGrabbed() && !arnesColliderTarget.conditionCompleted;
    }
    #endregion
}
