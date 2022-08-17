using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneData_SO", menuName = "SceneData")]
public class SceneData : ScriptableObject
{
    #region PARAMETERS
    public string SceneName;
    public string SectionName;
    public int SceneID;
    #endregion
}
