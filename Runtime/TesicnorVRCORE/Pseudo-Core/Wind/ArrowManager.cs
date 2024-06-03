using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowManager : MonoBehaviour
{
    #region PARAMETERS
    [Header("La flecha que se tiene que desactivar")]
    public GameObject ArrowToDisable;

    [Header("La flecha que se tiene que activar (Opciona)")]
    public GameObject ArrowToEnable;
    #endregion

    #region METHODS

    public void ChangeIndicator()
    {
        if(ArrowToDisable) ArrowToDisable.SetActive(false);
        if(ArrowToEnable) ArrowToEnable.SetActive(true);
    }

    #endregion
}
