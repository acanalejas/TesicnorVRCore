using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabArnes : VR_Task
{
    #region PARAMETERS
    [Header("El componente VRCollider del arnes")]
    public VRCollider arnesCollider;
    #endregion

    #region FUNCTIONS
    private void Awake()
    {
        arnesCollider.target.canReleaseObject = false;
    }
    public override void myUpdate()
    {
        base.myUpdate();

        if (isCompleted()) CompleteTask();
    }

    bool isCompleted()
    {
        return arnesCollider.isGrabbed();
    }
    #endregion
}
