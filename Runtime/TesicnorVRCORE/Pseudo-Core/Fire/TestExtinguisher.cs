using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;

[RequireComponent(typeof(LineRenderer))]
public class TestExtinguisher : MonoBehaviour
{
    private void Start()
    {

    }

    private void Update()
    {
        Ray ray = new Ray(this.transform.position, this.transform.forward);

        GetComponent<LineRenderer>().positionCount = 2;
        Vector3[] positions = { this.transform.position, this.transform.position + this.transform.forward * 4 };
        GetComponent<LineRenderer>().SetPositions(positions);

        TesicFire.FireObject[] allFires = GameObject.FindObjectsOfType<TesicFire.FireObject>();
        
        int index = 0;
        bool pressed = false;
        if(GetComponent<XRController>().inputDevice.TryGetFeatureValue(CommonUsages.primaryButton, out pressed) && pressed)
        {
            foreach (var fires in allFires) { fires.ExtinguishWithCone(this.transform.position, this.transform.forward); }
        }
    }
}
