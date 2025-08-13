using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;

public class LocomotionComponent : MonoBehaviour
{
    #region PARAMETERS
    private enum ControllerTypes { Movement, Rotation}
    [Header("Este mando se usa para moverse o girar")]
    [SerializeField] private ControllerTypes controllerType = ControllerTypes.Movement;

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

    //private XRController controller;
    #endregion

    #region FUNCTIONS
    private void Awake()
    {
        //controller = GetComponent<XRController>();
        //if (!controller) controller = gameObject.AddComponent<XRController>();
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
                    if (controllerType == ControllerTypes.Movement)
                    {
                        MovePlayer(coreInteraction.Interaction.RightJoystick.ReadValue<Vector2>());
                    }
                    else
                    {
                        RotatePlayer(coreInteraction.Interaction.RightJoystick.ReadValue<Vector2>());
                    }
                }
            };
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
                    if (controllerType == ControllerTypes.Movement)
                    {
                        MovePlayer(coreInteraction.Interaction.LeftJoystick.ReadValue<Vector2>());
                    }
                    else
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
    /// Nos da la dirección para el movimiento basado en el joystick y el forward de la camara.
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
    #endregion
}
