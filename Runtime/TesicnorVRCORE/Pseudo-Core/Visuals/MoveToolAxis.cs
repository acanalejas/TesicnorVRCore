using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class MoveToolAxis : MonoBehaviour
{
    #region FIELDS
    public enum Axis { X,Y, Z}
    public Axis axis = Axis.X;

    private Vector3 vector;
    public Vector3 axis_mult
    {
        get { switch (axis) { case Axis.X: vector = new Vector3(1, 0, 0); break;
                case Axis.Y: vector = new Vector3(0, 1, 0); break;
                case Axis.Z: vector = new Vector3(0, 0, 1); break;
            } return vector;
        } }

    float sizeX { get { switch (axis) 
            { 
                case Axis.X:
                    sizex = 0.01901045f;
                    break;
                case Axis.Y:
                    sizex = 0.0080729f;
                    break;
                case Axis.Z:
                    sizex = 0.0080729f;
                    break;
            } return sizex; } }
    float sizex;

    float sizeY { get
        {
            switch (axis)
            {
                case Axis.X:
                    sizey = 0.0050926f;
                    break;
                case Axis.Y:
                    sizey = 0.03842605f;
                    break;
                case Axis.Z:
                    sizey = 0.0050926f;
                    break;
            }
            return sizey;
        } }
    float sizey;
    #endregion

    #region METHODS

    public Vector3 MoveVertex(Vector3 vertexPos)
    {
        Vector3 mouse = Event.current.mousePosition;
        mouse = new Vector3(mouse.x, Screen.height - mouse.y, mouse.z);
        Vector3 mouseInWorld = UnityEditor.HandleUtility.GUIPointToWorldRay(mouse).origin;

        Vector3 mouseInWorld_axis = new Vector3(mouse.x * axis_mult.x, mouse.y * axis_mult.y, mouse.z * axis_mult.z);

        Debug.Log(mouseInWorld);

        return ModifyModelsWindow.modifiedGameObject.transform.InverseTransformPoint(this.GetComponentInParent<MoveTool>().transform.position);
    }

    Vector3 objectViewportPosition;
    Vector2 mouseViewport;
    Vector3 hitPoint;
    public string isBeingClicked()
    {
        Vector2 mouse = ModifyModelsWindow.previewCamera.ScreenToViewportPoint(Event.current.mousePosition);
        Vector3 objectPos = ModifyModelsWindow.previewCamera.WorldToViewportPoint(this.transform.position);
        mouse = new Vector2(mouse.x, 1f - mouse.y);
        mouseViewport = mouse;

        Ray mouseRay = ModifyModelsWindow.previewCamera.ViewportPointToRay(mouse);
        RaycastHit hit;

        if(Physics.Raycast(mouseRay, out hit))
        {
            hitPoint = hit.collider.transform.position;
            if (hit.collider)
            {
                Debug.Log(hit.collider.gameObject.name);
                if (hit.collider.gameObject.name == this.gameObject.name)
                {
                    return hit.collider.gameObject.name;
                }
            }
        }

        return "";
    }

    Vector3 mouseWorldPosition;
    Vector3 objectWorldPosition;
    public void MoveHandle(Transform forMove)
    {
        mouseViewport = ModifyModelsWindow.previewCamera.ScreenToViewportPoint(Event.current.mousePosition);
        mouseViewport = new Vector2(mouseViewport.x, 1 - mouseViewport.y);
        Ray mouseRay = ModifyModelsWindow.previewCamera.ViewportPointToRay(mouseViewport);
        Vector3 position = Vector3.zero;

        RaycastHit hit;
        Physics.Raycast(mouseRay, out hit, Vector3.Distance(forMove.position, ModifyModelsWindow.previewCamera.transform.position));

        
        mouseWorldPosition = hit.point;
        switch (axis)
        {
            case Axis.X:
                position = new Vector3(mouseWorldPosition.x, forMove.position.y, forMove.position.z);
                break;

            case Axis.Y:
                position = new Vector3(forMove.position.x, mouseWorldPosition.y, forMove.position.z);
                break;

            case Axis.Z:

                break;
        }

        //Debug.Log("Object name is : " + this.gameObject.name + "\nMouse position is : " + mouseWorldPosition + "\n result position is : " + position);

        mouseWorldPosition = ModifyModelsWindow.previewCamera.ViewportToWorldPoint(position);
        if (position != Vector3.zero)
            forMove.position = position;
    }

    public void OnDrawGizmos()
    {
        if (!ModifyModelsWindow.previewCamera) return;
        Gizmos.DrawSphere(ModifyModelsWindow.previewCamera.ViewportToWorldPoint(objectViewportPosition), 0.04f);
        Gizmos.DrawCube(mouseWorldPosition, new Vector3(0.08f,0.08f,0.08f));
        Gizmos.DrawCube(hitPoint, new Vector3(0.07f, 0.07f, 0.07f));
    }
    #endregion
}
#endif
