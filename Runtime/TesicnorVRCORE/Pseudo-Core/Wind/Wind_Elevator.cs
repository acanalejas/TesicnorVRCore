using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum Direction { Up, Down, Stop, NoBrakes}

[RequireComponent(typeof(PlayerDetector))]
public class Wind_Elevator : MonoBehaviour
{
    #region PARAMETERS
    #region Events
    [Header("Cuando el elevador sube")]
    public UnityEvent WhileElevatorGoesUp;

    [Header("Cuando el elevador baja")]
    public UnityEvent WhileElevatorGoesDown;

    [Header("Cuando el elevador baja sin frenos")]
    public UnityEvent WhileElevatorNoBrakes;

    [Header("Cuando el elevador se para")]
    public UnityEvent OnElevatorStops;

    [Header("Cuando el elevador llega a la altura m�xima")]
    public UnityEvent OnElevatorTopReached;

    [Header("Cuando el elevador llega a la altura minima")]
    public UnityEvent OnElevatorBottomReached;

    [Header("Cuando el elevador empieza a moverse hacia arriba")]
    public UnityEvent OnElevatorGoesUp;

    [Header("Cuando el elevador se mueve con los frenos")]
    public UnityEvent OnElevatorBrake;

    [Header("Cuando el elevador empieza a bajar")]
    public UnityEvent OnElevatorGoesDown;

    [Header("Cuando el elevador entra en emergencia")]
    public UnityEvent OnEmergency;

    [Header("Cuando el elevador sale de emergencia")]
    public UnityEvent OnEmergencyExit;

    [Header("Cuando el elevador recibe electricidad")]
    public UnityEvent OnElectricityEnabled;

    [Header("Cuando el elevador deja de recibir electricidad")]
    public UnityEvent OnElectricityDisabled;
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

    [Header("La altura m�xima a la que llega el elevador")]
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

    public bool IsMoving { get { return isMoving; } }

    private bool isMoving;
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
    public bool Emergency { get { return emergency; } }

    private bool emergency = false;

    [Header("Esta dado el botón de marcha?")][SerializeField]
    private bool marcha = true;

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

    [Header("La electricidad est� activada?")]
    private bool IsElectricityOn = false;

    [Header("El evento que se lanza al abrir la puerta")]
    public UnityEvent OnDoorOpened;

    [Header("El evento que se lanza al cerrar la puerta")]
    public UnityEvent OnDoorClosed;

    [Header("EL animator de la puerta")] [SerializeField]
    private Animator DoorAnim;

    private PlayerDetector InsideDetector;

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
        InsideDetector = GetComponent<PlayerDetector>();

        InsideDetector.OnPlayerDetected.AddListener(() => { InsideElevator = true; });
        InsideDetector.OnPlayerExitDetection.AddListener(() => { InsideElevator = false; });
    }

    public virtual void MoveElevator(Direction direction)
    {
        if ((Emergency || !marcha || !IsElectricityOn) && (direction != Direction.NoBrakes)) return;

        CurrentDirection = direction;

        if (direction == Direction.Up && !IsAtTop()) OnElevatorGoesUp.Invoke();

        else if (direction == Direction.Down && !IsAtBottom()) OnElevatorGoesDown.Invoke();
        
        else if (direction == Direction.NoBrakes && !IsAtBottom()) OnElevatorBrake.Invoke();
        
        else { Debug.Log("El elevador no se puede mover porque ha llegado al extremo de la direccion elegida");return; }

        StartCoroutine(nameof(MovementBroadcast));
    }

    public virtual void SetEmergency()
    {
        emergency = !emergency;

        if (emergency) OnEmergency.Invoke();
        else OnEmergencyExit.Invoke();
    }

    public virtual void SetEmergency(bool _value)
    {
        emergency = _value;

        if (emergency) OnEmergency.Invoke();
        else OnEmergencyExit.Invoke();
    }

    public virtual void SetMarcha()
    {
        marcha = true;
    }

    public virtual void SetMarcha(bool _value)
    {
        marcha = _value;
    }

    public virtual void SetElectricity(bool _value)
    {
        IsElectricityOn = _value;

        if (IsElectricityOn) OnElectricityEnabled.Invoke();
        else OnElectricityDisabled.Invoke();
    }

    public virtual void SetElectricity()
    {
        IsElectricityOn = !IsElectricityOn;

        if (IsElectricityOn) OnElectricityEnabled.Invoke();
        else OnElectricityDisabled.Invoke();
    }

    public virtual void OpenDoor()
    {
        DoorOpened = true;
        OnDoorOpened.Invoke();
    }

    public virtual void CloseDoor()
    {
        DoorOpened = false;
        OnDoorClosed.Invoke();
    }

    public virtual void ToggleDoor()
    {
        if (!this.IsAtBottom() && !this.IsAtTop()) return;
        DoorOpened = !DoorOpened;
        if(DoorOpened) OnDoorOpened.Invoke();
        else OnDoorClosed.Invoke();;
        
        DoorAnim.SetTrigger("Open");
    }

    public virtual void StopElevator()
    {
        OnElevatorStops.Invoke();

        StopAllSounds();

        StopCoroutine(nameof(MovementBroadcast));

        isMoving = false;

        Debug.Log("Stopping elevator");
    }

    protected virtual void TranslateElevator()
    {
        var direction = CurrentDirection == Direction.Up ? Vector3.up : Vector3.down;
        var speed = CurrentDirection == Direction.NoBrakes ? elevatorBrakeSpeed : ElevatorSpeed;

        this.transform.position = Vector3.Lerp(this.transform.position, this.transform.position + direction, Time.deltaTime * speed);
        if(InsideElevator) TesicnorPlayer.Instance.gameObject.transform.position = Vector3.Lerp(TesicnorPlayer.Instance.gameObject.transform.position, TesicnorPlayer.Instance.gameObject.transform.position + direction, Time.deltaTime * speed);
        Debug.Log("Moving elevator to " + direction.ToString());
    }

   
    protected IEnumerator MovementBroadcast()
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
                    WhileElevatorNoBrakes.Invoke();
                    break;
            }

            TranslateElevator();

            isMoving = true;

            if (CurrentDirection == Direction.Up && IsAtTop()) {StopElevator(); OnElevatorTopReached.Invoke();}
            if ((CurrentDirection == Direction.Down || CurrentDirection == Direction.NoBrakes) && IsAtBottom()) {StopElevator(); OnElevatorBottomReached.Invoke();}
            if (Emergency) StopElevator();
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
        if(ElevatorMotorSound != null)
        {
            ElevatorMotorSound.loop = true;
            ElevatorMotorSound.Play();
        }

        if (!disableRest) return;

        if(AlarmElevatorSound)AlarmElevatorSound.Stop();
        if(BrakeElevatorSound)BrakeElevatorSound.Stop();
    }

    /// <summary>
    /// Reproduce el sonido de los frenos del elevador
    /// </summary>
    /// <param name="disableRest"></param>
    public void PlayBrakeSound(bool disableRest)
    {
        if(BrakeElevatorSound != null)
        {
            BrakeElevatorSound.loop = true;
            BrakeElevatorSound.Play();
        }

        if (!disableRest) return;

        if(ElevatorMotorSound)ElevatorMotorSound.Stop();
        if(AlarmElevatorSound)AlarmElevatorSound.Stop();
    }

    /// <summary>
    /// Reproduce el sonido de alarma del elevador
    /// </summary>
    /// <param name="disableRest"></param>
    public void PlayAlarmSound(bool disableRest)
    {
        if (AlarmElevatorSound)
        {
            AlarmElevatorSound.loop = true;
            AlarmElevatorSound.Play();
        }

        if (!disableRest) return;
        
        if(ElevatorMotorSound)ElevatorMotorSound.Stop();
        if(BrakeElevatorSound)BrakeElevatorSound.Stop();
    }

    public void StopAllSounds()
    {
        if(AlarmElevatorSound) AlarmElevatorSound.Stop();
        if(ElevatorMotorSound) ElevatorMotorSound.Stop();
        if(BrakeElevatorSound) BrakeElevatorSound.Stop();
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
