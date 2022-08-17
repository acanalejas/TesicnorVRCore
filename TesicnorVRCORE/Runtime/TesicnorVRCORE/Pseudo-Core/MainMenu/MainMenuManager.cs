using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    #region PARAMETERS
    [Header("El primer menú que nos aparece")]
    public GameObject initialMenu;

    [Header("El canvas que nos pregunta si queremos el modo guiado o el libre")]
    public GameObject guidedCanvas;

    #endregion

    #region FUNCTIONS

    protected void ChangeCanvas(GameObject currentCanvas, GameObject nextCanvas)
    {
        currentCanvas.SetActive(false);
        nextCanvas.SetActive(true);
    }

    #endregion
}
