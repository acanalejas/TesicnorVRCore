using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class EPISCreation 
{
    public static string CascoFBXPath = "Packages/com.tesicnor.tesicnorvrcore/Runtime/TesicnorVRCORE/Pseudo-Core/Models/EPIS/Casco/CascoEolica.fbx";
    public static string ArnesFBXPath = "Packages/com.tesicnor.tesicnorvrcore/Runtime/TesicnorVRCORE/Pseudo-Core/Models/EPIS/Arnes/ArnesSinPiernas.fbx";
    public static string RanaFBXPath = "Packages/com.tesicnor.tesicnorvrcore/Runtime/TesicnorVRCORE/Pseudo-Core/Models/EPIS/Rana/LineaVidaRANA.fbx";

    public static string CascoMatPath = "Packages/com.tesicnor.tesicnorvrcore/Runtime/TesicnorVRCORE/Pseudo-Core/Models/EPIS/Casco/Casco_mat.mat";
    public static string ArnesMatPath = "Packages/com.tesicnor.tesicnorvrcore/Runtime/TesicnorVRCORE/Pseudo-Core/Models/EPIS/Arnes/Arnes_Mat.mat";

    public static string CableMatPath = "Packages/com.tesicnor.tesicnorvrcore/Runtime/TesicnorVRCORE/Pseudo-Core/Materials/Cable_mat.mat";
#if UNITY_EDITOR
    #region FUNCTIONS
    [MenuItem("Tesicnor/EPIS/Casco")]
    public static void Create_Casco()
    {
        Object casco_obj = (Object)AssetDatabase.LoadAssetAtPath(CascoFBXPath, typeof(Object));
        GameObject casco_GO = (GameObject)GameObject.Instantiate(casco_obj);
        casco_GO.gameObject.name = "Casco";

        if (Selection.gameObjects.Length > 0) casco_GO.transform.parent = Selection.gameObjects[0].transform;
        casco_GO.transform.localPosition = Vector3.zero;
        casco_GO.transform.localRotation = Quaternion.Euler(Vector3.zero);
        casco_GO.transform.localScale = new Vector3(12, 12, 12);

        casco_GO.AddComponent<BoxCollider>();
        VRCollider casco_collider2 = casco_GO.AddComponent<VRCollider>();
        //casco_collider = new VRCollider();

        if (casco_collider2 == null) Debug.Log("is null");
        casco_collider2.simulateOnDrop = true;
        casco_collider2.canRelease = false;
        casco_collider2.DropTeleport = false;

        Material cascoMat = (Material)AssetDatabase.LoadAssetAtPath(CascoMatPath, typeof(Material));
        MeshRenderer cascoMesh = casco_GO.GetComponent<MeshRenderer>();
        cascoMesh.material = cascoMat;

        //return casco_GO;
    }

    [MenuItem("Tesicnor/EPIS/Arnes")]
    public static void Create_Arnes()
    {
        Object arnes_obj = (Object)AssetDatabase.LoadAssetAtPath(ArnesFBXPath, typeof(Object));
        GameObject arnes_GO = (GameObject)GameObject.Instantiate(arnes_obj);

        if (Selection.gameObjects.Length > 0) arnes_GO.transform.parent = Selection.gameObjects[0].transform;
        arnes_GO.transform.localPosition = Vector3.zero;
        arnes_GO.transform.localRotation = Quaternion.Euler(Vector3.zero);
        arnes_GO.transform.localScale = Vector3.one;
        arnes_GO.name = "Arnes";

        arnes_GO.AddComponent<BoxCollider>();
        VRCollider arnes_collider = arnes_GO.AddComponent<VRCollider>();
        arnes_collider.simulateOnDrop = true;
        arnes_collider.canRelease = false;
        arnes_collider.DropTeleport = false;

        Material arnesMat = (Material)AssetDatabase.LoadAssetAtPath(ArnesMatPath, typeof(Material));
        MeshRenderer arnesMesh = arnes_GO.GetComponent<MeshRenderer>();
        arnesMesh.material = arnesMat;

        Create_Rana(arnes_GO);

        //return arnes_GO;
    }

    [MenuItem("Tesicnor/EPIS/Rana")]
    public static void Create_Rana()
    {
        GameObject rana_parent = new GameObject("Rana");
        
        Object rana_obj = (Object)AssetDatabase.LoadAssetAtPath(RanaFBXPath, typeof(Object));
        GameObject rana_GO = (GameObject)GameObject.Instantiate(rana_obj);
        rana_GO.transform.parent = rana_parent.transform;

        if(Selection.gameObjects.Length > 0) rana_parent.transform.parent = Selection.gameObjects[0].transform;

        GameObject arnes = GameObject.Find("ArnesSinPiernas(Clone)");
        if (arnes == null) arnes = GameObject.Find("ArnesSinPiernas");
        if (arnes == null) arnes = GameObject.Find("Arnes");
        if (arnes == null) arnes = GameObject.Find("arnes");

        if (arnes) rana_parent.transform.parent = arnes.transform;

        rana_GO.transform.localPosition = new Vector3(-0.1323f, 0.102f, 0.011f);
        rana_GO.transform.localRotation = Quaternion.Euler(Vector3.zero);
        rana_GO.transform.localScale = Vector3.one;

        GameObject rana_holder = new GameObject("Rana Holder");
        rana_holder.transform.parent = rana_GO.transform.parent;
        rana_holder.transform.position = rana_GO.transform.position;
        rana_holder.transform.rotation = rana_GO.transform.rotation;

        Transform[] allChilds = rana_GO.GetComponentsInChildren<Transform>();

        Transform mosqueton = null;
        Transform leva = null;

        GameObject mosqueton_holder = new GameObject("Mosqueton Holder");
        mosqueton_holder.transform.parent = rana_GO.transform.parent;
        GameObject leva_holder = new GameObject("Leva Holder");
        leva_holder.transform.parent = rana_GO.transform.parent;

        foreach (Transform child in allChilds)
        {
            if (child.gameObject.name == "Mosquetón") mosqueton = child;
            else if (child.gameObject.name == "Leva") leva = child;
        }

        if (mosqueton)
        {
            mosqueton.transform.localPosition = new Vector3(0.0769f, 0.036f, -0.0186f);
            mosqueton.transform.localRotation = Quaternion.Euler(new Vector3(-38.137f, 17.286f, 51.683f));

            
            mosqueton_holder.transform.position = mosqueton.transform.position;
            mosqueton_holder.transform.rotation = mosqueton.transform.rotation;
        }
        if(leva)
        {
            leva_holder.transform.position = leva.position;
            leva_holder.transform.rotation = leva.rotation;
        }

        GameObject cable_origen = new GameObject("Cable Origen");
        if (leva) cable_origen.transform.parent = leva.transform;
        else cable_origen.transform.parent = rana_GO.transform;

        if (leva) cable_origen.transform.localPosition = new Vector3(-0.07291f, 0.00049f);
        else cable_origen.transform.localPosition = new Vector3(-0.02955414f, -0.007746905f, -0.008347806f);

        GameObject cable_final = new GameObject("Cable Final");
        cable_final.transform.parent = rana_GO.transform;
        cable_final.transform.localPosition = new Vector3(-0.02955414f, -0.007746905f, -0.008347806f);

        rana_GO.AddComponent<BoxCollider>();
        Rana rana_rana = rana_GO.AddComponent<Rana>();

        rana_rana.mosqueton = mosqueton;
        rana_rana.mosqueton_holder = mosqueton_holder.transform;
        if(arnes)rana_rana.arnes = arnes.transform;
        rana_rana.cableOrigen = leva;
        rana_rana.cableOrigen_holder = leva_holder.transform;
        rana_rana.cable_origen = cable_origen.transform;
        rana_rana.cable_target = cable_final.transform;
        rana_rana.rana_holder = rana_holder.transform;
        rana_rana.release = VRCollider.releaseType.holder;
        rana_rana.holder = rana_holder.transform;

        LineRenderer lineRenderer = rana_GO.GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.010f;
        lineRenderer.endWidth = 0.015f;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, cable_origen.transform.position);
        lineRenderer.SetPosition(0, cable_final.transform.position);
        lineRenderer.material = (Material) AssetDatabase.LoadAssetAtPath(CableMatPath, typeof(Material));

    }

    public static void Create_Rana(GameObject arnes)
    {
        GameObject rana_parent = new GameObject("Rana");
        Object rana_obj = (Object)AssetDatabase.LoadAssetAtPath(RanaFBXPath, typeof(Object));
        GameObject rana_GO = (GameObject)GameObject.Instantiate(rana_obj);
        rana_GO.transform.parent = rana_parent.transform;

        if (Selection.gameObjects.Length > 0) rana_parent.transform.parent = Selection.gameObjects[0].transform;

        if (arnes) rana_parent.transform.parent = arnes.transform;

        rana_GO.transform.localPosition = new Vector3(-0.1323f, 0.102f, 0.011f);
        rana_GO.transform.localRotation = Quaternion.Euler(Vector3.zero);
        rana_GO.transform.localScale = Vector3.one;

        GameObject rana_holder = new GameObject("Rana Holder");
        rana_holder.transform.parent = rana_GO.transform.parent;
        rana_holder.transform.position = rana_GO.transform.position;
        rana_holder.transform.rotation = rana_GO.transform.rotation;

        Transform[] allChilds = rana_GO.GetComponentsInChildren<Transform>();

        Transform mosqueton = null;
        Transform leva = null;

        GameObject mosqueton_holder = new GameObject("Mosqueton Holder");
        mosqueton_holder.transform.parent = rana_GO.transform.parent;
        GameObject leva_holder = new GameObject("Leva Holder");
        leva_holder.transform.parent = rana_GO.transform.parent;

        foreach (Transform child in allChilds)
        {
            if (child.gameObject.name == "Mosquetón") mosqueton = child;
            else if (child.gameObject.name == "Leva") leva = child;
        }

        if (mosqueton)
        {
            mosqueton.transform.localPosition = new Vector3(0.0769f, 0.036f, -0.0186f);
            mosqueton.transform.localRotation = Quaternion.Euler(new Vector3(-38.137f, 17.286f, 51.683f));


            mosqueton_holder.transform.position = mosqueton.transform.position;
            mosqueton_holder.transform.rotation = mosqueton.transform.rotation;
        }
        if (leva)
        {
            leva_holder.transform.position = leva.position;
            leva_holder.transform.rotation = leva.rotation;
        }

        GameObject cable_origen = new GameObject("Cable Origen");
        if (leva) cable_origen.transform.parent = leva.transform;
        else cable_origen.transform.parent = rana_GO.transform;

        if (leva) cable_origen.transform.localPosition = new Vector3(-0.07291f, 0.00049f);
        else cable_origen.transform.localPosition = new Vector3(-0.02955414f, -0.007746905f, -0.008347806f);

        GameObject cable_final = new GameObject("Cable Final");
        cable_final.transform.parent = rana_GO.transform;
        cable_final.transform.localPosition = new Vector3(-0.02955414f, -0.007746905f, -0.008347806f);

        rana_GO.AddComponent<BoxCollider>();
        Rana rana_rana = rana_GO.AddComponent<Rana>();

        rana_rana.mosqueton = mosqueton;
        rana_rana.mosqueton_holder = mosqueton_holder.transform;
        rana_rana.arnes = arnes.transform;
        rana_rana.cableOrigen = leva;
        rana_rana.cableOrigen_holder = leva_holder.transform;
        rana_rana.cable_origen = cable_origen.transform;
        rana_rana.cable_target = cable_final.transform;
        rana_rana.rana_holder = rana_holder.transform;

        LineRenderer lineRenderer = rana_GO.GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.010f;
        lineRenderer.endWidth = 0.015f;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, cable_origen.transform.position);
        lineRenderer.SetPosition(0, cable_final.transform.position);
        lineRenderer.material = (Material)AssetDatabase.LoadAssetAtPath(CableMatPath, typeof(Material));
    }

    [MenuItem("Tesicnor/EPIS/Todos")]
    public static void Create_Todos()
    {
        Create_Casco();
        Create_Arnes();
    }

    [MenuItem("Tesicnor/EPIS/Todos con misiones")]
    public static void Create_TodosConMisiones()
    {
        Create_Casco();
        GameObject casco_GO = GameObject.Find("Casco");
        Create_Arnes();
        GameObject arnes_GO = GameObject.Find("Arnes");
        GameObject rana_GO = arnes_GO.GetComponentInChildren<Rana>().gameObject;

        
        VRCollider casco_collider = casco_GO.GetComponent<VRCollider>();
        VRCollider arnes_collider = arnes_GO.GetComponent<VRCollider>();
        Rana rana_rana = rana_GO.GetComponent<Rana>();

        casco_collider.DropTeleport = true;
        casco_collider.canRelease = false;
        casco_collider.hasTarget = true;
        casco_collider.attachmentMode = VRCollider.AttachmentMode.Normal;

        arnes_collider.DropTeleport = true;
        arnes_collider.canRelease = false;
        arnes_collider.hasTarget = true;
        arnes_collider.attachmentMode = VRCollider.AttachmentMode.Normal;

        rana_rana.DropTeleport = true;
        rana_rana.canRelease = false;
        rana_rana.hasTarget = true;
        rana_rana.attachmentMode = VRCollider.AttachmentMode.Normal;

        GameObject _TaskManager = null;
#if UNITY_2023_OR_NEWER
        if(GameObject.FindFirstObjectByType<TaskManager>()) _TaskManager = GameObject.FindFirstObjectByType<TaskManager>().gameObject;
#endif
#if UNITY_2021
        if (GameObject.FindObjectOfType<TaskManager>()) _TaskManager = GameObject.FindObjectOfType<TaskManager>().gameObject;
#endif
        if (!_TaskManager)
        {
            _TaskManager = new GameObject("Task Manager", typeof(TaskManager));

            if (Selection.gameObjects.Length > 0) _TaskManager.transform.parent = Selection.gameObjects[0].transform;
            _TaskManager.transform.localPosition = Vector3.zero;
            _TaskManager.transform.localRotation = Quaternion.Euler(Vector3.zero);
            _TaskManager.transform.localScale = Vector3.one;
        }

        //PARA AGARRAR EL CASCO
        GameObject grabHelmet = new GameObject("Grab Helmet", typeof(GrabHelmet));
        grabHelmet.transform.parent = _TaskManager.transform;
        grabHelmet.transform.localPosition = Vector3.zero;
        grabHelmet.transform.localRotation = Quaternion.Euler(Vector3.zero);
        grabHelmet.transform.localScale = Vector3.one;

        GrabHelmet grabHelmetTask = grabHelmet.GetComponent<GrabHelmet>();
        grabHelmetTask.helmetCollider = casco_collider;

        // PARA SOLTAR EL CASCO
        GameObject releaseHelmet = new GameObject("Release Helmet", typeof(ReleaseHelmet));
        releaseHelmet.transform.parent = _TaskManager.transform;
        releaseHelmet.transform.localPosition = Vector3.zero;
        releaseHelmet.transform.localRotation = Quaternion.Euler(Vector3.zero);
        releaseHelmet.transform.localScale = Vector3.one;

        GameObject helmetTarget = new GameObject("HelmetTarget", typeof(VRColliderReleaseTarget));
        helmetTarget.transform.parent = _TaskManager.transform.parent;
        helmetTarget.transform.localPosition = Vector3.zero;
        helmetTarget.transform.localRotation = Quaternion.Euler(Vector3.zero);
        helmetTarget.transform.localScale = Vector3.one;

        VRColliderReleaseTarget helmetColliderTarget = helmetTarget.GetComponent<VRColliderReleaseTarget>();
        helmetColliderTarget.DisableWhenRelease = false;
        helmetColliderTarget.seeksTarget = true;
        helmetColliderTarget.canReleaseObject = true;
        helmetColliderTarget.canBeCanceled = false;
        helmetColliderTarget.needsGrabbing = true;
        casco_collider.target = helmetColliderTarget;

        ReleaseHelmet releaseHelmetTask = releaseHelmet.GetComponent<ReleaseHelmet>();
        releaseHelmetTask.helmetCollider = casco_collider;
        releaseHelmetTask.helmetColliderTarget = helmetColliderTarget;

        // PARA AGARRAR EL ARNES
        GameObject grabArnes = new GameObject("Grab Arnes", typeof(GrabArnes));
        grabArnes.transform.parent = _TaskManager.transform;
        grabArnes.transform.localPosition = Vector3.zero;
        grabArnes.transform.localRotation = Quaternion.Euler(Vector3.zero);
        grabArnes.transform.localScale = Vector3.one;

        GrabArnes grabArnesTask = grabArnes.GetComponent<GrabArnes>();
        grabArnesTask.arnesCollider = arnes_collider;

        // PARA SOLTAR EL ARNES
        GameObject releaseArnes = new GameObject("GrabArnes", typeof(ReleaseArnes));
        releaseArnes.transform.parent = _TaskManager.transform;
        releaseArnes.transform.localPosition = Vector3.zero;
        releaseArnes.transform.localRotation = Quaternion.Euler(Vector3.zero);
        releaseArnes.transform.localScale = Vector3.one;

        GameObject arnesTarget = new GameObject("Arnes Target", typeof(VRColliderReleaseTarget));
        arnesTarget.transform.parent = _TaskManager.transform.parent;
        arnesTarget.transform.localPosition = Vector3.zero;
        arnesTarget.transform.localRotation = Quaternion.Euler(Vector3.zero);
        arnesTarget.transform.localScale = Vector3.one;

        VRColliderReleaseTarget arnesColliderTarget = arnesTarget.GetComponent<VRColliderReleaseTarget>();
        arnesColliderTarget.DisableWhenRelease = false;
        arnesColliderTarget.seeksTarget = true;
        arnesColliderTarget.needsGrabbing = true;
        arnesColliderTarget.canReleaseObject = true;
        arnesColliderTarget.canBeCanceled = false;
        arnes_collider.target = arnesColliderTarget;

        ReleaseArnes releaseArnesTask = releaseArnes.GetComponent<ReleaseArnes>();
        releaseArnesTask.arnesCollider = arnes_collider;
        releaseArnesTask.arnesColliderTarget = arnesColliderTarget;

        // PARA AGARRAR LA RANA
        GameObject grabRana = new GameObject("Grab Rana", typeof(GrabRana));
        grabRana.transform.parent = _TaskManager.transform;
        grabRana.transform.localPosition = Vector3.zero;
        grabRana.transform.localRotation = Quaternion.Euler(Vector3.zero);
        grabRana.transform.localScale = Vector3.one;

        GrabRana grabRanaTask = grabRana.GetComponent<GrabRana>();
        grabRanaTask.rana_collider = rana_rana;

        // PARA SOLTAR LA RANA
        GameObject releaseRana = new GameObject("Release Rana", typeof(ReleaseRana));
        releaseRana.transform.parent = _TaskManager.transform;
        releaseRana.transform.localPosition = Vector3.zero;
        releaseRana.transform.localRotation = Quaternion.Euler(Vector3.zero);
        releaseRana.transform.localScale = Vector3.one;

        GameObject ranaTarget = new GameObject("Rana Target", typeof(VRColliderReleaseTarget));
        ranaTarget.transform.parent = _TaskManager.transform.parent;
        ranaTarget.transform.localPosition = Vector3.zero;
        ranaTarget.transform.localRotation = Quaternion.Euler(Vector3.zero);
        ranaTarget.transform.localScale = Vector3.one;

        VRColliderReleaseTarget ranaColliderTarget = ranaTarget.GetComponent<VRColliderReleaseTarget>();
        ranaColliderTarget.DisableWhenRelease = false;
        ranaColliderTarget.seeksTarget = true;
        ranaColliderTarget.needsGrabbing = true;
        ranaColliderTarget.canReleaseObject = true;
        ranaColliderTarget.canBeCanceled = true;
        rana_rana.target = ranaColliderTarget;

        ReleaseRana releaseRanaTask = releaseRana.GetComponent<ReleaseRana>();
        releaseRanaTask.rana_collider = rana_rana;
        releaseRanaTask.rana_target = ranaColliderTarget;

        //PARA RECUPERAR LA RANA
        GameObject recoverRana = new GameObject("Recover Rana", typeof(RecoverRana));
        recoverRana.transform.parent = _TaskManager.transform;
        recoverRana.transform.localPosition = Vector3.zero;
        recoverRana.transform.localRotation = Quaternion.Euler(Vector3.zero);
        recoverRana.transform.localScale = Vector3.one;

        RecoverRana recoverRanaTask = recoverRana.GetComponent<RecoverRana>();
        recoverRanaTask.rana_collider = rana_rana;
        recoverRanaTask.rana_target = ranaColliderTarget;

        GameObject bodyHolder = GameObject.Find("Body Holder");
        if (!bodyHolder) bodyHolder = GameObject.Find("BodyHolder");
        if (!bodyHolder) bodyHolder = GameObject.Find("bodyHolder");
        if (!bodyHolder) bodyHolder = GameObject.Find("bodyholder");
        if (!bodyHolder) bodyHolder = GameObject.Find("body holder");
        if (!bodyHolder) bodyHolder = GameObject.Find("Body holder");
        if (!bodyHolder) bodyHolder = GameObject.Find("Bodyholder");

        if (bodyHolder)
        { 
            arnesTarget.transform.parent = bodyHolder.transform;
            arnesTarget.transform.localPosition = Vector3.zero;

            BoxCollider targetCollider = arnesTarget.GetComponent<BoxCollider>();
            targetCollider.center = new Vector3(0.03378287f, -0.2350044f, -0.05177772f);
            targetCollider.size = new Vector3(0.2857336f, 0.7880974f, 0.3503982f);
        }

        Camera[] allCameras = GameObject.FindObjectsOfType<Camera>();
        foreach(Camera cam in allCameras)
        {
            if(cam.gameObject.name == "Main Camera" || cam.gameObject.name == "CenterEyeAnchor")
            {
                helmetTarget.transform.parent = cam.transform;
                helmetTarget.transform.localPosition = new Vector3(0, 0.049f, -0.067f);
                helmetTarget.transform.localRotation = Quaternion.Euler(new Vector3(270, 0, 0));

                BoxCollider targetCollider = helmetTarget.GetComponent<BoxCollider>();
                targetCollider.center = new Vector3(0.02210477f, -0.03f, -0.06527613f);
                targetCollider.size = new Vector3(0.2738602f, 0.2953396f, 0.3285074f);
            }
        }

        grabHelmetTask.enabled = false;
        releaseHelmetTask.enabled = false;
        grabArnesTask.enabled = false;
        releaseArnesTask.enabled = false;
        grabRanaTask.enabled = false;
        releaseRanaTask.enabled = false;
        recoverRanaTask.enabled = false;

        TaskManager taskManager = _TaskManager.GetComponent<TaskManager>();

        VR_Task[] allTasks = { grabHelmetTask, releaseHelmetTask, grabArnesTask, releaseHelmetTask, grabRanaTask, releaseRanaTask, recoverRanaTask };
        VR_Task[] currentTasks = taskManager.totalTasks;
        if (currentTasks == null) currentTasks = new VR_Task[0];
        List<VR_Task> newTasks = new List<VR_Task>();
        newTasks.AddRange(allTasks);
        newTasks.AddRange(currentTasks);

        taskManager.totalTasks = newTasks.ToArray();
    }

    [MenuItem("Tesicnor/EPIS/Misiones")]
    public static void Create_Misiones()
    {
        GameObject casco_GO = GameObject.Find("CascoEolica(Clone)");
        GameObject arnes_GO = GameObject.Find("Arnes");
        GameObject rana_GO = null;

        if (arnes_GO) rana_GO = arnes_GO.GetComponentInChildren<Rana>().gameObject;

        VRCollider casco_collider = null;
        VRCollider arnes_collider = null;
        Rana rana_rana = null;

        if (casco_GO) casco_collider = casco_GO.GetComponent<VRCollider>();
        if (arnes_GO) arnes_collider = arnes_GO.GetComponent<VRCollider>();
        if (rana_GO) rana_rana = rana_GO.GetComponent<Rana>();

        GameObject _TaskManager = null;
#if UNITY_2023_OR_NEWER
        if (GameObject.FindFirstObjectByType<TaskManager>()) _TaskManager = GameObject.FindFirstObjectByType<TaskManager>().gameObject;
#endif

#if UNITY_2021
        if (GameObject.FindObjectOfType<TaskManager>()) _TaskManager = GameObject.FindObjectOfType<TaskManager>().gameObject;
#endif
        if (!_TaskManager)
        {
            _TaskManager = new GameObject("Task Manager", typeof(TaskManager));

            if (Selection.gameObjects.Length > 0) _TaskManager.transform.SetParent(Selection.gameObjects[0].transform);
            _TaskManager.transform.localPosition = Vector3.zero;
            _TaskManager.transform.localRotation = Quaternion.Euler(Vector3.zero);
            _TaskManager.transform.localScale = Vector3.one;
        }

        //PARA AGARRAR EL CASCO
        GameObject grabHelmet = new GameObject("Grab Helmet", typeof(GrabHelmet));
        grabHelmet.transform.SetParent(_TaskManager.transform);
        grabHelmet.transform.localPosition = Vector3.zero;
        grabHelmet.transform.localRotation = Quaternion.Euler(Vector3.zero);
        grabHelmet.transform.localScale = Vector3.one;

        GrabHelmet grabHelmetTask = grabHelmet.GetComponent<GrabHelmet>();
        grabHelmetTask.helmetCollider = casco_collider;

        // PARA SOLTAR EL CASCO
        GameObject releaseHelmet = new GameObject("Release Helmet", typeof(ReleaseHelmet));
        releaseHelmet.transform.parent = _TaskManager.transform;
        releaseHelmet.transform.localPosition = Vector3.zero;
        releaseHelmet.transform.localRotation = Quaternion.Euler(Vector3.zero);
        releaseHelmet.transform.localScale = Vector3.one;

        GameObject helmetTarget = new GameObject("HelmetTarget", typeof(VRColliderReleaseTarget));
        helmetTarget.transform.parent = _TaskManager.transform.parent;
        helmetTarget.transform.localPosition = Vector3.zero;
        helmetTarget.transform.localRotation = Quaternion.Euler(Vector3.zero);
        helmetTarget.transform.localScale = Vector3.one;

        VRColliderReleaseTarget helmetColliderTarget = helmetTarget.GetComponent<VRColliderReleaseTarget>();
        helmetColliderTarget.DisableWhenRelease = false;
        helmetColliderTarget.seeksTarget = true;
        helmetColliderTarget.canReleaseObject = true;
        helmetColliderTarget.canBeCanceled = false;
        helmetColliderTarget.needsGrabbing = true;
        casco_collider.target = helmetColliderTarget;

        ReleaseHelmet releaseHelmetTask = releaseHelmet.GetComponent<ReleaseHelmet>();
        releaseHelmetTask.helmetCollider = casco_collider;
        releaseHelmetTask.helmetColliderTarget = helmetColliderTarget;

        // PARA AGARRAR EL ARNES
        GameObject grabArnes = new GameObject("Grab Arnes", typeof(GrabArnes));
        grabArnes.transform.parent = _TaskManager.transform;
        grabArnes.transform.localPosition = Vector3.zero;
        grabArnes.transform.localRotation = Quaternion.Euler(Vector3.zero);
        grabArnes.transform.localScale = Vector3.one;

        GrabArnes grabArnesTask = grabArnes.GetComponent<GrabArnes>();
        grabArnesTask.arnesCollider = arnes_collider;

        // PARA SOLTAR EL ARNES
        GameObject releaseArnes = new GameObject("GrabArnes", typeof(ReleaseArnes));
        releaseArnes.transform.parent = _TaskManager.transform;
        releaseArnes.transform.localPosition = Vector3.zero;
        releaseArnes.transform.localRotation = Quaternion.Euler(Vector3.zero);
        releaseArnes.transform.localScale = Vector3.one;

        GameObject arnesTarget = new GameObject("Arnes Target", typeof(VRColliderReleaseTarget));
        arnesTarget.transform.parent = _TaskManager.transform.parent;
        arnesTarget.transform.localPosition = Vector3.zero;
        arnesTarget.transform.localRotation = Quaternion.Euler(Vector3.zero);
        arnesTarget.transform.localScale = Vector3.one;

        VRColliderReleaseTarget arnesColliderTarget = arnesTarget.GetComponent<VRColliderReleaseTarget>();
        arnesColliderTarget.DisableWhenRelease = false;
        arnesColliderTarget.seeksTarget = true;
        arnesColliderTarget.needsGrabbing = true;
        arnesColliderTarget.canReleaseObject = true;
        arnesColliderTarget.canBeCanceled = false;
        arnes_collider.target = arnesColliderTarget;

        ReleaseArnes releaseArnesTask = releaseArnes.GetComponent<ReleaseArnes>();
        releaseArnesTask.arnesCollider = arnes_collider;
        releaseArnesTask.arnesColliderTarget = arnesColliderTarget;

        // PARA AGARRAR LA RANA
        GameObject grabRana = new GameObject("Grab Rana", typeof(GrabRana));
        grabRana.transform.parent = _TaskManager.transform;
        grabRana.transform.localPosition = Vector3.zero;
        grabRana.transform.localRotation = Quaternion.Euler(Vector3.zero);
        grabRana.transform.localScale = Vector3.one;

        GrabRana grabRanaTask = grabRana.GetComponent<GrabRana>();
        grabRanaTask.rana_collider = rana_rana;

        // PARA SOLTAR LA RANA
        GameObject releaseRana = new GameObject("Release Rana", typeof(ReleaseRana));
        releaseRana.transform.parent = _TaskManager.transform;
        releaseRana.transform.localPosition = Vector3.zero;
        releaseRana.transform.localRotation = Quaternion.Euler(Vector3.zero);
        releaseRana.transform.localScale = Vector3.one;

        GameObject ranaTarget = new GameObject("Rana Target", typeof(VRColliderReleaseTarget));
        ranaTarget.transform.parent = _TaskManager.transform.parent;
        ranaTarget.transform.localPosition = Vector3.zero;
        ranaTarget.transform.localRotation = Quaternion.Euler(Vector3.zero);
        ranaTarget.transform.localScale = Vector3.one;

        VRColliderReleaseTarget ranaColliderTarget = ranaTarget.GetComponent<VRColliderReleaseTarget>();
        ranaColliderTarget.DisableWhenRelease = false;
        ranaColliderTarget.seeksTarget = true;
        ranaColliderTarget.needsGrabbing = true;
        ranaColliderTarget.canReleaseObject = true;
        ranaColliderTarget.canBeCanceled = true;
        rana_rana.target = ranaColliderTarget;

        ReleaseRana releaseRanaTask = releaseRana.GetComponent<ReleaseRana>();
        releaseRanaTask.rana_collider = rana_rana;
        releaseRanaTask.rana_target = ranaColliderTarget;

        //PARA RECUPERAR LA RANA
        GameObject recoverRana = new GameObject("Recover Rana", typeof(RecoverRana));
        recoverRana.transform.parent = _TaskManager.transform;
        recoverRana.transform.localPosition = Vector3.zero;
        recoverRana.transform.localRotation = Quaternion.Euler(Vector3.zero);
        recoverRana.transform.localScale = Vector3.one;

        RecoverRana recoverRanaTask = recoverRana.GetComponent<RecoverRana>();
        recoverRanaTask.rana_collider = rana_rana;
        recoverRanaTask.rana_target = ranaColliderTarget;

        GameObject bodyHolder = GameObject.Find("Body Holder");
        if (!bodyHolder) bodyHolder = GameObject.Find("BodyHolder");
        if (!bodyHolder) bodyHolder = GameObject.Find("bodyHolder");
        if (!bodyHolder) bodyHolder = GameObject.Find("bodyholder");
        if (!bodyHolder) bodyHolder = GameObject.Find("body holder");
        if (!bodyHolder) bodyHolder = GameObject.Find("Body holder");
        if (!bodyHolder) bodyHolder = GameObject.Find("Bodyholder");

        if (bodyHolder)
        {
            arnesTarget.transform.parent = bodyHolder.transform;
            arnesTarget.transform.localPosition = new Vector3(0, -0.388f, 0);
            arnesTarget.transform.localRotation = Quaternion.Euler(new Vector3(0,90,0));

            BoxCollider targetCollider = arnesTarget.GetComponent<BoxCollider>();
            targetCollider.center = new Vector3(0.03378287f, -0.2350044f, -0.05177772f);
            targetCollider.size = new Vector3(0.2857336f, 0.7880974f, 0.3503982f);
        }

        Camera[] allCameras = GameObject.FindObjectsOfType<Camera>();
        foreach (Camera cam in allCameras)
        {
            if (cam.gameObject.name == "Main Camera" || cam.gameObject.name == "CenterEyeAnchor")
            {
                helmetTarget.transform.parent = cam.transform;
                helmetTarget.transform.localPosition = new Vector3(0, 0.049f, -0.067f);
                helmetTarget.transform.localRotation = Quaternion.Euler(new Vector3(270, 0, 0));

                BoxCollider targetCollider = helmetTarget.GetComponent<BoxCollider>();
                targetCollider.center = new Vector3(0.02210477f, -0.03f, -0.06527613f);
                targetCollider.size = new Vector3(0.2738602f, 0.2953396f, 0.3285074f);
            }
        }

        grabHelmetTask.enabled = false;
        releaseHelmetTask.enabled = false;
        grabArnesTask.enabled = false;
        releaseArnesTask.enabled = false;
        grabRanaTask.enabled = false;
        releaseRanaTask.enabled = false;
        recoverRanaTask.enabled = false;

        TaskManager taskManager = _TaskManager.GetComponent<TaskManager>();

        VR_Task[] allTasks = { grabHelmetTask, releaseHelmetTask, grabArnesTask, releaseArnesTask, grabRanaTask, releaseRanaTask, recoverRanaTask };
        VR_Task[] currentTasks = taskManager.totalTasks;
        if (currentTasks == null) currentTasks = new VR_Task[0];
        List<VR_Task> newTasks = new List<VR_Task>();
        newTasks.AddRange(allTasks);
        newTasks.AddRange(currentTasks);

        taskManager.totalTasks = newTasks.ToArray();
    }
#endregion
#endif
    }
