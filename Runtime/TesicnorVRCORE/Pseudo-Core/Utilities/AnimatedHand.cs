using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;

/// <summary>
/// Clase dedicada a cuando se usa una mano animada en vez del controlador
/// </summary>
public class AnimatedHand : MonoBehaviour
{
    #region PARAMETERS
    /// <summary>
    /// El componente Animator de la mano
    /// </summary>
    [Header("El componente Animator de la mano")]
    [SerializeField] private Animator animator;

    /// <summary>
    /// El nombre del parámetro del trigger
    /// </summary>
    [Header("El nombre del parámetro del trigger")]
    [SerializeField] private string triggerName = "trigger";

    /// <summary>
    /// El nombre del parámetro del grip
    /// </summary>
    [Header("El nombre del parámetro del grip")]
    [SerializeField] private string gripName = "grip";

    /// <summary>
    /// El componente XRController de la mano que queremos animar
    /// </summary>
    protected XRController xrController;
    #endregion

    #region FUNCTIONS
    private void Awake()
    {
        xrController = GetComponent<XRController>();

        if (!xrController)
        {
            xrController = GetComponentInParent<XRController>();
            if (!xrController)
            {
                xrController = GetComponentInChildren<XRController>();
                if (!xrController) xrController = gameObject.AddComponent<XRController>();
            }
        }
    }

    private void Update()
    {
        CheckTrigger();
        CheckGrip();
    }

    /// <summary>
    /// Checkea el nivel de pulsación del trigger y se lo manda al animator
    /// </summary>
    private void CheckTrigger()
    {
        float trigger = 0;
        xrController.inputDevice.TryGetFeatureValue(CommonUsages.trigger, out trigger);

        animator.SetFloat(triggerName, trigger);
    }

    /// <summary>
    /// Checkea el nivel de pulsación del grip y se lo manda al animator
    /// </summary>
    private void CheckGrip()
    {
        float grip = 0;
        xrController.inputDevice.TryGetFeatureValue(CommonUsages.grip, out grip);

        animator.SetFloat(gripName, grip);
    }
    #endregion
}
