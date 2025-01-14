using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VRColliderPath : VRCollider
{
    #region PARAMETERS
    /// <summary>
    /// Define que tipo de camino hay que seguir, si el objeto tiene que rotar
    /// o en cambio si tiene que moverse para seguirlo
    /// </summary>
    public enum PathType
    {
        Rotation,
        Position
    }

    /// <summary>
    /// Define el tipo de camino de este objeto
    /// </summary>
    [Space(20)][Header("============ SPECIFIC ATRIBUTES ============")][Space(10)]
    [Header("Define el tipo de camino de este objeto")]
    public PathType pathType = PathType.Position;

    [Header("Evento que se lanza al llegar al final del camino")]
    public UnityEvent OnPathEndReached;

    [Header("Evento que se lanza al volver al principio del camino")]
    public UnityEvent OnPathBeginingReached;

    [Header("Se deberia desactivar al llegar al final?")]
    public bool bShouldDisableOnEnd;

    /// <summary>
    /// Numero de puntos en los que se divide el camino
    /// </summary>
    [Header("Número de puntos en los que se divide el camino")]
    [SerializeField] protected int pointNumber = 30;

    /// <summary>
    /// Lista de puntos en el camino
    /// </summary>
    public List<Vector3> pathPoints = new List<Vector3>();

    /// <summary>
    /// El punto en el que estamos actualmente del camino
    /// </summary>
    [HideInInspector] public int currentPoint = 0;


    /// <summary>
    /// Si el punto inicial es la posicion inicial del objeto
    /// </summary>
    [Header("========== FOR POSITION ===========")]
    [Space(10)]
    [Header("Si el punto inicial es la posicion inicial del objeto")]
    [SerializeField] bool initOnPosition;

    /// <summary>
    /// La posicion inicial del camino
    /// </summary>
    [Header("La posición inicial del camino")]
    public Vector3 initialPosition;

    /// <summary>
    /// La posicion final del objeto
    /// </summary>
    [Header("La posicion final del objeto")]
    public Vector3 finalPosition;

    /// <summary>
    /// La posicion de la mano al agarrar
    /// </summary>
    protected Vector3 initialHandPosition;

    /// <summary>
    /// La distancia que se mueve la mano
    /// </summary>
    protected Vector3 handDistance;

    /// <summary>
    /// Si empieza en la rotacion inicial del objeto
    /// </summary>
    [Header("========= FOR ROTATION =========")]
    [Space(10)]
    [Header("Si empieza en la rotación inicial del objeto")]
    [SerializeField] bool initOnRotation;

    /// <summary>
    /// La rotacion inicial del objeto
    /// </summary>
    [Header("La rotacion inicial del camino")]
    [SerializeField] float initialRotation;

    /// <summary>
    /// La rotacion final del objeto
    /// </summary>
    [Header("La rotacion final del camino")]
    [SerializeField] float finalRotation;

    /// <summary>
    /// El margen de error para considerar el camino terminado
    /// </summary>
    [Header("El margen de error para considerar el camino terminado")]
    [SerializeField] private float Threshold = 20;

    /// <summary>
    /// El pivote de la rotación
    /// </summary>
    [Header("El pivote de la rotacion")]
    [SerializeField] Transform rotationPivot;

    /// <summary>
    /// Si la rotación va en positivo o en negativo
    /// </summary>
    [Header("Si la rotación va en positivo o en negativo")]
    [SerializeField] bool isPositive = true;

    /// <summary>
    /// La distancia inicial de la mano al pivote al agarrar el objeto
    /// </summary>
    private Vector3 initialDistance;

    /// <summary>
    /// Los ángulos girados 
    /// </summary>
    private float currentAngles = 0;

    /// <summary>
    /// Los ejes sobre los que se puede rotar
    /// </summary>
    public enum Axis { x, y, z};
    /// <summary>
    /// Eje sobre el que se gira para hacer el camino
    /// </summary>
    [Header("Eje sobre el que se gira para hacer el camino")]
    public Axis axis = Axis.y;

    private float anglePerSection = 0;

    private Vector3 InitialRotation;

    #endregion

    #region FUNCTIONS
    public override void Awake()
    {
        base.Awake();
        SetPathPoints();
        SetRotationPath();

        if (bShouldDisableOnEnd) OnPathEndReached.AddListener(DisableOnEnd);

        currentAngles = axis == Axis.x ? transform.localRotation.x : axis == Axis.y ? transform.localRotation.y : transform.localRotation.z;

        InitialRotation = this.transform.localRotation.eulerAngles;
    }

    private void DisableOnEnd()
    {
        this.Release();
        this.GetComponent<Collider>().enabled = false;
    }

    /// <summary>
    /// Elige que camino setear, dependiendo del modo en el que se elija hacer el camino
    /// </summary>
    void SetPathPoints()
    {
        if (pathType == PathType.Position) SetPositionPath();
    }

    void SetPositionPath()
    {
        Vector3 _initialPosition = Vector3.zero;

        //Elijo cual es la posicion inicial, dependiendo de si se elige la del objeto, o se setea aparte
        if (initOnPosition) _initialPosition = this.transform.localPosition;
        else _initialPosition = initialPosition;

        //Obtengo la longitud de cada segmento y la direccion del camino
        float pathMult = Vector3.Distance(_initialPosition, finalPosition) / pointNumber;
        Vector3 direction = (finalPosition - _initialPosition).normalized;

        //Añado cada punto al camino para poder seguirlo
        pathPoints.Add(initialPosition);
        for(int i = 1; i < pointNumber - 1; i++)
        {
            pathPoints.Add(_initialPosition + direction * pathMult * i);
        }
        pathPoints.Add(finalPosition);
    }

    void SetRotationPath()
    {
        //if (axis != Axis.z || axis != Axis.x) return;

        float xExtent = this.GetComponent<MeshRenderer>() ? this.GetComponent<MeshRenderer>().bounds.extents.x : this.GetComponent<BoxCollider>().bounds.extents.x;
        float yExtent = this.GetComponent<MeshRenderer>() ? this.GetComponent<MeshRenderer>().bounds.extents.y : this.GetComponent<BoxCollider>().bounds.extents.y;
        float zExtent = this.GetComponent<MeshRenderer>() ? this.GetComponent<MeshRenderer>().bounds.extents.z : this.GetComponent<BoxCollider>().bounds.extents.z;

        float extent = axis == Axis.z ? yExtent : axis == Axis.y ? xExtent : zExtent;

        Vector3 _direction = axis == Axis.z ? this.transform.up * extent : axis == Axis.x ? this.transform.forward * extent : this.transform.right * extent;
        float angleDiff = finalRotation - initialRotation;
        anglePerSection = angleDiff / pointNumber;

        for(int i = 0; i < pointNumber - 1; i++)
        {
            Vector3 point = Quaternion.AngleAxis(initialRotation + i * anglePerSection, axis == Axis.z ? transform.forward : axis == Axis.x ? transform.right : transform.up) * _direction;
            pathPoints.Add(point);
            Debug.DrawLine(rotationPivot ? rotationPivot.position : this.transform.position, point, Color.red, Mathf.Infinity);
            
        }
        
    }
    public override void Grab(GrippingHand hand)
    {
        base.Grab(hand);

        if(this.transform.parent != null)
        initialHandPosition = this.transform.parent.InverseTransformPoint(hand.transform.position);
        else initialHandPosition = hand.transform.position;

        SelectCoroutine();
    }
    public override void Release()
    {
        base.Release();
        StopAllCoroutines();
    }
    public override IEnumerator Attach()
    {
        yield return new WaitForEndOfFrame();
    }
    void SelectCoroutine()
    {
        switch (pathType)
        {
            case PathType.Position:
                StartCoroutine("AttachPosition");
                break;
            case PathType.Rotation:
                StartCoroutine("AttachRotation");
                break;
        }
    }
    WaitForEndOfFrame frame = new WaitForEndOfFrame();
    private IEnumerator AttachPosition()
    {
        while (isGrabbed())
        {
            handDistance = this.transform.parent.InverseTransformPoint(grippingHand.transform.position) - initialHandPosition;
            this.transform.localPosition = pathPoints[pointToMove(handDistance)];
            if (isPathCompleted()) OnPathEndReached.Invoke();
            yield return frame;
        }
    }

    private IEnumerator AttachRotation()
    {
        initialDistance = grippingHand.transform.position - rotationPivot.position;
        while (isGrabbed())
        {
            Vector3 direction = (grippingHand.transform.position - rotationPivot.transform.position).normalized;

            float distance = Vector3.Distance(this.transform.position, this.rotationPivot.transform.position);

            this.transform.position = distance * direction + rotationPivot.position;

            float angles = pointToRotate() * anglePerSection + initialRotation;

            //Make a snap
            if (currentPoint == 0) angles = initialRotation;
            else if (currentPoint == pathPoints.Count - 1) angles = finalRotation;

            //Adjust the local rotation of the object
            this.transform.localRotation = Quaternion.Euler(new Vector3(
                axis == Axis.x ? Mathf.Clamp(angles, isPositive ? initialRotation : finalRotation, (isPositive ? finalRotation : initialRotation) + Threshold) : InitialRotation.x,
                axis == Axis.y ? Mathf.Clamp(angles, isPositive ? initialRotation : finalRotation, (isPositive ? finalRotation : initialRotation) + Threshold) : InitialRotation.y,
                axis == Axis.z ? Mathf.Clamp(angles, isPositive ? initialRotation : finalRotation, (isPositive ? finalRotation : initialRotation) + Threshold) : InitialRotation.z));


            if (axis == Axis.y) currentAngles = this.transform.localRotation.y;
            else if (axis == Axis.x) currentAngles = this.transform.localRotation.x;
            else if (axis == Axis.z) currentAngles = this.transform.localRotation.z;

            if (isPathCompleted()) OnPathEndReached.Invoke();
            if (IsPathBegining()) OnPathBeginingReached.Invoke();

            yield return new WaitForEndOfFrame();
        }
    }

    /// <summary>
    /// Detecta el punto al que moverse, como no se puede mover a cualquier punto del camino, se mira que esté
    /// entre el siguiente y el anterior para evitar grandes saltos
    /// </summary>
    /// <returns></returns>
    protected virtual int pointToMove()
    {
        int previousIndex = currentPoint - 1;
        if (currentPoint == 0) previousIndex = 0;
        int nextIndex = currentPoint + 1;
        if (currentPoint == pathPoints.Count - 1) nextIndex = currentPoint;

        Vector3 _currentPoint =     pathPoints[currentPoint];
        Vector3 _previousPoint =    pathPoints[previousIndex];
        Vector3 _nextPoint =        pathPoints[nextIndex];

        float _currentDistance =    Vector3.Distance(grippingHand.transform.position, _currentPoint);
        float _previousDistance =   Vector3.Distance(grippingHand.transform.position, _previousPoint);
        float _nextDistance =       Vector3.Distance(grippingHand.transform.position, _nextPoint);

        if(_previousDistance < _currentDistance || _nextDistance < _currentDistance)
        {
            if (_previousDistance < _nextDistance) { currentPoint = previousIndex; return previousIndex; }

            else if (_nextDistance < _previousDistance) { currentPoint = nextIndex; return nextIndex; }
        }

        return currentPoint;
    }
    protected virtual int pointToRotate()
    {
        int previousIndex = currentPoint - 1;
        if (currentPoint == 0) previousIndex = 0;
        int nextIndex = currentPoint + 1;
        if (currentPoint == pathPoints.Count - 1) nextIndex = currentPoint;

        Vector3 _currentPoint = pathPoints[currentPoint];
        Vector3 _previousPoint = pathPoints[previousIndex];
        Vector3 _nextPoint = pathPoints[nextIndex];

        Vector3 currentLocalPosition = this.transform.InverseTransformPoint(grippingHand.transform.position);
        float _currentDistance = Vector3.Distance(currentLocalPosition, _currentPoint);
        float _previousDistance = Vector3.Distance(currentLocalPosition, _previousPoint);
        float _nextDistance = Vector3.Distance(currentLocalPosition, _nextPoint);

        if (_previousDistance < _currentDistance || _nextDistance < _currentDistance)
        {
            if (_previousDistance < _nextDistance) { currentPoint = previousIndex; return previousIndex; }

            else if (_nextDistance < _previousDistance) { currentPoint = nextIndex; return nextIndex; }
        }

        return currentPoint;
    }
    /// <summary>
    /// Detecta el punto al que moverse, comparando entre el punto actual, el anterior y el siguiente.
    /// Se usa como referencia la distancia que se ha desplazado la mano desde que se agarra
    /// </summary>
    /// <param name="distance"></param>
    /// <returns></returns>
    protected virtual int pointToMove(Vector3 distance)
    {
        int previousIndex = currentPoint - 1;
        if (currentPoint == 0) previousIndex = 0;
        int nextIndex = currentPoint + 1;
        if (currentPoint == pathPoints.Count - 1) nextIndex = currentPoint;

        Vector3 _currentPoint =     pathPoints[currentPoint];
        Vector3 _previousPoint =    pathPoints[previousIndex];
        Vector3 _nextPoint =        pathPoints[nextIndex];

        float _currentDistance =    Vector3.Distance(_currentPoint, _currentPoint + distance);
        float _previousDistance =   Vector3.Distance(_previousPoint, _currentPoint + distance);
        float _nextDistance =       Vector3.Distance(_nextPoint, _currentPoint + distance);

        if (_previousDistance < _currentDistance || _nextDistance < _currentDistance)
        {
            if (_previousDistance < _nextDistance) { currentPoint = previousIndex; initialHandPosition = grippingHand.transform.position; return previousIndex; }

            else if (_nextDistance < _previousDistance) { currentPoint = nextIndex; initialHandPosition = grippingHand.transform.position; return nextIndex; }
        }

        return currentPoint;
    }

    float GetAngleBetweenHandAndUp()
    {
        Vector2 _up = this.transform.up;
        Vector2 _handDistance = (Vector2)(grippingHand.transform.position - this.transform.position).normalized;

        float angles = Mathf.Acos((Vector3.Dot(_up, _handDistance)) / (_up.magnitude * _handDistance.magnitude)) * 57.2958f;

        return angles;
    }

    public bool isPathCompleted()
    {
        if(target != null)
        {
            if (target.conditionCompleted) return true;
        }

        return currentPoint == pathPoints.Count - 1;
    }

    public bool IsPathBegining()
    {
        return currentPoint == 0;
    }
    #endregion
}
