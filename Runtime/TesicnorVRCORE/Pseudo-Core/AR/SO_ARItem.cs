using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AR Item",menuName = "New AR Item")]
public class SO_ARItem : ScriptableObject
{
    [Header("El prefab asociado para spawnear")]
    public GameObject prefab;

    [Header("El tipo de extintor correcto a utilizar")]
    public SO_FireExtinguisher[] extinguisher;

    [Header("El Mejor tipo de extintor a usar")]
    public SO_FireExtinguisher Best;

    [Header("El peor tipo de extintor a usar")]
    public SO_FireExtinguisher Worst;

    [Header("La mesh que se usa de preview")]
    public Mesh previewMesh;
}
