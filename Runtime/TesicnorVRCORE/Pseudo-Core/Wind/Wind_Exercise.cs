using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Wind_Exercise : MonoBehaviour
{
    #region SINGLETON
    private static Wind_Exercise instance;
    public static Wind_Exercise Instance { get { return instance; } }

    private void CheckSingleton()
    {
        if (!instance) instance = this;
        else Destroy(this);
    }
    #endregion
    #region PARAMETERS
    #region Events

    [Header("Evento lanzado al empezar el ejercicio")]
    public UnityEvent OnExerciseBegin;

    [Header("Evento lanzado cuando se llega al final del ejercicio")]
    public UnityEvent OnExerciseEnd;

    [Header("Evento que se lanza en caso de necesitar una emergencia")]
    public UnityEvent OnExerciseEmergency;

    #endregion
    #endregion

    #region METHODS

    protected virtual void Awake()
    {
        CheckSingleton();
    }

    public virtual void BeginExercise()
    {
        OnExerciseBegin.Invoke();
    }

    public virtual void ExerciseEmergency()
    {
        OnExerciseEmergency.Invoke();
    }

    public virtual void EndExercise()
    {
        OnExerciseEnd.Invoke();
    }

    #endregion
}
