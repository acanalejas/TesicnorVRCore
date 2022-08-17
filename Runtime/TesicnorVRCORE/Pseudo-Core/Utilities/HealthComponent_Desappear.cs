using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthComponent_Desappear : HealthComponent
{
    #region PARAMETERS
    [Header("================ SPECIFIC ATRIBUTES =================")]
    [Space(10)]
    [Header("OPCIONAL : El GameObject que queremos que desaparezca")]
    [SerializeField] private GameObject desappear;
    #endregion

    #region FUNCTIONS
    public override void Death()
    {
        base.Death();

        if (desappear) desappear.SetActive(false);
    }

    public override void Recover(float amount)
    {
        base.Recover(amount);
        if (desappear) desappear.SetActive(true);
    }
    #endregion
}
