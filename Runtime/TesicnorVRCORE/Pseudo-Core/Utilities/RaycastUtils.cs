using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RaycastUtils
{
    #region PARAMETERS
    public static int RaycastLayer { get { return raycastLayer; } }
    private static int raycastLayer = 20;

    #region Setters
    public static void SetRaycastLayer(int _input) { raycastLayer = _input; }
    #endregion
    #endregion
    #region METHODS
    public static RaycastHit RaycastGameobject(GameObject _origin, GameObject _target, bool ignoreRest = true)
    {
        float initialLayer = _target.layer;
        if (ignoreRest)
        {
            _target.layer = raycastLayer;
        }

        Vector3 originPos = _origin.transform.position;
        Vector3 targetPos = _target.transform.position;

        Ray ray = new Ray(originPos, targetPos);
        RaycastHit hit;

        Physics.Raycast(ray, out hit, Vector3.Distance(originPos, targetPos), raycastLayer);

        return hit;
    }
    #endregion
}
