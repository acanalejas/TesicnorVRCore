using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEditor;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class GrippingHand : MonoBehaviour, VRHandInterface
{
    #region PARAMETERS
    /// <summary>
    /// Controla si usamos mandos o las propias manos
    /// </summary>
    [Header("============= COMMON ATRIBUTES ==============")][Space(10)]
    [Header("Controla si usamos mandos o las propias manos")]
    [HideInInspector]public bool isController = false;

    /// <summary>
    /// Si se desactiva la geometria de la mano al agarrar
    /// </summary>
    [Header("Si se desactiva la geometría de la mano al agarrar")]
    [HideInInspector]public bool hideOnGrab = false;

    /// <summary>
    /// El componente que renderiza la geometria de la mano
    /// </summary>
    [Header("El componente que renderiza la geometria de la mano")]
    [HideInInspector]public Renderer handRenderer;

    /// <summary>
    /// Offset en la posicion del colider de agarrado
    /// </summary>
    [Header("Offset en la posicion del collider que detecta si se agarra")]
    [HideInInspector]public Vector3 positionOffset;

    /// <summary>
    /// Radio de la esfera que se usa para detectar el agarre
    /// </summary>
    [Header("Radio del collider que detecta si se agarra")]
    [HideInInspector]public float colliderRadius = 0.2f;

    /// <summary>
    /// Controlador de la mano 
    /// </summary>
    [Header("============ CONTROLLER ATRIBUTES =============")][Space(10)]
    [Header("Controlador de la mano")]
    [HideInInspector]public XRController handController;

   
    /// <summary>
    /// OPCIONAL : Hueso al que enganchar el collider
    /// </summary>
    [Header("OPCIONAL : Hueso al que enganchar el collider")]
    [Header("============= HANDS ATRIBUTES ==============")]
    [Space(10)]
    [HideInInspector]public Transform colliderBone;

    /// <summary>
    /// El transform del player en general
    /// </summary>
    [Header("El Transform del player en general")]
    [HideInInspector]public Transform player;
   
    /// <summary>
    /// Velocidad a la que se mueve la mano
    /// </summary>
    [HideInInspector] public Vector3 velocity;
    public enum HandType { right, left};

    [Header("Define si la mano es la izquierda o derecha")]
    [HideInInspector]public HandType handType;

    /// <summary>
    /// Lista de GO basada en los objetos dentro del trigger
    /// </summary>
    private List<GameObject> overlappingObjects = new List<GameObject>();

    /// <summary>
    /// El objeto agarrado
    /// </summary>
    [HideInInspector] public GameObject grippedObject;

    /// <summary>
    /// Devuelve si está agarrando
    /// </summary>
    [HideInInspector] public bool isGripping = false;

    public static bool PC_Debug_Editor;

    private Vector3 lastPosition;

    #endregion

    #region FUNCTIONS
    /// <summary>
    /// Setea el trigger que se usa para saber si hay algun objeto al alcance
    /// </summary>
    private void SetTrigger()
    {
        SphereCollider gripCollider = GetComponent<SphereCollider>();

        gripCollider.isTrigger = true;
        gripCollider.radius = colliderRadius;
        if (handType == HandType.right) gripCollider.center += new Vector3(0, 0.015f, 0);

        gameObject.GetComponent<Rigidbody>().useGravity = false;
        gameObject.GetComponent<Rigidbody>().isKinematic = true;

        if (colliderBone) gripCollider.center = -this.transform.InverseTransformPoint(colliderBone.position);
    }
    private void Awake()
    {
        SetTrigger();
        this.gameObject.tag = "Hand";
    }

    private void FixedUpdate()
    {
        DetectTheInput();
        CalculateVelocity();
        SetHandsTracking();
    }
    public void Update()
    {
        //CheckListSecurity();
    }

    Vector3 lastFramePosition = Vector3.zero;
    private void CalculateVelocity()
    {
        Vector3 position = this.transform.position;
        if (lastFramePosition == Vector3.zero) lastFramePosition = position;

        velocity = (position - lastFramePosition) / Time.fixedDeltaTime;

        lastFramePosition = position;
    }

    /// <summary>
    /// Comprueba la lista por seguridad en el Update, para evitar
    /// que haya GameObjects que no se ha detectado la salida del trigger
    /// </summary>
    private void CheckListSecurity()
    {
        foreach(GameObject go in overlappingObjects)
        {
            ///Para saber si un punto está dentro de una esfera
            ///centro de la esfera -> c = (cx, cy, cz)
            ///punto -> p = (x,y,z)
            ///
            ///Ecuación -> (x - cx)^2 + (y - cy)^2 + (z - cz)^2 < radius

            Vector3 center = gameObject.transform.position;
            Vector3 point = go.GetComponent<Collider>().ClosestPoint(gameObject.transform.position);

            bool ecuation = (point.x - center.x) * (point.x - center.x) + (point.y - center.y) * (point.y - center.y) + (point.z - center.z) * (point.z - center.z) < colliderRadius;

            if (!ecuation) { overlappingObjects.Remove(go);}

        }
    }

    private void CheckIfHandGrabs()
    {

    }

    /// <summary>
    /// Setea el estado del renderer de la mano.
    /// </summary>
    /// <param name="_value"></param>
    private void SetRendererEnable(bool _value)
    {
        handRenderer.enabled = _value;
    }

    /// <summary>
    /// Checkea en caso de que se usen mandos, el input para saber si el jugador esta agarrando
    /// </summary>
    bool alreadyGrabbed = false;
    private void CheckIfControllerGrabs()
    {
        bool pressed = false;
        bool released = false;

        //Cuando se pulsa el gatillo
        if (handController.inputDevice.TryGetFeatureValue(CommonUsages.gripButton, out pressed) && pressed && canGrabSomething() && !isGripping)
        {
            Grab();
            alreadyGrabbed = true;
        }

        //Cuando se suelta el gatillo
        if (handController.inputDevice.TryGetFeatureValue(CommonUsages.gripButton, out released) && !released && alreadyGrabbed)
        {
            Release();
            alreadyGrabbed = false;
        }
    }

    bool lastPressed = false;
    private void CheckIfControllerGrabs_Trigger()
    {
        float tpressed = 0;
        float treleased = 0;
        
        handController.inputDevice.TryGetFeatureValue(CommonUsages.trigger, out tpressed);
        if (tpressed > 0.4f)
        {
            Grab();
            lastPressed = true;
        }
        //Cuando se suelta el gatillo
        handController.inputDevice.TryGetFeatureValue(CommonUsages.trigger, out treleased);
        if (treleased < 0.4f && lastPressed)
        {
            Release();
            lastPressed = false;
        }
    }
    #region VRHandInterface
    /// <summary>
    /// Funcion que sirve para agarrar un objeto
    /// </summary>
    public void Grab()
    {
        GameObject grabObject = closestObjectToGrab();
        if (!isGrabbing())
        {
            if (grabObject /*&& !grabObject.GetComponent<VRGripInterface>().isGrabbed()*/)
            {
                //if (grabObject.GetComponent<VRGripInterface>().isGrabbed()) grabObject.GetComponent<VRGripInterface>().GetGrippingHand().Release();
                if(grabObject.GetComponent<VRGripInterface>().canBeGrabbed())grabObject.GetComponent<VRGripInterface>().Grab(this);
                if(hideOnGrab)SetRendererEnable(false);
            }
        }
            
       
        if(grippedObject)Debug.Log("Grabbing an object with name  :  " + grippedObject.name);
    }

    /// <summary>
    /// Funcion que sirve para soltar un objeto
    /// </summary>
    public void Release()
    {
        if (grippedObject)
        {
            if (overlappingObjects.Contains(grippedObject)) { overlappingObjects.Remove(grippedObject); grippedObject.GetComponent<VRGripInterface>().SetAvailableEffects(false); }
        }
        if (isGrabbing()) grippedObject.GetComponent<VRGripInterface>().Release();
        if(hideOnGrab) SetRendererEnable(true);
        //Safety control
        if (grippedObject) grippedObject = null;
        isGripping = false;
    }
    
    /// <summary>
    /// Usar en el update, detecta el input del botón con el que queramos agarrar
    /// </summary>
    public void DetectTheInput()
    {
        if (isController)
        {
            CheckIfControllerGrabs();
            CheckIfControllerGrabs_Trigger();
            if(this.handController.controllerNode == XRNode.LeftHand)
            {
                bool pressed = false;
                bool doOnce = false;
                if (handController.inputDevice.TryGetFeatureValue(CommonUsages.menuButton, out pressed) && pressed && !doOnce)
                {
                    TesicnorPlayer player = TesicnorPlayer.Instance;

                    if(player != null)
                    player.TogglePause(!player.bIsInPause);
                    doOnce = true;
                }
                else if(!pressed && doOnce) doOnce = false;
            }
        }
        else
        {
            CheckIfHandGrabs();
        }
        
    }

    /// <summary>
    /// Controla si hay objetos que se puedan agarrar
    /// </summary>
    /// <returns></returns>
    public bool canGrabSomething()
    {
        if (overlappingObjects.Count > 0) return true;
        return false;
    }

    /// <summary>
    /// Busca de los objetos disponibles para agarrar, cual está más cerca de esta mano
    /// </summary>
    /// <returns></returns>
    public GameObject closestObjectToGrab()
    {
        float minDistance = 0;
        GameObject selectedObject = null;

        foreach (GameObject Object in overlappingObjects)
        {
            Vector3 handCenter = transform.TransformVector(GetComponent<SphereCollider>().center);
            Vector3 otherCenter = transform.TransformVector(Object.GetComponent<Collider>().bounds.center);

            float distance = Vector3.Distance(handCenter, otherCenter);

            if (distance < minDistance || minDistance == 0)
            {
                minDistance = distance;
                selectedObject = Object;
            }
        }
        return selectedObject;
    }

    /// <summary>
    /// Devuelve si la mano esta agarrando algun objeto
    /// </summary>
    /// <returns></returns>
    public bool isGrabbing()
    {
        return grippedObject != null && isGripping;
    }

    Vector3 lastTrackedPosition;
    Vector3 lastTrackedRotation;
    /// <summary>
    /// Solucion para cuando se pierde el tracking de las manos y se van lejos.
    /// TO DO
    /// </summary>
    public void SetHandsTracking()
    {
        if(!isController)
        {
            if (lastTrackedPosition != Vector3.zero && lastTrackedRotation != Vector3.zero)
            {
                Vector3 posDif = this.transform.position - lastTrackedPosition;
                if (posDif.magnitude > 0.1f) this.transform.position = lastTrackedPosition;
                else this.transform.localPosition = Vector3.zero;
            }
            lastTrackedPosition = this.transform.position;
            lastTrackedRotation = this.transform.rotation.eulerAngles;
        }
    }
    /// <summary>
    /// Devuelve la velocidad a la que se mueve la mano en m/s
    /// </summary>
    /// <returns></returns>
    /// <exception cref="System.NotImplementedException"></exception>
    public Vector3 GetVelocity()
    {
        return velocity;
    }

    #region Trigger
    private void OnTriggerStay(Collider other)
    {
        VRGripInterface gripInterface = other.gameObject.GetComponent<VRGripInterface>();
        if (gripInterface != null)
        {
            if(!overlappingObjects.Contains(other.gameObject) && gripInterface.canBeGrabbed())
            {
                overlappingObjects.Add(other.gameObject);
                Debug.Log("Added a grippable object to the list with name  :  " + other.gameObject.name);
                gripInterface.SetAvailableEffects(true);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        VRGripInterface gripInterface = other.gameObject.GetComponent<VRGripInterface>();
        if (gripInterface != null)
        {
            if (overlappingObjects.Contains(other.gameObject))
            {   
                overlappingObjects.Remove(other.gameObject);
                gripInterface.SetAvailableEffects(false);
                Debug.Log("Removed a grippable object from the list with name  :  " + other.gameObject.name);
            }
        }
    }
    #endregion
    #endregion
    #endregion
}

#if UNITY_EDITOR
[CustomEditor(typeof(GrippingHand), true)]
public class GrippingEditor : Editor
{
    #region PARAMETERS
    [HideInInspector] public GrippingHand grippingHand;
    #endregion

    #region FUNCTIONS
    private void OnSceneGUI()
    {
        
    }


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        grippingHand = target as GrippingHand;

        GUILayout.Label("Es un mando o una mano?", EditorStyles.boldLabel);
        grippingHand.isController = GUILayout.Toggle(grippingHand.isController, "Is a Controller?");

        GUILayout.Space(10);

        if (grippingHand.isController) GUILayout.Label("Es el mando izquierdo o el derecho?", EditorStyles.boldLabel);
        else GUILayout.Label("Es la mano derecha o la izquierda?", EditorStyles.boldLabel);
        grippingHand.handType = (GrippingHand.HandType)EditorGUILayout.EnumPopup("Tipo de mano", grippingHand.handType);

        GUILayout.Space(10);

        GUILayout.Label("El offset del collider que detecta si se agarra", EditorStyles.boldLabel);
        grippingHand.positionOffset = EditorGUILayout.Vector3Field("Collider offset position", grippingHand.positionOffset);

        GUILayout.Space(10);

        GUILayout.Label("El radio del collider que detecta el agarre", EditorStyles.boldLabel);
        grippingHand.colliderRadius = EditorGUILayout.FloatField("Collider Radius", grippingHand.colliderRadius);

        GUILayout.Space(10);

        SerializedProperty player = serializedObject.FindProperty("player");
        EditorGUILayout.PropertyField(player, new GUIContent("El player que contiene esta mano"));

        GUILayout.Space(10);


        if (grippingHand.isController)
        {
            GUILayout.Label("Se oculta el mando al agarrar?", EditorStyles.boldLabel);
            grippingHand.hideOnGrab = GUILayout.Toggle(grippingHand.hideOnGrab, "The controller hides on grab?");

            SerializedProperty xrController = serializedObject.FindProperty("handController");
            EditorGUILayout.PropertyField(xrController, new GUIContent("XRController Component"));
        }
        else
        {
            GUILayout.Label("Se oculta la mano al agarrar?", EditorStyles.boldLabel);
            grippingHand.hideOnGrab = GUILayout.Toggle(grippingHand.hideOnGrab, "The hand hides on grab?");

            GUILayout.Space(10);

            SerializedProperty colliderBone = serializedObject.FindProperty("colliderBone");
            EditorGUILayout.PropertyField(colliderBone, new GUIContent("Collider Parent"));
        }

        if (grippingHand.hideOnGrab)
        {
            SerializedProperty renderer = serializedObject.FindProperty("handRenderer");
            EditorGUILayout.PropertyField(renderer, new GUIContent("Renderer"));

            GUILayout.Space(10);
        }

        serializedObject.ApplyModifiedProperties();
    }
#endregion
}
#endif
