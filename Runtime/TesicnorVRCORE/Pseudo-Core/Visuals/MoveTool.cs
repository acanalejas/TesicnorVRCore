using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
public class MoveTool : MonoBehaviour
{
    #region SINGLETON
    private static MoveTool instance;
    public static MoveTool Instance { get { return instance; } }

    private void CheckSingleton()
    {
        if (instance == null) instance = this;
        else DestroyImmediate(this.gameObject);
    }

    #endregion

    #region FIELDS
    public Vector3 positionOffset;
    [SerializeField] private float test;
    #endregion

    

    #region METHODS
    /// <summary>
    /// Ata la herramienta de traslación a un punto
    /// </summary>
    /// <param name="vertex"></param>
    public void AppendToVertex(Vector3 vertex)
    {
        this.transform.position = vertex + positionOffset;
        UnityEditor.Handles.ArrowHandleCap(0, vertex, Quaternion.identity, UnityEditor.HandleUtility.GetHandleSize(vertex), EventType.Repaint);
    }

    /// <summary>
    /// Se está clickando la herramienta de traslación?
    /// Si -> Devuelve el nombre del eje seleccionado
    /// No -> Devuelve un string vacio
    /// </summary>
    /// <returns></returns>
    public string isBeingClicked()
    {
        Vector2 mouse = ModifyModelsWindow.previewCamera.ScreenToViewportPoint(Event.current.mousePosition);
        Vector3 objectPos = ModifyModelsWindow.previewCamera.WorldToViewportPoint(this.transform.position);
        mouse = new Vector2(mouse.x, 1f - mouse.y);

        Ray mouseRay = ModifyModelsWindow.previewCamera.ViewportPointToRay(mouse);
        RaycastHit hit;

        if (Physics.Raycast(mouseRay, out hit))
        {
            if (hit.collider)
            {
                Debug.Log(hit.collider.gameObject.name);

                return hit.collider.gameObject.name;
            }
        }

        return "";
    }

    Vector2 mouseViewport;
    Vector3 mouseWorldPosition;

    /// <summary>
    /// Mueve el trasnform seleccionado dependiendo de la interacción del ratón
    /// </summary>
    /// <param name="forMove"></param>
    /// <param name="name"></param>
    public void Hastalapolla(Transform forMove, string name)
    {
        //Punto del viewport que se está clickando
        mouseViewport = ModifyModelsWindow.previewCamera.ScreenToViewportPoint(Event.current.mousePosition);
        //Se invierten las coordenadas en el eje Y para hacerlo coincidir ya que sino por un error sale al revés
        mouseViewport = new Vector2(mouseViewport.x, 1 - mouseViewport.y);
        //Se saca un rayo desde el punto que se clicka hacia adelante basado en la vista de la camara
        Ray mouseRay = ModifyModelsWindow.previewCamera.ViewportPointToRay(mouseViewport);
        Vector3 position = Vector3.zero;

        RaycastHit hit;
        Physics.Raycast(mouseRay, out hit, Vector3.Distance(forMove.position, ModifyModelsWindow.previewCamera.transform.position));


        mouseWorldPosition = hit.point;
        //En funcion del eje seleccionado mueve en una direccion u otra
        switch (name)
        {
            case "X":
                position = new Vector3(mouseWorldPosition.x, forMove.position.y, forMove.position.z);
                break;

            case "Y":
                position = new Vector3(forMove.position.x, mouseWorldPosition.y, forMove.position.z);
                break;

            case "Z":

                break;
        }

        //Debug.Log("Object name is : " + name + "\nMouse position is : " + mouseWorldPosition + "\n result position is : " + position);

        mouseWorldPosition = ModifyModelsWindow.previewCamera.ViewportToWorldPoint(position);
        if (position != Vector3.zero && Vector3.Distance(this.transform.position,position) < 2)
            forMove.position = position;
    }
    #endregion
}
#endif