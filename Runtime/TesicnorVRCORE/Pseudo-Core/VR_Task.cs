using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VR_Task : MonoBehaviour
{
    #region PARAMETERS

    /// <summary>
    /// Si la tarea ha sido completada
    /// </summary>
    [HideInInspector]
    public bool taskCompleted = false;

    /// <summary>
    /// Si la tarea tiene cuenta atrás
    /// </summary>
    [Header("============COMMON ATRIBUTES=============")][Space(10)]
    [Header("Si la tarea tiene cuenta atrás")]
    [SerializeField] private bool hasCountdown = false;

    /// <summary>
    /// En caso de tener cuenta atrás, el tiempo a esperar
    /// </summary>
    [Header("En caso de tener cuenta atrás, \n el tiempo a esperar")]
    [SerializeField] private float timeToWait = 10;

    /// <summary>
    /// Accion llamada cuando se acaba el tiempo si hay cuenta atras
    /// </summary>
    public static Action onCountdownFinished;

    /// <summary>
    /// Lista de objetos que tienen que estar activos durante la tarea
    /// </summary>
    [Header("Lista de objetos que tienen que estar activos durante la tarea")]
    [SerializeField] private GameObject[] neededObjects;

    /// <summary>
    /// Lista de componentes que necesitemos activos cuando esté la tarea,
    /// pero no cuando este desactivada
    /// </summary>
    [Header("Lista de componentes, solo en caso de necesitarlos \n sin querer modificar el GO al que pertenecen")]
    [SerializeField] private MonoBehaviour[] neededComponents;

    private List<Vector3> neededObjectsPositions = new List<Vector3>();

    #region Data Received
    /// <summary>
    /// Tiempo que tarda en completarse la tarea
    /// </summary>
    private float timeToComplete;
    #endregion

    #endregion
    #region FUNCTIONS

    /// <summary>
    /// Método usado cuando se completa la tarea
    /// </summary>
    protected void CompleteTask()
    {
        taskCompleted = true;
        TaskManager.Instance.CheckAllTasksCompleted();
        if(TaskManager.Instance.activeTask < TaskManager.Instance.totalTasks.Length - 1)
            TaskManager.Instance.GoToNextTask();

        if(DebugVR.Instance) DebugVR.Instance.Log("Completed a task with name  :  " + this.name);
    }

    /// <summary>
    /// Método para cuando se falla la tarea
    /// </summary>
    protected void FailTask()
    {
        if(TaskManager.Instance)
            if(TaskManager.Instance.activeTask > 0)
                TaskManager.Instance.GoToLastTask();
    }
    /// <summary>
    /// Setea o desactiva los objetos propios de la tarea.
    /// </summary>
    /// <param name="enabled">Si se activa o se desactiva </param>
    protected void SetObjectsForThisTask(bool enabled)
    {
        int i = 0;
        foreach(GameObject go in neededObjects) {if(go)go.SetActive(enabled);
            if (enabled && go) neededObjectsPositions.Add(go.transform.position);
            else if(i < neededObjectsPositions.Count && go) go.transform.position = neededObjectsPositions[i];
            i++;
        }

        foreach(MonoBehaviour mono in neededComponents) {mono.enabled = enabled;  }
    }
    // ==================== PARA ACTIVACION O DESACTIVACION DEL SCRIPT ==========================
    public virtual void OnEnable()
    {
        //Normalmente seria redundante cuando el valor default es false, pero por si acaso se reactiva la tarea, nos da mayor seguridad
        taskCompleted = false;
        StartCoroutine("timer");
        if (hasCountdown) StartCoroutine("countdown");

        StartCoroutine("update");
        /*if(SceneChanger.Instance.isGuided)*/SetObjectsForThisTask(true);
    }

    public virtual void OnDisable()
    {
        StopAllCoroutines();
        SetObjectsForThisTask(false);
    }

    /// <summary>
    /// Método que se ejecuta en el "update"
    /// </summary>
    public virtual void myUpdate()
    {
    }
    //==========================================================================================


    //============================== COROUTINES ================================================
    //Declarado fuera por optimizacion
    protected WaitForEndOfFrame frame = new WaitForEndOfFrame();
    /// <summary>
    /// Coroutine que se encarga de medir el tiempo que tarda en completarse la tarea
    /// </summary>
    /// <returns></returns>
    private IEnumerator timer()
    {
        while (enabled)
        {
            timeToComplete += Time.deltaTime;
            yield return frame;
        }
    }

    /// <summary>
    /// Coroutine que en caso de tener cuenta atrás, se encarga de realizarla
    /// </summary>
    /// <returns></returns>
    public virtual IEnumerator countdown()
    {
        float _countdown = timeToWait;
        while (_countdown <= 0)
        {
            _countdown -= Time.deltaTime;
            yield return frame;
        }
        onCountdownFinished?.Invoke();
    }

    private IEnumerator update()
    {
        while (enabled)
        {
            myUpdate();
            yield return frame;
        }
    }
    //==========================================================================================
    #endregion
}
