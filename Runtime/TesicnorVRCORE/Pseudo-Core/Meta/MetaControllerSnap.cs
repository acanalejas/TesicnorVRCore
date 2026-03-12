using UnityEngine;

public class MetaControllerSnap : MonoBehaviour
{
    [Header("El mando del que coge la posicion y rotacion")]
    public OVRInput.Controller mando = OVRInput.Controller.LTouch;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    

    void LateUpdate()
    {
        transform.localPosition = OVRInput.GetLocalControllerPosition(mando);
        transform.localRotation = OVRInput.GetLocalControllerRotation(mando);
    }
}
