using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wind_MoveButton : VRInteractable_Button
{
    #region PARAMETERS
    [Header("La direccion en la que se movera el elevador")]
    public Direction direction = Direction.Up;

    [Header("Afecta el hecho de estar dentro o fuera?")]
    [SerializeField] private bool bAfecta = true;

    [Header("Esta dentro del elevador?")]
    [SerializeField] private bool bInsideElevator;

    [Header("Este botón necesita de hombre muerto?")] [SerializeField]
    private bool bUsesDeadMan = true;
    #endregion

    #region METHODS
    protected virtual void Start()
    {
        this.usesRay = false;
        this.changesColor = false;
        this.usesCollision = true;
        this.is3DObject = true;
    }
    public override void OnClick()
    {
        base.OnClick();
        if (!CanBeClicked()) return;
        if (!(this.bInsideElevator && Wind_Elevator.Instance.CanBeMovedWithSecondaryElectricity())) return;
        Wind_Elevator.Instance.DeadManButtonPressed = true;
        Wind_Elevator.Instance.MoveElevator(direction);
    }

    public override void OnRelease()
    {
        base.OnRelease();
        if(bUsesDeadMan) Wind_Elevator.Instance.DeadManButtonPressed = false;
        if(Wind_Elevator.Instance.IsMoving && bUsesDeadMan) Wind_Elevator.Instance.StopElevator();
    }

    protected bool CanBeClicked()
    {
        return (bAfecta && bInsideElevator && Wind_Elevator.Instance.InsideElevator && ((Wind_Elevator.Instance.bUsesExtIntSelector && Wind_Elevator.Instance.InteriorMode) || !Wind_Elevator.Instance.bUsesExtIntSelector)) || 
               (bAfecta && !bInsideElevator && !Wind_Elevator.Instance.InsideElevator && ((Wind_Elevator.Instance.bUsesExtIntSelector && !Wind_Elevator.Instance.InteriorMode) || !Wind_Elevator.Instance.bUsesExtIntSelector))
            || !bAfecta;
    }
    #endregion
}
