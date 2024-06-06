using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
using UnityEditor.XR.LegacyInputHelpers;
using UnityEngine.SpatialTracking;
using Oculus;

public static class PlayerCreator
{
    //FOR PLAYER CONTROLLERS
    static string handMeshPath = "Packages/com.tesicnor.tesicnorvrcore/Runtime/TesicnorVRCORE/Pseudo-Core/Models/Hands & Controllers/fully_gloved.fbx";
    static string rightOpenedSOPath = "Packages/com.tesicnor.tesicnorvrcore/Runtime/TesicnorVRCORE/Pseudo-Core/ScriptableObjects/Hand Poses/RightController_Opened.asset";
    static string rightClosedSOPath = "Packages/com.tesicnor.tesicnorvrcore/Runtime/TesicnorVRCORE/Pseudo-Core/ScriptableObjects/Hand Poses/RightController_Closed.asset";
    static string leftOpenedSOPath = "Packages/com.tesicnor.tesicnorvrcore/Runtime/TesicnorVRCORE/Pseudo-Core/ScriptableObjects/Hand Poses/LeftController_Opened.asset";
    static string leftClosedSOPath = "Packages/com.tesicnor.tesicnorvrcore/Runtime/TesicnorVRCORE/Pseudo-Core/ScriptableObjects/Hand Poses/LeftController_Closed.asset";

    static string quest2ControllerPath = "Packages/com.tesicnor.tesicnorvrcore/Runtime/TesicnorVRCORE/Pseudo-Core/Models/Hands & Controllers/Quest2Controller .fbx";

    //FOR PLAYER HANDS
    static string rightHandPrefabPath = "Assets/Oculus/VR/Prefabs/OVRCustomHandPrefab_R.prefab";
    static string leftHandPrefabPath = "Assets/Oculus/VR/Prefabs/OVRCustomHandPrefab_L.prefab";

    static string lineRendererMatPath = "Packages/com.tesicnor.tesicnorvrcore/Runtime/TesicnorVRCORE/Pseudo-Core/Materials/LineRenderer_Mat.mat";

#if UNITY_EDITOR
    #region FUNCTIONS
    [MenuItem("Tesicnor/Players/PlayerControllers_Hands")]
    public static void CreatePlayerControllers_Hands()
    {
        GameObject playerControllers = new GameObject("PlayerControllers", typeof(CameraOffset));
        if (Selection.gameObjects.Length > 0) playerControllers.transform.parent = Selection.gameObjects[0].transform;
        playerControllers.transform.localPosition = Vector3.zero;
        playerControllers.transform.localRotation = Quaternion.Euler(Vector3.zero);
        playerControllers.transform.localScale = Vector3.one;
        PlayerGravity gravity = playerControllers.AddComponent<PlayerGravity>();
        gravity.IsGravtyActive = true;

        // ================================== CREANDO LOS HIJOS DIRECTOS ==============================================
        GameObject cameraOffset = new GameObject("Camera Offset");
        cameraOffset.transform.parent = playerControllers.transform;
        cameraOffset.transform.localPosition = Vector3.zero;
        cameraOffset.transform.localRotation = Quaternion.Euler(Vector3.zero);
        cameraOffset.transform.localScale = Vector3.one;

        GameObject rightController = new GameObject("Right Controller", typeof(TrackedPoseDriver), typeof(UnityEngine.XR.Interaction.Toolkit.XRController), typeof(HandInteraction), typeof(GrippingHand), typeof(HandPoser));
        rightController.transform.parent = playerControllers.transform;
        rightController.transform.localPosition = Vector3.zero;
        rightController.transform.localRotation = Quaternion.Euler(Vector3.zero);
        rightController.transform.localScale = Vector3.one;

        GameObject leftController = new GameObject("Left Controller", typeof(TrackedPoseDriver), typeof(UnityEngine.XR.Interaction.Toolkit.XRController), typeof(HandInteraction), typeof(GrippingHand), typeof(HandPoser));
        leftController.transform.parent = playerControllers.transform;
        leftController.transform.localPosition = Vector3.zero;
        leftController.transform.localRotation = Quaternion.Euler(Vector3.zero);
        leftController.transform.localScale = Vector3.one;

        GameObject bodyHolder = new GameObject("Body Holder", typeof(SnapToBody));
        bodyHolder.transform.parent = playerControllers.transform;  
        bodyHolder.transform.localPosition = new Vector3(0,-0.34f,0);  
        bodyHolder.transform.localRotation = Quaternion.Euler(Vector3.zero);
        bodyHolder.transform.localScale = Vector3.one;
        gravity.BodyGO = bodyHolder;

        //=============================================================================================================

        // ============================= SETEANDO EL MANDO DERECHO ====================================================

        TrackedPoseDriver rightPose = rightController.GetComponent<TrackedPoseDriver>();
        rightPose.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRController, TrackedPoseDriver.TrackedPose.RightPose);

        //Seteando la clase GrippingHand
        GrippingHand rightGripping = rightController.GetComponent<GrippingHand>();
        rightGripping.isController = true;
        rightGripping.handType = GrippingHand.HandType.right;
        rightGripping.colliderRadius = 0.05f;
        rightGripping.colliderBone = rightController.transform;
        rightGripping.player = playerControllers.transform;
        rightGripping.hideOnGrab = false;
        rightGripping.handController = rightController.GetComponent<XRController>();

        //Seteando la clase HandInteraction
        HandInteraction rightInteraction = rightController.GetComponent<HandInteraction>();
        rightInteraction.isHandControlled = false;
        rightInteraction.isLeftHand = false;
        rightInteraction.usesRay = true;
        rightInteraction.lineRenderer = rightController.GetComponent<LineRenderer>();
        rightInteraction.interactionOrigin = rightController.transform;
        rightInteraction.nonDetectedColor = new Color(255, 35, 35, 255);
        rightInteraction.detectedColor = new Color(255, 149, 25, 255);
        rightInteraction.handController = rightController.GetComponent<XRController>();

        LineRenderer lineRenderer_right = rightController.GetComponent<LineRenderer>();
        lineRenderer_right.startWidth = 0.010f;
        lineRenderer_right.endWidth = 0.015f;
        lineRenderer_right.enabled = false;
        lineRenderer_right.material = (Material)AssetDatabase.LoadAssetAtPath(lineRendererMatPath, typeof(Material));

        //=============================================================================================================

        // ================================ SETEANDO LA MANO IZQUIERDA ================================================

        TrackedPoseDriver leftPose = leftController.GetComponent<TrackedPoseDriver>();
        leftPose.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRController, TrackedPoseDriver.TrackedPose.LeftPose);

        //Seteando el componente GrippingHand
        GrippingHand leftGripping = leftController.GetComponent<GrippingHand>();
        leftGripping.isController = true;
        leftGripping.handType = GrippingHand.HandType.left;
        leftGripping.colliderRadius = 0.05f;
        leftGripping.colliderBone = leftController.transform;
        leftGripping.player = playerControllers.transform;
        leftGripping.hideOnGrab = false;
        leftGripping.handController = leftController.GetComponent<XRController>();
        leftGripping.handController.controllerNode = XRNode.LeftHand;

        //Seteando el componente HandInteraction
        HandInteraction leftInteraction = leftController.GetComponent<HandInteraction>();
        leftInteraction.isHandControlled = false;
        leftInteraction.isLeftHand = true;
        leftInteraction.usesRay = true;
        leftInteraction.lineRenderer = leftController.GetComponent<LineRenderer>();
        leftInteraction.interactionOrigin = leftController.transform;
        leftInteraction.nonDetectedColor = new Color(255, 35, 35, 255);
        leftInteraction.detectedColor = new Color(255, 149, 25, 255);
        leftInteraction.handController = leftController.GetComponent<XRController>();

        LineRenderer lineRenderer_left = leftController.GetComponent<LineRenderer>();
        lineRenderer_left.startWidth = 0.010f;
        lineRenderer_left.endWidth = 0.015f;
        lineRenderer_left.enabled = false;
        lineRenderer_left.material = (Material)AssetDatabase.LoadAssetAtPath(lineRendererMatPath, typeof(Material));

        //=============================================================================================================

        //====================================== SETEANDO LA CAMARA ===================================================
        GameObject mainCamera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(TrackedPoseDriver));
        mainCamera.transform.parent = cameraOffset.transform;
        mainCamera.transform.localPosition = Vector3.zero;
        mainCamera.transform.localRotation = Quaternion.Euler(Vector3.zero);
        mainCamera.transform.localScale = Vector3.one;

        TrackedPoseDriver cameraPose = mainCamera.GetComponent<TrackedPoseDriver>();
        cameraPose.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRDevice, TrackedPoseDriver.TrackedPose.Center);

        Camera camera = mainCamera.GetComponent<Camera>();
        camera.nearClipPlane = 0.01f;
        camera.farClipPlane = 1000;

        SnapToBody snap = bodyHolder.GetComponent<SnapToBody>();
        snap.camera = mainCamera.transform;

        // =============================================================================================================

        // ================================ SETEANDO LA GEOMETRIA DE LAS MANOS =========================================

        Object rightMesh = AssetDatabase.LoadAssetAtPath(handMeshPath, typeof(Object)) as Object;
        GameObject rightMesh_GO = (GameObject)GameObject.Instantiate(rightMesh);
        rightMesh_GO.transform.parent = rightController.transform;
        rightMesh_GO.transform.localPosition = Vector3.zero;
        rightMesh_GO.transform.localRotation = Quaternion.Euler(Vector3.zero);
        rightMesh_GO.transform.localScale = Vector3.one;

        GameObject leftMesh_GO = (GameObject)GameObject.Instantiate(rightMesh);
        leftMesh_GO.transform.parent = leftController.transform;
        leftMesh_GO.transform.localPosition = Vector3.zero;
        leftMesh_GO.transform.localRotation = Quaternion.Euler(Vector3.zero);
        leftMesh_GO.transform.localScale = new Vector3(-1, 1, 1);

        // =============================================================================================================

        // ============================== SETEANDO LOS HANDPOSERS DE LAS MANOS =========================================

        //PARA LA MANO DERECHA
        HandPoser rightHandPoser = rightController.GetComponent<HandPoser>();
        Transform[] allChildren = rightMesh_GO.GetComponentsInChildren<Transform>();

        foreach(Transform child in allChildren)
        {
            if (child.gameObject.name == "hands:r_hand_world") rightHandPoser.rig = child;
            if(child.gameObject.name == "hands:b_r_index_ignore")
            {
                GameObject interactionOrigin_Right = new GameObject("Interaction Origin");
                interactionOrigin_Right.transform.parent = child;
                interactionOrigin_Right.transform.localPosition = Vector3.zero;
                interactionOrigin_Right.transform.localRotation = Quaternion.Euler(new Vector3(0, 80, 75));
                interactionOrigin_Right.transform.localScale = Vector3.one;
                rightInteraction.interactionOrigin = interactionOrigin_Right.transform;
            }
        }

        rightHandPoser.parent = rightMesh_GO.transform;
        HandPose_SO rightOpened = (HandPose_SO)AssetDatabase.LoadAssetAtPath(rightOpenedSOPath, typeof(HandPose_SO));
        HandPose_SO rightClosed = (HandPose_SO)AssetDatabase.LoadAssetAtPath(rightClosedSOPath, typeof(HandPose_SO));
        rightHandPoser.openedPose = rightOpened;
        rightHandPoser.closedPose = rightClosed;
        rightHandPoser.controller = rightController.GetComponent<XRController>();

        //PARA LA MANO IZQUIERDA
        HandPoser leftHandPoser = leftController.GetComponent<HandPoser>();
        allChildren = leftMesh_GO.GetComponentsInChildren<Transform>();

        foreach(Transform child in allChildren)
        {
            if(child.gameObject.name == "hands:r_hand_world") leftHandPoser.rig = child;
            if (child.gameObject.name == "hands:b_r_index_ignore")
            {
                GameObject interactionOrigin_Left = new GameObject("Interaction Origin");
                interactionOrigin_Left.transform.parent = child;
                interactionOrigin_Left.transform.localPosition = Vector3.zero;
                interactionOrigin_Left.transform.localRotation = Quaternion.Euler(new Vector3(0, 80, 75));
                interactionOrigin_Left.transform.localScale = Vector3.one;
                leftInteraction.interactionOrigin = interactionOrigin_Left.transform;
            }
        }

        leftHandPoser.parent = leftMesh_GO.transform;
        HandPose_SO leftOpened = (HandPose_SO)AssetDatabase.LoadAssetAtPath(leftOpenedSOPath, typeof(HandPose_SO));
        HandPose_SO leftClosed = (HandPose_SO)AssetDatabase.LoadAssetAtPath(leftClosedSOPath, typeof(HandPose_SO));
        leftHandPoser.openedPose = leftOpened;
        leftHandPoser.closedPose = leftClosed;
        leftHandPoser.controller = leftController.GetComponent<XRController>();
        
        
        // =============================================================================================================
    }

    [MenuItem("Tesicnor/Players/PlayerControllers_Controllers")]
    public static void CreatePlayerControllers_Controllers()
    {
        GameObject playerControllers = new GameObject("PlayerControllers", typeof(CameraOffset));
        if (Selection.gameObjects.Length > 0) playerControllers.transform.parent = Selection.gameObjects[0].transform;
        playerControllers.transform.localPosition = Vector3.zero;
        playerControllers.transform.localRotation = Quaternion.Euler(Vector3.zero);
        playerControllers.transform.localScale = Vector3.one;

        // ================================== CREANDO LOS HIJOS DIRECTOS ==============================================
        GameObject cameraOffset = new GameObject("Camera Offset");
        cameraOffset.transform.parent = playerControllers.transform;
        cameraOffset.transform.localPosition = Vector3.zero;
        cameraOffset.transform.localRotation = Quaternion.Euler(Vector3.zero);
        cameraOffset.transform.localScale = Vector3.one;

        GameObject rightController = new GameObject("Right Controller", typeof(TrackedPoseDriver), typeof(UnityEngine.XR.Interaction.Toolkit.XRController), typeof(HandInteraction), typeof(GrippingHand));
        rightController.transform.parent = playerControllers.transform;
        rightController.transform.localPosition = Vector3.zero;
        rightController.transform.localRotation = Quaternion.Euler(Vector3.zero);
        rightController.transform.localScale = Vector3.one;

        GameObject leftController = new GameObject("Left Controller", typeof(TrackedPoseDriver), typeof(UnityEngine.XR.Interaction.Toolkit.XRController), typeof(HandInteraction), typeof(GrippingHand));
        leftController.transform.parent = playerControllers.transform;
        leftController.transform.localPosition = Vector3.zero;
        leftController.transform.localRotation = Quaternion.Euler(Vector3.zero);
        leftController.transform.localScale = Vector3.one;

        GameObject bodyHolder = new GameObject("Body Holder", typeof(SnapToBody));
        bodyHolder.transform.parent = playerControllers.transform;
        bodyHolder.transform.localPosition = new Vector3(0, -0.34f, 0);
        bodyHolder.transform.localRotation = Quaternion.Euler(Vector3.zero);
        bodyHolder.transform.localScale = Vector3.one;

        //=============================================================================================================

        // ============================= SETEANDO EL MANDO DERECHO ====================================================

        TrackedPoseDriver rightPose = rightController.GetComponent<TrackedPoseDriver>();
        rightPose.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRController, TrackedPoseDriver.TrackedPose.RightPose);

        //Seteando la clase GrippingHand
        GrippingHand rightGripping = rightController.GetComponent<GrippingHand>();
        rightGripping.isController = true;
        rightGripping.handType = GrippingHand.HandType.right;
        rightGripping.colliderRadius = 0.05f;
        rightGripping.colliderBone = rightController.transform;
        rightGripping.player = playerControllers.transform;
        rightGripping.hideOnGrab = false;
        rightGripping.handController = rightController.GetComponent<XRController>();

        //Seteando la clase HandInteraction
        HandInteraction rightInteraction = rightController.GetComponent<HandInteraction>();
        rightInteraction.isHandControlled = false;
        rightInteraction.isLeftHand = false;
        rightInteraction.usesRay = true;
        rightInteraction.lineRenderer = rightController.GetComponent<LineRenderer>();
        rightInteraction.interactionOrigin = rightController.transform;
        rightInteraction.nonDetectedColor = new Color(255, 35, 35, 255);
        rightInteraction.detectedColor = new Color(255, 149, 25, 255);
        rightInteraction.handController = rightController.GetComponent<XRController>();

        LineRenderer lineRenderer_right = rightController.GetComponent<LineRenderer>();
        lineRenderer_right.startWidth = 0.010f;
        lineRenderer_right.endWidth = 0.015f;
        lineRenderer_right.enabled = false;
        lineRenderer_right.material = (Material)AssetDatabase.LoadAssetAtPath(lineRendererMatPath, typeof(Material));

        //=============================================================================================================

        // ================================ SETEANDO LA MANO IZQUIERDA ================================================

        TrackedPoseDriver leftPose = leftController.GetComponent<TrackedPoseDriver>();
        leftPose.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRController, TrackedPoseDriver.TrackedPose.LeftPose);

        //Seteando el componente GrippingHand
        GrippingHand leftGripping = leftController.GetComponent<GrippingHand>();
        leftGripping.isController = true;
        leftGripping.handType = GrippingHand.HandType.left;
        leftGripping.colliderRadius = 0.05f;
        leftGripping.colliderBone = leftController.transform;
        leftGripping.player = playerControllers.transform;
        leftGripping.hideOnGrab = false;
        leftGripping.handController = leftController.GetComponent<XRController>();
        leftGripping.handController.controllerNode = XRNode.LeftHand;

        //Seteando el componente HandInteraction
        HandInteraction leftInteraction = leftController.GetComponent<HandInteraction>();
        leftInteraction.isHandControlled = false;
        leftInteraction.isLeftHand = true;
        leftInteraction.usesRay = true;
        leftInteraction.lineRenderer = leftController.GetComponent<LineRenderer>();
        leftInteraction.interactionOrigin = leftController.transform;
        leftInteraction.nonDetectedColor = new Color(255, 35, 35, 255);
        leftInteraction.detectedColor = new Color(255, 149, 25, 255);
        leftInteraction.handController = leftController.GetComponent<XRController>();

        LineRenderer lineRenderer_left = leftController.GetComponent<LineRenderer>();
        lineRenderer_left.startWidth = 0.010f;
        lineRenderer_left.endWidth = 0.015f;
        lineRenderer_left.enabled = false;
        lineRenderer_left.material = (Material)AssetDatabase.LoadAssetAtPath(lineRendererMatPath, typeof(Material));

        //=============================================================================================================

        //====================================== SETEANDO LA CAMARA ===================================================
        GameObject mainCamera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(TrackedPoseDriver));
        mainCamera.transform.parent = cameraOffset.transform;
        mainCamera.transform.localPosition = Vector3.zero;
        mainCamera.transform.localRotation = Quaternion.Euler(Vector3.zero);
        mainCamera.transform.localScale = Vector3.one;

        TrackedPoseDriver cameraPose = mainCamera.GetComponent<TrackedPoseDriver>();
        cameraPose.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRDevice, TrackedPoseDriver.TrackedPose.Center);

        Camera camera = mainCamera.GetComponent<Camera>();
        camera.nearClipPlane = 0.01f;
        camera.farClipPlane = 1000;

        SnapToBody snap = bodyHolder.GetComponent<SnapToBody>();
        snap.camera = mainCamera.transform;

        // =============================================================================================================

        // ================================ SETEANDO LA GEOMETRIA DE LAS MANOS =========================================

        Object rightMesh = AssetDatabase.LoadAssetAtPath(quest2ControllerPath, typeof(Object)) as Object;
        GameObject BothControllers = (GameObject)GameObject.Instantiate(rightMesh);

        Transform[] controllers = BothControllers.GetComponentsInChildren<Transform>();

        GameObject leftMesh_GO = null;
        GameObject rightMesh_GO = null;
        foreach (Transform controller in controllers)
        {
            if (controller.name == "OculusQuest2ControllerLeft") leftMesh_GO = controller.gameObject;
            else if (controller.name == "OculusQuest2ControllerRight") rightMesh_GO = controller.gameObject;
        }

        leftMesh_GO.transform.parent = leftController.transform;
        leftMesh_GO.transform.localPosition = Vector3.zero;
        leftMesh_GO.transform.localRotation = Quaternion.Euler(Vector3.zero);
        leftMesh_GO.transform.localScale = new Vector3(10, 10, 10);

        rightMesh_GO.transform.parent = rightController.transform;
        rightMesh_GO.transform.localPosition = Vector3.zero;
        rightMesh_GO.transform.localRotation = Quaternion.Euler(Vector3.zero);
        rightMesh_GO.transform.localScale = new Vector3(10, 10, 10);

        GameObject.DestroyImmediate(BothControllers.gameObject);
        // =============================================================================================================



        // =============================================================================================================
    }

    [MenuItem("Tesicnor/Players/PlayerHands")]
    public static void CreatePlayerHands()
    {
        GameObject playerHands = new GameObject("Player Hands", typeof(CharacterController), typeof(OVRPlayerController), typeof(OVRSceneSampleController), typeof(OVRDebugInfo));
        if (Selection.gameObjects.Length > 0) playerHands.transform.parent = Selection.gameObjects[0].transform;
        playerHands.transform.localPosition = Vector3.zero;
        playerHands.transform.localRotation = Quaternion.Euler(Vector3.zero);
        playerHands.transform.localScale = Vector3.one;

        //CREANDO EL PRIMER HIJO

        GameObject ovrCameraRig = new GameObject("OVR Camera Rig", typeof(OVRCameraRig), typeof(OVRManager), typeof(OVRHeadsetEmulator));
        ovrCameraRig.transform.parent = playerHands.transform;
        ovrCameraRig.transform.localPosition = Vector3.zero;
        ovrCameraRig.transform.localRotation = Quaternion.Euler(Vector3.zero);
        ovrCameraRig.transform.localScale = Vector3.one;

        OVRManager manager = ovrCameraRig.GetComponent<OVRManager>();
        manager.AllowRecenter = true;
        manager.trackingOriginType = OVRManager.TrackingOrigin.FloorLevel;

        Transform[] allChilds = ovrCameraRig.GetComponentsInChildren<Transform>();

        GameObject rightHand_GO = null;
        GameObject leftHand_GO = null;

        HandPoseDetector poseDetector_right = null;
        HandPoseDetector poseDetector_left = null;
        foreach (Transform child in allChilds)
        {
            if(child.gameObject.name == "RightHandAnchor")
            {
                GameObject rightHand = (GameObject)AssetDatabase.LoadAssetAtPath(rightHandPrefabPath, typeof(GameObject));
                rightHand_GO = GameObject.Instantiate(rightHand);
                rightHand_GO.transform.parent = child;
                rightHand_GO.transform.localPosition = Vector3.zero;
                rightHand_GO.transform.localRotation = Quaternion.Euler(Vector3.zero);
                rightHand_GO.transform.localScale = Vector3.one;

                GameObject handPoseDetector_right = new GameObject("PoseDetector_Right", typeof(HandPoseDetector));
                handPoseDetector_right.transform.parent = child;

                poseDetector_right = handPoseDetector_right.GetComponent<HandPoseDetector>();
                poseDetector_right.skeleton = rightHand_GO.GetComponent<OVRCustomSkeleton>();
                //poseDetector_right.skeleton.TryAutoMapBonesByName();
                
            }
            else if(child.gameObject.name == "LeftHandAnchor")
            {
                GameObject leftHand = (GameObject)AssetDatabase.LoadAssetAtPath(leftHandPrefabPath, typeof(GameObject));
                leftHand_GO = GameObject.Instantiate(leftHand);
                leftHand_GO.transform.parent = child;
                leftHand_GO.transform.localPosition = Vector3.zero;
                leftHand_GO.transform.localRotation = Quaternion.Euler(Vector3.zero);
                leftHand_GO.transform.localScale = Vector3.one;

                GameObject handPoseDetector_left = new GameObject("PoseDetector_Left", typeof(HandPoseDetector));
                handPoseDetector_left.transform.parent = child;

                poseDetector_left = handPoseDetector_left.GetComponent<HandPoseDetector>();
                poseDetector_left.skeleton = leftHand_GO.GetComponent<OVRCustomSkeleton>();
                //poseDetector_left.skeleton.TryAutoMapBonesByName();
            }
        }

        //Adding core components to the hands

        //FOR RIGHT HAND
        GrippingHand rightGripping = rightHand_GO.AddComponent<GrippingHand>(); HandInteraction rightInteraction = rightHand_GO.AddComponent<HandInteraction>();

        rightGripping.isController = false;
        rightGripping.handType = GrippingHand.HandType.right;
        rightGripping.colliderRadius = 0.08f;
        rightGripping.player = playerHands.transform;
        rightGripping.hideOnGrab = false;

        allChilds = rightHand_GO.GetComponentsInChildren<Transform>();

        foreach (Transform child in allChilds) if (child.gameObject.name == "r_middle_mcp_fe_axis_marker") rightGripping.colliderBone = child;

        rightInteraction.isHandControlled = true;
        rightInteraction.usesRay = false;
        rightInteraction.isLeftHand = false;
        rightInteraction.fingerCube = new Vector3(0.03f, 0.03f, 0.03f);
        rightInteraction.lineRenderer = rightHand_GO.GetComponent<LineRenderer>();
        
        foreach(Transform child in allChilds)
        {
            if (child.gameObject.name == "b_r_index3") rightInteraction.indice = child;
            else if (child.gameObject.name == "b_r_middle3") rightInteraction.dedoCorazon = child;
            else if (child.gameObject.name == "b_r_pinky3") rightInteraction.dedoMenique = child;
            else if (child.gameObject.name == "b_r_ring3") rightInteraction.dedoAnular = child;
            else if (child.gameObject.name == "b_r_thumb3") rightInteraction.dedoGordo = child;
        }

        rightInteraction.lineRenderer.startWidth = 0.010f;
        rightInteraction.lineRenderer.endWidth = 0.015f;
        rightInteraction.lineRenderer.enabled = false;
        rightInteraction.lineRenderer.material = (Material)AssetDatabase.LoadAssetAtPath(lineRendererMatPath, typeof(Material));
        rightInteraction.poseDetector = poseDetector_right;

        //FOR LEFT HAND

        GrippingHand leftGripping = leftHand_GO.AddComponent<GrippingHand>(); HandInteraction leftInteraction = leftHand_GO.AddComponent<HandInteraction>();

        leftGripping.isController = false;
        leftGripping.handType = GrippingHand.HandType.left;
        leftGripping.colliderRadius = 0.08f;
        leftGripping.player = playerHands.transform;
        leftGripping.hideOnGrab = false;

        allChilds = leftHand_GO.GetComponentsInChildren<Transform>();

        foreach (Transform child in allChilds) if (child.gameObject.name == "l_middle_mcp_fe_axis_marker") leftGripping.colliderBone = child;

        leftInteraction.isHandControlled = true;
        leftInteraction.usesRay = false;
        leftInteraction.isLeftHand = true;
        leftInteraction.fingerCube = new Vector3(0.03f, 0.03f, 0.03f);
        leftInteraction.lineRenderer = leftHand_GO.GetComponent<LineRenderer>();

        foreach (Transform child in allChilds)
        {
            if (child.gameObject.name == "b_l_index3") leftInteraction.indice = child;
            else if (child.gameObject.name == "b_l_middle3") leftInteraction.dedoCorazon = child;
            else if (child.gameObject.name == "b_l_pinky3") leftInteraction.dedoMenique = child;
            else if (child.gameObject.name == "b_l_ring3") leftInteraction.dedoAnular = child;
            else if (child.gameObject.name == "b_l_thumb3") leftInteraction.dedoGordo = child;
        }

        leftInteraction.lineRenderer.startWidth = 0.010f;
        leftInteraction.lineRenderer.endWidth = 0.015f;
        leftInteraction.lineRenderer.enabled = false;
        leftInteraction.lineRenderer.material = (Material)AssetDatabase.LoadAssetAtPath(lineRendererMatPath, typeof(Material));
        leftInteraction.poseDetector = poseDetector_left;
    }
    #endregion
#endif
}
