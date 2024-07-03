using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface para a�adir a las manos o los mandos, es decir el sistema de input del jugador
/// REQUIRE LINERENDERER
/// </summary>
public interface VRInteractionInterface 
{
    /// <summary>
    /// Para detectar el input de la interacci�n, y el m�todo que tengamos para detectar los interactables usando las manos
    /// /// </summary>
    void DetectInteraction_Hands();
    /// <summary>
    ///  Para detectar el input de la interacci�n, y el m�todo que tengamos para detectar los interactables usando los mandos
    /// </summary>
    void DetectInteraction_Controllers();
    /// <summary>
    /// Funcion para usar en el update para comprobar la interacci�n con las dos funciones superiores DetectInteraction_Hands() & DetectInteraction_Controllers()
    /// </summary>
    void DetectInteraction();

    /// <summary>
    /// La clase que herede esta interfaz debe tener un LineRenderer y esta funcion cambiar� su color 
    /// </summary>
    /// <param name="detected"></param>
    void SetLineRendererColor(bool detected);

    /// <summary>
    /// Para cuando se usen las manos se setean en los dedos lo necesario para la interacci�n "fisica" con botones
    /// es decir, setear colliders y dem�s
    /// </summary>
    void SetupFingers();

    /// <summary>
    /// Funci�n que determina que se hace en el evento del click
    /// </summary>
    public void Click();
    /// <summary>
    /// Funcion que determina que se hace en el evento del release
    /// </summary>
    public void Release();

    /// <summary>
    /// Bool que comprueba si est� pulsando alg�n dedo alg�n interactable
    /// </summary>
    /// <returns></returns>
    bool isFingerPressing3DObject();

    /// <summary>
    /// El raycast que usamos para la detecci�n
    /// </summary>
    /// <param name="_origin"></param>
    /// <returns></returns>
    RaycastHit GetRaycastHit(Transform _origin);

    /// <summary>
    /// Devuelve el origen desde el que se lanza el rayo
    /// </summary>
    /// <returns></returns>
    Transform GetOrigin();

    
}

/// <summary>
/// Interface para los objetos con los que se puede interactuar (botones, etc)
/// REQUIERE BOX COLLIDER
/// </summary>
public interface VRInteractableInterface 
{
    /// <summary>
    /// Setea el collider en caso de que sea un objeto de UI, ya que funciona un poco diferente que con objetos 3D
    /// Con el rectTransform debe coincidir el Size(x,y,z) del collider con el Size del RectTransform(width,height)
    /// </summary>
    void SetupUICollider();

    /// <summary>
    /// Funcion que se lanza al hacer click
    /// </summary>
    void OnClick();
    /// <summary>
    /// Funcion que se lanza al poner el puntero por encima
    /// </summary>
    void OnHovered();

    /// <summary>
    /// Funcion que se lanza al dejar de se�alar el objeto
    /// </summary>
    void OnExitHover();
    /// <summary>
    /// Funcion para cuando se suelta el interactable
    /// </summary>
    void OnRelease();

    /// <summary>
    /// Cambia de color al interactuar
    /// </summary>
    /// <param name="color"></param>
    void ChangeColor(int color);

    /// <summary>
    /// Setea si el bot�n se puede pulsar o no
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    bool SetCanBePressed(bool value);
    /// <summary>
    /// Devuelve si el boton en este instante se puede pulsar o no
    /// </summary>
    /// <returns></returns>
    bool GetCanBePressed();

    /// <summary>
    /// Se est� pulsando el interactable?
    /// </summary>
    /// <returns></returns>
    bool GetIsClicking();
    /// <summary>
    /// Esta el puntero por encima?
    /// </summary>
    /// <returns></returns>
    bool GetIsHovered();
    /// <summary>
    /// Es este interactable accionable por colision directa?
    /// </summary>
    /// <returns></returns>
    bool GetIsTouchable();
    /// <summary>
    /// Setea la mano que interactua
    /// </summary>
    /// <param name="_hand"></param>
    void SetHand(GameObject _hand);
    /// <summary>
    /// Devuelve la mano que interactua
    /// </summary>
    /// <returns></returns>
    GameObject GetHand();
}
