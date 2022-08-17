using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecoverRana : VR_Task
{
    #region PARAMETERS
    [Header("El collider de la rana")]
    [SerializeField] private VRCollider rana_collider;

    [Header("El target de la rana")]
    [SerializeField] private VRColliderReleaseTarget rana_target;
    #endregion

    #region FUNCTIONS
    public override void OnEnable()
    {
        base.OnEnable();
        rana_collider.SetGrabbable(true);
        rana_target.canBeCanceled = true;
    }

    public override void myUpdate()
    {
        base.myUpdate();

        if (isCompleted()) { CompleteTask(); rana_collider.SetGrabbable(false); rana_target.GetComponent<BoxCollider>().enabled = false; }
    }

    private bool isCompleted()
    {
        if (!rana_collider.isGrabbed() && !rana_target.conditionCompleted) return true;

        return false;
    }
    #endregion
}
