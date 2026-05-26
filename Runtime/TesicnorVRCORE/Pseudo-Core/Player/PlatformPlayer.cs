
using UnityEngine;

public class PlatformPlayer : MonoBehaviour
{
#if UNITY_EDITOR
    #region PARAMETERS

    [Header("Para que plataforma se usa este player?")]
    public PlatformType Platform;

    #endregion
    #endif
}