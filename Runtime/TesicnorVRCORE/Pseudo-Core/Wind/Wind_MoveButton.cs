using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wind_MoveButton : VRInteractable_Button
{
    #region PARAMETERS
    [Header("La direccion en la que se movera el elevador")]
    public Direction direction = Direction.Up;
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
        Wind_Elevator.Instance.DeadManButtonPressed = true;
        Wind_Elevator.Instance.MoveElevator(direction);
    }

    public override void OnRelease()
    {
        base.OnRelease();
        Wind_Elevator.Instance.DeadManButtonPressed = false;
        Wind_Elevator.Instance.StopElevator();
    }
    #endregion
}
