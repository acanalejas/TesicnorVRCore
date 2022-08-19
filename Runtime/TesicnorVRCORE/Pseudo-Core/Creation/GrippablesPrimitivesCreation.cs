using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class GrippablesPrimitivesCreation 
{
#if UNITY_EDITOR
    #region FUNCTIONS

    [MenuItem("Tesicnor/Primitives/VRCube")]
    public static void Create_VRCube()
    {
        GameObject self = GameObject.CreatePrimitive(PrimitiveType.Cube);
        if(Selection.gameObjects.Length > 0) self.transform.parent = Selection.gameObjects[0].transform;
        self.transform.localPosition = Vector3.zero;
        self.transform.localRotation = Quaternion.Euler(Vector3.zero);
        self.transform.localScale = Vector3.one;

        VRCollider collider = self.AddComponent<VRCollider>();
        collider.simulateOnDrop = true;
        collider.canRelease = true;
        collider.hasTarget = false;
    }

    [MenuItem("Tesicnor/Primitives/VRCapsule")]
    public static void Create_VRCapsule()
    {
        GameObject self = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        if (Selection.gameObjects.Length > 0) self.transform.parent = Selection.gameObjects[0].transform;
        self.transform.localPosition = Vector3.zero;
        self.transform.localRotation = Quaternion.Euler(Vector3.zero);
        self.transform.localScale = Vector3.one;

        VRCollider collider = self.AddComponent<VRCollider>();
        collider.simulateOnDrop = true;
        collider.canRelease = true;
        collider.hasTarget = false;
    }

    [MenuItem("Tesicnor/Primitives/VRCylinder")]
    public static void Create_VRCylinder()
    {
        GameObject self = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        if (Selection.gameObjects.Length > 0) self.transform.parent = Selection.gameObjects[0].transform;
        self.transform.localPosition = Vector3.zero;
        self.transform.localRotation = Quaternion.Euler(Vector3.zero);
        self.transform.localScale = Vector3.one;

        VRCollider collider = self.AddComponent<VRCollider>();
        collider.simulateOnDrop = true;
        collider.canRelease = true;
        collider.hasTarget = false;
    }

    [MenuItem("Tesicnor/Primitives/VRPlane")]
    public static void Create_VRPlane()
    {
        GameObject self = GameObject.CreatePrimitive(PrimitiveType.Plane);
        if (Selection.gameObjects.Length > 0) self.transform.parent = Selection.gameObjects[0].transform;
        self.transform.localPosition = Vector3.zero;
        self.transform.localRotation = Quaternion.Euler(Vector3.zero);
        self.transform.localScale = Vector3.one;

        VRCollider collider = self.AddComponent<VRCollider>();
        collider.simulateOnDrop = true;
        collider.canRelease = true;
        collider.hasTarget = false;
    }

    [MenuItem("Tesicnor/Primitives/VRQuad")]
    public static void Create_VRQuad()
    {
        GameObject self = GameObject.CreatePrimitive(PrimitiveType.Quad);
        if (Selection.gameObjects.Length > 0) self.transform.parent = Selection.gameObjects[0].transform;
        self.transform.localPosition = Vector3.zero;
        self.transform.localRotation = Quaternion.Euler(Vector3.zero);
        self.transform.localScale = Vector3.one;

        VRCollider collider = self.AddComponent<VRCollider>();
        collider.simulateOnDrop = true;
        collider.canRelease = true;
        collider.hasTarget = false;
    }

    [MenuItem("Tesicnor/Primitives/VRSphere")]
    public static void Create_VRSphere()
    {
        GameObject self = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        if (Selection.gameObjects.Length > 0) self.transform.parent = Selection.gameObjects[0].transform;
        self.transform.localPosition = Vector3.zero;
        self.transform.localRotation = Quaternion.Euler(Vector3.zero);
        self.transform.localScale = Vector3.one;

        VRCollider collider = self.AddComponent<VRCollider>();
        collider.simulateOnDrop = true;
        collider.canRelease = true;
        collider.hasTarget = false;
    }

    [MenuItem("Tesicnor/Primitives/Basic VR Target")]
    public static void Create_BasicVRTarget()
    {
        GameObject self = new GameObject("Basic VR Target", typeof(VRColliderReleaseTarget));
        if (Selection.gameObjects.Length > 0) self.transform.parent = Selection.gameObjects[0].transform;
        self.transform.localPosition = Vector3.zero;
        self.transform.localRotation = Quaternion.Euler(Vector3.zero);
        self.transform.localScale = Vector3.one;

        VRColliderReleaseTarget target = self.GetComponent<VRColliderReleaseTarget>();
        target.canBeCanceled = true;
        target.canReleaseObject = true;
        target.seeksTarget = true;
        target.DisableWhenRelease = false;
        target.needsGrabbing = false;

        self.GetComponent<Rigidbody>().useGravity = false;
        self.GetComponent<BoxCollider>().isTrigger = true;
    }

    [MenuItem("Tesicnor/Primitives/VRCube Interactable")]
    public static void Create_VRCubeInteractable()
    {
        GameObject self = GameObject.CreatePrimitive(PrimitiveType.Cube);
        if (Selection.gameObjects.Length > 0) self.transform.parent = Selection.gameObjects[0].transform;
        self.transform.localPosition = Vector3.zero;
        self.transform.localRotation = Quaternion.Euler(Vector3.zero);
        self.transform.localScale = Vector3.one;

        VR_Interactable interactable = self.AddComponent<VR_Interactable>();
        interactable.is3DObject = true;
        interactable.hasToFollow = true;
        interactable.changesColor = false;
        interactable.canBePressed = true;
        interactable.normalPosition = self.transform.localPosition;
        interactable.pressedPosition = -self.transform.InverseTransformDirection(self.transform.forward);
    }

    [MenuItem("Tesicnor/Primitives/VRCapsule Interactable")]
    public static void Create_VRCapsuleInteractable()
    {
        GameObject self = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        if (Selection.gameObjects.Length > 0) self.transform.parent = Selection.gameObjects[0].transform;
        self.transform.localPosition = Vector3.zero;
        self.transform.localRotation = Quaternion.Euler(Vector3.zero);
        self.transform.localScale = Vector3.one;

        VR_Interactable interactable = self.AddComponent<VR_Interactable>();
        interactable.is3DObject = true;
        interactable.hasToFollow = true;
        interactable.changesColor = false;
        interactable.canBePressed = true;
        interactable.normalPosition = self.transform.localPosition;
        interactable.pressedPosition = -self.transform.InverseTransformDirection(self.transform.forward);

        GameObject.DestroyImmediate(self.GetComponent<CapsuleCollider>());
    }

    [MenuItem("Tesicnor/Primitives/VRCylinder Interactable")]
    public static void Create_VRCylinderInteractable()
    {
        GameObject self = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        if (Selection.gameObjects.Length > 0) self.transform.parent = Selection.gameObjects[0].transform;
        self.transform.localPosition = Vector3.zero;
        self.transform.localRotation = Quaternion.Euler(Vector3.zero);
        self.transform.localScale = Vector3.one;

        VR_Interactable interactable = self.AddComponent<VR_Interactable>();
        interactable.is3DObject = true;
        interactable.hasToFollow = true;
        interactable.changesColor = false;
        interactable.canBePressed = true;
        interactable.normalPosition = self.transform.localPosition;
        interactable.pressedPosition = -self.transform.InverseTransformDirection(self.transform.forward);

        GameObject.DestroyImmediate(self.GetComponent<CapsuleCollider>());
    }

    [MenuItem("Tesicnor/Primitives/VRPlane Interactable")]
    public static void Create_VRPlaneInteractable()
    {
        GameObject self = GameObject.CreatePrimitive(PrimitiveType.Plane);
        if (Selection.gameObjects.Length > 0) self.transform.parent = Selection.gameObjects[0].transform;
        self.transform.localPosition = Vector3.zero;
        self.transform.localRotation = Quaternion.Euler(Vector3.zero);
        self.transform.localScale = Vector3.one;

        VR_Interactable interactable = self.AddComponent<VR_Interactable>();
        interactable.is3DObject = true;
        interactable.hasToFollow = true;
        interactable.changesColor = false;
        interactable.canBePressed = true;
        interactable.normalPosition = self.transform.localPosition;
        interactable.pressedPosition = -self.transform.InverseTransformDirection(self.transform.forward);

        GameObject.DestroyImmediate(self.GetComponent<MeshCollider>());
    }

    [MenuItem("Tesicnor/Primitives/VRQuad Interactable")]
    public static void Create_VRQuadInteractable()
    {
        GameObject self = GameObject.CreatePrimitive(PrimitiveType.Quad);
        if (Selection.gameObjects.Length > 0) self.transform.parent = Selection.gameObjects[0].transform;
        self.transform.localPosition = Vector3.zero;
        self.transform.localRotation = Quaternion.Euler(Vector3.zero);
        self.transform.localScale = Vector3.one;

        VR_Interactable interactable = self.AddComponent<VR_Interactable>();
        interactable.is3DObject = true;
        interactable.hasToFollow = true;
        interactable.changesColor = false;
        interactable.canBePressed = true;
        interactable.normalPosition = self.transform.localPosition;
        interactable.pressedPosition = -self.transform.InverseTransformDirection(self.transform.forward);

        GameObject.DestroyImmediate(self.GetComponent<MeshCollider>());
    }

    [MenuItem("Tesicnor/Primitives/VRSphere Interactable")]
    public static void Create_VRSphereInteractable()
    {
        GameObject self = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        if (Selection.gameObjects.Length > 0) self.transform.parent = Selection.gameObjects[0].transform;
        self.transform.localPosition = Vector3.zero;
        self.transform.localRotation = Quaternion.Euler(Vector3.zero);
        self.transform.localScale = Vector3.one;

        VR_Interactable interactable = self.AddComponent<VR_Interactable>();
        interactable.is3DObject = true;
        interactable.hasToFollow = true;
        interactable.changesColor = false;
        interactable.canBePressed = true;
        interactable.normalPosition = self.transform.localPosition;
        interactable.pressedPosition = -self.transform.InverseTransformDirection(self.transform.forward);

        GameObject.DestroyImmediate(self.GetComponent<SphereCollider>());
    }
    #endregion
#endif
}
