//#define MODOT
//Descomenta esto para cambiar a modo trigonometria en vez de lanzamiento horizontal
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LocomotionComponent : MonoBehaviour
{
    #region PARAMETERS
    public enum ControllerTypes { Movement, Rotation}
    [Header("Este mando se usa para moverse o girar")]
    [SerializeField] public ControllerTypes controllerType = ControllerTypes.Movement;
    
    private enum MovementType {Teleport, Locomotion}

    [Header("Que tipo de movimiento usa?")] [HideInInspector]
    [SerializeField] private MovementType movementType = MovementType.Locomotion;

    [Header("La tag que se usa para detectar el suelo")] [HideInInspector] [SerializeField]
    private string FloorTag = "Floor";

    [Header("El mando del que se trata")]
    [SerializeField] private GrippingHand.HandType handType = GrippingHand.HandType.right;

    [Header("La velocidad a la que se mueve el personaje")]
    [SerializeField] private float playerSpeed = 2;

    [Header("La velocidad a la que gira el personaje")]
    [SerializeField] private float playerRotationSpeed = 4;

    [Header("La camara que se va a usar")]
    [SerializeField] private Transform camera;

    [Header("El player al que queremos mover")]
    [SerializeField] private Transform player;

    private CoreInteraction coreInteraction;

    bool joystickButton = false;

    #region Teleport Visuals

    [Header("La altura del arco")] [SerializeField] [HideInInspector]
    private float arcHeight = 2f;

    [Header("La cantidad de puntos del arco")] [SerializeField] [HideInInspector]
    private int resolution = 20;

    [Header("El lineRenderer encargado de hacer la preview")] [SerializeField] [HideInInspector]
    private LineRenderer teleportRenderer;

    #endregion

    //private XRController controller;
    #endregion

    #region FUNCTIONS
    private void Awake()
    {
        //controller = GetComponent<XRController>();
        //if (!controller) controller = gameObject.AddComponent<XRController>();
        
        CreateTeleportVisuals();
    }

    private void CreateTeleportVisuals()
    {
        if (teleportRenderer != null) return;

        GameObject lineRendererHolder = new GameObject("TeleportVisuals", typeof(LineRenderer));
        lineRendererHolder.transform.parent = this.transform;
        lineRendererHolder.transform.localPosition = Vector3.zero;
        lineRendererHolder.transform.localRotation = Quaternion.identity;

        teleportRenderer = lineRendererHolder.GetComponent<LineRenderer>();
        teleportRenderer.positionCount = resolution;
    }

    private void SetupInput()
    {
        if (TesicnorPlayer.Instance == null || coreInteraction != null || TesicnorPlayer.Instance.coreInteraction == null) return;

        coreInteraction = TesicnorPlayer.Instance.coreInteraction;


        if (handType == GrippingHand.HandType.right)
        {
            coreInteraction.Interaction.RightJoystickButton.started += (UnityEngine.InputSystem.InputAction.CallbackContext context)=>{ joystickButton = true; };
            coreInteraction.Interaction.RightJoystickButton.canceled += (UnityEngine.InputSystem.InputAction.CallbackContext context) => { joystickButton = false; };
            //joystickButton = coreInteraction.Interaction.RightJoystickButton.ReadValue<bool>();
            coreInteraction.Interaction.RightJoystick.performed += (UnityEngine.InputSystem.InputAction.CallbackContext context) =>
            {
                if (joystickButton)
                {
                    if (controllerType == ControllerTypes.Movement && movementType == MovementType.Locomotion)
                    {
                        MovePlayer(coreInteraction.Interaction.RightJoystick.ReadValue<Vector2>());
                    }
                    else if(controllerType == ControllerTypes.Rotation)
                    {
                        RotatePlayer(coreInteraction.Interaction.RightJoystick.ReadValue<Vector2>());
                    }
                }
            };

            if (this.controllerType == ControllerTypes.Movement && movementType == MovementType.Teleport)
            {
                coreInteraction.Interaction.Teleport.performed += (context =>
                {
                    ToggleTeleportVisuals(true);
                });

                coreInteraction.Interaction.Teleport.canceled += (context =>
                {
                    ToggleTeleportVisuals(false);
                    TeleportPlayer();
                });
            }
        }
        else
        {
            coreInteraction.Interaction.LeftJoystickButton.started += (UnityEngine.InputSystem.InputAction.CallbackContext context) => { joystickButton = true; };
            coreInteraction.Interaction.LeftJoystickButton.canceled += (UnityEngine.InputSystem.InputAction.CallbackContext context) => { joystickButton = false; };
            //joystickButton = coreInteraction.Interaction.LeftJoystickButton.va
            coreInteraction.Interaction.LeftJoystick.performed += (UnityEngine.InputSystem.InputAction.CallbackContext context) =>
            {
                if (joystickButton)
                {
                    if (controllerType == ControllerTypes.Movement && coreInteraction != null && movementType == MovementType.Locomotion)
                    {
                        MovePlayer(coreInteraction.Interaction.LeftJoystick.ReadValue<Vector2>());
                    }
                    else if (coreInteraction != null && controllerType == ControllerTypes.Rotation)
                    {
                        RotatePlayer(coreInteraction.Interaction.LeftJoystick.ReadValue<Vector2>());
                    }
                }
            };
        }
    }

    /// <summary>
    /// Mueve al jugador en la direccion dada
    /// </summary>
    /// <param name="_direction"></param>
    private void MovePlayer(Vector3 _direction)
    {
        Vector3 newDirection = new Vector3(_direction.x, 0, _direction.y);
        newDirection = camera.transform.InverseTransformVector(newDirection);
        newDirection.y = 0;
        player.transform.localPosition = Vector3.Lerp(player.transform.localPosition, player.transform.localPosition 
            + new Vector3(camera.transform.forward.x, 0, camera.transform.forward.z) * _direction.y 
            + new Vector3(camera.transform.right.x, 0, camera.transform.right.z) * _direction.x, Time.deltaTime * playerSpeed);
    }

    /// <summary>
    /// Rota al jugador en la direccion dada
    /// </summary>
    /// <param name="_direction"></param>
    private void RotatePlayer(Vector3 _direction)
    {
        float angles = 10;
        if (_direction.x < 0) angles = -10;

        Vector3 rotation = new Vector3(0, angles, 0);
        player.rotation = Quaternion.Euler(Vector3.Lerp(player.rotation.eulerAngles, player.rotation.eulerAngles + rotation, Time.deltaTime * playerRotationSpeed));
    }

    private void Update()
    {

        SetupInput();
        //if(controller.inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out joystick)  && joystick != Vector2.zero && 
        //    controller.inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out joystickButton) && joystickButton)
        //{
        //    if(controllerType == ControllerTypes.Movement)
        //    {
        //        MovePlayer(GetDirection(joystick));
        //    }
        //    else
        //    {
        //        RotatePlayer(joystick);
        //    }
        //}
    }

    /// <summary>
    /// Nos da la direcci�n para el movimiento basado en el joystick y el forward de la camara.
    /// La da en espacio global
    /// </summary>
    /// <param name="joystick"> Direccion del joystick </param>
    /// <returns></returns>
    Vector3 GetDirection(Vector3 joystick)
    {
        Vector3 _direction = new Vector3(joystick.x, 0, joystick.y);

        Vector3 cameraForward = camera.forward;
        Vector3 cameraRight = camera.right;

        Vector3 direction = (cameraForward.normalized * joystick.y) + (cameraRight.normalized * joystick.x);
        direction = new Vector3(direction.x, 0, direction.z);

        return direction;
    }

    #region TELEPORT

    private bool bvalidHit = false;

    private float initialVelocity = 5;

    private float gravity = 9.81f;

    private Vector3 TeleportPoint;
    
    Vector3 GetTeleportPoint()
    {
        bvalidHit = false;
        
        Vector3 result = Vector3.zero;

        Vector3 direction = this.transform.forward;
        Vector3 position = this.transform.position;

        float height = 0;

        RaycastHit groundHit;

        //altura objetivo del suelo por si es irregular
        if (Physics.Raycast(position, direction, out groundHit) && groundHit.collider && groundHit.collider.gameObject.CompareTag(FloorTag))
        {
            bvalidHit = true;
        }
        else if (Physics.Raycast(position, Vector3.down, out groundHit) && groundHit.collider &&
                 groundHit.collider.gameObject.CompareTag(FloorTag))
        {
            bvalidHit = true;
        }

        if (groundHit.collider != null) height = groundHit.point.y;
        
        //Punto de intersección con el suelo trigonometria
        #if MODOT
        if (Math.Abs(direction.y) > 0.0001f)
        {
            float t = (height - position.y) / direction.y;
            if (t > 0)
            {
                result = position + direction * t;
            }
        }
        #else
        //Punto de intersección con el suelo lanzamiento horizontal imaginario
        Vector3 vInitialVelocity = direction * initialVelocity;


        float time = vInitialVelocity.y + Mathf.Sqrt((vInitialVelocity.y * vInitialVelocity.y) -
                                                     2 * gravity * (height - position.y));

        result = position + vInitialVelocity * time;
        result.y = height;

        #endif
        
        return result;
    }

    private void TeleportPlayer()
    {
        player.position = TeleportPoint;
    }
    
    //TELEPORT VISUALS

    Vector3 Bezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1 - t;
        return u * u * p0 + 2 * u * t * p1 + t * t * p2;
    }

    private void DrawBezier()
    {
        Vector3 p0 = this.transform.position;
        Vector3 p2 = GetTeleportPoint();
        TeleportPoint = p2;

        Vector3 mid = (p0 + p2) * 0.5f;
        Vector3 p1 = mid + Vector3.up * arcHeight;

        for (int i = 0; i < teleportRenderer.positionCount; i++)
        {
            float t = i / (float)(teleportRenderer.positionCount - 1);
            Vector3 point = Bezier(p0, p1, p2, t);
            teleportRenderer.SetPosition(i, point);
        }
    }

    private WaitForEndOfFrame frame = new WaitForEndOfFrame();
    IEnumerator TeleportVisualsUpdate()
    {
        while (true)
        {
            DrawBezier();
            yield return frame;
        }
    }

    public void ToggleTeleportVisuals(bool _value)
    {
        teleportRenderer.enabled = _value;
        if(_value) StartCoroutine(nameof(TeleportVisualsUpdate));
        else StopCoroutine(nameof(TeleportVisualsUpdate));
    }
    #endregion
    #endregion
}
#if UNITY_EDITOR
[CustomEditor(typeof(LocomotionComponent), true)]
[CanEditMultipleObjects]
public class LocomotionComponentEditor : Editor
{
    private LocomotionComponent Target;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        Target = target as LocomotionComponent;

        if (Target.controllerType == LocomotionComponent.ControllerTypes.Movement)
        {
            SerializedProperty movementType = serializedObject.FindProperty("movementType");
            EditorGUILayout.PropertyField(movementType);
            
            GUILayout.Space(10);
            SerializedProperty floorTag = serializedObject.FindProperty("FloorTag");
            EditorGUILayout.PropertyField(floorTag);
            
            GUILayout.Space(10);
            SerializedProperty arcHeight = serializedObject.FindProperty("arcHeight");
            EditorGUILayout.PropertyField(arcHeight);
            
            GUILayout.Space(10);
            SerializedProperty resolution = serializedObject.FindProperty("resolution");
            EditorGUILayout.PropertyField(resolution);
            
            GUILayout.Space(10);
            SerializedProperty teleportRenderer = serializedObject.FindProperty("teleportRenderer");
            EditorGUILayout.PropertyField(teleportRenderer);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
