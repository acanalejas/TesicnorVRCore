using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabHelmet : VR_Task
{
    #region PARAMETERS
    [Header("El componente VRCollider que debe llevar el casco")]
    public VRCollider helmetCollider;
    #endregion

    #region FUNCTIONS

    public override void OnEnable()
    {
        base.OnEnable();
        helmetCollider.target.GetComponent<BoxCollider>().enabled = false;
    }
    public override void myUpdate()
    {
        base.myUpdate();
        if (isCompleted()) CompleteTask();
    }

    bool isCompleted()
    {
        bool result = helmetCollider.isGrabbed();
        if (result) helmetCollider.target.GetComponent<BoxCollider>().enabled = true;
        return result;
    }
    #endregion
}
