using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapToBody : MonoBehaviour
{
    #region PARAMETES
    [Header("La cámara que actúa como cabeza del jugador")]
    public Transform camera;

    [Header("Tiene que rotar?")]
    public bool bShouldRotate = true;

    private Vector3 initialDistance;
    #endregion

    #region FUNCTIONS
    private void Awake()
    {
        StartCoroutine("update");
        initialDistance = this.transform.position - camera.transform.position;
    }

    WaitForEndOfFrame frame = new WaitForEndOfFrame();
    private IEnumerator update()
    {
        while (true)
        {
            //Snap();
            SnapByDistance();
            yield return new WaitForEndOfFrame();
        }
    }

    Vector3 cameraLastPosition;
    Vector3 cameraLastRotation;
    private void Snap()
    {
        if(cameraLastPosition != Vector3.zero && cameraLastRotation != Vector3.zero)
        {
            //FOR POSITION
            Vector3 distance = camera.position - cameraLastPosition;
            this.transform.position += distance;

            //FOR ROTATION
            Vector3 rotDistance = camera.rotation.eulerAngles - cameraLastRotation;
            if(bShouldRotate)
            this.transform.rotation *= Quaternion.Euler(0, rotDistance.y, 0);
        }
        cameraLastPosition = camera.position;
        cameraLastRotation = camera.rotation.eulerAngles;
    }

    private void SnapByDistance()
    {
        this.transform.position = camera.position + initialDistance;
        this.transform.rotation = Quaternion.Euler(new Vector3(0, camera.rotation.eulerAngles.y, 0));
    }
    #endregion
}
