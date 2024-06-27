using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PlayerPauseManager : MonoBehaviour
{
    #region PARAMETERS
    [Header("El Canvas que contiene la pantalla de pausa")]
    [SerializeField] private GameObject PauseCanvas;

    [Header("El indice de compilacion de la escena de menu")]
    public int MenuBuildIndex = 0;

    [Header("Se ha pausado la experiencia?")]
    public bool bIsPaused = false;

    [Header("El evento que se usa para la pausa")]
    public UnityEvent<bool> OnTogglePause;

    [Header("El evento que se llama al pausar")]
    public UnityEvent OnPause;

    [Header("El evento que se usa para cuando se continua la experiencia")]
    public UnityEvent OnContinue;

    [Header("El evento que se usa al reiniciar la escena")]
    public UnityEvent OnRestart;

    [Header("El evento que se usa al volver al menu")]
    public UnityEvent OnReturnToMenu;
    #endregion

    #region METHODS
    public void TogglePause(bool Value)
    {
        bIsPaused = Value;
        if (PauseCanvas)
        {
            PauseCanvas.SetActive(Value);
        }

        OnTogglePause.Invoke(Value);
    }

    public void Pause()
    {
        TogglePause(true);
        OnPause.Invoke();

        BackendTimeManager timeManager = FindObjectOfType<BackendTimeManager>();
        if (timeManager) timeManager.PauseTime();
    }

    public void Continue()
    {
        TogglePause(false);
        OnContinue.Invoke();

        BackendTimeManager timeManager = FindObjectOfType<BackendTimeManager>();
        if(timeManager) timeManager.ResumeTime();
    }
    public void Restart()
    {
        OnRestart.Invoke();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void ReturnToMenu()
    {
        OnReturnToMenu.Invoke();
        SceneManager.LoadScene(MenuBuildIndex);
    }

    #endregion

    #region SINGLETON
    private static PlayerPauseManager instance;

    public static PlayerPauseManager Instance { get { return instance; } }

    private void Awake()
    {
        if (!instance) instance = this;
        else Destroy(this);
    }
    #endregion
}
