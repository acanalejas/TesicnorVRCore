using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffsetManager : MonoBehaviour
{
    #region SINGLETON
    static OffsetManager instance;
    public static OffsetManager Instance { get { return instance; } }

    void CheckSingleton()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }
    #endregion
    #region PARAMETERS
    public HandsOffset currentHandsOffset;
    #endregion

    #region FUNCTIONS
    private void Awake()
    {
        CheckSingleton();
    }
    #endregion
}
