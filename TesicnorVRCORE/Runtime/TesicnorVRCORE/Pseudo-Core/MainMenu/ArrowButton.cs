using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ArrowButton : VR_Interactable
{
    #region PARAMETERS
    [Header("Es la flecha de la izquierda?")]
    [SerializeField] private bool isLeftArrow = false;
    #endregion

    #region FUNCTIONS
    public void MoveButton()
    {
       if(!MainMenuRCP.Instance.isMoving()) MainMenuRCP.Instance.ChangePanel(isLeftArrow);
    }
    #endregion
}
