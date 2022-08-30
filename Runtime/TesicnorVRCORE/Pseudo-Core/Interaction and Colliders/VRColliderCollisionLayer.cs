using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TesicnorVR
{
    public class VRColliderCollisionLayer : VRCollider
    {
        #region PARAMETERS
        [Header("El nombre de la capa en la que se encuentra este objeto")]
        public string layerName = "Collision";
        #endregion

        #region FUNCTIONS
        public override void Awake()
        {
            base.Awake();
            this.gameObject.layer = 6;
            
            Collider collider = GetComponent<Collider>();
            if (collider.GetType() == typeof(BoxCollider)) gameObject.AddComponent<BoxCollider>();
            else if (collider.GetType() == typeof(SphereCollider)) gameObject.AddComponent<SphereCollider>();
            else if (collider.GetType() == typeof(MeshCollider)) gameObject.AddComponent<MeshCollider>();
            else if (collider.GetType() == typeof(CapsuleCollider)) gameObject.AddComponent<CapsuleCollider>();
            else gameObject.AddComponent<BoxCollider>();

            GetComponent<Rigidbody>().mass = mass;
            
        }

        Vector3 localRotation = Vector3.zero;
        Vector3 localPosition = Vector3.zero;
        public override void Grab(GrippingHand hand)
        {
            onGrab?.Invoke();
            SetParamsOnGrab(hand);
            GetComponent<Rigidbody>().useGravity = false;
            SetSoundOnGrab();
            if (gameObject.activeSelf) StartCoroutine("Attach");
            localRotation = this.transform.localRotation.eulerAngles;
            localPosition = this.transform.localPosition;
        }

        public override IEnumerator Attach()
        {
            return base.Attach();
        }
        public void FixedUpdate()
        {
            if(GetGrippingHand() != null && GetComponent<Rigidbody>())
            {
                GetComponent<Rigidbody>().velocity = GetGrippingHand().velocity;
                this.transform.localRotation = Quaternion.Euler(localRotation);
                this.transform.localPosition = localPosition;
            }
        }
        private void OnCollisionEnter(Collision collision)
        {
            if (GetComponent<Rigidbody>() && !grippingHand)
            {
                GetComponent<Rigidbody>().isKinematic = false;
                GetComponent<Rigidbody>().useGravity = true;
            }
            else if (!grippingHand)
            {
                Rigidbody rb = gameObject.AddComponent<Rigidbody>();
                rb.isKinematic = false;
                rb.useGravity = true;
            }
        }

        #endregion
    }
}
