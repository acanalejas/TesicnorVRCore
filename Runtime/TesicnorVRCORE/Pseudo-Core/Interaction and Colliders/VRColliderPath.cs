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

    private float initialZAngle = 0;

    #endregion

    #region FUNCTIONS
    public override void Awake()
    {
        base.Awake();
        SetPathPoints();

        if (bShouldDisableOnEnd) OnPathEndReached.AddListener(DisableOnEnd);

        currentAngles = this.transform.localRotation.y;
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

    public override void Grab(GrippingHand hand)
    {
        base.Grab(hand);
        //initialRotation = currentAngles;
        if(this.transform.parent != null)
        initialHandPosition = this.transform.parent.InverseTransformPoint(hand.transform.position);
        else initialHandPosition = hand.transform.position;

        if (axis == Axis.z) initialZAngle = GetAngleBetweenHandAndUp();
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

            float handDistance = Vector3.Distance(grippingHand.transform.position, this.transform.position);

            float distance = Vector3.Distance(this.transform.position, this.rotationPivot.transform.position);

            this.transform.position = distance * direction + rotationPivot.position;

            if (isPositive)
            {
                if(axis == Axis.y)
                this.transform.forward = new Vector3(-direction.z, 0, direction.x);

                if(axis == Axis.z)
                {
                    this.transform.up = new Vector3(direction.x, direction.y, 0);
                    float angles = 0;
                    angles = -Mathf.Clamp(this.transform.localRotation.z, initialRotation, finalRotation);
                    //if (angles >= -initialRotation) angles = -initialRotation;
                    //if (angles <= -finalRotation) angles = -finalRotation;
                    this.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, angles));
                }

                //if (axis == Axis.z)
                //{
                //    //this.transform.up = new Vector3(-direction.x, direction.y, this.transform.parent.up.z);
                //
                //    float anglesDiff = GetAngleBetweenHandAndUp() - initialZAngle;
                //    Quaternion rotation = Quaternion.FromToRotation(this.transform.position, grippingHand.transform.position);
                //    this.transform.up = rotation * Vector3.forward;
                //    this.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, this.transform.localRotation.eulerAngles.z));
                //
                //}
                   
            }
            
            else this.transform.right = -new Vector3(-direction.z, 0, direction.x);

            if (axis == Axis.y) currentAngles = this.transform.localRotation.y;
            else if (axis == Axis.x) currentAngles = this.transform.localRotation.x;
            else if (axis == Axis.z) currentAngles = this.transform.localRotation.z;
            //this.transform.localRotation = Quaternion.Euler(GetAxis() * anglesToMove());
            if (isPathCompleted()) OnPathEndReached.Invoke();
            Debug.Log("Rotating");
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
    /// <summary>
    /// Detecta el ángulo al que debe girar el objeto en base a la distancia inicial del pivote con la mano y la actual
    /// </summary>
    /// <returns></returns>
    protected virtual float anglesToMove()
    {
        Vector3 currentDistance = grippingHand.transform.position - rotationPivot.position;

        float angle = Mathf.Acos((currentDistance.x * initialDistance.x + currentDistance.y * initialDistance.y + currentDistance.z * initialDistance.z) / (initialDistance.magnitude * currentDistance.magnitude)) * 57.2958f;
        angle += initialRotation;
        if (finalRotation < 0) angle *= -1;
        //currentAngles = angle;

        if (angle > finalRotation - 10 && angle < finalRotation + 10) angle = finalRotation;

        return angle;
    }

    /// <summary>
    /// Devuelve el eje que se ha escogido en forma de vector
    /// </summary>
    /// <returns></returns>
    Vector3 GetAxis()
    {
        Vector3 _axis = Vector3.zero;
        switch (axis)
        {
            case Axis.x:
                _axis = new Vector3(1, 0, 0);
                break;
            case Axis.y:
                _axis = new Vector3(0, 1, 0);
                break;
            case Axis.z:
                _axis = new Vector3(0, 0, 1);
                break;
        }

        return _axis;
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
        bool result = false;

        if(target != null)
        {
            if (target.conditionCompleted) return true;
        }
        switch (pathType)
        {
            case PathType.Position:
                if (currentPoint == pathPoints.Count - 1) result = true;
                break;
            case PathType.Rotation:
                if(finalRotation > initialRotation)
                {
                    if (currentAngles >= finalRotation - Threshold) result = true;
                }
                else
                {
                    if(currentAngles < finalRotation + Threshold) result = true;
                }
                break;
        }

        return result;
    }
    #endregion
}
