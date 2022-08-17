using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum axis { x, y, z, xNeg, yNeg, zNeg};

[System.Serializable]
public struct axisVector
{
    public axisVector(axis _x, axis _y, axis _z)
    {
        x = _x;
        y = _y;
        z = _z;
    }

    [SerializeField]
    public axis x;
    [SerializeField]
    public axis y;
    [SerializeField]
    public axis z;
}
[CreateAssetMenu(fileName ="HandsOffset_SO", menuName ="ScriptableObjects")]
public class HandsOffset : ScriptableObject
{
    public Vector3 LeftPositionMult = new Vector3(1,1,1);

    public Vector3 LeftHandRotationOffset = Vector3.zero;

    public Vector3 RightPositionMult = new Vector3(1,1,1);

    public Vector3 RightHandRotationOffset = Vector3.zero;

    public Vector3 ForwardRotation = Vector3.zero;

    [SerializeField]
    public axisVector ForwardVectorLeft;

    [SerializeField]
    public axisVector ForwardVectorRight;

    public float LeftHandMultiplierOffsetRotation = 1;

    public float RightHandMultiplierOffsetRotation = 1;

    /// <summary>
    /// Cuando se quiere obtener una posicion respecto al forward de la mano
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="isLeftHand"></param>
    /// <returns></returns>
    public Vector3 GetOffsetPositionFromForward(Vector3 offset, bool isLeftHand)
    {
        Vector3 result = Vector3.zero;

        axisVector forwardVector = ForwardVectorRight;
        if (isLeftHand) forwardVector = ForwardVectorLeft;

        switch (forwardVector.x)
        {
            case axis.x:
                result.x = offset.x;
                break;
            case axis.xNeg:
                result.x = -offset.x;
                break;
            case axis.y:
                result.x = offset.y;
                break;
            case axis.yNeg:
                result.x = -offset.y;
                break;
            case axis.z:
                result.x = offset.z;
                break;
            case axis.zNeg:
                result.x = -offset.z;
                break;
        }
        switch (forwardVector.y)
        {
            case axis.x:
                result.y = offset.x;
                break;
            case axis.xNeg:
                result.y = -offset.x;
                break;
            case axis.y:
                result.y = offset.y;
                break;
            case axis.yNeg:
                result.y = -offset.y;
                break;
            case axis.z:
                result.y = offset.z;
                break;
            case axis.zNeg:
                result.y = -offset.z;
                break;
        }
        switch (forwardVector.z)
        {
            case axis.x:
                result.z = offset.x;
                break;
            case axis.xNeg:
                result.z = -offset.x;
                break;
            case axis.y:
                result.z = offset.y;
                break;
            case axis.yNeg:
                result.z = -offset.y;
                break;
            case axis.z:
                result.z = offset.z;
                break;
            case axis.zNeg:
                result.z = -offset.z;
                break;
        }
        return result;
    }

    /// <summary>
    /// Devuelve el forward de la mano dependiendo de como tengamos seteado el offset de hacia donde apunta el forward de la mano
    /// EJ: en oculus el "forward" de la mano es Vector3.right. Eso seria axisVector(z,y,x);
    /// </summary>
    /// <param name="_forward"></param>
    /// <param name="isLeftHand"></param>
    /// <returns></returns>
    public Vector3 GetForward(Vector3 _forward, bool isLeftHand)
    {
        float x = 0;
        float y = 0;
        float z = 0;

        axisVector forward = ForwardVectorRight;
        if (isLeftHand) forward = ForwardVectorLeft;

        switch (forward.x)
        {
            case axis.x:
                x = _forward.x;
                break;
            case axis.y:
                x = _forward.y;
                break;
            case axis.z:
                x = _forward.z;
                break;
            case axis.xNeg:
                x = -_forward.x;
                break;
            case axis.yNeg:
                x = -_forward.y;
                break;
            case axis.zNeg:
                x = -_forward.z;
                break;
        }

        switch (forward.y)
        {
            case axis.x:
                y = _forward.x;
                break;
            case axis.y:
                y = _forward.y;
                break;
            case axis.z:
                y = _forward.z;
                break;
            case axis.xNeg:
                y = -_forward.x;
                break;
            case axis.yNeg:
                y = -_forward.y;
                break;
            case axis.zNeg:
                y = -_forward.z;
                break;
        }

        switch (forward.z)
        {
            case axis.x:
                z = _forward.x;
                break;
            case axis.y:
                z = _forward.y;
                break;
            case axis.z:
                z = _forward.z;
                break;
            case axis.xNeg:
                z = -_forward.x;
                break;
            case axis.yNeg:
                z = -_forward.y;
                break;
            case axis.zNeg:
                z = -_forward.z;
                break;
        }
        Vector3 result = Quaternion.Euler(ForwardRotation) * new Vector3(x, y, z);

        return result;
    }

    public Vector3 GetForwardFromRightVector(Vector3 _forward, Vector3 _localUp)
    {
        return Quaternion.AngleAxis(-90, _localUp) * _forward;
    }
    public Vector3 GetForwardFromLeftVector(Vector3 _forward, Vector3 _localUp)
    {
        return Quaternion.AngleAxis(90, _localUp) * _forward;
    }
}
