using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manguera : MonoBehaviour
{
    #region PARAMETERS
    [Header("El primer hueso de la manguera")]
    [SerializeField] private GameObject FirstBone;

    [Header("La distancia entre huesos")]
    [SerializeField] private float BoneDistance = 0.32f;

    [Header("El hueso inicial se mantiene estatico?")]
    [SerializeField] private bool FirstBoneStatic = true;

    [Header("El hueso final se mantiene estatico?")]
    [SerializeField] private bool LastBoneStatic = true;

    [Header("Se reposiciona al reemparentar los huesos")]
    [SerializeField] private bool ResetOnReparent = true;

    [Header("El holder del hueso inicial")]
    [SerializeField] private Transform FirstBoneHolder;

    [Header("El holder del hueso final")]
    [SerializeField] private Transform LastBoneHolder;

    private List<Transform> Bones = new List<Transform> ();
    private List<Rigidbody> Rigidbodies = new List<Rigidbody> ();
    private List<BoxCollider> Colliders = new List<BoxCollider> ();
    private List<ConfigurableJoint> Joints = new List<ConfigurableJoint> ();
    #endregion

    #region METHODS

    private void Start()
    {
        ConfigureEachBone();
    }

    #region Configuracion
    void ConfigureEachBone()
    {
        Bones.AddRange(FirstBone.transform.GetComponentsInChildren<Transform> ());

        foreach(var bone in Bones)
        {
            if (bone.GetComponent<Rigidbody>() == null) Rigidbodies.Add(bone.gameObject.AddComponent<Rigidbody>());
            else Rigidbodies.Add(bone.GetComponent<Rigidbody>());
            //if (!bone.GetComponent<BoxCollider>()) Colliders.Add(bone.gameObject.AddComponent<BoxCollider>());
            //else Colliders.Add(bone.GetComponent<BoxCollider>());
            if(!bone.GetComponent<ConfigurableJoint>()) Joints.Add(bone.gameObject.AddComponent<ConfigurableJoint>());
            else Joints.Add(bone.GetComponent<ConfigurableJoint>());
        }

        foreach (Rigidbody rb in Rigidbodies) { rb.mass = 0.2f; rb.drag = 1.5f; }

        Rigidbody StartRB = Rigidbodies[0]; Rigidbody LastRB = Rigidbodies[Rigidbodies.Count - 1];

        StartRB.isKinematic = true; LastRB.isKinematic = true;

        ConfigurableJoint lastJoint = null;
        foreach(var joint in Joints)
        {
            if (lastJoint != null) joint.connectedBody = lastJoint.GetComponent<Rigidbody>();

            joint.xMotion = ConfigurableJointMotion.Limited;
            joint.yMotion = ConfigurableJointMotion.Limited;
            joint.zMotion = ConfigurableJointMotion.Limited;

            joint.angularXMotion = ConfigurableJointMotion.Limited;
            joint.angularYMotion = ConfigurableJointMotion.Limited;
            joint.angularZMotion = ConfigurableJointMotion.Limited;

            SoftJointLimit sjl = new SoftJointLimit();
            sjl.limit = BoneDistance;
            sjl.bounciness = -BoneDistance;
            sjl.contactDistance = BoneDistance*2;
            joint.linearLimit = sjl;

            //joint.enableCollision = true;

            lastJoint = joint;
        }

        if (FirstBoneHolder)
        {
            FirstBone.transform.GetChild(0).parent = FirstBone.transform.parent;
            FirstBone.transform.parent = FirstBoneHolder;

            ConfigurableJoint initialJoint = FirstBone.GetComponent<ConfigurableJoint>();
            initialJoint.xMotion = ConfigurableJointMotion.Locked;
            initialJoint.yMotion = ConfigurableJointMotion.Locked;
            initialJoint.zMotion = ConfigurableJointMotion.Locked;

            if (ResetOnReparent) FirstBone.transform.localPosition = Vector3.zero;
        }
        if (LastBoneHolder)
        {
            lastJoint.transform.parent = LastBoneHolder;
            lastJoint.xMotion = ConfigurableJointMotion.Locked;
            lastJoint.yMotion = ConfigurableJointMotion.Locked;
            lastJoint.zMotion = ConfigurableJointMotion.Locked;
            if (ResetOnReparent) lastJoint.transform.localPosition = Vector3.zero;
        }
        //foreach(var collider in Colliders)
        //{
        //    collider.size = new Vector3(BoneDistance * 0.45f, BoneDistance * 0.45f, BoneDistance * 0.45f);
        //}
    }
    #endregion
    #endregion
}
