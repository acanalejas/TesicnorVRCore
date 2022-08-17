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
    [HideInInspector]public bool isHandControlled = true;

    [Header("Distancia m�xima del Raycast de deteccion")]
    [HideInInspector] public float maxDistance = 10;

    [Header("Es la mano izquierda?")]
    [HideInInspector] public bool isLeftHand = false;

    [Header("Usa el rayo o solo colision?")]
    [HideInInspector] public bool usesRay = true;

    [Header("LineRenderer que nos da feedback visual de la interaccion")]
    [HideInInspector] public LineRenderer lineRenderer;

    [Header("Color del LineRenderer cuando no apunta a nada")]
    [HideInInspector] public Color nonDetectedColor;

    [Header("Color del LineRenderer cuando detecta algun interactable")]
    [HideInInspector] public Color detectedColor;

    /// <summary>
    /// El objeto con el que esta interactuando la mano
    /// </summary>
    private GameObject interactingObject;

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

    #region Controllers
    [Header("============ CUANDO SE USAN MANDOS =================")][Space(10)]
    [Header("El punto desde el que se lanza el rayo")]
    [HideInInspector] public Transform interactionOrigin;
    [Header("El componente XRController de la mano")]
    [HideInInspector] public XRController handController;
    #endregion

    #region Hands
    [Header("============= CUANDO SE USAN LAS MANOS ===============")][Space(10)]
    [Header("Hueso de la falange del dedo gordo")]
    [HideInInspector] public Transform dedoGordo;

    [Header("Hueso de la falange del dedo indice")]
    [HideInInspector] public Transform indice;

    [Header("Hueso de la falange del dedo corazon")]
    [HideInInspector] public Transform dedoCorazon;

    [Header("Hueso de la falange del dedo anular")]
    [HideInInspector] public Transform dedoAnular;

    [Header("Hueso de la falange del dedo me�ique")]
    [HideInInspector] public Transform dedoMenique;

    [Header("HandPoseDetector encargado de comprobar la mano")]
    [HideInInspector] public HandPoseDetector poseDetector;

    [Header("'Cubo' de deteccion de colision")]
    [HideInInspector] public Vector3 fingerCube;

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
                hand.interactingObject = collision.gameObject;
                hand.interactingObject.GetComponent<VRInteractableInterface>().SetHand(hand.gameObject);
                hand.interactingObject.GetComponent<VRInteractableInterface>().OnClick();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if(other.gameObject.tag == "Interactable")
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
    }
    private void FixedUpdate()
    {
        DetectInteraction();
    }

    /// <summary>
    /// Funcion usada en el Update para poder detectar el input de la interaccion de la mano
    /// </summary>
    public void DetectInteraction()
    {
        if (isHandControlled) DetectInteraction_Hands();
        else DetectInteraction_Controllers();
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
        Vector3 direction = OffsetManager.Instance.currentHandsOffset.GetForward(originalDirection, isLeftHand);
        if (isHandControlled) direction = OffsetManager.Instance.currentHandsOffset.GetForwardFromLeftVector(originalDirection, _origin.up);
        Ray ray = new Ray(origin, direction);

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, origin);
        Vector3 addedDistance = direction.normalized * maxDistance;

        if (Physics.Raycast(ray, out interactionHit, maxDistance))
        {
            if (interactionHit.collider)
            {
                VRInteractableInterface interactable = interactionHit.collider.gameObject.GetComponent<VRInteractableInterface>();
                if (interactable != null)
                {
                    addedDistance = direction.normalized * interactionHit.distance;

                    interactingObject = interactionHit.collider.gameObject;
                    interactable.OnHovered();
                }
                else
                {
                    if (interactingObject != null)
                    {
                        interactingObject.GetComponent<VRInteractableInterface>().ChangeColor(0);
                        //if(interactingObject.GetComponent<VRInteractableInterface>().GetIsClicking()) interactingObject.GetComponent<VRInteractableInterface>().OnRelease();
                        interactingObject = null;
                    }
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

    #region Controllers
    bool lastPressed = false;
    /// <summary>
    /// Detecta la interaccion (incluido input) cuando se usan los mandos
    /// </summary>
    public void DetectInteraction_Controllers()
    {
        if (usesRay) lineRenderer.enabled = true;
        if(usesRay) getRaycastHit(interactionOrigin);

        if (interactingObject != null)
        {
            float pressed = 0;
            if (handController.inputDevice.TryGetFeatureValue(CommonUsages.trigger, out pressed) && pressed > 0.7f)
            {
                lastPressed = true;
                interactingObject.GetComponent<VRInteractableInterface>().SetHand(this.gameObject);
                interactingObject.GetComponent<VRInteractableInterface>().OnClick();
            }
            else if (handController.inputDevice.TryGetFeatureValue(CommonUsages.trigger, out pressed) && pressed < 0.7f && lastPressed)
            {
                lastPressed = false;
                interactingObject.GetComponent<VRInteractableInterface>().OnRelease();
                interactingObject.GetComponent<VRInteractableInterface>().SetHand(null);
            }
            else { lastPressed = false; }
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


        if (poseDetector.Recognize().GestureName == "Click")
        {
            Click();
        }
        else if (handPressed)
        {
            Release();
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
