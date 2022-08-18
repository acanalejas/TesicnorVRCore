using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Estructura que guarda los datos del gesto
/// </summary>
[System.Serializable]
public struct Gesture
{
    public string GestureName;
    public List<Vector3> fingersPositions;
    public UnityEvent OnRecognizeGesture;
}
public class HandPoseDetector : MonoBehaviour
{
    #region PARAMETERS
    [Header("Margen de error entre posicion de cada hueso")]
    [SerializeField] float Threshold = 0.1f;
    [Header("El esqueleto de la mano")]
    public OVRCustomSkeleton skeleton;
    [Header("Lista de gestos disponibles")]
    [SerializeField] List<Gesture> gestures;
    private List<OVRBone> fingersBones;
    private Gesture previousGesture;
    #endregion

    #region FUNCTIONS
    private void Start()
    {
        fingersBones = new List<OVRBone>(skeleton.Bones);
        previousGesture = new Gesture();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Save();
        }

        Gesture newGesture = Recognize();
        bool hasRecognized = !newGesture.Equals(new Gesture());

        if (hasRecognized && !newGesture.Equals(previousGesture))
        {
            previousGesture = newGesture;
            newGesture.OnRecognizeGesture.Invoke();

            Debug.Log(newGesture.GestureName);
        }
    }

    /// <summary>
    /// Usado para guardar gestos nuevos en runtime
    /// </summary>
    public void Save()
    {
        Gesture gesture = new Gesture();
        gesture.GestureName = "new Gesture";
        List<Vector3> fingersPositions = new List<Vector3>();
        foreach(var bone in skeleton.Bones)
        {
            fingersPositions.Add(skeleton.transform.InverseTransformPoint(bone.Transform.position));
        }

        gesture.fingersPositions = fingersPositions;
        gestures.Add(gesture);
    }

    /// <summary>
    /// Compara la postura de la mano con los gestos almacenados y devuelve si coincide con alguno
    /// </summary>
    /// <returns></returns>
    public Gesture Recognize()
    {
        Gesture currentGesture = new Gesture();
        float currentMin = Mathf.Infinity;

        foreach(var gesture in gestures)
        {
            float sumDistance = 0;
            bool isDiscarded = false;

            for(int i = 0; i < skeleton.Bones.Count; i++)
            {
                Vector3 currentData = skeleton.transform.InverseTransformPoint(skeleton.Bones[i].Transform.position);
                float distance = Vector3.Distance(currentData, gesture.fingersPositions[i]);

                if(distance > Threshold)
                {
                    isDiscarded = true;
                    break;
                }
                sumDistance += distance;
            }
            if (!isDiscarded && sumDistance < currentMin)
            {
                currentMin = sumDistance;
                currentGesture = gesture;
            }
        }

        return currentGesture;
    }

    /// <summary>
    /// Fuerza una postura en la mano
    /// </summary>
    /// <param name="gestureName"></param>
    public void ForceGesture(string gestureName)
    {
        Gesture gesture = new Gesture();

        foreach(var _gesture in gestures)
        {
            if (_gesture.GestureName == gestureName) gesture = _gesture;
        }
        //Si no se encuentra con ese nombre, devuelve nulo 
        if (gesture.fingersPositions.Count == 0) return;

        int i = 0;
        foreach(var bone in fingersBones)
        {
            bone.Transform.localPosition = gesture.fingersPositions[i];
            i++;
        }
    }
    #endregion
}
