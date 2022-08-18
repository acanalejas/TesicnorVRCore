using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class EPISCreation 
{
    static string CascoFBXPath = "Packages/com.tesicnor.tesicnorvrcore/Runtime/TesicnorVRCORE/Pseudo-Core/Models/EPIS/Casco/CascoEolica.fbx";
    static string ArnesFBXPath = "Packages/com.tesicnor.tesicnorvrcore/Runtime/TesicnorVRCORE/Pseudo-Core/Models/EPIS/Arnes/ArnesSinPiernas.fbx";
    static string RanaFBXPath = "Packages/com.tesicnor.tesicnorvrcore/Runtime/TesicnorVRCORE/Pseudo-Core/Models/EPIS/Rana/LineaVidaRANA.fbx";

    static string CascoMatPath = "Packages/com.tesicnor.tesicnorvrcore/Runtime/TesicnorVRCORE/Pseudo-Core/Models/EPIS/Casco/Casco_mat.mat";
    static string ArnesMatPath = "Packages/com.tesicnor.tesicnorvrcore/Runtime/TesicnorVRCORE/Pseudo-Core/Models/EPIS/Arnes/Arnes_Mat.mat";

    static string CableMatPath = "Packages/com.tesicnor.tesicnorvrcore/Runtime/TesicnorVRCORE/Pseudo-Core/Materials/Cable_mat.mat";
#if UNITY_EDITOR
    #region FUNCTIONS
    [MenuItem("Tesicnor/EPIS/Casco")]
    public static void Create_Casco()
    {
        Object casco_obj = (Object)AssetDatabase.LoadAssetAtPath(CascoFBXPath, typeof(Object));
        GameObject casco_GO = (GameObject)GameObject.Instantiate(casco_obj);

        if (Selection.gameObjects.Length > 0) casco_GO.transform.parent = Selection.gameObjects[0].transform;
        casco_GO.transform.localPosition = Vector3.zero;
        casco_GO.transform.localRotation = Quaternion.Euler(Vector3.zero);
        casco_GO.transform.localScale = new Vector3(12, 12, 12);

        VRCollider casco_collider = casco_GO.AddComponent<VRCollider>();
        casco_collider.simulateOnDrop = true;
        casco_collider.canRelease = false;
        casco_collider.DropTeleport = false;

        Material cascoMat = (Material)AssetDatabase.LoadAssetAtPath(CascoMatPath, typeof(Material));
        MeshRenderer cascoMesh = casco_GO.GetComponent<MeshRenderer>();
        cascoMesh.material = cascoMat;
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

        VRCollider arnes_collider = arnes_GO.AddComponent<VRCollider>();
        arnes_collider.simulateOnDrop = true;
        arnes_collider.canRelease = false;
        arnes_collider.DropTeleport = false;

        Material arnesMat = (Material)AssetDatabase.LoadAssetAtPath(ArnesMatPath, typeof(Material));
        MeshRenderer arnesMesh = arnes_GO.GetComponent<MeshRenderer>();
        arnesMesh.material = arnesMat;

        Create_Rana(arnes_GO);
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
    #endregion
#endif
}
