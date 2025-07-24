using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningObject : MonoBehaviour
{
    #region PARAMETERS
    [Header("Sonido de la alarma")]
    [SerializeField] protected AudioSource WarningSource;

    [Header("Visuales de la alarma")]
    [SerializeField] protected GameObject WarningVisuals;
    #endregion

    #region METHODS

    private void Awake()
    {
        if(!WarningSource) WarningSource = GetComponent<AudioSource>();

        WarningSource.playOnAwake = false;
        WarningSource.loop = true;
        WarningSource.Stop();
    }

    public void EnableWarning()
    {
        if(this.WarningSource) WarningSource.Play();
        if(this.WarningVisuals) WarningVisuals.SetActive(true);
    }

    public void DisableWarning()
    {
        if (this.WarningSource) WarningSource.Stop();
        if (this.WarningVisuals) WarningVisuals.SetActive(false);
    }

    #endregion
}
