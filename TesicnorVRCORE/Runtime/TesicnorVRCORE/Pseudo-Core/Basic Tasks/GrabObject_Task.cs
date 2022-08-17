using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabObject_Task : VR_Task
{
    #region PARAMETERS
    /// <summary>
    /// El componente VRCollider del objeto
    /// </summary>
    [Header("El componente VRCollider del objeto")]
    [SerializeField] private VRCollider collider;

    /// <summary>
    /// Se prohibe cogerlo hasta esta tarea?
    /// </summary>
    [Header("Se prohibe cogerlo hasta esta tarea?")]
    [SerializeField] private bool disable_init = false;

    #endregion

    #region FUNCTIONS
    private void Awake()
    {
        if (disable_init) collider.SetGrabbable(false);
    }
    public override void OnEnable()
    {
        base.OnEnable();

        if (disable_init) collider.SetGrabbable(true);
    }
    public override void myUpdate()
    {
        base.myUpdate();

        if (isCompleted()) CompleteTask();
    }

    bool isCompleted()
    {
        return collider.isGrabbed();
    }
    #endregion
}
