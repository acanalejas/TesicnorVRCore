using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;

[RequireComponent(typeof(Collider))]
public class VRCollider : MonoBehaviour, VRGripInterface
{
    #region PARAMETERS
    /// <summary>
    /// El sonido que hace al agarrarse
    /// </summary>
    [Header("El sonido que hace al agarrarse")]
    [HideInInspector]public AudioSource grabSound;

    /// <summary>
    /// El sonido que hace al soltarse
    /// </summary>
    [Header("El sonido que hace al soltarse")]
    [HideInInspector] public AudioSource releaseSound;

    /// <summary>
    /// El sonido que hace al llegar al target
    /// </summary>
    [Header("El sonido que hace al llegar al target")]
    [HideInInspector] public AudioSource targetSound;

    /// <summary>
    /// Controla si al soltarlo cae, o se mantiene estático
    /// </summary>
    [Header("Controla si al soltarlo cae, o se mantiene estático")]
    [HideInInspector] public bool simulateOnDrop = false;

    /// <summary>
    /// Controla si se puede soltar sin más, o necesita alguna condición
    /// </summary>
    [Header("Controla si se puede soltar sin más, o necesita alguna condición")]
    [HideInInspector] public bool canRelease = false;

    /// <summary>
    /// Controla si tiene un target donde soltarlo
    /// </summary>
    [Header("Controla si tiene un target donde soltarlo")]
    [HideInInspector] public bool hasTarget = false;

    /// <summary>
    /// Se ilumina el objeto al ser seleccionable?
    /// </summary>
    [Header("Se ilumina el objeto al ser seleccionable?")]
    [HideInInspector] public bool hasHighlight = false;

    public enum AttachmentMode { Normal, positionOffset, rotationAndPositionOffset, Animation, None};

    /// <summary>
    /// El modo en el que se pega a la mano al agarrar
    /// </summary>
    [Header("El modo en el que se pega a la mano al agarrar")]
    [HideInInspector] public AttachmentMode attachmentMode;

    /// <summary>
    /// El offset en la posicion cuando se agarra el objeto
    /// </summary>
    [Header("El offset en la posición cuando se agarra el objeto")]
    [HideInInspector] public Vector3 positionOffset;

    /// <summary>
    /// El offset en la rotacion cuando se agarra el objeto
    /// </summary>
    [Header("El offset en la rotacion cuando se agarra el objeto")]
    [HideInInspector] public Vector3 rotationOffset;

    /// <summary>
    /// Evento que se ejecuta cuando el jugador agarra el objeto
    /// </summary>
    [Header("Evento que se ejecuta cuando el jugador agarra el objeto")]
    [HideInInspector] public UnityEvent onGrab;

    [Header("Evento que se ejecuta cuando el jugador suelta el objeto")]
    [HideInInspector] public UnityEvent onRelease;

    [Header("Evento que se ejecuta cuando el objeto llega al objetivo")]
    [HideInInspector] public UnityEvent OnTargetReached;

    /// <summary>
    /// Evento que se ejecuta cuando el objeto se suelta en el objetivo
    /// </summary>
    [Header("Evento que se ejecuta cuando el objeto se suelta en el objetivo")]
    [HideInInspector] public UnityEvent<GameObject> onTargetReached;

    /// <summary>
    /// Maneja si se puede agarrar
    /// </summary>
    [Header("Maneja si se puede agarrar")]
    [HideInInspector] public bool grabbable = true;

    /// <summary>
    /// Collider usado para tener colisión con el escenario
    /// cuando el objeto se suelta y se cae
    /// </summary>
    private Collider dropCollider;

    /// <summary>
    /// Rigidbody de este objeto
    /// </summary>
    private Rigidbody rigidbody;

    /// <summary>
    /// La mano que esta agarrando el objecto en este instante
    /// </summary>
    protected GrippingHand grippingHand = null;

    /// <summary>
    /// La distancia del objeto a la mano en el momento de agarre
    /// </summary>
    private float distanceOnGrab;

    /// <summary>
    /// Posicion de este objeto cuando se agarra
    /// </summary>
    private Vector3 positionOnGrab;

    /// <summary>
    /// Rotacion de este objeto cuando se agarra
    /// </summary>
    private Vector3 rotationOnGrab;

    /// <summary>
    /// Masa del objeto en kg
    /// </summary>
    [Header("Masa del objeto en kg")]
    [HideInInspector] public float mass = 5;

    /// <summary>
    /// Cantidad de rotación de la mano cada frame que se agarra
    /// </summary>
    private Vector3 lastFrameRotation = Vector3.zero;

    #region Release Condition
    [Space(20)]
    [Header("========= SOLO SI NO SE PUEDE SOLTAR NORMAL ============")]
    [Space(20)]

    [Header("Tiene multiples objetivos?")]
    [HideInInspector]
    [SerializeField] public bool hasMultipleTargets = false;

    [Header("Los targets donde debe acercarse el objeto")]
    [SerializeField]
    [HideInInspector] public VRColliderReleaseTarget[] targets;
    /// <summary>
    /// El target donde debe acercarse el objeto
    /// </summary>
    [Header("El target donde debe acercarse el objeto")]
    [SerializeField][HideInInspector] public VRColliderReleaseTarget target;

    /// <summary>
    /// Si se teletransporta al entrar al trigger o si hay que soltarlo manualmente
    /// </summary>
    [Header("Si se teletransporta al entrar al trigger \n o si hay que soltarlo manualmente")]
    [HideInInspector] public bool DropTeleport = true;

    /// <summary>
    /// Si vuelve a una posicion fija, o a la posición del último agarre
    /// </summary>
    public enum releaseType { start, onGrab, holder}
    [Header("Si vuelve a una posición fija, o a la posición de último agarre")]
    [HideInInspector] public releaseType release = releaseType.start;

    /// <summary>
    /// En caso de ser tipo holder, el holder
    /// </summary>
    [Header("En caso de ser de tipo holder, el holder")]
    [HideInInspector] public Transform holder;

    /// <summary>
    /// Posición al iniciar la escena
    /// </summary>
    private Vector3 startPosition;

    /// <summary>
    /// Rotación al iniciar la escena
    /// </summary>
    private Vector3 startRotation;
    #endregion

    #endregion
    #region FUNCTIONS

    public virtual void Awake()
    {
        gameObject.GetComponent<Collider>().isTrigger = true;
        rigidbody = gameObject.GetComponent<Rigidbody>();
        if (!rigidbody) rigidbody = gameObject.AddComponent<Rigidbody>();

        rigidbody.useGravity = false;

        startPosition = this.transform.position;
        startRotation = this.transform.rotation.eulerAngles;
    }

    /// <summary>
    /// Desactiva las fisicas cuando se agarra el objeto
    /// </summary>
    protected void SetPhysicsOnGrab()
    {
        if (rigidbody) { rigidbody.useGravity = false; rigidbody.isKinematic = true; }


        //SOLO POR SEGURIDAD
        //if (GetComponent<Rigidbody>()) Destroy(GetComponent<Rigidbody>());
    }

    /// <summary>
    /// Setea los parámetros necesarios cuando se agarra el objeto
    /// mayormente variables para simulación
    /// </summary>
    /// <param name="hand"></param>
    protected void SetParamsOnGrab(GrippingHand hand)
    {
        if (grippingHand != null) /*this.NormalRelease();*/  grippingHand.grippedObject = null; 
        hand.grippedObject = this.gameObject;
        hand.isGripping = true;
        grippingHand = hand;
        distanceOnGrab = Vector3.Distance(grippingHand.transform.position, this.transform.position);
        positionOnGrab = this.transform.position;
        rotationOnGrab = this.transform.rotation.eulerAngles;
    }

    public void SetSoundOnGrab()
    {
        if(grabSound != null)
        {
            grabSound.loop = false;
            grabSound.Play();
        }
    }

    public void SetSoundOnRelease()
    {
        if(releaseSound != null)
        {
            releaseSound.loop = false;
            releaseSound.Play();
        }
    }

    /// <summary>
    /// Añade un impulso cuando se suelta el objeto,
    /// basado en la velocidad de la mano del jugador
    /// </summary>
    private void AddImpulseAtRelease()
    {
        if (rigidbody && grippingHand)
        {
            Vector3 force = (grippingHand.velocity/0.5f) * mass;
            rigidbody.AddForce(force, ForceMode.Impulse);
        }
    }

    /// <summary>
    /// Funcion usada para soltar el objeto sin condiciones
    /// </summary>
    protected virtual void NormalRelease()
    {
        StopCoroutine("Attach");
        this.transform.parent = null;
        if (simulateOnDrop) SetSimulateOnDrop();
        if (grippingHand) grippingHand.grippedObject = null;

        grippingHand = null;
    }
    /// <summary>
    /// Funcion que se usa para soltar el objeto en caso de que no se pueda soltar normal y se requieran condiciones
    /// </summary>
    private void ConditionsRelease()
    {
        NormalRelease();

        if (target && target.conditionCompleted && hasTarget)
        {
            this.transform.position = target.gameObject.transform.position;
            this.transform.rotation = target.gameObject.transform.rotation;
            if (target.DisableWhenRelease) this.gameObject.SetActive(false);
            OnTargetReached.Invoke();
            onTargetReached.Invoke(target.gameObject);
        }
        else
        {
            if (release == releaseType.onGrab)
            {
                this.transform.position = positionOnGrab;
                this.transform.rotation = Quaternion.Euler(rotationOnGrab);
            }
            else if (release == releaseType.start)
            {
                this.transform.position = startPosition;
                this.transform.rotation = Quaternion.Euler(startRotation);
            }
            else if (release == releaseType.holder)
            {
                this.transform.position = holder.transform.position;
                this.transform.parent = holder.parent;
                this.transform.rotation = holder.rotation;
            }
        }
    }


    #region GRIP INTERFACE

    /// <summary>
    /// Grab this object
    /// </summary>
    /// <param name="hand"> hand that grabs</param>
    public virtual void Grab(GrippingHand hand)
    {
        onGrab?.Invoke();
        SetParamsOnGrab(hand);
        SetPhysicsOnGrab();
        SetSoundOnGrab();
        if (gameObject.activeSelf) StartCoroutine("Attach");
    }

    /// <summary>
    /// Reposition the object with the hand's position
    /// </summary>
    public void RepositionOnGrab()
    {
        //Position
        this.transform.parent = grippingHand.transform;

        //Rotation
        //Si es el primer frame, no debería rotar
        if (lastFrameRotation == Vector3.zero) lastFrameRotation = grippingHand.transform.rotation.eulerAngles;

        Vector3 rotationDistance = grippingHand.transform.rotation.eulerAngles - lastFrameRotation;
        Vector3 rotation = this.transform.rotation.eulerAngles + rotationDistance;
        //this.transform.rotation = Quaternion.Euler(rotation);

        lastFrameRotation = grippingHand.transform.rotation.eulerAngles;
    }

    /// <summary>
    /// Reposition the object with the hand's position with an offset
    /// </summary>
    /// <param name="_offset"></param>
    public void RepositionOnGrab(Vector3 _offset)
    {
        RepositionOnGrab();
        Vector3 _fixed = Vector3.zero;
        //if (grippingHand.handType == GrippingHand.HandType.left) _fixed = OffsetManager.Instance.currentHandsOffset.GetOffsetPositionFromForward(_offset, true);
        //else _fixed = OffsetManager.Instance.currentHandsOffset.GetOffsetPositionFromForward(_offset, false);
        /*if (grippingHand.handType == GrippingHand.HandType.right) _fixed = new Vector3(OffsetManager.Instance.currentHandsOffset.RightPositionMult.x * _offset.x,
            OffsetManager.Instance.currentHandsOffset.RightPositionMult.y * _offset.y, OffsetManager.Instance.currentHandsOffset.RightPositionMult.z * _offset.z);

        else if (grippingHand.handType == GrippingHand.HandType.left) _fixed = new Vector3(OffsetManager.Instance.currentHandsOffset.LeftPositionMult.x * _offset.x,
             OffsetManager.Instance.currentHandsOffset.RightPositionMult.y * _offset.y, OffsetManager.Instance.currentHandsOffset.RightPositionMult.z * _offset.z);*/
        this.transform.localPosition = _offset;
    }

    /// <summary>
    /// Reposition the object with the hand's position, rotation, and both with offsets
    /// </summary>
    /// <param name="_offsetPosition"></param>
    /// <param name="_offsetRotation"></param>
    public void RepositionOnGrab(Vector3 _offsetPosition, Vector3 _offsetRotation)
    {
        RepositionOnGrab(_offsetPosition);
        Vector3 rotation = Vector3.zero;
        float multiplier = 1;
        //if (grippingHand.handType == GrippingHand.HandType.left) { rotation = OffsetManager.Instance.currentHandsOffset.LeftHandRotationOffset; multiplier = OffsetManager.Instance.currentHandsOffset.LeftHandMultiplierOffsetRotation; }
        //if (grippingHand.handType == GrippingHand.HandType.right) { rotation = OffsetManager.Instance.currentHandsOffset.RightHandRotationOffset; multiplier = OffsetManager.Instance.currentHandsOffset.RightHandMultiplierOffsetRotation; }
        this.transform.localRotation = Quaternion.Euler(_offsetRotation);
    }

    /// <summary>
    /// Release this object
    /// </summary>
    public virtual void Release()
    {
        if (canBeReleased()) NormalRelease();
        else ConditionsRelease();

        onRelease?.Invoke();
        SetSoundOnRelease();
    }

    /// <summary>
    /// If this object can be grabbed
    /// </summary>
    /// <returns></returns>
    public bool canBeGrabbed()
    {
        return grabbable;
    }

    /// <summary>
    /// If this object can just be released, or it has some conditions
    /// </summary>
    /// <returns></returns>
    public bool canBeReleased()
    {
        return canRelease;
    }

    /// <summary>
    /// Esta siendo agarrado el objeto?
    /// </summary>
    /// <returns></returns>
    public bool isGrabbed()
    {
        return grippingHand;
    }
    /// <summary>
    /// Get the hand that is grabbing this object
    /// </summary>
    /// <returns></returns>
    public GrippingHand GetGrippingHand()
    {
        return grippingHand;
    }

    /// <summary>
    /// Set all the necesary values to simulate physics on object drop
    /// </summary>
    public void SetSimulateOnDrop()
    {
        if (!rigidbody) rigidbody = gameObject.AddComponent<Rigidbody>();

        rigidbody.useGravity = true;
        rigidbody.isKinematic = false;
        rigidbody.freezeRotation = false;
        rigidbody.mass = mass;

        Collider actualCollider = GetComponent<Collider>();

        if (!dropCollider)
        {
            if (actualCollider.GetType() == typeof(BoxCollider)) dropCollider = gameObject.AddComponent<BoxCollider>();
            else if (actualCollider.GetType() == typeof(MeshCollider)) dropCollider = gameObject.AddComponent<MeshCollider>();
            else if (actualCollider.GetType() == typeof(SphereCollider)) dropCollider = gameObject.AddComponent<SphereCollider>();
            else if (actualCollider.GetType() == typeof(CapsuleCollider)) dropCollider = gameObject.AddComponent<CapsuleCollider>();
            else dropCollider = gameObject.AddComponent<BoxCollider>();

            if(actualCollider.GetType() == typeof(BoxCollider))
            {
                BoxCollider box = dropCollider as BoxCollider;
                BoxCollider actual = actualCollider as BoxCollider;
                box.size = actual.size; box.center = actual.center;
            }
            else if(actualCollider.GetType() == typeof(CapsuleCollider))
            {
                CapsuleCollider capsule = dropCollider as CapsuleCollider;
                CapsuleCollider actual = actualCollider as CapsuleCollider;
                capsule.radius = actual.radius; capsule.center = actual.center; capsule.direction = actual.direction; capsule.height = actual.height;
            }
            else if(actualCollider.GetType() == typeof(SphereCollider))
            {
                SphereCollider sphere = dropCollider as SphereCollider;
                SphereCollider actual = actualCollider as SphereCollider;
                sphere.radius = actual.radius; sphere.center = actual.center;
            }
            
        }

        AddImpulseAtRelease();
    }


    WaitForEndOfFrame frame = new WaitForEndOfFrame();
    public virtual IEnumerator Attach()
    {
        while (grippingHand != null)
        {
            switch (attachmentMode)
            {
                case AttachmentMode.Normal:
                    RepositionOnGrab();
                    break;
                case AttachmentMode.positionOffset:
                    RepositionOnGrab(positionOffset);
                    break;
                case AttachmentMode.rotationAndPositionOffset:
                    RepositionOnGrab(positionOffset, rotationOffset);
                    break;
                case AttachmentMode.Animation:
                    GetComponent<Animator>().SetTrigger("Grab");
                    break;
                case AttachmentMode.None:

                    break;
            }
            yield return frame;
        }
    }

    /// <summary>
    /// Setea el valor del bool que controla si se puede o no agarrar
    /// </summary>
    /// <param name="_value"></param>
    /// <returns></returns>
    public bool SetGrabbable(bool _value)
    {
        grabbable = _value;

        return _value;
    }

    public void SetAvailableEffects(bool value)
    {
        if (isGrabbed())
        {
            SetHighlightEffect(false);
            return;
        }
        SetHighlightEffect(value);
    }

    private void SetHighlightEffect(bool value)
    {
        var effect = GetComponent<HighlightEffect>();
        if(effect != null)
        effect.SetHighlight(value);
    }
    #endregion
    #endregion
}

#if UNITY_EDITOR
[CustomEditor(typeof(VRCollider), true)]
[CanEditMultipleObjects]
public class VRColliderEditor : Editor
{
    [HideInInspector] public VRCollider collider;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        collider = target as VRCollider;

        GUILayout.Label("Cuando se suelta tiene físicas?", EditorStyles.boldLabel);
        collider.simulateOnDrop = GUILayout.Toggle(collider.simulateOnDrop, "Simulate on drop");

        GUILayout.Space(10);

        if (collider.simulateOnDrop)
        {

            GUILayout.Label("La masa del objeto en Kg", EditorStyles.boldLabel);
            collider.mass = EditorGUILayout.FloatField(collider.mass, "Masa del objeto");

            GUILayout.Space(10);
        }

        GUILayout.Label("Se ilumina el objeto al estar en rango para agarrarlo?");
        collider.hasHighlight = GUILayout.Toggle(collider.hasHighlight, "Has Highlight");

        GUILayout.Space(10);

        if (collider.hasHighlight)
        {
            if (!collider.GetComponent<HighlightEffect>())
            {
                collider.gameObject.AddComponent<HighlightEffect>();
            }
        }
        else
        {
            if (collider.GetComponent<HighlightEffect>())
            {
                DestroyImmediate(collider.gameObject.GetComponent<HighlightEffect>());
            }
        }

        GUILayout.Label("Se puede soltar normal o tiene condiciones?", EditorStyles.boldLabel);
        collider.canRelease = GUILayout.Toggle(collider.canRelease, "Can be released?");

        GUILayout.Space(10);

        GUILayout.Label("Tiene un objetivo donde soltarse?", EditorStyles.boldLabel);
        collider.hasTarget = GUILayout.Toggle(collider.hasTarget, "Has a target?");

        GUILayout.Space(10);

        GUILayout.Label("El modo en el que se añade a la mano al ser agarrado", EditorStyles.boldLabel);
        collider.attachmentMode = (VRCollider.AttachmentMode)EditorGUILayout.EnumPopup(collider.attachmentMode);

        if(collider.attachmentMode == VRCollider.AttachmentMode.positionOffset)
        {
            GUILayout.Space(10);

            GUILayout.Label("Offset en la posición local del objeto respecto a la mano al agarrar", EditorStyles.boldLabel);
            collider.positionOffset = EditorGUILayout.Vector3Field("Position offset", collider.positionOffset);
        }
        else if(collider.attachmentMode == VRCollider.AttachmentMode.rotationAndPositionOffset)
        {
            GUILayout.Space(10);

            GUILayout.Label("Offset en la posición local del objeto respecto a la mano al agarrar", EditorStyles.boldLabel);
            collider.positionOffset = EditorGUILayout.Vector3Field("Position offset", collider.positionOffset);

            GUILayout.Space(10);

            GUILayout.Label("Offset en la rotación local del objeto respecto a la mano al agarrar", EditorStyles.boldLabel);
            collider.rotationOffset = EditorGUILayout.Vector3Field("Rotation offset", collider.rotationOffset);
        }

        if (collider.hasTarget)
        {
            GUILayout.Label("Tiene multiples objetivos el objeto?");
            collider.hasMultipleTargets = GUILayout.Toggle(collider.hasMultipleTargets, "Has Multiple Targets");
            GUILayout.Space(10);

            if (!collider.hasMultipleTargets)
            {
                SerializedProperty target = serializedObject.FindProperty("target");
                EditorGUILayout.PropertyField(target, new GUIContent("Target"));
            }
            else
            {
                SerializedProperty targets = serializedObject.FindProperty("targets");
                EditorGUILayout.PropertyField(targets, new GUIContent("Targets"));
            }
            

            GUILayout.Space(10);

            SerializedProperty targetSound = serializedObject.FindProperty("targetSound");
            EditorGUILayout.PropertyField(targetSound, new GUIContent("Target Sound"));

            GUILayout.Space(10);
        }

        if (!collider.canRelease)
        {
            GUILayout.Label("Se teletransporta al soltar?", EditorStyles.boldLabel);
            collider.DropTeleport = GUILayout.Toggle(collider.DropTeleport, "Drop Teleport");

            GUILayout.Space(10);

            if (collider.DropTeleport)
            {
                GUILayout.Label("A donde se teletransporta?", EditorStyles.boldLabel);
                collider.release = (VRCollider.releaseType)EditorGUILayout.EnumPopup(collider.release);

                GUILayout.Space(10);
                if(collider.release == VRCollider.releaseType.holder)
                {
                    SerializedProperty holder = serializedObject.FindProperty("holder");
                    EditorGUILayout.PropertyField(holder, new GUIContent("Holder"));
                }
            }
        }

        SerializedProperty onGrabAudio = serializedObject.FindProperty("grabSound");
        EditorGUILayout.PropertyField(onGrabAudio, new GUIContent("OPCIONAL : Grab sound"));

        SerializedProperty onReleaseAudio = serializedObject.FindProperty("releaseSound");
        EditorGUILayout.PropertyField(onReleaseAudio, new GUIContent("OPCIONAL : Release sound"));

        SerializedProperty onGrab = serializedObject.FindProperty("onGrab");
        EditorGUILayout.PropertyField(onGrab, new GUIContent("On Grab"));

        SerializedProperty onRelease = serializedObject.FindProperty("onRelease");
        EditorGUILayout.PropertyField(onRelease, new GUIContent("On Release"));

        SerializedProperty onTargetReached = serializedObject.FindProperty("OnTargetReached");
        EditorGUILayout.PropertyField(onTargetReached, new GUIContent("On Target Reached"));

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
