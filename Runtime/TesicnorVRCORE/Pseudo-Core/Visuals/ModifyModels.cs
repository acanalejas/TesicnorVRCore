using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Windows.Input;
#endif
using System.Threading.Tasks;

#if UNITY_EDITOR
struct ModifyModelsActions
{
    public Mesh mesh;
    public int selectedVertex;
    public List<int> selectedFace;
}
public class ModifyModelsWindow : EditorWindow
{
    [MenuItem("Window/ Modify Model")]
    public static void ShowWindow()
    {
        cw = EditorWindow.GetWindow(typeof(ModifyModelsWindow));
        if (cw == null)
        {
            cw = EditorWindow.CreateWindow<ModifyModelsWindow>();
        }
        cw.ShowUtility();
    }
    static EditorWindow cw;
    public static Mesh modifiedMesh;
    public static GameObject modifiedGameObject;
    static GameObject cameraGameObject;
    public static Camera previewCamera;
    static RenderTexture rt;
    public static MoveTool MoveTool_comp;

    static string matPath = "Packages/com.tesicnor.tesicnorvrcore/Runtime/TesicnorVRCORE/Pseudo-Core/Materials/Default_Modeling.mat";
    static string spritePath = "Packages/com.tesicnor.tesicnorvrcore/Runtime/TesicnorVRCORE/Pseudo-Core/Materials/VertexMat.mat";
    static string moveToolPrefabPath = "Packages/com.tesicnor.tesicnorvrcore/Runtime/TesicnorVRCORE/Pseudo-Core/Visuals/EjesUnity.prefab";
    static string highlightTriangleMatPath = "Packages/com.tesicnor.tesicnorvrcore/Runtime/TesicnorVRCORE/Pseudo-Core/Materials/HighlightTriangle_mat.mat";

    public enum SelectionMode { Vertex, Faces}
    public SelectionMode mode = SelectionMode.Vertex;

    int lastModifiedVertex;

    float scaleMult = 0.5f;
    float axisScaleMult = 1;

    public Mesh highlightTriangle_mesh;
    public GameObject highlightTriangle_go;

    private List<ModifyModelsActions> allActions = new List<ModifyModelsActions>();

    public void OnEnable()
    {
        CreateCamera();
        CreateMoveTool();
        CreateHighlightTriangle();
    }

    /// <summary>
    /// Crea la herramienta de mover
    /// </summary>
    void CreateMoveTool()
    {
        MoveTool moveTool = GameObject.FindObjectOfType<MoveTool>();

        if (!moveTool)
        {
            GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath(moveToolPrefabPath, typeof(GameObject));
            GameObject moveToolGO = GameObject.Instantiate(prefab);
            moveTool = moveToolGO.GetComponent<MoveTool>();
        }

        MoveTool_comp = moveTool;
    }

    /// <summary>
    /// Crea la camara que se usa para renderizar la pantalla
    /// </summary>
    void CreateCamera()
    {
        rt = new RenderTexture(1920, 1080, 24);
        cameraGameObject = GameObject.Find("Camera Preview");
        if (!cameraGameObject)
        {
            cameraGameObject = new GameObject("Camera Preview", typeof(Camera));
            cameraGameObject.transform.position = new Vector3(1000, 1000, 1000);
        }
        previewCamera = cameraGameObject.GetComponent<Camera>();
        previewCamera.nearClipPlane = 0.01f;
        previewCamera.targetTexture = rt;
        RenderTexture.active = rt;
    }

    /// <summary>
    /// Crea el triangulo que se usa para enseñar el triangulo seleccionado
    /// </summary>
    void CreateHighlightTriangle()
    {
        highlightTriangle_go = GameObject.Find("highlightTriangle");
        if (!highlightTriangle_go) highlightTriangle_go = new GameObject("highlightTriangle", typeof(MeshFilter), typeof(MeshRenderer));
        if (highlightTriangle_mesh == null) highlightTriangle_mesh = new Mesh();

        Material triangleMat = (Material)AssetDatabase.LoadAssetAtPath(highlightTriangleMatPath, typeof(Material));
        highlightTriangle_go.GetComponent<MeshRenderer>().sharedMaterial = triangleMat;
    
        highlightTriangle_go.GetComponent<MeshFilter>().sharedMesh = highlightTriangle_mesh;
    }

    public void OnGUI()
    {
        if(mode == SelectionMode.Vertex)
        CreateVertexBalls();
        CreatePreview();

        GUI.DrawTexture(new Rect(0, 0, position.width, position.height), rt);
        DisplayButtons();
       
        DetectInput();
        RotatePreview();
        if (mode == SelectionMode.Vertex)
        DisplayVertex();
        else if(balls.Count > 0)
        {
            EmptyBallsPool();
        }

        AppendHighlightTriangle();
    }

    /// <summary>
    /// Se usa en OnGUI para crear los botones que se usarán además de algún elemento más de UI
    /// </summary>
    void DisplayButtons()
    {
        GUILayout.BeginHorizontal();
        bool _vertices = GUILayout.Button("VERTICES", EditorStyles.miniButtonLeft);
        GUILayout.Space(10);
        bool _faces = GUILayout.Button("FACES", EditorStyles.miniButtonRight);
        GUILayout.EndHorizontal();

        GUILayout.Label("BALL SIZE");
        scaleMult = GUILayout.HorizontalSlider(scaleMult, 0.1f, 1f);

        GUILayout.Label("ARROWS SIZE");
        axisScaleMult = GUILayout.HorizontalSlider(axisScaleMult, 0.1f, 1f);

        if (_vertices) mode = SelectionMode.Vertex;
        if (_faces) mode = SelectionMode.Faces;

        MoveTool_comp.transform.localScale = new Vector3(axisScaleMult * 0.5f, axisScaleMult * 0.5f, axisScaleMult * 0.5f);
    }

    public void Update()
    {
        this.Repaint();

        if (!this.hasFocus) EmptyBallsPool();
    }

    Vector2 mouseFinalPosition = Vector2.zero;

    bool clicking = false;
    /// <summary>
    /// Detects whenever the mouse is clicking or not
    /// </summary>
    void DetectInput()
    {
        DetectMoveToolInteraction();
        if (moving) return;

        if (Event.current.type == EventType.MouseDown && !moving) 
        { 
            clicking = true;
            mouseFinalPosition = Event.current.mousePosition;
        }
        else if(Event.current.type == EventType.MouseUp)
        {
            clicking = false;
            mouseLastPosition = Vector3.zero;
            ModifyModelsActions action = new ModifyModelsActions();
            action.mesh = new Mesh();
            action.mesh.SetVertices(modifiedMesh.vertices);
            for(int i = 0; i < modifiedMesh.subMeshCount; i++)
            {
                action.mesh.SetTriangles(modifiedMesh.GetTriangles(i), i);
            }
            action.mesh.normals = modifiedMesh.normals;
            action.mesh.uv = modifiedMesh.uv;
            action.mesh.tangents = modifiedMesh.tangents;
            action.selectedVertex = selectedVertex;
            action.selectedFace = selectedFace;

            if (allActions.Count > 0)
            {
                if (!Equals(action, allActions[allActions.Count - 1])) allActions.Add(action);
            }
            else allActions.Add(action);
        }

        if(!moving && movingAxis == null && mode == SelectionMode.Vertex)
        DetectVertexInteraction();
        if (!moving && movingAxis == null && mode == SelectionMode.Faces && Event.current.type == EventType.MouseDown)
            DetectFaceInteraction();

        if (selectedVertex >= 0) lastModifiedVertex = selectedVertex;

        DetectCtrlZ();
        DetectDelete();
        DetectZoom();
    }

    void DetectCtrlZ()
    {
        bool ctrl = Event.current.shift;

        if (!ctrl) return;
        if (allActions.Count <= 0) return;
        ModifyModelsActions lastAction = allActions[allActions.Count - 1];
        if(Event.current.keyCode == KeyCode.Z && Event.current.type == EventType.KeyDown)
        {
            modifiedMesh.SetVertices(lastAction.mesh.vertices);
            for(int i = 0; i < modifiedMesh.subMeshCount; i++)
            {
                modifiedMesh.SetTriangles(lastAction.mesh.GetTriangles(i), i);
            }
            modifiedMesh.SetNormals(lastAction.mesh.normals);
            modifiedMesh.uv = lastAction.mesh.uv;
            modifiedMesh.tangents = lastAction.mesh.tangents;
            selectedFace = lastAction.selectedFace;
            selectedVertex = lastAction.selectedVertex;

            allActions.RemoveAt(allActions.Count - 1);
        }
    }

    void DetectDelete()
    {
        if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Delete)
        {
            if (mode == SelectionMode.Vertex) DeleteSelectedVertex();
            else if (mode == SelectionMode.Faces) DeleteSelectedTriangle();
        }
    }

    void DetectZoom()
    {
        if(Event.current.type == EventType.ScrollWheel)
        {
            Debug.Log(Event.current.delta);

            cameraGameObject.transform.position += new Vector3(0, 0, -Event.current.delta.y * 0.1f);
            cameraGameObject.transform.position = new Vector3(cameraGameObject.transform.position.x, cameraGameObject.transform.position.y, Mathf.Clamp(cameraGameObject.transform.position.z, 980, modifiedGameObject.transform.position.z - modifiedMesh.bounds.extents.z));
        }
    }

    VertexBall currentBall;
    Ray ray;
    bool moving;
    string childName = "";
    string _name = "";
    MoveToolAxis movingAxis = null;
    void DetectMoveToolInteraction()
    {
        MoveToolAxis[] axis = GameObject.FindObjectsOfType<MoveToolAxis>();


        if (Event.current.type == EventType.MouseDown)
        {
            _name = MoveTool_comp.isBeingClicked();
            if (_name != "")
            {
                Transform found = MoveTool_comp.transform.Find(_name);
                Debug.Log(found);
                if (found)
                {
                    if (movingAxis == null)
                        foreach (var a in axis)
                        {
                            if (a.gameObject.name == _name) { movingAxis = a; moving = true; }
                        }
                }
            }
        }
        else if (Event.current.type == EventType.MouseDrag)
        {
            if (moving)
            {
                MoveTool_comp.Hastalapolla(MoveTool_comp.transform, _name);
                if (mode == SelectionMode.Vertex) MoverVertex();
                if (mode == SelectionMode.Faces) MoveFace();
                
            }
        }
        else if (Event.current.type == EventType.MouseUp) { moving = false; movingAxis = null; }
    }

    /// <summary>
    /// Mueve el vertice seleccionado con la herramienta de mover
    /// </summary>
    void MoverVertex()
    {
        Vector3[] vertices = modifiedMesh.vertices;
        Debug.Log("SelectedVertex is : " + selectedVertex + "\nVertices Length is : " + vertices.Length);
        if (selectedVertex < 0) selectedVertex = lastModifiedVertex;
        if (selectedVertex > 0 && selectedVertex < vertices.Length)
            vertices[selectedVertex] = modifiedGameObject.transform.InverseTransformPoint(MoveTool_comp.transform.position - MoveTool_comp.positionOffset);
        modifiedMesh.SetVertices(vertices);
    }

    /// <summary>
    /// Mueve la cara seleccionada con la herramienta de mover
    /// </summary>
    void MoveFace()
    {
        Vector3[] _vertices = modifiedMesh.vertices;

        if (selectedFace.Count != 3) return;

        Vector3 point1 = _vertices[selectedFace[0]];
        Vector3 point2 = _vertices[selectedFace[1]];
        Vector3 point3 = _vertices[selectedFace[2]];

        Vector3 center = (point1 + point2 + point3) / 3;
        Vector3 distance_center = modifiedGameObject.transform.InverseTransformPoint(MoveTool_comp.transform.position - MoveTool_comp.positionOffset) - center;

        _vertices[selectedFace[0]] = point1 + distance_center;
        _vertices[selectedFace[1]] = point2 + distance_center;
        _vertices[selectedFace[2]] = point3 + distance_center; 

        modifiedMesh.SetVertices(_vertices);

        AppendHighlightTriangle();
    }

    int selectedVertex;

    /// <summary>
    /// Detecta la interaccion con los vertices de la malla, seleccionando sobre el que se clicka (mas o menos)
    /// </summary>
    void DetectVertexInteraction()
    {
        if (moving || movingAxis != null) return;
        Vector2[] screenPoints = VertexScreenPoints();

        int vertexIndex = GetClickedVertex(screenPoints);

        if (vertexIndex >= 0 && Event.current.type == EventType.MouseDown)
        {
            VertexBall _currentBall = balls[vertexIndex].GetComponent<VertexBall>();
            if (_currentBall != currentBall && currentBall) currentBall.OnNormal();
            currentBall = _currentBall;
            selectedVertex = vertexIndex;
        }
        if(currentBall && !moving)
        MoveTool_comp.AppendToVertex(currentBall.transform.position);
        if (Event.current.type == EventType.MouseDown && currentBall) currentBall.OnClick();
        else if (currentBall) currentBall.OnHover();
    }

    List<int> selectedFace = new List<int>();

    /// <summary>
    /// Detecta la interaccion con las caras de la malla para seleccionar una
    /// </summary>
    void DetectFaceInteraction()
    {
        if (moving || movingAxis != null) return;
        List<int> Triangle = GetClickedFace();

        if (Triangle.Count != 3) return;

        selectedFace = Triangle;

        Vector3[] _vertices = modifiedMesh.vertices;
        Vector3 point1 = _vertices[Triangle[0]];
        Vector3 point2 = _vertices[Triangle[1]];
        Vector3 point3 = _vertices[Triangle[2]];
        Vector3 _center = (point1 + point2 + point3) / 3;

        AppendHighlightTriangle();

        MoveTool_comp.transform.position = modifiedGameObject.transform.TransformPoint(_center) + MoveTool_comp.positionOffset;

        MoveTool_comp.AppendToVertex(modifiedGameObject.transform.TransformPoint(_center));
    }

    /// <summary>
    /// Setea constantemente la posicion, rotacion y los vertices de el highlight del triangulo seleccionado
    /// </summary>
    void AppendHighlightTriangle()
    {
        if (selectedFace.Count != 3 || !modifiedGameObject || !highlightTriangle_mesh || !highlightTriangle_go || !modifiedMesh) return;
        Vector3[] highlightTriangle_vertices = { modifiedMesh.vertices[selectedFace[0]], modifiedMesh.vertices[selectedFace[1]], modifiedMesh.vertices[selectedFace[2]] };
        int[] highlightTriangle_triangles = { 0, 1, 2, 0, 2, 1 };

        highlightTriangle_mesh.SetVertices(highlightTriangle_vertices);
        highlightTriangle_mesh.SetTriangles(highlightTriangle_triangles, 0);

        highlightTriangle_go.transform.position = modifiedGameObject.transform.position + new Vector3(0, 0, -0.01f);
        highlightTriangle_go.transform.rotation = modifiedGameObject.transform.rotation;
        highlightTriangle_go.transform.localScale = modifiedGameObject.transform.localScale;
    }

    Dictionary<Vector3, List<int>> _allCenters = new Dictionary<Vector3, List<int>>();

    /// <summary>
    /// Nos devuelve la lista de índices dentro del array de vertices, que componen el triangulo seleccionado
    /// </summary>
    /// <returns></returns>
    List<int> GetClickedFace()
    {
        int[] _triangles = modifiedMesh.triangles;
        Vector3[] _vertices = modifiedMesh.vertices;
        Vector3[] _normals = modifiedMesh.normals;

        _allCenters.Clear();
        for(int i = 0; i < _triangles.Length - 3; i += 3)
        {
            Vector3 point1 = _vertices[_triangles[i]];
            Vector3 point2 = _vertices[_triangles[i + 1]];
            Vector3 point3 = _vertices[_triangles[i + 2]];

            List<int> indices = new List<int>();
            indices.Add(_triangles[i]); indices.Add(_triangles[i + 1]); indices.Add(_triangles[i + 2]);

            _allCenters.Add((point1 + point2 + point3) / 3, indices);
        }

        float distance = 0;
        Vector3 selectedCenter = Vector3.zero;
        Vector2 _mouseViewport = previewCamera.ScreenToViewportPoint(Event.current.mousePosition);
        _mouseViewport.y = 1 - _mouseViewport.y;
        foreach(var center in _allCenters)
        {
            Vector3 normal1 = modifiedGameObject.transform.TransformDirection(_normals[center.Value[0]]);
            Vector3 normal2 = modifiedGameObject.transform.TransformDirection(_normals[center.Value[1]]);
            Vector3 normal3 = modifiedGameObject.transform.TransformDirection(_normals[center.Value[2]]);

            Vector3 faceNormal = (normal1 + normal2 + normal3) / 3;
            if (faceNormal.z > 0 || faceNormal.x > 0.5f || faceNormal.x < -0.5f || faceNormal.y > 0.5f || faceNormal.y < -0.5f) continue;

            Vector2 centerViewport = previewCamera.WorldToViewportPoint(modifiedGameObject.transform.TransformPoint(center.Key));
            float _distance = Vector2.Distance(centerViewport, _mouseViewport);
            if (distance == 0 || distance > _distance) { distance = _distance; selectedCenter = center.Key; }
        }

        List<int> result = new List<int>();
        _allCenters.TryGetValue(selectedCenter, out result);
        return result;
    }

    Vector2 mouseLastPosition = Vector2.zero;
    /// <summary>
    /// Rotates the preview depending on the movement of the mouse while clicking
    /// </summary>
    void RotatePreview()
    {
        if (clicking)
        {
            if(mouseLastPosition != Vector2.zero)
            {
                mouseFinalPosition = Event.current.mousePosition;
                Vector2 distance = mouseFinalPosition - mouseLastPosition;

                float x = distance.x;
                float y = distance.y;

                //y/currentAngle = ScreenWidth/360
                //(y*360)/ScreenWidth = currentAngle

                float xAngle = (x * 360) / 1920;
                float yAngle = (y * 360) / 1080;

                Vector3 rotation = modifiedGameObject.transform.rotation.eulerAngles + new Vector3(yAngle, xAngle, 0);
                modifiedGameObject.transform.rotation = Quaternion.Euler(rotation);
            }
            mouseLastPosition = mouseFinalPosition;
        }
    }

    /// <summary>
    /// Creates the preview object
    /// </summary>
    void CreatePreview()
    {
        GameObject toDisplay = null;
        if (Selection.gameObjects.Length > 0)
            toDisplay = Selection.gameObjects[0];

        Mesh mesh = null;

        if (toDisplay != null)
            if (toDisplay.GetComponent<MeshFilter>())
            {
                mesh = toDisplay.GetComponent<MeshFilter>().sharedMesh;
            }

        modifiedGameObject = GameObject.Find("Preview");

        if (mesh != modifiedMesh && mesh != null)
        {
            if (modifiedGameObject) DestroyImmediate(modifiedGameObject);

            modifiedGameObject = new GameObject("Preview", typeof(MeshFilter), typeof(MeshRenderer));
            modifiedGameObject.GetComponent<MeshFilter>().sharedMesh = mesh;

            GameObject previewRayCol = GameObject.Find("previewRayCol");
            if (!previewRayCol)
            {
                previewRayCol = new GameObject("previewRayCol", typeof(BoxCollider));
                previewRayCol.transform.localScale = Vector3.one;
                BoxCollider col = previewRayCol.GetComponent<BoxCollider>();
                col.size = new Vector3(100, 100, 0.1f);
            }
            
            if (cameraGameObject)
                modifiedGameObject.transform.position = cameraGameObject.transform.position + mesh.bounds.size.magnitude * cameraGameObject.transform.forward;
            previewRayCol.transform.position = modifiedGameObject.transform.position;

            Material mat = AssetDatabase.LoadAssetAtPath(matPath, typeof(Material)) as Material;
            modifiedGameObject.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }
        
    }

    /// <summary>
    /// Sets the visuals for the vertex in the screen
    /// </summary>
    void VertexInScreen()
    {
        if (!modifiedGameObject) return;
        modifiedMesh = modifiedGameObject.GetComponent<MeshFilter>().sharedMesh;
        List<Vector2> result = new List<Vector2>();

        if (!modifiedMesh) return;
        if (balls.Count != modifiedMesh.vertices.Length) return;

        Vector3[] allVertex = modifiedMesh.vertices;
        Vector3[] allNormals = modifiedMesh.normals;

        List<Vector3> _allVertex = new List<Vector3>();
        modifiedMesh.GetVertices(_allVertex);
        int i = 0;
        foreach(var v in allVertex)
        {
            Vector3 localNormal = modifiedGameObject.transform.TransformDirection(allNormals[i]);

            balls[i].transform.position = modifiedGameObject.transform.TransformPoint(v);
            if (localNormal.z < 0)
            {
                result.Add(modifiedGameObject.transform.TransformPoint(v));
                balls[i].SetActive(true);
            }
            else { balls[i].SetActive(false); }
            i++;
        }
        return;
    }

    /// <summary>
    /// Displays the vertex in the screen
    /// </summary>
    void DisplayVertex()
    {
        VertexInScreen();

    }

    int pixelsThreshold = 17;

    Vector2[] VertexScreenPoints()
    {
        List<Vector2> result = new List<Vector2>();

            foreach (var ball in balls)
            {
                result.Add(previewCamera.WorldToScreenPoint(ball.transform.position));
            }

        return result.ToArray();
    }

    int GetClickedVertex(Vector2[] screenPoints)
    {
        int index = 0;
        if(screenPoints.Length > 0)
        foreach(var point in screenPoints)
        {
            Vector2 _point = new Vector2(point.x, Screen.height - point.y);
            float distance = Vector2.Distance(_point, Event.current.mousePosition);

            if (distance <= pixelsThreshold && balls[index].activeSelf) return index;
            index++;
                
        }
        return -1;
    }

    List<GameObject> balls = new List<GameObject>();
    /// <summary>
    /// Creates the balls for showing the vertex
    /// </summary>
    void CreateVertexBalls()
    {
        if (!modifiedMesh) return;
        if (balls.Count == modifiedMesh.vertices.Length) { 
            foreach(var ball in balls)
            {
                ball.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f) * scaleMult;
            }
            return;
        } 

        Mesh sphereMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Packages/com.tesicnor.tesicnorvrcore/Runtime/TesicnorVRCORE/Pseudo-Core/Models/UVSphere.fbx") as Mesh;
        Material ball_mat = AssetDatabase.LoadAssetAtPath(spritePath, typeof(Material)) as Material;
        EmptyBallsPool();

        int index = 0;
        foreach(var v in modifiedMesh.vertices)
        {
            GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.GetComponent<MeshRenderer>().sharedMaterial = ball_mat;
            ball.GetComponent<MeshFilter>().sharedMesh = sphereMesh;
            VertexBall vb = ball.AddComponent<VertexBall>();
            vb.index = index;
            ball.SetActive(false);
            ball.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f) * scaleMult;
            balls.Add(ball);
            index++;
        }
    }
    /// <summary>
    /// Empties the list of balls and deletes them all
    /// </summary>
    void EmptyBallsPool()
    {
        foreach(var ball in balls)
        {
            DestroyImmediate(ball);
        }
        balls.Clear();
    }

    private void OnDestroy()
    {
        EmptyBallsPool();
    }

    private void OnDisable()
    {
        EmptyBallsPool();
    }

    bool editingVertex = false;
    VertexBall lastvertex;
    void DetectVertexInteraction(VertexBall vertex)
    {
        if (Event.current.type == EventType.MouseDown)
        {
            vertex.OnClick();
        }
        else vertex.OnHover();

        if (lastvertex)
            if (lastvertex != vertex) lastvertex.OnNormal();

        lastvertex = vertex;
    }

    void DeleteSelectedVertex()
    {
        if (selectedVertex < 0) return;

        Vector3[] _vertices = modifiedMesh.vertices;
        List<Vector3> newVertices = new List<Vector3>();

        for(int i = 0; i < _vertices.Length; i++)
        {
            if (i != selectedVertex) newVertices.Add(_vertices[i]);
        }

        DeleteTrianglesContainingOneVertex(selectedVertex);

        modifiedMesh.SetVertices(newVertices.ToArray());
        selectedVertex = -1;
        selectedFace.Clear();
    }

    void DeleteTrianglesContainingOneVertex(int containedVertex)
    {
        int[] _triangles = modifiedMesh.triangles;
        List<int> newTriangles = new List<int>();

        for(int i = 0; i < _triangles.Length - 3; i += 3)
        {
            if (_triangles[i] != containedVertex && _triangles[i + 1] != containedVertex && _triangles[i + 2] != containedVertex)
            {
                newTriangles.Add(_triangles[i]); newTriangles.Add(_triangles[i + 1]); newTriangles.Add(_triangles[i + 2]);
            }
        }

        modifiedMesh.triangles = newTriangles.ToArray();
    }

    void DeleteSelectedTriangle()
    {
        if (selectedFace.Count != 3) return;
        int[] _triangles = modifiedMesh.triangles;
        List<int> newTriangles = new List<int>();

        for(int i = 0; i < _triangles.Length - 3; i += 3)
        {
            if (_triangles[i] != selectedFace[0] || _triangles[i + 1] != selectedFace[1] || _triangles[i + 2] != selectedFace[2])
            {
                newTriangles.Add(_triangles[i]); newTriangles.Add(_triangles[i + 1]); newTriangles.Add(_triangles[i + 2]);
            }
        }

        selectedFace.Clear();
       
        modifiedMesh.triangles = newTriangles.ToArray();
    }
}

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class VertexBall : MonoBehaviour
{
    static string highlightMatPath = "Packages/com.tesicnor.tesicnorvrcore/Runtime/TesicnorVRCORE/Pseudo-Core/Materials/VertexHoverMat.mat";
    static string clickMatPath = "Packages/com.tesicnor.tesicnorvrcore/Runtime/TesicnorVRCORE/Pseudo-Core/Materials/VertexClickMat.mat";
    static string normalMatPath = "Packages/com.tesicnor.tesicnorvrcore/Runtime/TesicnorVRCORE/Pseudo-Core/Materials/VertexMat.mat";

    public int index;

    private void Start()
    {
        GetComponent<Rigidbody>().isKinematic = true;
    }

    private void OnMouseDown()
    {
        OnClick();
    }

    public void OnHover()
    {
        Material hoverMat = (Material)AssetDatabase.LoadAssetAtPath(highlightMatPath, typeof(Material));
        if (hoverMat)
        {
            GetComponent<MeshRenderer>().sharedMaterial = hoverMat;
        }
    }

    public void OnClick()
    {
        Material ClickMat = (Material)AssetDatabase.LoadAssetAtPath(clickMatPath, typeof(Material));
        if (ClickMat) GetComponent<MeshRenderer>().sharedMaterial = ClickMat;

        ModifyModelsWindow.MoveTool_comp.AppendToVertex(this.transform.position);
    }

    public void OnNormal()
    {
        Material normalMat = (Material)AssetDatabase.LoadAssetAtPath(normalMatPath, typeof(Material));
        if (normalMat) GetComponent<MeshRenderer>().sharedMaterial = normalMat;
    }
}
#endif
