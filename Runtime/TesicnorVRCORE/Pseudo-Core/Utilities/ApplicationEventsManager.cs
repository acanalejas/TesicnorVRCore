using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Clase usada para tener eventos en cada punto importante de la ejecuci�n de la aplicaci�n
/// </summary>

public class ApplicationEventsManager : MonoBehaviour
{
    #region PARAMETERS

    [Header("Evento usado cuando se cierra la aplicaci�n")]
    public UnityEvent onApplicationQuit;

    [Header("Evento usado cuando se pausa o reanuda la aplicaci�n")]
    public UnityEvent<bool> onApplicationPause;

    [Header("Evento usado cuando se vuelve o se deja en segundo plano a la aplicaci�n")]
    public UnityEvent<bool> onApplicationFocus;

    [Header("Evento usado cuando se cambia la escena activa")]
    public UnityEvent onActiveSceneChange;

    [Header("Evento usado cuando se carga una escena")]
    public UnityEvent onSceneLoaded;

    #endregion

    #region METHODS
    private void Awake()
    {
        DontDestroyOnLoad(this);
        SceneManager.activeSceneChanged += OnSceneChanged;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnApplicationQuit()
    {
        onApplicationQuit.Invoke();
    }

    private void OnApplicationFocus(bool focus)
    {
        onApplicationFocus.Invoke(focus);
    }

    private void OnApplicationPause(bool pause)
    {
        onApplicationPause.Invoke(pause);
    }

    private void OnSceneChanged(Scene firstScene, Scene secondScene)
    {
        onActiveSceneChange.Invoke();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        onSceneLoaded.Invoke();
    }
    #endregion

    #region SINGLETON
    private static ApplicationEventsManager instance;
    public static ApplicationEventsManager Instance { get { return instance; } }

    private void CheckSingleton()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }
    #endregion
}
