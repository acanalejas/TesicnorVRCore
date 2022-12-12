using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CustomAnimation : MonoBehaviour
{
    #region PARAMETERS
    /// <summary>
    /// Animation curve that manages the speed
    /// </summary>
    [SerializeField, HideInInspector]
    public AnimationCurve curve;

    /// <summary>
    /// Max speed
    /// </summary>
    [SerializeField, HideInInspector]
    public float speed = 1;

    /// <summary>
    /// Time to perform the animation
    /// </summary>
    [SerializeField, HideInInspector]
    public float time = 1;

    /// <summary>
    /// Total distance to travel
    /// </summary>
    [SerializeField, HideInInspector]
    public float distance = 1;

    /// <summary>
    /// The time that should be waited before coming back
    /// </summary>
    [SerializeField, HideInInspector]
    public float timeToWaitBeforeReturn = 0;

    public enum AnimationSpace { World, Local}
    /// <summary>
    /// The space of the simulation
    /// </summary>
    [SerializeField, HideInInspector]
    public AnimationSpace space;

    public enum AnimationDirection { Forward, Backward, Right, Left, Up, Down, Custom}
    /// <summary>
    /// The direction of the animation
    /// </summary>
    [SerializeField, HideInInspector]
    public AnimationDirection direction;

    public enum AnimationType { Linear, CurveAbove, CurveBelow, ZigZag, Overlap}
    /// <summary>
    /// The type of animation performed
    /// </summary>
    [SerializeField, HideInInspector]
    public AnimationType type;

    /// <summary>
    /// In case that we need a custom direction for the animation
    /// </summary>
    [SerializeField, HideInInspector]
    public Vector3 customDirection;

    /// <summary>
    /// Should make the reverse animation for returning to the origin of the animation?
    /// </summary>
    [SerializeField, HideInInspector]
    public bool returnToOrigin;

    /// <summary>
    /// Should be the animation a loop?
    /// </summary>
    [SerializeField, HideInInspector]
    public bool loop;

    /// <summary>
    /// Should the animation apply root motion?
    /// </summary>
    [SerializeField, HideInInspector]
    public bool applyRootMotion;

    /// <summary>
    /// Should the animation launch at start?
    /// </summary>
    [SerializeField, HideInInspector]
    public bool launchOnStart;

    /// <summary>
    /// Frames per second of the animation
    /// </summary>
    private int fps = 30;

    /// <summary>
    /// Is the animation playing?
    /// </summary>
    private bool isPlaying;

    /// <summary>
    /// Path of the animation composed of all the points that the object is going to go through 
    /// </summary>
    Vector3[] path;

    #endregion

    #region FUNCTIONS
    private void Start()
    {
        if (launchOnStart) LaunchAnimation();
    }

    /// <summary>
    /// Gets the direction that is going to be used in the animation
    /// </summary>
    /// <returns></returns>
    Vector3 GetDirection()
    {
        switch (space)
        {
            case AnimationSpace.World:
                switch (direction)
                {
                    case AnimationDirection.Up:
                        return Vector3.up;
                        break;
                    case AnimationDirection.Down:
                        return Vector3.down;
                        break;
                    case AnimationDirection.Forward:
                        return Vector3.forward;
                        break;
                    case AnimationDirection.Backward:
                        return Vector3.back;
                        break;
                    case AnimationDirection.Right:
                        return Vector3.right;
                        break;
                    case AnimationDirection.Left:
                        return Vector3.left;
                        break;
                    case AnimationDirection.Custom:
                        return customDirection;
                        break;
                }
                break;
            case AnimationSpace.Local:
                switch (direction)
                {
                    case AnimationDirection.Up:
                        return transform.up;
                        break;
                    case AnimationDirection.Down:
                        return -transform.up;
                        break;
                    case AnimationDirection.Forward:
                        return transform.forward;
                        break;
                    case AnimationDirection.Backward:
                        return -transform.forward;
                        break;
                    case AnimationDirection.Right:
                        return transform.right;
                        break;
                    case AnimationDirection.Left:
                        return -transform.right;
                    case AnimationDirection.Custom:
                        return transform.InverseTransformDirection(customDirection);
                        break;
                }
                break;
        }

        return Vector3.zero;
    }

    /// <summary>
    /// Gets the final point of the animation in world space
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    Vector3 GetDestiny(Vector3 direction)
    {
        return transform.position + direction.normalized * distance;
    }

    /// <summary>
    /// Gets all the points in the path that are going to be used by the object in world space
    /// </summary>
    /// <returns></returns>
    Vector3[] GetPathPointsLinear()
    {
        List<Vector3> points = new List<Vector3>();

        Vector3 destiny = GetDestiny(GetDirection());
        Vector3 start = transform.position;

        int animationFrames = (int)(fps * time);

        float _distance = Vector3.Distance(start, destiny);
        Vector3 norm_direction = (destiny - start).normalized;

        float _distance_perPoint = _distance / animationFrames;

        for(int i = 0; i < animationFrames; i++)
        {
            Vector3 point = start + norm_direction * (_distance_perPoint * i);
            points.Add(point);
        }

        return points.ToArray();
    }

    /// <summary>
    /// Gets all the points in the path that are going to be used by the object in world space
    /// </summary>
    /// <returns></returns>
    Vector3[] GetPathPointsCurveAbove()
    {
        List<Vector3> points = new List<Vector3>();

        Vector3 star = transform.position;
        Vector3 destiny = GetDestiny(GetDirection());

        int animationFrames = (int)(fps * time);
        float _distance = Vector3.Distance(star, destiny);
        Vector3 norm_direction = (destiny - star).normalized;

        float _distance_perPoint = _distance / animationFrames;

        List<Vector3> linearPoints = new List<Vector3>();
        for(int i = 0; i< animationFrames; i++)
        {
            Vector3 point = star + norm_direction * (_distance_perPoint * i);
            linearPoints.Add(point);
        }

        int mediumPoint = (int)(animationFrames / 2);

        for(int i = 0; i < linearPoints.Count; i++)
        {
            int difference = Mathf.Abs(mediumPoint - i);
            difference = Mathf.Clamp(difference, 1, 50000);
            float maxMultiplier = distance / 3;

            float multiplier = maxMultiplier/(difference);

            Vector3 upVector = Vector3.up;
            if (space == AnimationSpace.Local) upVector = transform.up;

            Vector3 point = linearPoints[i] + multiplier * upVector.normalized;
            points.Add(point);
        }

        return points.ToArray();
    }

    /// <summary>
    /// Launches the animation
    /// </summary>
    public void LaunchAnimation()
    {
        switch (type)
        {
            case AnimationType.Linear:
                path = GetPathPointsLinear();
                break;
            case AnimationType.CurveAbove:
                path = GetPathPointsCurveAbove();
                break;
        }
        InvokeRepeating(nameof(SetPosition), 0, Time.deltaTime);
    }

    /// <summary>
    /// Set the position in the current frame
    /// </summary>
    void SetPosition()
    {
        if(this.transform.position == path[path.Length - 1]) {
            if (returnToOrigin || loop) { InvokeRepeating(nameof(SetPositionReverse),0,Time.deltaTime); } 

            CancelInvoke(nameof(SetPosition)); 

            return; 
        }
        else if (!path.Contains(this.transform.position)) this.transform.position = path[0];
        else
        {
            int index = 0;
            foreach (var v in path)
            {
                if (this.transform.position == v) { index++; break; }

                index++;
            }

            this.transform.position = path[index];
        }

        Debug.Log("Moving");
    }

    /// <summary>
    /// Sets the position in the current frame when coming back
    /// </summary>
    void SetPositionReverse()
    {
        Debug.Log("Moving reverse");
        if (this.transform.position == path[0]) {
            if (loop) { InvokeRepeating(nameof(SetPosition), 0, Time.deltaTime); } 

            CancelInvoke(nameof(SetPositionReverse)); 

            return; 
        }
        else
        {
            int index = path.Length - 1;
            for(int i = path.Length - 1; i >= 0; i--)
            {
                if(this.transform.position == path[i]) { index--; break; }
                index--;
            }
            this.transform.position = path[index];
        }
    }

    /// <summary>
    /// Is the animation playing?
    /// </summary>
    /// <returns></returns>
    public bool IsPlaying()
    {
        return isPlaying;
    }
    #endregion
}
#if UNITY_EDITOR
[CustomEditor(typeof(CustomAnimation), true)]
[CanEditMultipleObjects]
[DisallowMultipleComponent]
public class CustomAnimationEditor : Editor
{

    bool foldout_1;
    bool foldout_2;
    bool foldout_3;
    bool foldout_4;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var animation = target as CustomAnimation;

        GUILayout.Label("SOLO SIRVE PARA ANIMACIONES DE TRANSFORM DEL CONTENEDOR", EditorStyles.boldLabel);
        GUILayout.Space(20);

        foldout_1 = EditorGUILayout.BeginFoldoutHeaderGroup(foldout_1, "Speed & Time");

        GUILayout.Space(10);

        if (foldout_1)
        {
            GUILayout.Label("Curva de velocidad de la animación", EditorStyles.boldLabel);
            SerializedProperty curve = serializedObject.FindProperty("curve");
            EditorGUILayout.PropertyField(curve);

            GUILayout.Space(10);

            GUILayout.Label("La velocidad máxima de la animación", EditorStyles.boldLabel);
            animation.speed = EditorGUILayout.FloatField(animation.speed, EditorStyles.miniTextField);

            GUILayout.Space(10);

            GUILayout.Label("El tiempo que dura la animación", EditorStyles.boldLabel);
            animation.time = EditorGUILayout.FloatField(animation.time, EditorStyles.miniTextField);
        }

        EditorGUILayout.EndFoldoutHeaderGroup();

        GUILayout.Space(10);

        foldout_2 = EditorGUILayout.BeginFoldoutHeaderGroup(foldout_2, "Looping");

        GUILayout.Space(10);

        if (foldout_2)
        {
            GUILayout.Label("Vuelve a la posición inicial?", EditorStyles.boldLabel);
            animation.returnToOrigin = GUILayout.Toggle(animation.returnToOrigin, "", EditorStyles.toggle);

            GUILayout.Space(10);

            GUILayout.Label("Se reproduce en bucle la animación?", EditorStyles.boldLabel);
            animation.loop = GUILayout.Toggle(animation.loop, "", EditorStyles.toggle);

            GUILayout.Space(10);

            GUILayout.Label("Se aplica el movimiento a la animación?", EditorStyles.boldLabel);
            animation.applyRootMotion = GUILayout.Toggle(animation.applyRootMotion, "", EditorStyles.toggle);

            GUILayout.Space(10);

            if (animation.loop || animation.returnToOrigin)
            {
                GUILayout.Label("Tiempo que para antes de volver", EditorStyles.boldLabel);
                animation.timeToWaitBeforeReturn = EditorGUILayout.FloatField(animation.timeToWaitBeforeReturn, EditorStyles.miniTextField);
                GUILayout.Space(10);
            }
        }

        EditorGUILayout.EndFoldoutHeaderGroup();

        GUILayout.Space(10);

        foldout_3 = EditorGUILayout.BeginFoldoutHeaderGroup(foldout_3, "Space & Direction");

        GUILayout.Space(10);

        if (foldout_3)
        {
            GUILayout.Label("La distancia que se desplaza el objeto", EditorStyles.boldLabel);
            animation.distance = EditorGUILayout.FloatField(animation.distance);

            GUILayout.Space(10);

            GUILayout.Label("El espacio en el que se ejecuta la animación", EditorStyles.boldLabel);
            animation.space = (CustomAnimation.AnimationSpace)EditorGUILayout.EnumPopup(animation.space);

            GUILayout.Space(10);

            GUILayout.Label("La dirección de la animación", EditorStyles.boldLabel);
            animation.direction = (CustomAnimation.AnimationDirection)EditorGUILayout.EnumPopup(animation.direction);

            GUILayout.Space(10);

            GUILayout.Label("El tipo de animación", EditorStyles.boldLabel);
            animation.type = (CustomAnimation.AnimationType)EditorGUILayout.EnumPopup(animation.type);

            GUILayout.Space(10);

            if (animation.direction == CustomAnimation.AnimationDirection.Custom)
            {
                GUILayout.Label("La dirección en la que se reproducirá la animación", EditorStyles.boldLabel);
                animation.customDirection = EditorGUILayout.Vector3Field("", animation.customDirection);

                GUILayout.Space(10);
            }

        }

        EditorGUILayout.EndFoldoutHeaderGroup();

        GUILayout.Space(10);

        foldout_4 = EditorGUILayout.BeginFoldoutHeaderGroup(foldout_4, "Options");

        GUILayout.Space(10);

        if (foldout_4)
        {
            GUILayout.Label("Se lanza la animación al empezar?", EditorStyles.boldLabel);
            animation.launchOnStart = GUILayout.Toggle(animation.launchOnStart, "", EditorStyles.toggle);

            GUILayout.Space(10);
        }

        EditorGUILayout.EndFoldoutHeaderGroup();

        GUILayout.Space(10);

        serializedObject.ApplyModifiedProperties();
    }

    public void OnEnable()
    {
        var animation = target as CustomAnimation;

        animation.curve = AnimationCurve.Linear(0, 0, 1, 1);
    }
}
#endif