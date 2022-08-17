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
    [SerializeField] private Transform rig;

    /// <summary>
    /// El padre que modifica la escala
    /// </summary>
    [Header("El padre que modifica la escala")]
    [SerializeField] private Transform parent;

    /// <summary>
    /// El SO con la posición abierta
    /// </summary>
    [Header("El SO con la posición abierta")]
    [SerializeField] private HandPose_SO openedPose;

    /// <summary>
    /// El SO con la posición cerrada
    /// </summary>
    [Header("El SO con la posición cerrada")]
    [SerializeField] private HandPose_SO closedPose;

    /// <summary>
    /// El componente XRController del mando
    /// </summary>
    [Header("El componente XRController del mando")]
    [SerializeField] private XRController controller;

    /// <summary>
    /// La tecla con la que se guarda la pose abierta
    /// </summary>
    [Header("La tecla con la que se guarda la pose abierta")]
    [SerializeField] private KeyCode openCode = KeyCode.RightAlt;

    /// <summary>
    /// La tecla con la que se guarda la pose cerrada
    /// </summary>
    [Header("La tecla con la que se guarda la pose cerrada")]
    [SerializeField] private KeyCode closeCode = KeyCode.RightShift;

    /// <summary>
    /// Se tienen que aplicar las poses?
    /// </summary>
    [Header("Se tienen que aplicar las poses?")]
    [SerializeField] private bool applyPoses = true;

    /// <summary>
    /// La lista que contiene todos los huesos del rig
    /// </summary>
    private Transform[] AllBones;
    #endregion

    #region FUNCTIONS
    private void Start()
    {
        //Se recogen todos los huesos
        AllBones = rig.GetComponentsInChildren<Transform>();
    }

    private void Update()
    {
        if (applyPoses) { CheckInput(); CheckInputTrigger(); }
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
    /// Checkea el input para mover los huesos
    /// </summary>
    private void CheckInput()
    {
        float grip = 0;
        
        if(controller.inputDevice.TryGetFeatureValue(CommonUsages.grip, out grip) && grip > 0.02f)
        {
            SetPose(grip);
            //Debug.Log("grip  :  " + grip);
        }
        
    }

    private void CheckInputTrigger()
    {
        float trigger = 0;

        if (controller.inputDevice.TryGetFeatureValue(CommonUsages.trigger, out trigger) && trigger > 0.02f)
        {
            SetPose(trigger);
            //Debug.Log("trigger  :  " + trigger);
        }
    }

    /// <summary>
    /// Devuelve la lista de las posiciones en las que debe estar cada hueso dependiendo de un parámetro que le pasemos de 0 a 1
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
    /// Devuelve las rotaciones actuales de los huesos dependiendo de un parámetro que le pasemos que vaya de 0 a 1
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

            Vector3 currentDistance = v + distance * grip;
            result.Add(currentDistance);
            i++;
        }

        return result;
    }

    /// <summary>
    /// Setea cada posición y rotación de los huesos dependiendo de un parámetro que le demos de 0 a 1
    /// </summary>
    /// <param name="grip"></param>
    private void SetPose(float grip)
    {
        List<Vector3> positions = currentBonePositions(grip);
        List<Vector3> rotations = currentBoneRotations(grip);

        int i = 0;
        foreach(Transform _transform in AllBones)
        {
            _transform.position = rig.TransformPoint(positions[i]);
            _transform.localRotation = Quaternion.Euler(rotations[i]);
            i++;
        }
    }
    #endregion
}
