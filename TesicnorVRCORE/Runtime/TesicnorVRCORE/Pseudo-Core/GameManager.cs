using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region SINGLETON
    public static GameManager Instance { get { return instance; } }
    private static GameManager instance;

    private void CheckSingleton()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }
    #endregion
    #region PARAMETERS
    /// <summary>
    /// SO del player que se quiera usar
    /// </summary>
    [Header("SO del player que se quiera usar")]
    public SO_PlayerData playerData;

    [Header("Canvas del final")]
    [SerializeField] GameObject endCanvas;
    #endregion

    #region FUNCTIONS
    private void Awake()
    {
        CheckSingleton();
    }

    public void BadEnd()
    {

    }

    public void GoodEnd()
    {

    }
    #endregion
}
