using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class MainMenuRCP : MainMenuManager
{
    #region SINGLETON

    private static MainMenuRCP instance;
    public static MainMenuRCP Instance { get { return instance; } }

    private void CheckSingleton()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    #endregion
    #region PARAMETERS
    [Header("Todos los paneles donde se muestran los escenarios")]
    public GameObject[] escenarios;

    private List<Panel> escenariosPanels = new List<Panel>();

    [Header("Posicion a la derecha de la mascara")]
    public Transform rightPosition;

    [Header("Posicion a la izquierda de la mascara")]
    public Transform leftPosition;

    [Header("Posicion en el centro de la mascara")]
    public Transform centerPosition;

    [HideInInspector]public int currentPanel;
    #endregion

    #region FUNCTIONS

    private void Awake()
    {
        CheckSingleton();
        SetInitialPanel();
    }

    private void SetInitialPanel()
    {
        foreach (var escenario in escenarios) 
        {
            escenariosPanels.Add(escenario.AddComponent<Panel>());
            escenario.transform.position = rightPosition.position;
        }

        currentPanel = 0;
        escenarios[currentPanel].transform.position = centerPosition.position;
    }
    public void ChangePanel(bool isLeft)
    {
        if (isLeft && currentPanel > 0)
        {
            escenariosPanels[currentPanel].SetTarget(rightPosition);
            currentPanel--;
            escenariosPanels[currentPanel].transform.position = leftPosition.position;
            escenariosPanels[currentPanel].SetTarget(centerPosition);
        }
        else if(currentPanel < escenarios.Length - 1 && !isLeft)
        {
            escenariosPanels[currentPanel].SetTarget(leftPosition);
            currentPanel++;
            escenariosPanels[currentPanel].transform.position = rightPosition.position;
            escenariosPanels[currentPanel].SetTarget(centerPosition);
        }
    }
    
    public bool isMoving()
    {
        if (!escenariosPanels[currentPanel].isMoving()) return false;

        return true;
    }

    public void ChangeToGuidePanel()
    {
        ChangeCanvas(initialMenu, guidedCanvas);
    }
    #endregion
}
