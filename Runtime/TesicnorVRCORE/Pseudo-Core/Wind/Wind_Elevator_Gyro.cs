using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wind_Elevator_Gyro : MonoBehaviour
{
    #region PARAMETERS
    [Header("El multiplicador de dirección que sigue el giroscopio al ir hacia arriba")]
    [SerializeField] private int DirectionMultiplier = 1;

    [Header("La velocidad a la que gira el giroscopio")]
    [SerializeField] private float Speed = 1;

    [Header("Angulos por frame")]
    [SerializeField] private float AnglesPerFrame = 10;

    [Header("El eje sobre el que gira el giroscopio")]
    [SerializeField] private Vector3 RotationAxis = Vector3.forward;
    #endregion

    #region METHODS
    private void Start()
    {
        Wind_Elevator.Instance.OnElevatorGoesUp.AddListener(() =>
        {
            InvokeRepeating(nameof(RotateGyroUp), 0, 0.05f);
        });
        Wind_Elevator.Instance.OnElevatorGoesDown.AddListener(() =>
        {
            InvokeRepeating(nameof(RotateGyroDown), 0, 0.05f);
        });
        Wind_Elevator.Instance.OnElevatorStops.AddListener(() =>
        {
            CancelInvoke(nameof(RotateGyroUp));
            CancelInvoke(nameof(RotateGyroDown));
        });
    }

    private void RotateGyroUp()
    {
        this.transform.rotation *= Quaternion.AngleAxis(AnglesPerFrame * Speed * DirectionMultiplier, RotationAxis);
    }

    private void RotateGyroDown()
    {
        this.transform.rotation *= Quaternion.AngleAxis(AnglesPerFrame * Speed * (DirectionMultiplier * -1), RotationAxis);
    }
    #endregion
}
