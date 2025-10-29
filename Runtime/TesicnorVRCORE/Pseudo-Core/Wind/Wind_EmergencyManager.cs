using System;
using System.Collections.Generic;
using UnityEngine;

public class Wind_EmergencyManager : MonoBehaviour
{
    #region PARAMETERS
    [Header("Los botones de emergencia en la escena")][SerializeField]
    private List<Wind_EmergencyButton> EmergencyButtons;

    #endregion

    #region METHODS

    private void Start()
    {
        //Simplemente por comprobar que tenemos todos los botones localizados
        Wind_EmergencyButton[] _buttons = FindObjectsByType<Wind_EmergencyButton>(FindObjectsSortMode.None);

        foreach (var _button in _buttons)
        {
            if(!EmergencyButtons.Contains(_button)) EmergencyButtons.Add(_button);
        }
    }

    public void CheckEmergency()
    {
        foreach (var _button in EmergencyButtons)
        {
            if (_button.isActive)
            {
                if (Wind_Elevator.Instance.Emergency) return;
                else{Wind_Elevator.Instance.SetEmergency(true);
                    return;
                }
            }
        }

        if (!Wind_Elevator.Instance.Emergency) return;
        else Wind_Elevator.Instance.SetEmergency(false);
    }

    #endregion

    #region SINGLETON

    private static Wind_EmergencyManager instance;
    public static Wind_EmergencyManager Instance
    {
        get { return instance; }
    }

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    #endregion
}
