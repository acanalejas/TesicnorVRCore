using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wind_Brake : VRColliderPath
{
    #region PARAMETERS

    #endregion

    #region METHODS
    private void Start()
    {
        this.pathType = PathType.Rotation;
    }

    public void Brake()
    {
        Wind_Elevator.Instance.MoveElevator(Direction.NoBrakes);
    }
    #endregion
}
