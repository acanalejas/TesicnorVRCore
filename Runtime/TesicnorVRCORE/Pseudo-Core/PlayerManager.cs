using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Oculus;

public class PlayerManager : MonoBehaviour
{
    #region PARAMETERS

    /// <summary>
    /// El canvas que enseña el mensaje de las manos
    /// </summary>
    [Header("El canvas que enseña el mensaje de las manos")]
    [SerializeField] private GameObject useHandsCanvas;

    /// <summary>
    /// El canvas que enseña el mensaje de los mandos
    /// </summary>
    [Header("El canvas que enseña el mensaje de los mandos")]
    [SerializeField] private GameObject useControllersCanvas;

    [Header("El GameObject del escenario")]
    [SerializeField] private GameObject scenario;

    [Header("El GameObject de la sala blanca")]
    [SerializeField] private GameObject room;

    /// <summary>
    /// El punto al que se quiere llevar el canvas del mensaje de las manos
    /// </summary>
    [Header("El punto al que se quiere llevar el canvas del mensaje de las manos")]
    [SerializeField] private Transform handsCanvasParent;

    /// <summary>
    /// Main camera
    /// </summary>
    [Header("Main camera")]
    [SerializeField] private Transform mainCamera;

    /// <summary>
    /// El padre de la mano derecha
    /// </summary>
    [Header("El padre de la mano derecha")]
    [SerializeField] private GameObject rightHandAnchor;

    /// <summary>
    /// El padre de la mano izquierda
    /// </summary>
    [Header("El padre de la mano izquierda")]
    [SerializeField] private GameObject leftHandAnchor;

    /// <summary>
    /// El controlador de la mano derecha
    /// </summary>
    [Header("El controlador derecho")]
    [SerializeField] private GameObject rightController;

    /// <summary>
    /// El controlador de la mano izquierda
    /// </summary>
    [Header("El controlador izquierdo")]
    [SerializeField] private GameObject leftController;

    /// <summary>
    /// El GO del Player que usa las manos
    /// </summary>
    [Header("El GO del Player que usa las manos")]
    [SerializeField] private GameObject playerHands;

    /// <summary>
    /// El GO del Player que usa los mandos
    /// </summary>
    [Header("El GO del Player que usa los mandos")]
    [SerializeField] private GameObject playerControllers;

    /// <summary>
    /// El anchor de la cámara en el player de las manos
    /// </summary>
    [Header("El anchor de la cámara en el player de las manos")]
    [SerializeField] private Transform playerHands_Camera;

    /// <summary>
    /// El anchor de la cámara en el player que usa los mandos
    /// </summary>
    [Header("El anchor de la cámara en el player que usa los mandos")]
    [SerializeField] private Transform playerControllers_Camera;

    /// <summary>
    /// El componente encargado de recentrar el mundo
    /// </summary>
    [Header("El componente encargado de recentrar el mundo")]
    [SerializeField] RecenterWorld recenterWorld;

    /// <summary>
    /// Controla si en la escena actual se necesitan las manos
    /// </summary>
    public static bool needsHands = true;


    #endregion

    #region FUNCTIONS
    private void Awake()
    {
        this.transform.position = Vector3.zero;
        needsHands = true;
    }
    private void Update()
    {
        if (needsHands) ShowHandsMessage();
        else ShowControllersMessage();
    }

    /// <summary>
    /// Están los mandos activos y en uso?
    /// </summary>
    /// <returns></returns>
    bool areControllersEnabled()
    {
        bool result = true;

        List<InputDevice> devices_R = new List<InputDevice>();
        List<InputDevice> devices_L = new List<InputDevice>();
        InputDevices.GetDevicesWithRole(InputDeviceRole.RightHanded, devices_R);
        InputDevices.GetDevicesWithRole(InputDeviceRole.LeftHanded, devices_L);

        if (devices_L.Count == 0 && devices_R.Count == 0) result = false;

        return result;
    }

    bool alreadyUsingHands = false;
    /// <summary>
    /// Funcion que enseña y posiciona / desactiva el mensaje para soltar los mandos
    /// </summary>
    void ShowHandsMessage()
    {
        useControllersCanvas.SetActive(false);
        if (areControllersEnabled())
        {
            //playerHands.SetActive(false);
            playerControllers.SetActive(true);
            useHandsCanvas.SetActive(true);
            rightHandAnchor.SetActive(false);
            leftHandAnchor.SetActive(false);
            leftController.SetActive(true);
            rightController.SetActive(true);

            //useHandsCanvas.transform.position = Vector3.Lerp(useHandsCanvas.transform.position, handsCanvasParent.position, Time.deltaTime * 2);
            //useHandsCanvas.transform.LookAt(mainCamera);
            alreadyUsingHands = false;
        }
        else
        {
            playerHands.SetActive(true);
            useHandsCanvas.SetActive(false);
            rightHandAnchor.SetActive(true);
            leftHandAnchor.SetActive(true);
            leftController.SetActive(false);
            rightController.SetActive(false);

            //if (!alreadyUsingHands) { recenterWorld.StartCoroutine("RecenterFromControllers"); alreadyUsingHands = true;}
            //playerControllers.SetActive(false);
        }
        //if(room)scenario.SetActive(!useHandsCanvas.activeSelf);
        //if(room)room.SetActive(useHandsCanvas.activeSelf);
    }

    bool alreadyChanged = false;
    void ShowControllersMessage()
    {
        useHandsCanvas.SetActive(false);
        //playerHands.SetActive(false);
        playerControllers.SetActive(true);
        if (!alreadyChanged)
        {
            alreadyChanged = true;
            playerControllers_Camera.position = new Vector3(playerControllers_Camera.position.x, playerHands_Camera.position.y, playerControllers_Camera.position.z);
        }
        if (!areControllersEnabled())
        {
            useControllersCanvas.SetActive(true);
            rightHandAnchor.SetActive(false);
            leftHandAnchor.SetActive(false);
            rightController.SetActive(false);
            leftController.SetActive(false);

            //useControllersCanvas.transform.position = Vector3.Lerp(useControllersCanvas.transform.position, handsCanvasParent.position, Time.deltaTime * 2);
            //useControllersCanvas.transform.LookAt(mainCamera);
        }

        else
        {
            useControllersCanvas.SetActive(false);
            rightController.SetActive(true);
            leftController.SetActive(true);
        }

        //if(room)scenario.SetActive(!useControllersCanvas.activeSelf);
        //if(room)room.SetActive(useControllersCanvas.activeSelf);
    }
    #endregion
}
