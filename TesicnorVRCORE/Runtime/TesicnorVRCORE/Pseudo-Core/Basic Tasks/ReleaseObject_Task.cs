using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReleaseObject_Task : VR_Task
{
    #region PARAMETERS
    /// <summary>
    /// El componente VRCollider del objeto
    /// </summary>
    [Header("El componente VRCollider del objeto")]
    [SerializeField] private VRCollider collider;

    /// <summary>
    /// Se prohibe cogerlo al terminar la tarea?
    /// </summary>
    [Header("Se prohibe cogerlo al terminar la tarea?")]
    [SerializeField] private bool disable_final = false;
    #endregion

    #region FUNCTIONS
    public override void myUpdate()
    {
        base.myUpdate();

        if (isCompleted()) CompleteTask();
        if (isFailed()) FailTask();
    }
    public override void OnDisable()
    {
        base.OnDisable();
        if (disable_final) collider.SetGrabbable(false);
    }

    private bool isCompleted()
    {
        return !collider.isGrabbed() && collider.target.conditionCompleted;
    }

    private bool isFailed()
    {
        return !collider.isGrabbed() && !collider.target.conditionCompleted;
    }
    #endregion
}
