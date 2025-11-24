using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;

public class HandPoser : MonoBehaviour
{
    #region PARAMETERS
    /// <summary>
    /// El padre del rigging
    /// </summary>
    [Header("El padre del rigging")]
    public Transform rig;

    /// <summary>
    /// El padre que modifica la escala
    /// </summary>
    [Header("El padre que modifica la escala")]
    public Transform parent;

    /// <summary>
    /// El SO con la posici�n abierta
    /// </summary>
    [Header("El SO con la posici�n abierta")]
    public HandPose_SO openedPose;

    /// <summary>
    /// El SO con la posici�n cerrada
    /// </summary>
    [Header("El SO con la posici�n cerrada")]
    public HandPose_SO closedPose;

    /// <summary>
    /// El componente XRController del mando
    /// </summary>
    [Header("El componente GrippingHand del mando")]
    public GrippingHand controller;

    /// <summary>
    /// La tecla con la que se guarda la pose abierta
    /// </summary>
    [Header("La tecla con la que se guarda la pose abierta")]
    public KeyCode openCode = KeyCode.RightAlt;

    /// <summary>
    /// La tecla con la que se guarda la pose cerrada
    /// </summary>
    [Header("La tecla con la que se guarda la pose cerrada")]
    public KeyCode closeCode = KeyCode.RightShift;

    /// <summary>
    /// Se tienen que aplicar las poses?
    /// </summary>
    [Header("Se tienen que aplicar las poses?")]
    public bool applyPoses = true;

    /// <summary>
    /// La lista que contiene todos los huesos del rig
    /// </summary>
    private Transform[] AllBones;

    public Transform RigRoot;
    #endregion

    #region FUNCTIONS
    private void Start()
    {
        //Se recogen todos los huesos
        AllBones = rig.GetComponentsInChildren<Transform>();
    }

    private void Update()
    {
        if (applyPoses) { BindPosesToInput();}
        CheckInputEditor();
    }

    /// <summary>
    /// Checkea el input del editor para guardar poses
    /// </summary>
    public void CheckInputEditor()
    {
        if (Input.GetKeyDown(openCode))     SaveBones(openedPose);
        else if (Input.GetKeyDown(closeCode))   SaveBones(closedPose);
    }

    /// <summary>
    /// Guarda los huesos en el SO que le asignemos
    /// </summary>
    /// <param name="pose"></param>
    private void SaveBones(HandPose_SO pose)
    {
        pose.bonesPositions.Clear();
        pose.bonesRotations.Clear();

        Transform[] allBones = rig.GetComponentsInChildren<Transform>();

        foreach(Transform bone in allBones)
        {
            pose.bonesPositions.Add(rig.InverseTransformPoint(bone.position));
            pose.bonesRotations.Add(bone.localRotation.eulerAngles);
        }
    }

    /// <summary>
    /// Controla la posicion de la mano en funcion del input
    /// </summary>
    private void BindPosesToInput()
    {
        if (controller.handType == GrippingHand.HandType.left)
        {
            SetPose(TesicnorPlayer.Instance.coreInteraction.Interaction.Grab_Left.ReadValue<float>());
        }
        else
        {
            SetPose(TesicnorPlayer.Instance.coreInteraction.Interaction.Grab_Right.ReadValue<float>());
        }
    }

    /// <summary>
    /// Devuelve la lista de las posiciones en las que debe estar cada hueso dependiendo de un par�metro que le pasemos de 0 a 1
    /// </summary>
    /// <param name="grip"></param>
    /// <returns></returns>
    private List<Vector3> currentBonePositions(float grip)
    {
        List<Vector3> result = new List<Vector3>();

        int i = 0;
        foreach(Vector3 v in openedPose.bonesPositions)
        {
            Vector3 distance =      closedPose.bonesPositions[i] - v;

            Vector3 currentDistance = v + distance * grip;
            //currentDistance = new Vector3(currentDistance.x * parent.lossyScale.x, currentDistance.y * parent.lossyScale.y, currentDistance.z * parent.lossyScale.z);
            result.Add(currentDistance);
            i++;
        }

        return result;
    }

    /// <summary>
    /// Devuelve las rotaciones actuales de los huesos dependiendo de un par�metro que le pasemos que vaya de 0 a 1
    /// </summary>
    /// <param name="grip"></param>
    /// <returns></returns>
    private List<Vector3> currentBoneRotations(float grip)
    {
        List<Vector3> result = new List<Vector3>();

        int i = 0;
        foreach(Vector3 v in openedPose.bonesRotations)
        {
            Vector3 distance = closedPose.bonesRotations[i] - v;
//
            Vector3 currentDistance = v + distance * grip;
            result.Add(currentDistance);
            i++;
            
            if(grip >= 0.5f) result.Add(closedPose.bonesRotations[i]);
            else result.Add(openedPose.bonesRotations[i]);

            i++;
        }

        return result;
    }

    /// <summary>
    /// Setea cada posici�n y rotaci�n de los huesos dependiendo de un par�metro que le demos de 0 a 1
    /// </summary>
    /// <param name="grip"></param>
    private void SetPose(float grip)
    {
        List<Vector3> positions = currentBonePositions(grip);
        List<Vector3> rotations = currentBoneRotations(grip);

        int i = 0;
        foreach(Transform _transform in AllBones)
        {
            if (_transform.gameObject != RigRoot.gameObject || _transform.gameObject != rig.gameObject)
            {
                _transform.position = rig.TransformPoint(positions[i]);
                _transform.localRotation = Quaternion.Euler(rotations[i]);
            }
            i++;
        }
    }
    #endregion
}
