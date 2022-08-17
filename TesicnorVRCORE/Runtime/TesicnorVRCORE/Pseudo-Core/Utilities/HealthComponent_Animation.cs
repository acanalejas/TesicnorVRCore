using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthComponent_Animation : HealthComponent
{
    #region PARAMETERS
    [Header("================ SPECIFIC ATRIBUTES =================")]
    [Space(10)]
    [Header("El componente animator que nos interesa")]
    [SerializeField] private Animator animator;

    [Header("El nombre del parámetro tipo trigger que queremos lanzar al morir")]
    [SerializeField] private string deadParameter = "Death";

    [Header("El nombre del parámetro tipo trigger que queremos lanzar al revivir")]
    [SerializeField] private string recoverParameter = "Recover";
    #endregion

    #region FUNCTIONS
    public override void Death()
    {
        base.Death();
        animator.SetTrigger(deadParameter);
    }

    public override void Recover(float amount)
    {
        base.Recover(amount);
        animator.SetTrigger(recoverParameter);
    }
    #endregion
}
