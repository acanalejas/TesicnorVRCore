using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wind_Line : VRCollider
{
    #region PARAMETERS
    [Header("La tag correcta del punto de anclaje")]
    [SerializeField] string CorrectAnchorageTag = "AnchorageBlue";

    [Header("El GameObject de warning")]
    [SerializeField] GameObject WarningGO;
    #endregion

    #region METHODS

    #endregion

    #region Anclaje

    public class LineAnchor : VRCollider, AnchorInterface
    {
        #region PARAMETERS
        public Wind_Line owner;
        #endregion

        #region METHODS
        private void Start()
        {
            onTargetReached.AddListener(CheckAnchorage);
            this.SetGrabbable(false);

            this.target.canBeCanceled = true;
            this.simulateOnDrop = false;
        }

        private void CheckAnchorage(GameObject anchorage)
        {
            if (anchorage.tag == owner.CorrectAnchorageTag)
            {
                owner.WarningGO.SetActive(false);
            }
            else
            {
                owner.WarningGO.SetActive(true);
            }
        }

        public bool IsAnchored()
        { 
            return this.target.conditionCompleted && !owner.isGrabbed();
        }

        public void AnchorIt()
        {
            throw new System.NotImplementedException();
        }

        public void ReleaseIt()
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }

    #endregion
}
