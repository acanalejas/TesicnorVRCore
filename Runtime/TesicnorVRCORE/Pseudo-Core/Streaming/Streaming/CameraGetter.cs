using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraGetter : MonoBehaviour
{
    #region PARAMETERS
    public Camera playerCamera;

    private static CameraGetter instance;
    public static CameraGetter Instance { get { return instance; } }
    #endregion

    #region FUNCTIONS
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }
    #endregion
}
