using System;
using System.Collections;
using UnityEngine;

public class Wind_BrakeCollection : MonoBehaviour
{
    #region PARAMETERS

    [SerializeField] [Header("Las palancas o accionadores con recorrido que componen este freno en conjunto")]
    private VRColliderPath[] CollectionBrakes;

    [SerializeField] [Header(("Las palancas o accionadores SIN recorrido que componen el freno"))]
    private VRCollider[] CollecionBrakes_NoPath;

    #endregion

    #region METHODS

    private void Start()
    {
        StartCoroutine(nameof(update));
    }

    private WaitForEndOfFrame frame = new WaitForEndOfFrame();
    IEnumerator update()
    {
        while (true)
        {
            if(IsBeingUsed() && !Wind_Elevator.Instance.IsMoving) Wind_Elevator.Instance.MoveElevator(Direction.NoBrakes);
            yield return frame;
        }
    }

    bool CheckPathBrakes()
    {
        if (CollectionBrakes.Length <= 0) return false;

        foreach (var _brake in CollectionBrakes)
        {
            if (!_brake.isPathCompleted()) return false;
        }

        return true;
    }

    bool CheckNoPathBrakes()
    {
        if (CollecionBrakes_NoPath.Length <= 0) return false;

        foreach (var _brake in CollecionBrakes_NoPath)
        {
            if (!_brake.isGrabbed()) return false;
        }

        return true;
    }

    public bool IsBeingUsed()
    {
        return CheckPathBrakes() || CheckNoPathBrakes();
    }

    #endregion
}
