using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class MenuButton : VR_Interactable
{
    #region PARAMETERS
    [Header("Datos de la escena que queremos cargar")]
    [SerializeField] SceneData sceneData;
    #endregion

    #region FUNCTIONS
    /// <summary>
    /// Funcion para asignar al propio boton
    /// Cambia la escena a la deseada
    /// </summary>
    /// 
    public override void Awake()
    {
        base.Awake();
        onRelease.AddListener(PressedTheButton);
    }
    public void PressedTheButton()
    {
        SceneChanger.Instance.ChangeScene(sceneData);
    }

    public void PressedTheButton_Guided()
    {
        SceneChanger.Instance.SetScene(sceneData);
        MainMenuRCP.Instance.ChangeToGuidePanel();
    }

    #region Guide Canvas
    public void SetIsGuided(bool _value)
    {
        SceneChanger.Instance.SetIsGuided(_value);
        SceneChanger.Instance.ChangeScene();
    }
    #endregion
    #endregion
}
