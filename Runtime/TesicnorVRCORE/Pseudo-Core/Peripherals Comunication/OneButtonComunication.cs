using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class OneButtonComunication : MonoBehaviour
{
    #region SINGLETON
    private static OneButtonComunication instance;
    public static OneButtonComunication Instance { get { return instance; } }

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }
    #endregion

    #region PARAMETERS
    public Text debugText;
    public bool pressed = false;
    public UnityEvent onPressed;
    public UnityEvent onReleased;
    #endregion

    #region FUNCTIONS
    private void Start()
    {
        SerialManager.WhenReceiveDataCall += ReceiveData;
    }
    public void  ReceiveData(string incomingData)
    {
        int result;
        Debug.Log(incomingData);
        int.TryParse(incomingData, out result);

        if (result == 0)
        {
            if (pressed)
            {
                onReleased.Invoke();
            }
            pressed = false;
        }

        else
        {
            if (pressed)
            {
                onPressed.Invoke();
            }
            pressed = true;
        }
    }
    public void Update()
    {
        debugText.text = pressed.ToString();
    }
    #endregion
}
