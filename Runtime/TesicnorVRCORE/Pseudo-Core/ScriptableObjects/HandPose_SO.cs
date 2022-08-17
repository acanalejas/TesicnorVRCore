using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Hand Pose", menuName = "Hand Pose")]
public class HandPose_SO : ScriptableObject
{
    #region PARAMETERS
    [Header("La lista de cada posición relativa de los huesos")]
    public List<Vector3> bonesPositions = new List<Vector3>();
    public List<Vector3> bonesRotations = new List<Vector3>();
    #endregion
}
