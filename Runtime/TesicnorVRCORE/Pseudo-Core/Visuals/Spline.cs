using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public struct SplinePoint
{
    /// <summary>
    /// GameObject De este punto que es hijo del Spline
    /// </summary>
    public GameObject go;
    /// <summary>
    /// Vertices en espacio local respecto al GameObject del Spline
    /// </summary>
    public List<Vector3> vertex;
    /// <summary>
    /// En índice de la sección en la que queremos el punto
    /// </summary>
    public int SectionIndex;

    public SplinePoint(int i)
    {
        go = null;
        vertex = new List<Vector3>();
        SectionIndex = 0;
    }
}

public class SplinePoint_Class : MonoBehaviour
{
    public SplinePoint point;

    [HideInInspector] public bool SoftSelection = false;
    [HideInInspector] public float SoftSelection_Distance = 1;

    [HideInInspector] public bool drawGizmos = false;
    public void OnDrawGizmosSelected()
    {
        if (drawGizmos)
        {
            Gizmos.color = new Color(255, 0, 0, 0.5f);
            Gizmos.DrawSphere(this.transform.position, SoftSelection_Distance);
        }
    }

}
#if UNITY_EDITOR
[CustomEditor(typeof(SplinePoint_Class), true)]
public class SplinePoint_Editor: Editor
{
    Vector3 lastPosition = Vector3.zero;
    Quaternion lastRotation = Quaternion.identity;
    void SoftSelectionMovement(SplinePoint_Class _class)
    {
        SplinePoint_Class[] SplinePoints = GameObject.FindObjectsOfType<SplinePoint_Class>();
        List<SplinePoint_Class> AffectedPoints = new List<SplinePoint_Class>();

        Debug.Log("Spline points found  :  " + SplinePoints.Length);

        if(lastPosition == Vector3.zero) { lastPosition = _class.transform.localPosition; }

        Vector3 moved = _class.transform.localPosition - lastPosition;
        if (lastRotation == Quaternion.identity) lastRotation = _class.transform.localRotation;
        Vector3 rotated = _class.transform.localRotation.eulerAngles - lastRotation.eulerAngles;
        Debug.Log("Entered here");
        if (SplinePoints.Length > 0)
        {
            foreach (SplinePoint_Class p in SplinePoints)
            {
                Debug.Log("Checking Points");
                float distance = Vector3.Distance(p.transform.position, _class.transform.position);
                if (distance < _class.SoftSelection_Distance && _class != p) AffectedPoints.Add(p);
            }

            foreach (SplinePoint_Class p in AffectedPoints)
            {
                Debug.Log("MOVING AFFECTED POINTS");
                float distance = Vector3.Distance(p.transform.position, _class.transform.position);
                float multiplier = 1 - (distance / _class.SoftSelection_Distance);
                multiplier = Mathf.Clamp(multiplier, 0.01f, 1);
                p.transform.localPosition += moved * multiplier;

                p.transform.localRotation = Quaternion.Euler(-rotated * multiplier * 3 + _class.transform.localRotation.eulerAngles);
            }
        }

        Debug.Log(lastPosition);
        lastPosition = _class.transform.localPosition;
        lastRotation = _class.transform.localRotation;
    }
    private void OnSceneGUI()
    {
        SplinePoint_Class _class = target as SplinePoint_Class;
        Vector3 localRotation = _class.transform.localRotation.eulerAngles;
        Spline _spline = _class.GetComponentInParent<Spline>();

        if (_spline) {
            if (_class.SoftSelection) SoftSelectionMovement(_class);
            _spline.UpdatePoints();
        }
        else
        {
            _class.transform.localRotation = Quaternion.Euler(localRotation);
        }

        //PARA MODIFICAR LA ZONA DE AFECTACIÓN DE LA SELECCIÓN SUAVE
        Event e = Event.current;
        switch (e.type)
        {
            case EventType.KeyDown:

                if(e.keyCode == KeyCode.B)
                {
                    _class.drawGizmos = true;
                    _class.SoftSelection = true;

                    Ray ray = SceneView.currentDrawingSceneView.camera.ScreenPointToRay(e.mousePosition);

                    float distance = (_class.gameObject.transform.position - SceneView.currentDrawingSceneView.camera.transform.position).magnitude;
                    Vector3 mouseDistance = ray.GetPoint(distance);

                    _class.SoftSelection_Distance = Vector3.Distance(_class.transform.position, mouseDistance);
                }

                break;
            case EventType.KeyUp:

                if(e.keyCode == KeyCode.B)
                {
                    _class.drawGizmos = false;
                }

                break;
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        SplinePoint_Class _class = target as SplinePoint_Class;

        GUILayout.Label("Se está realizando una selección suave?", EditorStyles.boldLabel);
        _class.SoftSelection = EditorGUILayout.Toggle(_class.SoftSelection);

        GUILayout.Space(10);

        GUILayout.Label("La distancia en la que afecta la selección suave");
        _class.SoftSelection_Distance = EditorGUILayout.FloatField("Soft Selection Distance", _class.SoftSelection_Distance);
    }
}
#endif
public class Spline : ProceduralGeometry
{
    #region PARAMETERS
    public enum Detail { LOW, HIGH}
    [HideInInspector] public Detail detail;

    [HideInInspector] public int Faces
    {
        set
        {
#if UNITY_EDITOR
            if (value != faces) { int _value = Mathf.Clamp(value, 3, 40); faces = _value; CheckPoints(); }
#endif
        }
        get { return faces; }
    }
    public int faces;
    
    [HideInInspector] public int Points { 
        set {
#if UNITY_EDITOR
            if (value != points) { int _value = Mathf.Clamp(value, 2, 40); points = _value; CheckPoints(); }
#endif
        }
        get { return points; }
    }
    public int points;

    public bool DrawDebug = true;

    [HideInInspector] public float Radius
    {
        set
        {
#if UNITY_EDITOR
            if(value != radius) { float _value = Mathf.Clamp(value, 0.001f, 800); radius = _value; CheckPoints(); }
#endif
        }
        get { return radius; }  
    }

    public float radius;
    [HideInInspector] public List<SplinePoint> Points_data = new List<SplinePoint>();

    public List<Vector3> splineVertices;
    public List<int> splineTriangles;
    public List<Vector3> splineNormals;
    #endregion

    #region FUNCTIONS


    private void Awake()
    {
        //GetSection(0);
    }

#if UNITY_EDITOR

    [MenuItem("Tesicnor/Visuals/Spline")]
    public static void CreateSpline()
    {
        GameObject parent = new GameObject("Spline", typeof(Spline));
        if (Selection.gameObjects.Length > 0) parent.transform.parent = Selection.gameObjects[0].transform;

        parent.transform.localPosition = Vector3.zero;
        parent.transform.localRotation = Quaternion.identity;
        parent.transform.localScale = Vector3.one;

        Spline sp = parent.GetComponent<Spline>();
        sp.radius = 0.2f;
    }

#endif

    /// <summary>
    /// Añade una sección al final del spline
    /// </summary>
    public void Extrude()
    {
        Points += 1;
    }

    /// <summary>
    /// Comprueba los hijos que harán como puntos en el mundo para setear la geometría
    /// Deben ser igual en número a this.points
    /// </summary>
    public void CheckPoints()
    {
        //Guardamos todos los hijos
        Transform[] allChilds = this.gameObject.GetComponentsInChildren<Transform>();

        if (allChilds.Length - 1 != this.points)
        {
            //Si hay menos hijos que numero de puntos introducidos, añade hijos
            if (this.Points > allChilds.Length - 1)
            {
                for (int i = allChilds.Length; i <= this.points; i++)
                {
                    Vector3 offset = Vector3.zero;
                    Quaternion offsetRotation = Quaternion.identity;
                    if (allChilds.Length != 0 && allChilds[allChilds.Length - 1] != this.transform) { offset = allChilds[allChilds.Length - 1].localPosition;}
                    GameObject newChild = new GameObject("Point " + i.ToString(), typeof(SplinePoint_Class));
                    Vector3 _direction = Vector3.right;
                    if (allChilds.Length != 0)
                    {
                        _direction = allChilds[allChilds.Length - 1].right;
                        offsetRotation = allChilds[allChilds.Length - 1].rotation;
                    }
                    newChild.transform.parent = this.transform;
                    newChild.transform.localPosition = offset;
                    newChild.transform.right = _direction;
                    newChild.transform.rotation = offsetRotation;
                    float multiplier = Radius * 2 * (i - allChilds.Length + 1);
                    newChild.transform.position += newChild.transform.right.normalized * multiplier;
                }
            }
            //Si hay mas hijos que puntos introducidos, borra hijos
            else if(allChilds.Length != 0)
            {
                for (int i = allChilds.Length - 1; i > this.points; i--)
                {
                    GameObject _gameObject = allChilds[i].gameObject;
                    DestroyImmediate(_gameObject);
                }
            }
        }
        allChilds = this.gameObject.GetComponentsInChildren<Transform>();
        Points_data.Clear();
        for(int i = 0; i < Points + 1; i++)
        {
            if (i < allChilds.Length && allChilds[i].gameObject != this.gameObject)
            {
                SplinePoint point = new SplinePoint(1);
                point.SectionIndex = 0;
                point.go = allChilds[i].gameObject;
                if (!point.go.GetComponent<SplinePoint_Class>()) point.go.AddComponent<SplinePoint_Class>();
                point.go.GetComponent<SplinePoint_Class>().point = point;
                Points_data.Add(point);
            }
        }
        SetPoligons();
    }

    /// <summary>
    /// No comprueba si hay que crear o no hijos nuevos, solo actualiza la geometría
    /// </summary>
    public void UpdatePoints()
    {
        Transform[] allChilds = this.gameObject.GetComponentsInChildren<Transform>();
        Points_data.Clear();
        for (int i = 0; i < Points + 1; i++)
        {
            if (i < allChilds.Length && allChilds[i].gameObject != this.gameObject)
            {
                SplinePoint point = new SplinePoint(1);
                point.SectionIndex = 0;
                point.go = allChilds[i].gameObject;
                point.go.GetComponent<SplinePoint_Class>().point = point;
                Points_data.Add(point);
            }
        }
        SetPoligons();
    }

    /// <summary>
    /// Setea los poligonos de este objeto, es decir, los vertices, triangulos, etc.
    /// </summary>
    void SetPoligons()
    {
        int index = 0;

        //Creamos las listas necesarias para setear la geometría
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        if(Sections.Count == 0)
        {
            Sections.Add(new Section());
        }
        //Sections[0] = new Section();
        foreach (SplinePoint point in Points_data)
        {
            List<Vector3> localPoints = GetVertices(point);
            
            //Añadimos los vértices
            vertices.AddRange(localPoints);

            Points_data[index].vertex.AddRange(localPoints);

            if(index > 0)
            {
                for(int i = 0; i < Faces; i++)
                {
                    int _0 = 0;
                    int _1 = 0; 
                    int _2 = 0;
                    int _3 = 0;
                    if (i < Faces - 1)
                    {
                         _0 = vertices.Count - Faces + 1 * i;
                         _1 = vertices.Count - Faces + 1 * i + 1;
                         _2 = _0 - Faces;
                         _3 = _1 - Faces;
                    }
                    else
                    {
                        _0 = vertices.Count - 1;
                        _1 = vertices.Count - 1 - (Faces - 1);
                        _2 = _0 - Faces;
                        _3 = _1 - Faces;
                    }


                    int[] _triangles = { _0, _1, _2, _2, _1, _0, _2, _1, _3, _2, _3, _1 };

                    Sections[0].triangles_data.Add(new Triangle());
                    Sections[0].triangles_data[0].vertice0 = _0;
                    Sections[0].triangles_data[0].vertice1 = _1;
                    Sections[0].triangles_data[0].vertice2 = _2;
                    Sections[0].triangles_data[0].triangle.AddRange(_triangles);

                    Vector3[] square = { vertices[_0], vertices[_1], vertices[_2], vertices[_3] };
                    triangles.AddRange(_triangles);
                }
            }

            index++;
        }
        splineVertices = vertices;
        splineTriangles = triangles;
        
        //Sections[0].vertices = splineVertices;
        this.SetSectionData(0, splineVertices, splineTriangles, Sections[0].normals, Sections[0].tangents, Sections[0].uvs);
        SetBaseAndTop(Sections[0]);
        //splineNormals = GetNormals(splineTriangles, splineVertices);
        this.SetSection(0, splineVertices, splineTriangles, splineNormals, Sections[0].tangents, Sections[0].uvs);

    }

    /// <summary>
    /// Genera las caras de la base y la tapa de la geometría
    /// </summary>
    /// <param name="section"></param>
    void SetBaseAndTop(Section section)
    {
        if (Points_data.Count == 0) return;
        SplinePoint baseData =      Points_data[0];
        SplinePoint topData =       Points_data[Points_data.Count - 1];

        List<Vector3> baseVertices = new List<Vector3>();

        List<Vector3> topVertices = new List<Vector3>();
        for(int i = 0; i < Faces; i++)
        {
            baseVertices.Add(splineVertices[i]);
        }
        for(int i = splineVertices.Count - 1; i >= splineVertices.Count - Faces - 2; i--)
        {
            topVertices.Add(splineVertices[i]);
        }

        //En caso de ser un polígono de más de 4 lados, necesitaremos un centro
        if (Faces > 4)
        {

            //PARA LA TAPA
            Vector3 center = this.transform.InverseTransformPoint(topData.go.transform.position);

            splineVertices.Add(center);
            baseData.vertex.Add(center);

            for(int i = 0; i < Faces; i++)
            {
                int[] triangle = new int[3];
                if( i < Faces - 1)
                {
                    triangle[0] = splineVertices.Count - Faces + i;
                    triangle[1] = splineVertices.Count - 1;
                    triangle[2] = splineVertices.Count - Faces + i - 1;
                    
                }
                else
                {
                    triangle[0] = splineVertices.Count - Faces - 1;
                    triangle[1] = splineVertices.Count - 1;
                    triangle[2] = splineVertices.Count - Faces + i - 1;
                }

                splineTriangles.AddRange(triangle);
            }

            //PARA LA BASE
            Vector3 _center = this.transform.InverseTransformPoint(baseData.go.transform.position);

            splineVertices.Add(_center);
            topData.vertex.Add(_center);

            for(int i = 0; i < Faces; i++)
            {
                int[] triangle = new int[3];
                if (i < Faces - 1)
                {
                    triangle[0] = i;
                    triangle[1] = splineVertices.Count - 1;
                    triangle[2] = i + 1;
                }
                else
                {
                    triangle[0] = i;
                    triangle[1] = splineVertices.Count - 1;
                    triangle[2] = 0;
                }
                splineTriangles.AddRange(triangle);
            }
            return;
        }
        else if(Faces == 4)
        {
            //PARA LA TAPA

            int t0 = splineVertices.Count - (Faces);
            int t1 = splineVertices.Count - (Faces - 1);
            int t2 = splineVertices.Count - (Faces - 2);
            int t3 = splineVertices.Count - (Faces - 3);

            int[] firstTriangle_top     = { t0, t1, t2, t0, t2, t1 };
            int[] secondTriangle_top    = { t2, t0, t3, t2, t3, t0 };

            splineTriangles.AddRange(firstTriangle_top);
            splineTriangles.AddRange(secondTriangle_top);

            //PARA LA BASE

            int b0 = 0;
            int b1 = 1;
            int b2 = 2;
            int b3 = 3;

            int[] firstTriangle_base    = { b0, b1, b2, b0, b2, b1 };
            int[] secondTriangle_base   = { b2, b0, b3, b2, b3, b0 };

            splineTriangles.AddRange(firstTriangle_base);
            splineTriangles.AddRange(secondTriangle_base);
        }
        
        else if(Faces == 3)
        {
            //PARA LA TAPA

            int t0 = splineVertices.Count - Faces;
            int t1 = splineVertices.Count - (Faces - 1);
            int t2 = splineVertices.Count - (Faces - 2);

            int[] triangle_top = { t0, t1, t2, t0, t2, t1 };

            splineTriangles.AddRange(triangle_top);

            //PARA LA BASE

            int b0 = 0;
            int b1 = 1;
            int b2 = 2;

            int[] triangle_base = { b0, b1, b2, b0, b2, b1 };

            splineTriangles.AddRange(triangle_base);
        }
    }


    /// <summary>
    /// Devuelve los vértices de un punto específico
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public List<Vector3> GetVertices(SplinePoint point)
    {
        List<Vector3> verts = new List<Vector3>();

        //El centro de la circunferencia en el que estara inscrito el polígono
        Vector3 center = point.go.transform.position;

        //El vector up que usaremos para obtener el primer punto
        Vector3 up = point.go.transform.right.normalized;

        Vector3 p1_direction = up * Radius;

        //El primer punto del polígono
        Vector3 p1 = point.go.transform.position + point.go.transform.up.normalized * Radius * new Vector3(point.go.transform.localScale.y, point.go.transform.localScale.z).magnitude;

        //El ángulo en el que habrá que girar el vector p1_direction;
        float angle = 360 / Faces;

        //La lista de puntos que van a estar alrededor de el transform
        List<Vector3> _points = new List<Vector3>();
        _points.Add(p1);

        for (int i = 1; i < Faces; i++)
        {
            Vector3 p = Quaternion.AngleAxis(angle * i, point.go.transform.right) * (point.go.transform.up) * Radius * new Vector3(point.go.transform.localScale.y, point.go.transform.localScale.z).magnitude;
            p += center;
            _points.Add(p);
        }

        //Sacamos los puntos en espacio local respecto al Spline
        List<Vector3> localPoints = new List<Vector3>();
        foreach (Vector3 _p in _points)
        {
            localPoints.Add(this.transform.InverseTransformPoint(_p));
        }

        verts.AddRange(localPoints);

        return verts;
    }

    /// <summary>
    /// Actualiza la posición de los vértices
    /// </summary>
    /// <returns></returns>
    public List<Vector3> UpdateVertices()
    {
        List<Vector3> result = new List<Vector3>();

        foreach(SplinePoint point in Points_data)
        {
            result.AddRange(GetVertices(point));
        }

        return result;
    }


    private void OnDrawGizmos()
    {
        if (Application.isPlaying || this.splineVertices == null) { return; }
        foreach(Vector3 v in this.splineVertices)
        {
            if(DrawDebug)Gizmos.DrawSphere(this.transform.TransformPoint(v), 0.2f * Radius);
        }
    }
#endregion
}

#if UNITY_EDITOR
[CustomEditor(typeof(Spline), true)]
public class SplineEditor : Editor
{
    private Spline spline;

    

    public override void OnInspectorGUI()
    {
        spline = target as Spline;

        GUILayout.Label("El nivel de detalle (Aumenta el consumo de recursos)", EditorStyles.boldLabel);
        spline.detail = (Spline.Detail)EditorGUILayout.EnumPopup("Detail", spline.detail);

        GUILayout.Space(10);

        GUILayout.Label("El radio del Spline", EditorStyles.boldLabel);
        spline.Radius = EditorGUILayout.FloatField(spline.Radius);

        GUILayout.Space(10);

        GUILayout.Label("El número de caras laterales que tiene el Spline", EditorStyles.boldLabel);
        spline.Faces = Mathf.Clamp(spline.Faces, 3, 40);
        spline.Faces = EditorGUILayout.IntField(spline.Faces);

        GUILayout.Space(10);

        GUILayout.Label("El número de puntos que marcan el camino del Spline", EditorStyles.boldLabel);
        spline.Points = Mathf.Clamp(spline.points, 2, 400);
        spline.Points = EditorGUILayout.IntField(spline.Points);

        GUILayout.Space(10);

        GUILayout.Label("Se muestran los puntos en la escena?");
        spline.DrawDebug = EditorGUILayout.Toggle(spline.DrawDebug);

        GUILayout.Space(20);

        if (GUILayout.Button("EXTRUDE")) spline.Extrude();
        /*SerializedProperty _sections = serializedObject.FindProperty("Sections");
        EditorGUILayout.PropertyField(_sections);*/
    }

    public void OnSceneGUI(SceneView sceneView)
    {
        /*SceneView.RepaintAll();
        spline = target as Spline;

        spline.CheckPoints();
        Event e = Event.current;
        switch (Event.current.type)
        {
            case EventType.KeyDown:
                if(Event.current.keyCode == KeyCode.S)
                {
                    spline.Extrude();
                }
                break;
        }

        /*List<Vector3> vertices = new List<Vector3>();
        foreach(SplinePoint point in spline.Points_data)
        {
            vertices.AddRange(spline.GetVertices(point));
        }
        spline.UpdateSection(0, vertices.ToArray());*/
    }

    private void OnEnable()
    {
        //SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        //SceneView.duringSceneGui -= OnSceneGUI;
    }
}
#endif