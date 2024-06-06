using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum Direction { Up, Down, Stop, NoBrakes}

public class Wind_Elevator : MonoBehaviour
{
    #region PARAMETERS
    #region Events
    [Header("Cuando el elevador sube")]
    public UnityEvent WhileElevatorGoesUp;

    [Header("Cuando el elevador baja")]
    public UnityEvent WhileElevatorGoesDown;

    [Header("Cuando el elevador se para")]
    public UnityEvent OnElevatorStops;

    [Header("Cuando el elevador llega a la altura máxima")]
    public UnityEvent OnElevatorTopReached;

    [Header("Cuando el elevador llega a la altura minima")]
    public UnityEvent OnElevatorBottomReached;

    [Header("Cuando el elevador empieza a moverse hacia arriba")]
    public UnityEvent OnElevatorGoesUp;

    [Header("Cuando el elevador se mueve con los frenos")]
    public UnityEvent OnElevatorBrake;

    [Header("Cuando el elevador empieza a bajar")]
    public UnityEvent OnElevatorGoesDown;
    #endregion

    #region Movement
    /// <summary>
    /// Direccion actual del movimiento del elevador
    /// </summary>
    protected Direction CurrentDirection;

    /// <summary>
    /// Velocidad a la que se desplaza el elevador
    /// </summary>
    [Header("Velocidad a la que se desplaza el elevador")]
    [SerializeField] protected float elevatorSpeed;

    /// <summary>
    /// Variable publica por si se necesita recoger la velocidad en otra clase
    /// </summary>
    public float ElevatorSpeed { get { return elevatorSpeed; }}

    [Header("La velocidad del elevador al usar los frenos")]
    [SerializeField] protected float elevatorBrakeSpeed;

    /// <summary>
    /// Referencia publica para acceder a la velocidad al usar los frenos
    /// </summary>
    public float ElevatorBrakeSpeed { get { return elevatorBrakeSpeed; } }

    [Header("La altura máxima a la que llega el elevador")]
    [SerializeField] protected float maxHeight = 20;

    [Header("La altura minima a la que llega el elevador")]
    [SerializeField] protected float minHeight = 0;

    /// <summary>
    /// Deberia mover al jugador al desplazarse?
    /// </summary>
    [HideInInspector] public bool ShouldMovePlayer = false;

    /// <summary>
    /// Referencia publica para acceder a la altura maxima
    /// </summary>
    public float MaxHeight { get { return maxHeight; }}
    /// <summary>
    /// Referencia publica para acceder a la altura minima
    /// </summary>
    public float MinHeight { get { return minHeight; }}
    #endregion

    #region Sounds

    [Header("El sonido del motor del elevador al moverse")]
    [SerializeField] protected AudioSource ElevatorMotorSound;

    [Header("El sonido de la alarma de movimiento del elevador")]
    [SerializeField] protected AudioSource AlarmElevatorSound;

    [Header("El sonido de los frenos al moverse")]
    [SerializeField] protected AudioSource BrakeElevatorSound;

    #endregion

    #region Additional Parameters

    [Header("Esta en un estado de emergencia el elevador?")]
    public bool Emergency = false;

    [Header("Necesita que se pulse el boton de hombre muerto?")]
    public bool NeedsDeadManButton = true;

    [Header("Esta pulsado el boton de hombre muerto?")]
    public bool DeadManButtonPressed = false;

    [Header("Esta la puerta abierta?")]
    public bool DoorOpened = false;

    [Header("Puede ir hacia arriba?")]
    public bool CanGoUp = true;

    [Header("Puede ir hacia abajo?")]
    public bool CanGoDown = true;

    [Header("Esta el player dentro del elevador?")]
    public bool InsideElevator = false;

    #endregion

    #region Coroutines
    /// <summary>
    /// The standart WaitForEndOfFrame used for coroutines
    /// </summary>
    private WaitForEndOfFrame Frame = new WaitForEndOfFrame();
    #endregion
    #endregion

    #region METHODS
    private void Awake()
    {
        CheckSingleton();
    }

    public void MoveElevator(Direction direction)
    {
        CurrentDirection = direction;

        if (direction == Direction.Up && !IsAtTop()) OnElevatorGoesUp.Invoke();
        else { return; }
        if (direction == Direction.Down && !IsAtBottom()) OnElevatorGoesDown.Invoke();
        else { return; }
        if (direction == Direction.NoBrakes && !IsAtBottom()) OnElevatorBrake.Invoke();
        else { return; }

        StartCoroutine(nameof(MovementBroadcast));
    }

    public void StopElevator()
    {
        MoveElevator(Direction.Stop);

        OnElevatorStops.Invoke();

        StopAllSounds();

        StopCoroutine(nameof(MovementBroadcast));
    }

    public void TranslateElevator()
    {
        Vector3 direction = Vector3.zero;

        float speed = 0;

        switch (CurrentDirection)
        {
            default:

                break;
            case Direction.Up:
                direction = Vector3.up;
                speed = ElevatorSpeed;
                break;
            case Direction.Down:
                direction = Vector3.down;
                speed = ElevatorSpeed;
                break;
            case Direction.NoBrakes:
                direction = Vector3.down;
                speed = elevatorBrakeSpeed;
                break;
        }

        this.transform.position = Vector3.Lerp(this.transform.position, this.transform.position + direction, Time.deltaTime * speed);
    }

   
    IEnumerator MovementBroadcast()
    {
        while (true)
        {
            switch (CurrentDirection)
            {
                case Direction.Up:
                    WhileElevatorGoesUp.Invoke();
                    break;

                case Direction.Down:
                    WhileElevatorGoesDown.Invoke();
                    break;

                case Direction.Stop:

                    break;

                case Direction.NoBrakes:

                    break;
            }

            TranslateElevator();

            if (CurrentDirection == Direction.Up && IsAtTop()) StopElevator();
            if (CurrentDirection == Direction.Down && IsAtBottom()) StopElevator();
            yield return Frame;
        }
    }

    /// <summary>
    /// Is the elevator at the min height?
    /// </summary>
    /// <returns></returns>
    public bool IsAtBottom()
    {
        return this.transform.position.y <= MinHeight;
    }

    /// <summary>
    /// Is the elevator at top?
    /// </summary>
    /// <returns></returns>
    public bool IsAtTop()
    {
        return this.transform.position.y >= MaxHeight;
    }

    #region Sounds
    /// <summary>
    /// Reproduce el sonido del motor del elevador
    /// </summary>
    /// <param name="disableRest"></param>
    public void PlayElevatorSound(bool disableRest)
    {
        ElevatorMotorSound.loop = true;
        ElevatorMotorSound.Play();

        if (!disableRest) return;

        AlarmElevatorSound.Stop();
        BrakeElevatorSound.Stop();
    }

    /// <summary>
    /// Reproduce el sonido de los frenos del elevador
    /// </summary>
    /// <param name="disableRest"></param>
    public void PlayBrakeSound(bool disableRest)
    {
        BrakeElevatorSound.loop = true;
        BrakeElevatorSound.Play();

        if (!disableRest) return;

        ElevatorMotorSound.Stop();
        AlarmElevatorSound.Stop();
    }

    /// <summary>
    /// Reproduce el sonido de alarma del elevador
    /// </summary>
    /// <param name="disableRest"></param>
    public void PlayAlarmSound(bool disableRest)
    {
        AlarmElevatorSound.loop = true;
        AlarmElevatorSound.Play();

        if (!disableRest) return;

        ElevatorMotorSound.Stop();
        BrakeElevatorSound.Stop();
    }

    public void StopAllSounds()
    {
        AlarmElevatorSound.Stop();
        ElevatorMotorSound.Stop();
        BrakeElevatorSound.Stop();
    }
    #endregion
    #endregion

    #region SINGLETON

    private static Wind_Elevator instance;
    public static Wind_Elevator Instance { get { return instance; } }

    private void CheckSingleton()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    #endregion
}
