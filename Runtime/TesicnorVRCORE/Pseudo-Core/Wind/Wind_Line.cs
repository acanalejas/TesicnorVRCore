using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wind_Line : VRCollider
{
    #region PARAMETERS
    [Header("La tag correcta del punto de anclaje")]
    [SerializeField] string CorrectAnchorageTag = "AnchorageBlue";

    [Header("El GameObject de warning")]
    [SerializeField] GameObject WarningGO;
    #endregion

    #region METHODS

    private void Start()
    {
        onTargetReached.AddListener(CheckAnchorage);
    }
    private void CheckAnchorage(GameObject anchorage)
    {
        if(anchorage.tag == CorrectAnchorageTag)
        {
            WarningGO.SetActive(false);
        }
        else
        {
            WarningGO.SetActive(true);
        }
    }
    #endregion
}
