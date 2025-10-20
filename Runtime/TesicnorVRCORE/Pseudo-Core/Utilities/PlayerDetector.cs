using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent (typeof(BoxCollider))]
public class PlayerDetector : MonoBehaviour
{
    #region PARAMETERS
    [Header("Tag que se usa para la detección del personaje")]
    [SerializeField] private string PlayerTag = "Player";

    [Header("Evento usado para cuando el jugador entra en la detección")]
    public UnityEvent OnPlayerDetected;

    [Header("Evento usado para cuando se deja de detectar al jugador")]
    public UnityEvent OnPlayerExitDetection;

    [Header("Evento usado mientras el player continua en la zona de deteccion")]
    public UnityEvent OnPlayerStayDetection;
    #endregion

    #region METHODS
    private void Start()
    {
        //Just to make sure everything is setup correctly
        BoxCollider collider = GetComponent<BoxCollider>();
        collider.isTrigger = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == PlayerTag)
        {
            OnPlayerDetected.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == PlayerTag)
        {
            OnPlayerExitDetection.Invoke();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.tag == PlayerTag)
        {
            OnPlayerStayDetection.Invoke();
        }
    }
    #endregion
}
