using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface VRGripInterface
{
    /// <summary>
    /// Método para agarrar el objeto
    /// </summary>
    public void Grab(GrippingHand hand);

    /// <summary>
    /// Comprueba si el objeto se puede agarrar
    /// </summary>
    /// <returns>
    /// 1. True -> Se puede agarrar
    /// 2. False -> No se puede agarrar
    /// </returns>
    public bool canBeGrabbed();

    /// <summary>
    /// Reposiciona el objeto al agarrar
    /// </summary>
    /// <param name="_offset"> offset del objeto respecto a la mano </param>
    public void RepositionOnGrab(Vector3 _offset);
    /// <summary>
    /// Reposiciona el objeto al agarrar
    /// </summary>
    public void RepositionOnGrab();

    /// <summary>
    /// Reposiciona el objeto al agarrar
    /// </summary>
    /// <param name="_offsetPosition">offset de la posicion</param>
    /// <param name="_offsetRotation">offset de la rotacion</param>
    public void RepositionOnGrab(Vector3 _offsetPosition, Vector3 _offsetRotation);

    /// <summary>
    /// Comprueba si se puede soltar
    /// </summary>
    /// <returns>
    /// 1. True -> Se puede soltar
    /// 2. False -> No se puede soltar
    /// </returns>
    public bool canBeReleased();
    /// <summary>
    /// Método para soltar el objeto
    /// </summary>
    public void Release();

    /// <summary>
    /// Devuelve la mano que esta agarrando el objeto,
    /// en caso de que no este siendo agarrado devuelve null
    /// </summary>
    /// <returns></returns>
    public GrippingHand GetGrippingHand();

    /// <summary>
    /// Setea lo necesario para que el objeto simule físicas cuando deja de ser agarrado
    /// </summary>
    public void SetSimulateOnDrop();

    /// <summary>
    /// Setea los effectos necesarios cuando el objeto esta disponible para ser agarrado
    /// Ej: Highlight
    /// </summary>
    /// <param name="value"></param>
    public void SetAvailableEffects(bool value);

    /// <summary>
    /// Coroutine que sirve como "Update" para setear la posicion del objeto cuando se agarra
    /// </summary>
    /// <returns></returns>
    public IEnumerator Attach();

    /// <summary>
    /// Devuelve si el objeto esta siendo agarrado
    /// </summary>
    /// <returns></returns>
    public bool isGrabbed();

    /// <summary>
    /// Setea y devuelve si el objeto se puede agarrar
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool SetGrabbable(bool value);
}
public interface VRHandInterface
{

    public void Grab();

    public void Release();

    /// <summary>
    /// Detecta el input del mando para agarrar
    /// </summary>
    public void DetectTheInput();

    /// <summary>
    /// Detecta si en la zona del trigger hay algo que poder agarrar
    /// </summary>
    /// <returns></returns>
    public bool canGrabSomething();

    /// <summary>
    /// Devuelve el objeto más cercano a la mano para agarrarlo
    /// </summary>
    /// <returns></returns>
    public GameObject closestObjectToGrab();

    /// <summary>
    /// Devuelve si la mano está agarrando algo
    /// </summary>
    /// <returns></returns>
    public bool isGrabbing();


}
