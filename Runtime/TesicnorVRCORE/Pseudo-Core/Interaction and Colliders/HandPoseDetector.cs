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
    public bool forGrab;
    public bool forRelease;
}
public class HandPoseDetector : MonoBehaviour
{
    #region PARAMETERS
    [Header("Margen de error entre posicion de cada hueso")]
    [SerializeField] float Threshold = 0.1f;
    //[Header("El esqueleto de la mano")]
    //public OVRCustomSkeleton skeleton;
    [Header("Lista de gestos disponibles")]
    [SerializeField] List<Gesture> gestures;
    private Gesture previousGesture;
    #endregion

    #region FUNCTIONS
    private void Start()
    {
        //Debug.Log("Finger Bones Count : " + fingersBones.Count);
        previousGesture = new Gesture();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Save();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift)) ForceGesture("Grab");

        Gesture newGesture = Recognize();
        bool hasRecognized = !newGesture.Equals(new Gesture());

        if (hasRecognized && !bothGrab(newGesture, previousGesture) && !bothRelease(newGesture, previousGesture))
        {
            previousGesture = newGesture;
            newGesture.OnRecognizeGesture.Invoke();

            Debug.Log(newGesture.GestureName);
        }
    }

    bool bothRelease(Gesture _new, Gesture old)
    {
        if (_new.forRelease && old.forRelease) return true;
        else return false;
    }
    bool bothGrab(Gesture _new, Gesture old)
    {
        if (_new.forGrab && old.forGrab) return true;
        return false;
    }
    /// <summary>
    /// Usado para guardar gestos nuevos en runtime
    /// </summary>
    public void Save()
    {
        Gesture gesture = new Gesture();
        gesture.GestureName = "new Gesture";
        List<Vector3> fingersPositions = new List<Vector3>();
        //foreach(var bone in skeleton.Bones)
        //{
        //    fingersPositions.Add(skeleton.transform.InverseTransformPoint(bone.Transform.position));
        //}

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

        /*foreach(var gesture in gestures)
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
        }*/

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
            if (_gesture.GestureName == gestureName) {
                gesture = _gesture;
                gesture.fingersPositions = _gesture.fingersPositions; };
        }

        Debug.Log("Gesture Finger Positions : " + gesture.fingersPositions.Count);
        //Si no se encuentra con ese nombre, devuelve nulo 
        if (gesture.fingersPositions.Count == 0) { return; }
        Debug.Log("After Return");
        int i = 0;
        //foreach (OVRBone bone in skeleton.Bones) 
        //{
        //    Vector3 position = skeleton.transform.TransformPoint(gesture.fingersPositions[i]);
        //    bone.Transform.position = position;
        //    Debug.Log("Se intenta posicionar la mano");
        //    i++;
        //}
    }
    #endregion
}
