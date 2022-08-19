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
            
        }

        public override void Grab(GrippingHand hand)
        {
            onGrab?.Invoke();
            SetParamsOnGrab(hand);
            GetComponent<Rigidbody>().isKinematic = true;
            SetSoundOnGrab();
            if (gameObject.activeSelf) StartCoroutine("Attach");
        }

        public override IEnumerator Attach()
        {
            GetComponent<Rigidbody>().velocity = grippingHand.velocity;
            return base.Attach();
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
