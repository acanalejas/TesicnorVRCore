using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RecenterWorld : MonoBehaviour
{
    #region PARAMETERS
    [Header("============= FOR INITIAL RECENTER ===============")][Space(10)]
    [Header("Transform del mundo a recentrar")]
    [HideInInspector]public Transform worldTransform;

    [Header("Transform del player que se va a usar")]
    [HideInInspector] public Transform playerTransform;

    [Header("Transform del player que usa los mandos")]
    [HideInInspector] public Transform playerControllersTransform;

    [Header("Punto inicial en el que se posiciona al jugador")]
    [HideInInspector] public Transform originTransform;

    [Header("Se puede recentrar con los mandos?")]
    [HideInInspector] public bool canRecenterWithControllers = false;

    [Header("=========== FOR CONTROLLERS RECENTER =============")]
    [Space(10)]

    [Header("Se va a recentrar algo con el jugador en funcion de los mandos?")]
    [HideInInspector] public bool hasSomethingAttached = true;

    [Header("El transform que marca donde debería estar el mando físico")]
    [HideInInspector] public Transform rightController_holder;

    [Header("El transform que marca donde debería estar el mando izquierdo")]
    [HideInInspector] public Transform leftController_holder;

    [Header("OPCIONAL : El gameObject que sirve para mostrar el mando derecho escondido")]
    [HideInInspector] public GameObject rightHiddenGo;

    [Header("OPCIONAL : El gameObject que sirve para mostrar el mando izquierdo escondido")]
    [HideInInspector] public GameObject leftHiddenGo;

    [Header("El transform del mando derecho")]
    [HideInInspector] public Transform rightController;

    [Header("El transform del mando izquierdo")]
    [HideInInspector] public Transform leftController;

    [Header("El transform del maniqui virtual")]
    [HideInInspector] public Transform virtualPerson;

    [Header("El padre en la jerarquia del maniqui virtual")]
    [HideInInspector] public Transform virtualPersonParent;

    #endregion

    #region FUNCTIONS
    private void Start()
    {
        StartCoroutine("wait");
        StartCoroutine("update");
    }

    private IEnumerator update()
    {
        yield return new WaitForSeconds(0.5f);
        while (true)
        {
            //if (areHandsNearHolders() && !alreadyCentered) StartCoroutine("makeSure");
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator wait()
    {
        yield return new WaitForSeconds(0.1f);
        Recenter();
    }
    /// <summary>
    /// Cambia la posicion del escenario para que el player salga siempre en la misma posicion
    /// </summary>
    public void Recenter()
    {
        worldTransform.position += offsetPosition(originTransform, playerTransform);
    }

    /// <summary>
    /// Posicion del escenario dependiendo de la distancia entre el punto inicial del jugador, y donde está realmente teniendo en cuenta el tracking
    /// </summary>
    /// <param name="_originTransform"></param>
    /// <param name="_currentTransform"></param>
    /// <returns></returns>
    private Vector3 offsetPosition(Transform _originTransform, Transform _currentTransform)
    {
        Vector3 result = _currentTransform.position - _originTransform.position;
        result.y = 0;

        return result;
    }
    private Vector3 offsetPosition(Vector3 _originPosition, Transform _currentTransform)
    {
        Vector3 result = _currentTransform.position - _originPosition;

        return result;
    }

    private Vector3 offsetPosition(Vector3 _originPosition, Vector3 _currentPosition)
    {
        Vector3 result = _currentPosition - _originPosition;

        return result;
    }
    private Quaternion offsetRotation(Transform _originTransform, Transform _currentTransform)
    {
        Quaternion result = _currentTransform.rotation * Quaternion.Inverse(_originTransform.rotation);

        return result;
    }

    bool alreadyCentered = false;
    /// <summary>
    /// Reposiciona el mundo y el maniqui virtual adaptándose a donde se dejen los mandos
    /// </summary>
    /// <returns></returns>
    public IEnumerator RecenterFromControllers()
    {
        if (alreadyCentered || !canRecenterWithControllers) yield break;

        //First we are rotating the virtual body
        if (virtualPerson)
        {
            rightController_holder.transform.parent = rightController;
            rightController_holder.localPosition = Vector3.zero;
            rightController_holder.localRotation = Quaternion.Euler(0, 0, 0);
            rightController_holder.parent = null;
        }
        

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        Vector3 positionDifference = rightController.position - rightController_holder.position;

        worldTransform.position -=positionDifference;

        //Disable the visual holders
        leftController_holder.gameObject.SetActive(false);
        //rightController_holder.gameObject.SetActive(false);

        //if(rightHiddenGo) rightHiddenGo.SetActive(false);
        if(leftHiddenGo) leftHiddenGo.SetActive(false);

        playerControllersTransform.gameObject.SetActive(false);
        Debug.Log("RECENTERING PLAYER");

        alreadyCentered = true;
        StopCoroutine("RecenterFromControllers");
    }

    Vector3 lastFrameRightControllerPosition;
    Vector3 lastFrameLeftControllerPosition;

    Quaternion lastFrameRightControllerRotation;
    Quaternion lastFrameLeftControllerRotation;

    #endregion
}

#if UNITY_EDITOR
[CustomEditor(typeof(RecenterWorld), true)]
public class RecenterEditor : Editor
{
    [HideInInspector] public RecenterWorld recenter;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        recenter = target as RecenterWorld;

        GUILayout.Label("Se puede recentrar con los mandos?", EditorStyles.boldLabel);
        recenter.canRecenterWithControllers = GUILayout.Toggle(recenter.canRecenterWithControllers, "Can recenter with controllers");

        SerializedProperty worldPivot = serializedObject.FindProperty("worldTransform");
        EditorGUILayout.PropertyField(worldPivot, new GUIContent("World pivot"));

        SerializedProperty playerPivot = serializedObject.FindProperty("playerTransform");
        EditorGUILayout.PropertyField(playerPivot, new GUIContent("Player pivot"));

        SerializedProperty playerControllersPivot = serializedObject.FindProperty("playerControllersTransform");
        EditorGUILayout.PropertyField(playerControllersPivot, new GUIContent("Player controllers pivot"));

        SerializedProperty originPivot = serializedObject.FindProperty("originTransform");
        EditorGUILayout.PropertyField(originPivot, new GUIContent("Origin pivot"));

        if (recenter.canRecenterWithControllers)
        {
            GUILayout.Label("Tiene algo que se recentra en funcion de los mandos con el jugador?", EditorStyles.boldLabel);
            recenter.hasSomethingAttached = GUILayout.Toggle(recenter.hasSomethingAttached, "Has something attached?");

            GUILayout.Space(10);

            SerializedProperty rightController_holder = serializedObject.FindProperty("rightController_holder");
            EditorGUILayout.PropertyField(rightController_holder, new GUIContent("Right controller holder"));

            SerializedProperty leftController_holder = serializedObject.FindProperty("leftController_holder");
            EditorGUILayout.PropertyField(leftController_holder, new GUIContent("Left controller holder"));

            SerializedProperty rightController = serializedObject.FindProperty("rightController");
            EditorGUILayout.PropertyField(rightController, new GUIContent("Right Controller"));

            SerializedProperty leftController = serializedObject.FindProperty("leftController");
            EditorGUILayout.PropertyField(leftController, new GUIContent("Left Controller"));

            SerializedProperty rightHiddenGo = serializedObject.FindProperty("rightHiddenGo");
            EditorGUILayout.PropertyField(rightHiddenGo, new GUIContent("OPCIONAL : Right hidden GO"));

            SerializedProperty leftHiddenGo = serializedObject.FindProperty("leftHiddenGo");
            EditorGUILayout.PropertyField(leftHiddenGo, new GUIContent("OPCIONAL : Left hidden GO"));

            if (recenter.hasSomethingAttached)
            {
                SerializedProperty virtualPerson = serializedObject.FindProperty("virtualPerson");
                EditorGUILayout.PropertyField(virtualPerson, new GUIContent("Attached Transform"));

                SerializedProperty virtualPersonParent = serializedObject.FindProperty("virtualPersonParent");
                EditorGUILayout.PropertyField(virtualPersonParent, new GUIContent("Attached Transform Parent"));
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
