using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PressAButton_Task : VR_Task
{
    #region PARAMETERS
    /// <summary>
    /// El componente VR_Interactable del bot�n
    /// </summary>
    [Header("El componente VR_Interactable del boton")]
    [SerializeField] private VR_Interactable interactable;

    /// <summary>
    /// Se prohibe pulsar el bot�n hasta esta tarea?
    /// </summary>
    [Header("Se prohibe pulsar el bot�n hasta esta tarea?")]
    [SerializeField] private bool disable_init = false;

    /// <summary>
    /// Se prohibe pulsar el bot�n al terminar la tarea?
    /// </summary>
    [Header("Se prohibe pulsar el bot�n al terminar la tarea?")]
    [SerializeField] private bool disable_final = false;

    /// <summary>
    /// El tiempo despu�s de pulsar en el que se completa la tarea
    /// </summary>
    [Header("El tiempo despu�s de pulsar en el que se completa la tarea")]
    [SerializeField] private float completeTime = 1;

    /// <summary>
    /// Evento que se lanza cuando se completa la tarea
    /// </summary>
    [Header("Evento que se lanza cuando se completa la tarea")]
    [SerializeField] private UnityEvent onComplete;
    #endregion

    #region FUNCTIONS

    private void Awake()
    {
        if (disable_init) interactable.SetCanBePressed(false);
    }
    public override void myUpdate()
    {
        base.myUpdate();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        interactable.SetCanBePressed(true);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (disable_final) interactable.SetCanBePressed(false);
    }

    /// <summary>
    /// Funci�n para a�adir al evento Release del bot�n
    /// </summary>
    public void OnPress()
    {
        StartCoroutine("complete");
    }

    /// <summary>
    /// Coroutine que controla cuanto tarda en completarse la tarea
    /// </summary>
    /// <returns></returns>
    IEnumerator complete()
    {
        yield return new WaitForSeconds(completeTime);
        onComplete.Invoke();
        CompleteTask();
    }
    #endregion
}
