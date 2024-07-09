using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Clase que se usa para las utilidades generales y bindeos del player
/// </summary>
public class TesicnorPlayer : MonoBehaviour
{
    #region PARAMETERS
    #region Bools
    [Header("Se va a usar la pausa?")]
    public bool bUsePause = true;

    [HideInInspector] public bool bIsInPause = false;
    #endregion
    [Header("El PREFAB de la pantalla de pausa")]
    public GameObject PauseScreenPrefab;

    [Header("El objeto instanciado de la pantalla de pausa")]
    [SerializeField] private GameObject PauseScreen;

    [Header("El objeto al que se une la pantalla de pausa")]
    [SerializeField] private Transform PauseParent;

    [Header("Se activa solo el rayo de la mano izquierda al pausar?")]
    [SerializeField] private bool OnlyLeftRay = true;

    [Header("Evento que se usa al activar la pantalla de Pausa")]
    public UnityEvent OnPause;

    [Header("Evento que se usa al reanudar la experiencia")]
    public UnityEvent OnResume;

    public bool bShouldSearch = true;
    
    #endregion

    #region METHODS
    private void Awake()
    {
        CheckSingleton();

        if (!bShouldSearch) return;
        //Activate Ray
        OnPause.AddListener(() =>
        {
            HandInteraction[] interactors = FindObjectsOfType<HandInteraction>();

            foreach (var interactor in interactors)
            {
                if (!interactor.isLeftHand && OnlyLeftRay) continue;

                interactor.ToggleRay(true);
            }
        });

        //Deactivate Ray
        OnResume.AddListener(() =>
        {
            HandInteraction[] interactors = FindObjectsOfType<HandInteraction>();

            foreach (var interactor in interactors)
            {
                interactor.ToggleRay(false);
            }
        });
    }

    private void Update()
    {
        CheckInput();
    }

    private void CheckInput()
    {
        #region For Pause
        if (bUsePause)
        {
            if (OVRInput.GetDown(OVRInput.Button.Start))
            {
                CheckPauseScreen();

                TogglePause(!PauseScreen.activeSelf);
            }
        }
        #endregion
    }

    #region For Pause
    private void CheckPauseScreen()
    {
        if (PauseScreen != null) return;

        PauseScreen = GameObject.Instantiate(PauseScreenPrefab, PauseParent);
    }

    public void TogglePause(bool Value)
    {
        this.PauseScreen.SetActive(Value);
        if (Value) OnPause.Invoke();
        else OnResume.Invoke();

        if (Value) PlayerPauseManager.Instance.Pause();
        else PlayerPauseManager.Instance.Continue();

        bIsInPause = Value;
    }
    #endregion
    #endregion

    #region SINGLETON
    private static TesicnorPlayer instance;
    public static TesicnorPlayer Instance { get { return instance; } }

    void CheckSingleton()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }
    #endregion
}
