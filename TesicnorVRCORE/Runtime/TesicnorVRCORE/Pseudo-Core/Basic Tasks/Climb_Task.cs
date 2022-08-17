using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Climb_Task : VR_Task
{
    #region PARAMETERS
    /// <summary>
    /// El componente ClimbableCollider del objeto
    /// </summary>
    [Header("El componente ClimbableCollider del objeto")]
    [SerializeField] private ClimbableCollider collider;

    /// <summary>
    /// Hay que subir?
    /// </summary>
    [Header("Hay que subir?")]
    [SerializeField] private bool goingToTop = true;

    /// <summary>
    /// Se prohine agarrarlo antes de la tarea?
    /// </summary>
    [Header("Se prohibe agarrarlo antes de la tarea?")]
    [SerializeField] private bool disable_init = false;

    /// <summary>
    /// Se prohibe usarlo al terminar la tarea?
    /// </summary>
    [Header("Se prohibe usarlo al terminar la tarea?")]
    [SerializeField] private bool disable_final = false;
    #endregion

    #region FUNCTIONS
    private void Awake()
    {
        if (disable_init) collider.SetGrabbable(false);
    }
    public override void OnEnable()
    {
        base.OnEnable();
        collider.SetGrabbable(true);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (disable_final) collider.SetGrabbable(false);
    }

    public override void myUpdate()
    {
        base.myUpdate();

        if (isCompleted()) CompleteTask();
    }

    private bool isCompleted()
    {
        bool result = false;

        if (goingToTop) result = collider.isPlayerAtTop;
        else result = collider.isPlayerAtBottom;

        return result;
    }
    #endregion
}
