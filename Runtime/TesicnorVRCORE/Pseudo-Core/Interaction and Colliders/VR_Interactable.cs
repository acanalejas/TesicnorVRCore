using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEditor;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
#if UNITY_EDITOR
[RequireComponent(typeof(InteractableEditor))]
#endif
public class VR_Interactable : MonoBehaviour, VRInteractableInterface
{
    #region PARAMETERS
    [Header("Es un objeto 3D o es UI?")]
    [HideInInspector]public bool is3DObject = false;

    [Header("Usa el rallo o solo colision?")]
    [HideInInspector] public bool usesRay = true;

    [Header("Se puede tocar el boton?")]
    [HideInInspector] public bool usesCollision = false;

    [Header("Solo rellenar si es un objeto de UI")]
    [HideInInspector] public RectTransform canvasTransform;

    [Header("Cambia de color?")]
    [HideInInspector] public bool changesColor = true;

    private bool isHovered = false;
    private bool isClicking = false;

    [Header("Color en estado normal del objeto")]
    [HideInInspector] public Color NormalColor = Color.white;
    [Header("Color en Hover del objeto")]
    [HideInInspector] public Color HoverColor = Color.white;
    [Header("Color cuando el objeto esta pulsado")]
    [HideInInspector] public Color PressedColor = Color.white;

    [Header("Evento que se lanza al hacer click sobre el objeto")]
    [HideInInspector] public UnityEvent onClick;
    [Header("Evento que se lanza cuando el 'puntero' se posiciona sobre el objeto")]
    [HideInInspector] public UnityEvent onHover;
    [Header("Evento que se lanza cuando el puntero deja de estar posicionado sobre el objeto")]
    [HideInInspector] public UnityEvent onHoverExit;
    [Header("Evento que se lanza cuando se deja de hacer click sobre el objeto")]
    [HideInInspector]public UnityEvent onRelease;

    [Header("Si el boton puede usarse o no")]
    [HideInInspector] public bool canBePressed = true;

    [Header("===================== SOLO SI TIENE QUE SEGUIR AL COLLIDER ======================")]
    [Space(10)]
    [Header("Sigue al collider cuando se pulsa?")]
    [HideInInspector] public bool hasToFollow = false;

    [Header("La posición normal del objeto")]
    [HideInInspector] public Vector3 normalPosition;

    [Header("La posición hundida del objeto")]
    [HideInInspector] public Vector3 pressedPosition;

    [Header("El size del collider por si da fallos")]
    [HideInInspector] public Vector3 colliderSize;

    [Header("Se deberia modificar el collider?")] [HideInInspector]
    public bool shouldModifyCollider = true;

    [HideInInspector] public bool bHasBeenClicked = false;

    private BoxCollider boxCollider;

    protected GameObject hand;

    protected HandInteraction[] hands;
    #endregion

    #region FUNCTIONS

    public virtual void Awake()
    {
        //this.gameObject.tag = "Interactable";

        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Rigidbody>().isKinematic = true;

        if (!is3DObject) SetupUICollider();

        hands = GameObject.FindObjectsOfType<HandInteraction>();

        this.gameObject.layer = 5;
    }

    public virtual void Update()
    {
        if (!this.isHovered && !this.isClicking) ChangeColor(0);

        if (!isAnyHandInteractingWithThis()) ChangeColor(0);

        if (colliderSize != Vector3.zero && shouldModifyCollider) boxCollider.size = colliderSize;
    }

    bool isAnyHandInteractingWithThis()
    {
        bool interacting = false;

        foreach(var hand in hands)
        {
            if (hand.interactingObject == this.gameObject) interacting = true;
        }

        return interacting;
    }

    public void SetupUICollider()
    {
        if (!shouldModifyCollider) return;
        RectTransform rectTransform = this.GetComponent<RectTransform>();

        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;

        boxCollider = GetComponent<BoxCollider>();

        boxCollider.size = new Vector3(width, height, 0.1f);
    }

    Vector3 clickHandPosition;
    /// <summary>
    /// Cuando se hace click sobre este objeto
    /// </summary>
    public virtual void OnClick()
    {
        if (canBePressed)
        {
            onClick?.Invoke();
            ChangeColor(2);
            if(!isClicking && this.isActiveAndEnabled) StartCoroutine("follow");
            isClicking = true;
        }
        bHasBeenClicked = true;
    }

    /// <summary>
    /// Cuando el "puntero" esta sobre este objeto
    /// </summary>
    public virtual void OnHovered()
    {
        if (canBePressed)
        {
            onHover?.Invoke();
            ChangeColor(1);
            isHovered = true;
        }
    }

    public virtual void OnExitHover()
    {
        if (canBePressed)
        {
            onHoverExit?.Invoke();
            isHovered = false;
        }
    }
    /// <summary>
    /// Cuando dejamos de hacer click sobre el objeto
    /// </summary>
    public virtual void OnRelease()
    {
        if (canBePressed)
        {
            onRelease?.Invoke();
            ChangeColor(0);
            isClicking = false;
            StopCoroutine("follow");
            if (hasToFollow) this.transform.localPosition = normalPosition;
        }
    }

    /// <summary>
    /// Getter para el bool isClicking
    /// por mayor seguridad
    /// </summary>
    /// <returns></returns>
    public bool GetIsClicking()
    {
        return isClicking;
    }
    /// <summary>
    /// Getter para el bool isHovered
    /// por mayor seguridad
    /// </summary>
    /// <returns></returns>
    public bool GetIsHovered()
    {
        return isHovered;
    }

    /// <summary>
    /// Setea la mano que hace click
    /// </summary>
    /// <param name="_hand"></param>
    public void SetHand(GameObject _hand)
    {
        hand = _hand;
    }

    /// <summary>
    /// Devuelve la mano que ha hecho click
    /// </summary>
    /// <returns></returns>
    public GameObject GetHand()
    {
        return hand;
    }

    /// <summary>
    /// Cambia de color el objeto dependiendo de en que estado se encuentre (idle, hover, press)
    /// </summary>
    /// <param name="colorState">estado en el que se encuentra el objeto</param>
    public void ChangeColor(int colorState)
    {

        if (!changesColor) return;
        Color newColor = Color.white;

        switch (colorState)
        {
            //Para cuando esta en reposo
            case 0:
                newColor = NormalColor;
                break;
            //Cuando entra en hover
            case 1:
                newColor = HoverColor;
                break;
            //Cuando se pulsa
            case 2:
                newColor = PressedColor;
                break;
        }
        if (is3DObject)
        {
            Material material = new Material(GetComponent<Renderer>().material);
            material.color = newColor;

            GetComponent<Renderer>().material = material;
            
        }
        else
        {
            Image image = GetComponent<Image>();
            image.color = newColor;
        }
    }

    public void SetCanBePressed(bool value)
    {
        canBePressed = value;
    }

    public bool GetCanBePressed()
    {
        return canBePressed;
    }

    // =================== SOLO SI SIGUE AL COLLIDER =======================

    WaitForEndOfFrame frame = new WaitForEndOfFrame();
    private IEnumerator follow()
    {
        SetPath();
        clickHandPosition = hand.transform.position;
        while (hasToFollow)
        {
            this.transform.localPosition = currentPoint();
            yield return frame;
        }
    }

    List<Vector3> pathPoints = new List<Vector3>();
    private void SetPath()
    {
        Vector3 direction = pressedPosition - normalPosition;

        int pointsCount = 10;
        for(int i = 0; i < pointsCount; i++)
        {
            pathPoints.Add((direction / pointsCount) * i + normalPosition);
        }
    }

    int _currentPoint = 0;
    private Vector3 currentPoint()
    {
        Vector3 distanceSincePressed = hand.transform.position - clickHandPosition;

        distanceSincePressed = this.transform.InverseTransformVector(distanceSincePressed);

        int previousPoint =     _currentPoint - 1;
        int nextPoint =         _currentPoint + 1;

        if (_currentPoint <= 0)                                 previousPoint = 0;
        else if (_currentPoint >= pathPoints.Count - 1)         nextPoint = pathPoints.Count - 1;

        float previousDistance =    Vector3.Distance(normalPosition + distanceSincePressed, pathPoints[previousPoint]);
        float currentDistance =     Vector3.Distance(normalPosition + distanceSincePressed, pathPoints[_currentPoint]);
        float nextDistance =        Vector3.Distance(normalPosition + distanceSincePressed, pathPoints[nextPoint]);

        if (previousDistance < currentDistance && previousDistance < nextDistance) { _currentPoint = previousPoint; }
        if (nextDistance < currentDistance && nextDistance < previousDistance) { _currentPoint = nextPoint; }

        return pathPoints[_currentPoint];
       
    }

    public bool GetIsTouchable()
    {
        return usesCollision;
    }
    #endregion
}

#if UNITY_EDITOR
[CustomEditor(typeof(VR_Interactable), true)]
[CanEditMultipleObjects]
public class InteractableEditor : Editor
{
    VR_Interactable interactable;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        interactable = target as VR_Interactable;

        GUILayout.Label("Es un objeto 3D?", EditorStyles.boldLabel);
        interactable.is3DObject = GUILayout.Toggle(interactable.is3DObject, "Is 3D Object?");

        GUILayout.Space(10);

        GUILayout.Label("Se puede presionar con el rayo?", EditorStyles.boldLabel);
        interactable.usesRay = GUILayout.Toggle(interactable.usesRay, "Uses ray?");

        GUILayout.Space(10);

        GUILayout.Label("Se puede presionar con colisión?", EditorStyles.boldLabel);
        interactable.usesCollision = GUILayout.Toggle(interactable.usesCollision, "Uses collision?");

        GUILayout.Space(10);

        GUILayout.Label("Cambia de color?", EditorStyles.boldLabel);
        interactable.changesColor = GUILayout.Toggle(interactable.changesColor, "Changes color?");

        GUILayout.Space(10);

        GUILayout.Label("Al presionarlo con collider, sigue a la mano?", EditorStyles.boldLabel);
        interactable.hasToFollow = GUILayout.Toggle(interactable.hasToFollow, "Has to follow?");

        GUILayout.Space(10);

        if (interactable.changesColor)
        {
            GUILayout.Label("El color normal del objeto", EditorStyles.boldLabel);
            interactable.NormalColor = EditorGUILayout.ColorField(interactable.NormalColor);

            GUILayout.Space(10);

            GUILayout.Label("El color cuando el puntero está sobre el objeto", EditorStyles.boldLabel);
            interactable.HoverColor = EditorGUILayout.ColorField(interactable.HoverColor);

            GUILayout.Space(10);

            GUILayout.Label("El color cuando el objeto está siendo presionado", EditorStyles.boldLabel);
            interactable.PressedColor = EditorGUILayout.ColorField(interactable.PressedColor);

            GUILayout.Space(10);
        }

        if (interactable.hasToFollow)
        {
            GUILayout.Label("La posición local del botón sin estar presionado", EditorStyles.boldLabel);
            interactable.normalPosition = EditorGUILayout.Vector3Field("Posicion sin presionar", interactable.normalPosition);

            GUILayout.Space(10);

            GUILayout.Label("La posición local máxima cuando el objeto está presionado", EditorStyles.boldLabel);
            interactable.pressedPosition = EditorGUILayout.Vector3Field("Posicion presionada", interactable.pressedPosition);

            GUILayout.Space(10);
        }

        SerializedProperty onClick = serializedObject.FindProperty("onClick");
        EditorGUILayout.PropertyField(onClick, new GUIContent("On Click"));

        SerializedProperty onHover = serializedObject.FindProperty("onHover");
        EditorGUILayout.PropertyField(onHover, new GUIContent("On Hover"));

        SerializedProperty onHoverExit = serializedObject.FindProperty("onHoverExit");
        EditorGUILayout.PropertyField(onHoverExit, new GUIContent("On Hover Exit"));

        SerializedProperty onRelease = serializedObject.FindProperty("onRelease");
        EditorGUILayout.PropertyField(onRelease, new GUIContent("On Release"));

        GUILayout.Space(10);

        GUILayout.Label("El tamaño del collider, solo rellenar si da fallo y no hay mas opcion", EditorStyles.boldLabel);
        interactable.colliderSize = EditorGUILayout.Vector3Field("Collider Size", interactable.colliderSize);
        
        GUILayout.Space(10);
        
        GUILayout.Label("Se debería modificar el collider en runtime?");
        interactable.shouldModifyCollider =
            GUILayout.Toggle(interactable.shouldModifyCollider, "Should modify collider");
        

        serializedObject.ApplyModifiedProperties();
    }
}
#endif


