using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PC_DEBUG_PLAYER : MonoBehaviour
{
    #region SINGLETON
    private static PC_DEBUG_PLAYER instance;
    public static PC_DEBUG_PLAYER Instance { get { return instance; } }

    void CheckSingleton()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }
    #endregion
    #region PARAMETERS
    /// <summary>
    /// El GO del mando derecho
    /// </summary>
    [Header("El GO que contiene el mando derecho")]
    [SerializeField] private GameObject rightController_Anchor;

    /// <summary>
    /// El GO del mando izquierdo
    /// </summary>
    [Header("El GO que contiene el mando izquierdo")]
    [SerializeField] private GameObject leftController_Anchor;

    /// <summary>
    /// El componente Camera que usa este jugador
    /// </summary>
    [Header("El componente Camera de este jugador")]
    public Camera camera;

    /// <summary>
    /// La altura a la que se encuentra la camara en play
    /// </summary>
    [Header("La altura a la que estará la cámara")]
    [SerializeField] private float camera_Height = 1.8f;

    /// <summary>
    /// La velocidad a la que se mueve el jugador
    /// </summary>
    [Header("La velocidad a la que se mueve el personaje")]
    [SerializeField] private float player_Speed = 4;

    /// <summary>
    /// Tecla para simular el grip del mando izquierdo
    /// </summary>
    [Header("Tecla para simular el grip del mando izquierdo")]
    [SerializeField] private KeyCode gripCode_Left = KeyCode.Q;

    /// <summary>
    /// Tecla para simular el grip del mando derecho
    /// </summary>
    [Header("Tecla para simular el grip del mando derecho")]
    [SerializeField] private KeyCode gripCode_Right = KeyCode.E;

    /// <summary>
    /// Tecla para simular el trigger del mando izquierdo
    /// </summary>
    [Header("Tecla para simular el trigger del mando izquierdo")]
    [SerializeField] private KeyCode triggerCode_Left = KeyCode.LeftShift;

    /// <summary>
    /// Tecla para simular el trigger del mando derecho
    /// </summary>
    [Header("Tecla para simular el trigger del mando derecho")]
    [SerializeField] private KeyCode triggerCode_Right = KeyCode.RightShift;

    /// <summary>
    /// La tecla con la que nos movemos hacia delante
    /// </summary>
    [Header("La tecla con la que nos movemos hacia delante")]
    [SerializeField] private KeyCode moveCode_Forward = KeyCode.W;

    /// <summary>
    /// La tecla con la que nos movemos hacia detras
    /// </summary>
    [Header("La tecla con la que nos movemos hacia detras")]
    [SerializeField] private KeyCode moveCode_Back = KeyCode.S;

    /// <summary>
    /// La tecla con la que nos movemos hacia la derecha
    /// </summary>
    [Header("La tecla con la que nos movemos hacia la derecha")]
    [SerializeField] private KeyCode moveCode_Right = KeyCode.D;

    /// <summary>
    /// La tecla con la que nos movemos hacia la izquierda
    /// </summary>
    [Header("La tecla con la que nos movemos hacia la izquierda")]
    [SerializeField] private KeyCode moveCode_Left = KeyCode.A;

    /// <summary>
    /// Componente GrippingHand de la mano izquierda
    /// </summary>
    private GrippingHand leftController_GrippingHand;
    /// <summary>
    /// Componente GrippingHand de la mano derecha
    /// </summary>
    private GrippingHand rightController_GrippingHand;

    /// <summary>
    /// Componente HandInteraction de la mano izquierda
    /// </summary>
    private HandInteraction leftController_HandInteraction;
    /// <summary>
    /// Componente HandInteraction de la mano derecha
    /// </summary>
    private HandInteraction rightController_HandInteraction;

    #endregion

    #region FUNCTIONS
    private void Awake()
    {
        CheckSingleton();
        SetupPlayer();
        DontDestroyOnLoad(this.gameObject);

        this.gameObject.tag = "Player";
#if UNITY_EDITOR
        //Destroy(this.gameObject);
#endif
    }

    private void Update()
    {
        SetControllersPosition();
        CheckInput();
    }

    /// <summary>
    /// Setea los valores iniciales del jugador con lo que necesita para funcionar
    /// </summary>
    private void SetupPlayer()
    {
        camera.transform.position += new Vector3(0, camera_Height, 0);

        leftController_GrippingHand = leftController_Anchor.AddComponent<GrippingHand>();
        leftController_HandInteraction = leftController_Anchor.AddComponent<HandInteraction>();

        rightController_GrippingHand = rightController_Anchor.AddComponent<GrippingHand>();
        rightController_HandInteraction = rightController_Anchor.AddComponent<HandInteraction>();

        // =============== SETTING UP THE GRIPPING HANDS ======================

        leftController_GrippingHand.isController = true;
        leftController_GrippingHand.handType = GrippingHand.HandType.left;
        leftController_GrippingHand.handController = leftController_Anchor.AddComponent<XRController>();
        leftController_GrippingHand.player = leftController_Anchor.transform.parent;

        rightController_GrippingHand.isController = true;
        rightController_GrippingHand.handType = GrippingHand.HandType.right;
        rightController_GrippingHand.handController = rightController_Anchor.AddComponent<XRController>();
        rightController_GrippingHand.player = rightController_Anchor.transform.parent;

        // =============== SETTING UP THE HAND INTERACTION ============================

        leftController_HandInteraction.isHandControlled = false;
        leftController_HandInteraction.isLeftHand = true;
        leftController_HandInteraction.nonDetectedColor = Color.red;
        leftController_HandInteraction.detectedColor = Color.green;
        leftController_HandInteraction.usesRay = true;
        leftController_HandInteraction.handController = leftController_GrippingHand.handController;
        leftController_HandInteraction.interactionOrigin = leftController_Anchor.transform;
        rightController_HandInteraction.isHandControlled = false;
        rightController_HandInteraction.isLeftHand = false;
        rightController_HandInteraction.nonDetectedColor = Color.red;
        rightController_HandInteraction.detectedColor = Color.green;
        rightController_HandInteraction.usesRay = true;
        rightController_HandInteraction.handController = rightController_GrippingHand.handController;
        rightController_HandInteraction.interactionOrigin = rightController_Anchor.transform;
    }

    /// <summary>
    /// Funcion para usar en el update que controla la posicion de los mandos
    /// </summary>
    private void SetControllersPosition()
    {
        Vector3 leftController_ScreenPoint = new Vector2(Screen.width * 0.25f, Screen.height * 0.25f);
        Vector3 rightController_ScreenPoint = new Vector2(Screen.width * 0.75f, Screen.height * 0.25f);

        Vector3 leftController_newPosition = camera.ScreenToWorldPoint(leftController_ScreenPoint);
        Vector3 rightController_newPosition = camera.ScreenToWorldPoint(rightController_ScreenPoint);

        leftController_Anchor.transform.forward = -camera.transform.forward;
        rightController_Anchor.transform.forward = -camera.transform.forward;

        leftController_Anchor.transform.position = leftController_newPosition - leftController_Anchor.transform.forward * 0.5f;
        rightController_Anchor.transform.position = rightController_newPosition - rightController_Anchor.transform.forward * 0.5f;

        //leftController_Anchor.transform.localRotation =     Quaternion.Euler(leftController_RotationOffset);
        //rightController_Anchor.transform.localRotation =    Quaternion.Euler(rightController_RotationOffset);
    }

    /// <summary>
    /// Checkea el input del jugador para mover y ejecutar los comandos del Player
    /// </summary>
    private void CheckInput()
    {
        //For Grip
        if (Input.GetKeyDown(gripCode_Left)) leftController_GrippingHand.Grab();
        else if (Input.GetKeyUp(gripCode_Left)) leftController_GrippingHand.Release();

        if (Input.GetKeyDown(gripCode_Right)) rightController_GrippingHand.Grab();
        else if (Input.GetKeyUp(gripCode_Right)) rightController_GrippingHand.Release();

        //For Trigger
        if (Input.GetKeyDown(triggerCode_Left)) leftController_HandInteraction.Click();
        else if (Input.GetKeyUp(triggerCode_Left)) leftController_HandInteraction.Release();

        if (Input.GetKeyDown(triggerCode_Right)) rightController_HandInteraction.Click();
        else if (Input.GetKeyUp(triggerCode_Right)) rightController_HandInteraction.Release();

        //For Player Movement
        if (Input.GetKey(moveCode_Forward)) MovePlayer(camera.transform.forward);
        else if (Input.GetKey(moveCode_Back)) MovePlayer(-camera.transform.forward);
        else if (Input.GetKey(moveCode_Right)) MovePlayer(camera.transform.right);
        else if (Input.GetKey(moveCode_Left)) MovePlayer(-camera.transform.right);

        //For Camera Rotation
        RotatePlayer();
    }


    Vector3 lastFrameMousePosition;
    /// <summary>
    /// Rota al jugador dependiendo del movimiento del raton
    /// </summary>
    private void RotatePlayer()
    {
        if (lastFrameMousePosition != Vector3.zero)
        {
            Vector2 distance = Input.mousePosition - lastFrameMousePosition;

            /// FOR X AXIS
            float maxReferenceX = Screen.height;
            float maxValueX = 90;

            float currentReferenceX = Input.mousePosition.y;
            float currentValueX = -(maxValueX * currentReferenceX) / maxReferenceX;

            ///FOR Y AXIS
            float maxReferenceY = Screen.width;
            float maxValueY = 180;

            float currentReferenceY = distance.x;
            float currentValueY = maxValueY * currentReferenceY / maxReferenceY;

            ///SETTING AND CLAMPING ROTATION
            camera.transform.rotation *= Quaternion.Euler(0, currentValueY, 0);
            camera.transform.rotation = Quaternion.Euler(currentValueX,
                camera.transform.rotation.eulerAngles.y, 0);
        }
        lastFrameMousePosition = Input.mousePosition;
    }

    /// <summary>
    /// Mueve al jugador en la direccion que le pasemos
    /// </summary>
    /// <param name="direction"></param>
    void MovePlayer(Vector3 direction)
    {
        Vector3 newDirection = new Vector3(direction.x, 0, direction.z);

        this.transform.position = Vector3.Lerp(this.transform.position, this.transform.position + newDirection, Time.deltaTime * player_Speed);
    }
    #endregion
}
