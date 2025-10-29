using UnityEngine;

public class Wind_EmergencyButton : VRCollider
{
    #region PARAMETERS

    public bool isActive = false;

    #endregion

    #region METHODS

    public override void Grab(GrippingHand hand)
    {
        isActive = !isActive;
        Wind_EmergencyManager.Instance.CheckEmergency();
        base.Grab(hand);
    }

    #endregion
}
