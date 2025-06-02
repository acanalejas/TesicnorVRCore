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

    UnityEngine.XR.Interaction.Toolkit.XRController controller;
    
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

#if UNITY_2023
        var controllers = FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.XRController>(FindObjectsSortMode.None);
#endif
#if UNITY_2021
        var controllers = FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.XRController>();
#endif
        foreach(var _c in controllers)
        {
            if (_c.controllerNode == UnityEngine.XR.XRNode.LeftHand) controller = _c;
        }
    }

    private void Update()
    {
        CheckInput();
    }

    bool pausePressed = false;
    bool pause = false;
    private void CheckInput()
    {
#region For Pause
        if (bUsePause)
        {
            if (controller && controller.inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.menuButton, out pause) && pause)
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

    IEnumerator PauseCountdown()
    {
        yield return new WaitForSeconds(1);
        pausePressed = false;
    }

    public void TogglePause(bool Value)
    {
        if (pausePressed) return;
        pausePressed = true;

        StartCoroutine(PauseCountdown());

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
