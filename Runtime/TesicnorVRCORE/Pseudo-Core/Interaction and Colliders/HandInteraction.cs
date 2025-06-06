using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using System;
using UnityEditor;


[RequireComponent(typeof(LineRenderer))]
public class HandInteraction : MonoBehaviour, VRInteractionInterface
{
    #region PARAMETERS
    #region Common
    [Header("Esta controlado por manos?")]
    [SerializeField][HideInInspector]public bool isHandControlled = true;

    [Header("Distancia m�xima del Raycast de deteccion")]
    [SerializeField][HideInInspector] public float maxDistance = 10;

    [Header("Es la mano izquierda?")]
    [SerializeField][HideInInspector] public bool isLeftHand = false;

    [Header("Usa el rayo o solo colision?")]
    [SerializeField][HideInInspector] public bool usesRay = true;

    [Header("LineRenderer que nos da feedback visual de la interaccion")]
    [SerializeField][HideInInspector] public LineRenderer lineRenderer;

    [Header("Color del LineRenderer cuando no apunta a nada")]
    [SerializeField][HideInInspector] public Color nonDetectedColor;

    [Header("Color del LineRenderer cuando detecta algun interactable")]
    [SerializeField][HideInInspector] public Color detectedColor;

    /// <summary>
    /// El objeto con el que esta interactuando la mano
    /// </summary>
    [HideInInspector]public GameObject interactingObject;

    /// <summary>
    /// Resultado del Raycast que se usa para detectar las interacciones
    /// </summary>
    private RaycastHit interactionHit;

    /// <summary>
    /// Accion lanzada cuando se detecta una interaccion
    /// int = 0 --- Release</param>
    /// int = 1 --- Hover
    /// int = 2 --- Press
    /// </summary>
    private Action<int> onInteractionDetected;
    #endregion

    #region AR

    [Header("El evento que se lanza al spawnear un objeto")]
    [SerializeField]
    [HideInInspector] public UnityEngine.Events.UnityEvent OnARObjectSpawned;

    //El gestor de raycast en AR
    protected AR_PointRay ARPR;

    private bool ARInput = false;

    private bool ARPreview = false;

    private GameObject ARPreviewObject = null;

    private MeshFilter ARPreviewMF;

    private Mesh ARPreviewMesh = null;

    #endregion

    #region Controllers
    [Header("============ CUANDO SE USAN MANDOS =================")][Space(10)]
    [Header("El punto desde el que se lanza el rayo")]
    [SerializeField][HideInInspector] public Transform interactionOrigin;
    [Header("El componente XRController de la mano")]
    [SerializeField][HideInInspector] public XRController handController;
    #endregion

    #region Hands
    [Header("============= CUANDO SE USAN LAS MANOS ===============")][Space(10)]
    [Header("Hueso de la falange del dedo gordo")]
    [SerializeField][HideInInspector] public Transform dedoGordo;

    [Header("Hueso de la falange del dedo indice")]
    [SerializeField][HideInInspector] public Transform indice;

    [Header("Hueso de la falange del dedo corazon")]
    [SerializeField][HideInInspector] public Transform dedoCorazon;

    [Header("Hueso de la falange del dedo anular")]
    [SerializeField][HideInInspector] public Transform dedoAnular;

    [Header("Hueso de la falange del dedo me�ique")]
    [SerializeField][HideInInspector] public Transform dedoMenique;

    [Header("HandPoseDetector encargado de comprobar la mano")]
    [SerializeField][HideInInspector] public HandPoseDetector poseDetector;

    [Header("'Cubo' de deteccion de colision")]
    [SerializeField][HideInInspector] public Vector3 fingerCube;

    private BoxCollider meniqueCollider;
    private BoxCollider anularCollider;
    private BoxCollider corazonCollider;
    private BoxCollider indiceCollider;
    private BoxCollider pulgarCollider;

    /// <summary>
    /// Componente que se añadira a cada dedo en runtime para detectar si se intenta pulsar fisicamente un boton
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(Rigidbody))]
    class dedo: MonoBehaviour
    {
        /// <summary>
        /// Collider que se encargara de detectar la colision con los objetos interactuables
        /// </summary>
        public BoxCollider interactionCollider;
        /// <summary>
        /// Rigidbody que tenemos que añadir al GO para poder detectar colisiones
        /// </summary>
        public Rigidbody rb;
        /// <summary>
        /// El componente de interaccion de la mano a la que pertenece este dedo
        /// </summary>
        public HandInteraction hand;

        /// <summary>
        /// Setea las variables necesarias para que la clase funcione sin problemas
        /// </summary>
        /// <param name="collider"></param>
        /// <param name="_rb"></param>
        /// <param name="_hand"></param>
        public void setVariables(BoxCollider collider, Rigidbody _rb, HandInteraction _hand)
        {
            interactionCollider = collider;
            interactionCollider.center = Vector3.zero;
            interactionCollider.isTrigger = true;
            rb = _rb;
            if (!rb) rb = GetComponent<Rigidbody>();
            rb.useGravity = false;
            hand = _hand;
        }

        private void Awake()
        {
            interactionCollider = GetComponent<BoxCollider>();
            interactionCollider.isTrigger = true;
        }
        /*private void Start()
        {
            if (!rb) rb = GetComponent<Rigidbody>();
            rb.useGravity = false;
            interactionCollider.center = Vector3.zero;
            interactionCollider.isTrigger = true;
        }*/

        private void OnTriggerEnter(Collider collision)
        {
            VRInteractableInterface interactable = collision.GetComponent<VRInteractableInterface>();
            if(interactable != null)
            {
                if (!interactable.GetIsTouchable()) return;
                hand.interactingObject = collision.gameObject;
                hand.interactingObject.GetComponent<VRInteractableInterface>().SetHand(hand.gameObject);
                hand.interactingObject.GetComponent<VRInteractableInterface>().OnClick();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            VRInteractableInterface interactable = other.GetComponent<VRInteractableInterface> ();
            if(interactable != null)
            {
                if (hand.interactingObject != null) hand.interactingObject.GetComponent<VRInteractableInterface>().OnRelease();
                hand.interactingObject = null;
            }
        }
    }

    #endregion
    #endregion

    #region FUNCTIONS
    private void Start()
    {
        if (isHandControlled) SetupFingers();
        else SetupControllerCollider();
        if (!lineRenderer) lineRenderer = GetComponent<LineRenderer>();
        if (!usesRay) lineRenderer.enabled = false;
        if (!ARPreviewObject) ARPreviewObject = GameObject.FindGameObjectWithTag("MeshPreview");
        if (!ARPreviewMF && ARPreviewObject) ARPreviewMF = ARPreviewObject.GetComponent<MeshFilter>();

        SetupARInput();
    }

    #region AR

    private void SetupARInput()
    {
        if (ARPR) return;

        ARPR = FindFirstObjectByType<AR_PointRay>();

        if (!ARPR)
        {
            GameObject GO_ARPR = new GameObject("AR Point Ray", typeof(AR_PointRay));
            ARPR = GO_ARPR.GetComponent<AR_PointRay>();
        }

        if (!ARPR) ARPR = FindFirstObjectByType<AR_PointRay>();
    }


    public void ToggleARInput(bool _value)
    {
        ARInput = _value;
    }

    public void DetectARInput()
    {
        if (!ARInput) return;
        if(handController.inputDevice.TryGetFeatureValue(CommonUsages.trigger, out pressed) && pressed > 0.7f)
        {
            ARSpawnObject();
            ToggleARInput(false);
        }
    }

    public void ToggleARPreview(bool _value) { ARPreview = _value; if(ARPreviewObject) ARPreviewObject.SetActive(_value); }

    public void ShowARPreview()
    {
        if (!ARPreview || !ARPreviewMF) return;

        ARPreviewMF.mesh = AR_PointRay.spawnObject.previewMesh;
        ARPreviewObject.transform.position = GetARRaycastPosition();
        ARPreviewObject.transform.forward = (this.transform.position - ARPreviewObject.transform.position).normalized;
    }

    public void SetARPreviewMesh(Mesh _mesh) { ARPreviewMesh = _mesh; }

    public static bool canSpawn = true;
    public Vector3 GetARRaycastPosition()
    {
        XRRayInteractor interactor = GetComponent<XRRayInteractor>();
        XRInteractorLineVisual visual = GetComponent<XRInteractorLineVisual>();

        if (!interactor)
            return ARPR.ARRaycast(this.interactionOrigin.transform.forward, this.interactionOrigin.gameObject);

        else
        {
            RaycastHit hit;
            if (interactor.TryGetCurrent3DRaycastHit(out hit))
            {
                return hit.point;
            }
            else if (visual)
            {
                return visual.lineLength * this.interactionOrigin.transform.forward + this.interactionOrigin.transform.position;
            }
        }

        return this.interactionOrigin.position;
    }
    public void ARSpawnObject()
    {
        // XRRayInteractor interactor = GetComponent<XRRayInteractor>();
        // XRInteractorLineVisual visual = GetComponent<XRInteractorLineVisual>();
        //
        // GameObject result = null;
        //
        // if(!interactor)
        // result = ARPR.ARSpawnObject(this.interactionOrigin.transform.forward, this.interactionOrigin.gameObject);
        //
        // else
        // {
        //     RaycastHit hit;
        //     if (interactor.TryGetCurrent3DRaycastHit(out hit))
        //     {
        //         Vector3 position = hit.point;
        //         Quaternion rotation = Quaternion.LookRotation(Vector3.forward, hit.normal);
        //         result = ARPR.ARSpawnObject(position, Quaternion.identity);
        //     }
        //     else if (visual)
        //     {
        //         Vector3 position = visual.lineLength * this.interactionOrigin.transform.forward + this.interactionOrigin.transform.position;
        //         result = ARPR.ARSpawnObject(position, Quaternion.identity);
        //     }
        // }
        GameObject result = null;
        result = ARPR.ARSpawnObject(this.GetARRaycastPosition());
        result.transform.forward = (this.transform.position - result.transform.position).normalized;
        if(result != null)
         OnARObjectSpawned.Invoke();
    }

    private Vector3 GetARRaycastPoint()
    {
        return ARPR.ARRaycast(this.interactionOrigin.transform.forward, this.interactionOrigin.gameObject);
    }

    #endregion

    private void Update()
    {
        DetectInteraction();
        DetectARInput();
        ShowARPreview();
    }

    /// <summary>
    /// Funcion usada en el Update para poder detectar el input de la interaccion de la mano
    /// </summary>
    public void DetectInteraction()
    {
        if (isHandControlled) DetectInteraction_Hands();
        else DetectInteraction_Controllers();
    }

    public void ToggleRay(bool Value)
    {
        this.usesRay = Value;
        lineRenderer.enabled = Value;
    }

    public void EnableRay()
    {
        ToggleRay(true);
    }

    public void DisableRay()
    {
        ToggleRay(false);
    }

    public void WaitAndEnableRay(float _time)
    {
        Invoke(nameof(EnableRay), _time);
    }

    public void SetLineRendererColor(bool detected) 
    {
        Color color = Color.black;

        if (detected) color = detectedColor;
        else color = nonDetectedColor;

        if(lineRenderer.sharedMaterial) lineRenderer.sharedMaterial.color = color;
    }
    /// <summary>
    /// Devuelve el valor del choque del raycast que se usa para detectar la interaccion
    /// </summary>
    /// <returns></returns>
    public RaycastHit getRaycastHit(Transform _origin)
    {
        Vector3 origin = _origin.position;
        Vector3 originalDirection = _origin.forward;
        Vector3 direction = Vector3.zero;
        if (OffsetManager.Instance)
        {
            direction = OffsetManager.Instance.currentHandsOffset.GetForward(originalDirection, isLeftHand);
            //if (isHandControlled) direction = OffsetManager.Instance.currentHandsOffset.GetForwardFromLeftVector(originalDirection, _origin.up);
        }
        else direction = originalDirection;
        Ray ray = new Ray(origin, direction);

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, origin);
        Vector3 addedDistance = direction.normalized * maxDistance;

        if (Physics.Raycast(ray, out interactionHit, maxDistance, LayerMask.GetMask("UI")))
        {
            if (interactionHit.collider)
            {
                VRInteractableInterface interactable = interactionHit.collider.gameObject.GetComponent<VRInteractableInterface>();
                if (interactable != null && interactionHit.collider.gameObject.activeSelf)
                {
                    addedDistance = direction.normalized * interactionHit.distance;

                    if (interactingObject != null && interactingObject != interactionHit.collider.gameObject) interactingObject.GetComponent<VRInteractableInterface>().OnExitHover();
                    interactingObject = interactionHit.collider.gameObject;
                    interactable.SetHand(this.gameObject);
                    interactable.OnHovered();
                }
                
                else
                {
                    if (interactingObject != null)
                    {
                        interactingObject.GetComponent<VRInteractableInterface>().ChangeColor(0);
                        interactingObject.GetComponent<VRInteractableInterface>().OnExitHover();
                        //if(interactingObject.GetComponent<VRInteractableInterface>().GetIsClicking()) interactingObject.GetComponent<VRInteractableInterface>().OnRelease();
                        interactingObject = null;
                    }

                    RaycastHit hit;
                    if (ARPR && this.GetComponent<XRRayInteractor>() && this.GetComponent<XRRayInteractor>().TryGetCurrent3DRaycastHit(out hit)) addedDistance = hit.point - origin;
                }
            }
            /*else
            {
                interactingObject = null;
            }*/
        }
        else if (interactingObject != null)
        {
            interactingObject.GetComponent<VRInteractableInterface>().ChangeColor(0);
            interactingObject.GetComponent<VRInteractableInterface>().OnExitHover();
            interactingObject = null;
        }
        lineRenderer.SetPosition(1, origin + addedDistance);
        SetLineRendererColor(interactingObject != null);
        return interactionHit;
    }

    public Transform GetOrigin()
    {
        if (isHandControlled) return indice;
        else return interactionOrigin;
    }

    public void SetOrigin(Transform transform)
    {
        interactionOrigin = transform;
    }

    #region Controllers
    bool lastPressed = false;
    float pressed;
    GameObject pressedObject;
    /// <summary>
    /// Detecta la interaccion (incluido input) cuando se usan los mandos
    /// </summary>
    public void DetectInteraction_Controllers()
    {
        if (usesRay) lineRenderer.enabled = true;
        if(usesRay) getRaycastHit(interactionOrigin);

        if (interactingObject != null)
        {
            if (handController.inputDevice.TryGetFeatureValue(CommonUsages.trigger, out pressed) && pressed > 0.7f && !lastPressed)
            {
                interactingObject.GetComponent<VRInteractableInterface>().SetHand(this.gameObject);
                //if(!lastPressed)
                interactingObject.GetComponent<VRInteractableInterface>().OnClick();
                pressedObject = interactingObject;
                lastPressed = true;
            }
            else if (handController.inputDevice.TryGetFeatureValue(CommonUsages.trigger, out pressed) && pressed < 0.3f && lastPressed)
            {
                lastPressed = false;
                if(interactingObject == pressedObject)
                {
                    interactingObject.GetComponent<VRInteractableInterface>().OnRelease();
                    interactingObject.GetComponent<VRInteractableInterface>().SetHand(null);
                }
                else pressedObject.GetComponent<VRInteractableInterface>().SetHand(null);
            }
        }
    }

    void SetupControllerCollider()
    {
        interactionOrigin.gameObject.AddComponent<dedo>();

        interactionOrigin.GetComponent<BoxCollider>().size = fingerCube;
        interactionOrigin.GetComponent<BoxCollider>().center = interactionOrigin.position;

        interactionOrigin.GetComponent<dedo>().setVariables(interactionOrigin.GetComponent<BoxCollider>(), interactionOrigin.GetComponent<Rigidbody>(), this);
    }
    #endregion

    #region Hands

    bool handPressed = false;
    /// <summary>
    /// Detecta la interaccion (inlcuido input) cuando se usan las manos
    /// </summary>
    public void DetectInteraction_Hands()
    {
        if (usesRay) lineRenderer.enabled = true;
        if(usesRay)getRaycastHit(indice);

        if (poseDetector)
        {
            if (poseDetector.Recognize().GestureName == "Click")
            {
                Click();
            }
            else if (handPressed)
            {
                Release();
            }
        }
    }
    
    /// <summary>
    /// Setea todo lo necesario en los dedos para ser capaces de interactuar fisicamente con los botones
    /// </summary>
    public void SetupFingers()
    {
        meniqueCollider =       dedoMenique.gameObject.AddComponent<BoxCollider>();
        anularCollider =        dedoAnular.gameObject.AddComponent<BoxCollider>();
        corazonCollider =       dedoCorazon.gameObject.AddComponent<BoxCollider>();
        indiceCollider =        indice.gameObject.AddComponent<BoxCollider>();
        pulgarCollider =        dedoGordo.gameObject.AddComponent<BoxCollider>();

        meniqueCollider.size =    fingerCube;
        anularCollider.size =     fingerCube;
        corazonCollider.size =    fingerCube;
        indiceCollider.size =     fingerCube;
        pulgarCollider.size =     fingerCube;

        meniqueCollider.center =     dedoMenique.position;
        anularCollider.center =      dedoAnular.position;
        corazonCollider.center =     dedoCorazon.position;
        indiceCollider.center =      indice.position;
        pulgarCollider.center =      dedoGordo.position;

        dedo menique = dedoMenique.gameObject.AddComponent<dedo>();
        dedo anular = dedoAnular.gameObject.AddComponent<dedo>();
        dedo corazon = dedoCorazon.gameObject.AddComponent<dedo>();
        dedo _indice = indice.gameObject.AddComponent<dedo>();
        dedo gordo = dedoGordo.gameObject.AddComponent<dedo>();

        menique.setVariables(meniqueCollider, meniqueCollider.gameObject.AddComponent<Rigidbody>(), this);
        anular.setVariables(anularCollider, dedoAnular.gameObject.AddComponent<Rigidbody>(), this);
        corazon.setVariables(corazonCollider, corazon.gameObject.AddComponent<Rigidbody>(), this);
        _indice.setVariables(indiceCollider, indice.gameObject.AddComponent<Rigidbody>(), this);
        gordo.setVariables(pulgarCollider, gordo.gameObject.AddComponent<Rigidbody>(), this);

    }

    /// <summary>
    /// Comprueba si cualquier dedo esta pulsando algun boton
    /// </summary>
    /// <param name="distanceFromButton"></param>
    /// <returns></returns>
    bool isFingerPressing(float distanceFromButton)
    {
        if (interactingObject.GetComponent<MeshFilter>() || interactingObject.GetComponent<SkinnedMeshRenderer>())
        {
            Collider objectCollider = interactingObject.GetComponent<Collider>();
            if (isFingerPressing3DObject(meniqueCollider, objectCollider)) return true;
            if (isFingerPressing3DObject(anularCollider, objectCollider)) return true;
            if (isFingerPressing3DObject(corazonCollider, objectCollider)) return true;
            if (isFingerPressing3DObject(indiceCollider, objectCollider)) return true;
            if (isFingerPressing3DObject(pulgarCollider, objectCollider)) return true;
        }

        return false;
    }

    public void Click()
    {
        if (interactingObject)
        {
            interactingObject.GetComponent<VRInteractableInterface>().OnClick();
            lastPressed = true;
        }
    }
    public void Release()
    {
        if (interactingObject)
        {
            lastPressed = false;
            interactingObject.GetComponent<VRInteractableInterface>().OnRelease();
        }
    }
    /// <summary>
    /// El dedo que estamos comprobando esta pulsando algun objeto interactuable?
    /// </summary>
    /// <param name="fingerCollider">Collider del dedo que queremos comprobar</param>
    /// <param name="objectCollider">Collider del objeto con el que queremos interactuar</param>
    /// <returns></returns>
    public bool isFingerPressing3DObject(BoxCollider fingerCollider, Collider objectCollider)
    {
        bool result = false;

        Vector3 pointToCheck = fingerCollider.ClosestPoint(objectCollider.transform.position);

        Vector3 size = objectCollider.bounds.size/2;
        //Aplicamos la escala al tamaño para evitar errores de calculo
        size = new Vector3(size.x * objectCollider.transform.lossyScale.x, size.y * objectCollider.transform.lossyScale.y, size.z * objectCollider.transform.lossyScale.z);

        Vector3 center = objectCollider.transform.position;

        if (pointToCheck.x > center.x - size.x && pointToCheck.x < center.x + size.x &&
            pointToCheck.y > center.y - size.y && pointToCheck.y < center.y + size.y &&
            pointToCheck.z > center.z - size.z && pointToCheck.z < center.z + size.z) result = true;

        return result;
    }

    public bool isFingerPressing3DObject()
    {
        throw new NotImplementedException();
    }

    public RaycastHit GetRaycastHit(Transform _origin)
    {
        return getRaycastHit(_origin);
    }
    #endregion
    #endregion
}

#if UNITY_EDITOR
[CustomEditor(typeof(HandInteraction), true)]
[CanEditMultipleObjects]
public class InteractionEditor : Editor
{
    [HideInInspector] public HandInteraction handInteraction;


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        handInteraction = target as HandInteraction;

        GUILayout.Label("Se usa una mano o un mando?", EditorStyles.boldLabel);
        handInteraction.isHandControlled = GUILayout.Toggle(handInteraction.isHandControlled, "Is Hand Controlled?");

        GUILayout.Space(10);

        string label = "Es la mano izquierda?";
        if (!handInteraction.isHandControlled) label = "Es el mando izquierdo?";
        GUILayout.Label(label, EditorStyles.boldLabel);
        handInteraction.isLeftHand = GUILayout.Toggle(handInteraction.isLeftHand, "Is left hand?");

        GUILayout.Space(10);

        GUILayout.Label("Usa un rayo o solo colisión?", EditorStyles.boldLabel);
        handInteraction.usesRay = GUILayout.Toggle(handInteraction.usesRay, "Uses ray?");

        GUILayout.Space(10);
        GUILayout.Label("El tamaño del collider de interacción", EditorStyles.boldLabel);
        handInteraction.fingerCube = EditorGUILayout.Vector3Field("Collider Size", handInteraction.fingerCube);


        GUILayout.Space(10);

        GUILayout.Label("PARA AR", EditorStyles.boldLabel);

        GUILayout.Space(10);

        SerializedProperty OnARObjectSpawned = serializedObject.FindProperty("OnARObjectSpawned");
        EditorGUILayout.PropertyField(OnARObjectSpawned);


        GUILayout.Space(10);

        if (handInteraction.usesRay)
        {
            GUILayout.Label("La distancia a la que alcanza el rayo", EditorStyles.boldLabel);
            handInteraction.maxDistance = EditorGUILayout.FloatField(handInteraction.maxDistance, "Max ray distance");

            GUILayout.Space(10);

            SerializedProperty lineRenderer = serializedObject.FindProperty("lineRenderer");
            EditorGUILayout.PropertyField(lineRenderer, new GUIContent("Line Renderer"));

            GUILayout.Space(10);

            GUILayout.Label("El color cuando no se detecta ningun interactuable", EditorStyles.boldLabel);
            handInteraction.nonDetectedColor = EditorGUILayout.ColorField(handInteraction.nonDetectedColor);

            GUILayout.Space(10);

            GUILayout.Label("El color cuando se detecta algun interactuable", EditorStyles.boldLabel);
            handInteraction.detectedColor = EditorGUILayout.ColorField(handInteraction.detectedColor);

            GUILayout.Space(10);
        }

        if (!handInteraction.isHandControlled)
        {
            SerializedProperty interactionOrigin = serializedObject.FindProperty("interactionOrigin");
            EditorGUILayout.PropertyField(interactionOrigin, new GUIContent("Interaction Origin"));

            GUILayout.Space(10);

            SerializedProperty handController = serializedObject.FindProperty("handController");
            EditorGUILayout.PropertyField(handController, new GUIContent("XRController Component"));

            GUILayout.Space(10);
        }

        else
        {
            SerializedProperty dedoGordo = serializedObject.FindProperty("dedoGordo");
            EditorGUILayout.PropertyField(dedoGordo, new GUIContent("Falange pulgar"));

            SerializedProperty dedoIndice = serializedObject.FindProperty("indice");
            EditorGUILayout.PropertyField(dedoIndice, new GUIContent("Falange indice"));

            SerializedProperty dedoCorazon = serializedObject.FindProperty("dedoCorazon");
            EditorGUILayout.PropertyField(dedoCorazon, new GUIContent("Falange corazon"));

            SerializedProperty dedoAnular = serializedObject.FindProperty("dedoAnular");
            EditorGUILayout.PropertyField(dedoAnular, new GUIContent("Falange anular"));

            SerializedProperty dedoMeñique = serializedObject.FindProperty("dedoMenique");
            EditorGUILayout.PropertyField(dedoMeñique, new GUIContent("Falange meñique"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
