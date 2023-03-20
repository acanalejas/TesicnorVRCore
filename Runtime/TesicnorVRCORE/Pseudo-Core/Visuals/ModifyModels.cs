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
    public List<int> selectedVertex;
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

    List<int> lastModifiedVertex = new List<int>();

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
        DisplayTools();
       
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

    /// <summary>
    /// Se usa para que se muestren las herramientas disponibles en la ventana del editor
    /// </summary>
    void DisplayTools()
    {
        GUILayout.BeginArea(new Rect(0, position.height * 0.15f, position.width * 0.2f, position.height));

        bool extrude = GUILayout.Button("Extrude");
        if (extrude) Extrude();

        GUILayout.EndArea();
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
                action.mesh.triangles = modifiedMesh.triangles;
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

        if (selectedVertex.Count > 0) lastModifiedVertex = selectedVertex;

        DetectCtrlZ();
        DetectDelete();
        DetectZoom();
    }

    /// <summary>
    /// Detecta el input de hacer SHIFT + Z y actúa en consecuencia
    /// </summary>
    void DetectCtrlZ()
    {
        //Se está pulsando el shift mientras se ejecuta el evento actual?
        bool ctrl = Event.current.shift;

        //Si no se está pulsando no nos interesa
        if (!ctrl) return;
        //Si no hay acciones previas no nos interesa
        if (allActions.Count <= 0) return;
        //Selecciona la ultima accion realizada
        ModifyModelsActions lastAction = allActions[allActions.Count - 1];
        if(Event.current.keyCode == KeyCode.Z && Event.current.type == EventType.KeyDown)
        {
            //Setea los valores de la malla que se está modificando según la última accion
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

            //Quita la ultima accion de la lista para poder retroceder mas
            allActions.RemoveAt(allActions.Count - 1);
        }
    }

    /// <summary>
    /// Detecta el input de pulsar el botón de suprimir en el teclado
    /// </summary>
    void DetectDelete()
    {
        if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Delete)
        {
            //Se están editando los vertices?
            if (mode == SelectionMode.Vertex) DeleteSelectedVertex();
            //Se están editando los polígonos?
            else if (mode == SelectionMode.Faces) DeleteSelectedTriangle();
        }
    }

    /// <summary>
    /// Detecta el input de la rueda del ratón para hacer zoom
    /// </summary>
    void DetectZoom()
    {
        if(Event.current.type == EventType.ScrollWheel)
        {
            Debug.Log(Event.current.delta);

            cameraGameObject.transform.position += new Vector3(0, 0, -Event.current.delta.y * 0.1f);
            cameraGameObject.transform.position = new Vector3(cameraGameObject.transform.position.x, cameraGameObject.transform.position.y, Mathf.Clamp(cameraGameObject.transform.position.z, 980, modifiedGameObject.transform.position.z - modifiedMesh.bounds.extents.z));
        }
    }

    //La pelota asociada a un vertice que está actualmente seleccionada
    VertexBall currentBall;
    //El rayo que se usa para seleccionar
    Ray ray;
    //Se está moviendo algun vertice o alguna cara en este momento?
    bool moving;
    string childName = "";
    string _name = "";
    MoveToolAxis movingAxis = null;
    /// <summary>
    /// Detecta si se está interactuando con la herramienta de traslación (Las tres flechas que indican para mover un elemento en los ejes globales)
    /// </summary>
    void DetectMoveToolInteraction()
    {
        //Busca los ejes
        MoveToolAxis[] axis = GameObject.FindObjectsOfType<MoveToolAxis>();

        //En caso de que se esté pulsando el ratón
        if (Event.current.type == EventType.MouseDown)
        {
            //Recoge el nombre de el eje con el que se está interactuando
            _name = MoveTool_comp.isBeingClicked();
            if (_name != "")
            {
                //Recoge el transform del eje que se esta usando
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
        //En caso de que el ratón se mueva mientras se sigue clickando
        else if (Event.current.type == EventType.MouseDrag)
        {
            //En caso de que se esté moviendo
            if (moving)
            {
                MoveTool_comp.Hastalapolla(MoveTool_comp.transform, _name);
                if (mode == SelectionMode.Vertex) MoverVertex();
                if (mode == SelectionMode.Faces) MoveFace();
                
            }
            else
            {
                //En caso de haber por lo menos una cara seleccionada
                if(selectedFace.Count > 0)
                {
                    Vector3 center = Vector3.zero;
                    //Calcula el centro de los puntos que componen el/los polígono/s seleccionado/s
                    foreach (var i in selectedFace) center += modifiedMesh.vertices[i];

                    center /= selectedFace.Count;
                    //Transforma el punto para convertirlo en un punto basado en el espacio global
                    center = modifiedGameObject.transform.TransformPoint(center);

                    //Atamos la herramienta al centro del/ de los polígono/s
                    MoveTool_comp.AppendToVertex(center);
                }
            }
        }
        //Cuando se levanta el dedo del ratón, seteamos el bool como que ya no se mueve, y le quitamos el eje seleccionado
        else if (Event.current.type == EventType.MouseUp) { moving = false; movingAxis = null; }
    }

    /// <summary>
    /// Mueve el vertice seleccionado con la herramienta de mover
    /// </summary>
    void MoverVertex()
    {
        //Recogemos los vértices de la malla seleccionada
        Vector3[] vertices = modifiedMesh.vertices;
        //En caso de no tener seleccionado ningun vertice
        if (selectedVertex.Count <= 0) selectedVertex = lastModifiedVertex;
        //En caso de que si haya vértices seleccionados y los índices estén dentro de los parámetros de la malla
        if (selectedVertex.Count > 0 && ContainsVertex(vertices))
        {
            //Calculamos el centro de los vertices seleccionados
            Vector3 center = Vector3.zero;
            foreach (int v in selectedVertex) center += vertices[v];

            center /= selectedVertex.Count;

            //Calculamos la distancia al centro de la herramienta de traslación (ya que normalmente está atada al centro asi que la distancia
            //que se mueva del centro es la distancia que se debe mover cada vértice)
            Vector3 distance_center = modifiedGameObject.transform.InverseTransformPoint(MoveTool_comp.transform.position - MoveTool_comp.positionOffset) - center;

            //Aplicamos esa distancia a cada vértice seleccionado
            foreach(int v in selectedVertex)
            {
                vertices[v] += distance_center;
            }
        }
        //Aplicamos los cambios en los vertices a la malla
        modifiedMesh.SetVertices(vertices);
    }

    /// <summary>
    /// Los vértices seleccionados están en la malla?
    /// </summary>
    /// <param name="vertices"></param>
    /// <returns></returns>
    bool ContainsVertex(Vector3[] vertices)
    {
        foreach(var v in selectedVertex)
        {
            if (v >= vertices.Length) return false;
        }
        return true;
    }

    /// <summary>
    /// Mueve la cara seleccionada con la herramienta de mover
    /// </summary>
    void MoveFace()
    {
        Vector3[] _vertices = modifiedMesh.vertices;

        //Si hay menos de 3 puntos en la lista, no puede haber un triángulo seleccionado, asi que no nos interesa
        if (selectedFace.Count < 3) return;

        List<Vector3> points = new List<Vector3>();

        //Cogemos los puntos que corresponden a los vértices de las caras seleccionadas
        foreach (var v in selectedFace) points.Add(_vertices[v]);

        //Calculamos el centro de los polígonos seleccionados
        Vector3 center = Vector3.zero;

        foreach (var point in points) center += point;

        center = center/ selectedFace.Count;

        //Calculamos la distancia que se mueve la herramienta de traslación respecto al centro de los poligonos para saber que distancia debe moverse cada vertex
        Vector3 distance_center = modifiedGameObject.transform.InverseTransformPoint(MoveTool_comp.transform.position - MoveTool_comp.positionOffset) - center;

        //Aplicamos esa distancia a cada vertice
        for(int i = 0; i < selectedFace.Count; i++) { _vertices[selectedFace[i]] = points[i] + distance_center; }

        //Se aplican los cambios a la malla
        modifiedMesh.SetVertices(_vertices);

        AppendHighlightTriangle();
    }

    List<int> selectedVertex = new List<int>();

    /// <summary>
    /// Detecta la interaccion con los vertices de la malla, seleccionando sobre el que se clicka (mas o menos)
    /// </summary>
    void DetectVertexInteraction()
    {
        //Si se está moviendo la herramienta de traslación no nos interesa seguir para no liar interacciones
        if (moving || movingAxis != null) return;
        Vector2[] screenPoints = VertexScreenPoints();

        int vertexIndex = GetClickedVertex(screenPoints);

        if (vertexIndex >= 0 && Event.current.type == EventType.MouseDown)
        {
            VertexBall _currentBall = balls[vertexIndex].GetComponent<VertexBall>();
            if (_currentBall != currentBall && currentBall) currentBall.OnNormal();
            currentBall = _currentBall;
            if (!Event.current.shift) selectedVertex.Clear();
            selectedVertex.Add(vertexIndex);
        }
        
        if(currentBall && !moving)
        MoveTool_comp.AppendToVertex(modifiedGameObject.transform.TransformPoint(GetSelectedCenterVertex()));
        if (Event.current.type == EventType.MouseDown && currentBall) currentBall.OnClick();
        else if (currentBall) currentBall.OnHover();
    }

    /// <summary>
    /// Devuelve el punto central de los vertices seleccionados
    /// </summary>
    /// <returns></returns>
    Vector3 GetSelectedCenterVertex()
    {
        Vector3 center = Vector3.zero;
        foreach (var v in selectedVertex) center += modifiedMesh.vertices[v];
        center = center / selectedVertex.Count;
        return center;
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

        if (!Event.current.shift) selectedFace.Clear();
        selectedFace.AddRange(Triangle);

        Vector3[] _vertices = modifiedMesh.vertices;
        Vector3 point1 = _vertices[Triangle[0]];
        Vector3 point2 = _vertices[Triangle[1]];
        Vector3 point3 = _vertices[Triangle[2]];
        Vector3 _center = (point1 + point2 + point3) / 3;

        AppendHighlightTriangle();

        MoveTool_comp.AppendToVertex(modifiedGameObject.transform.TransformPoint(_center));
    }

    /// <summary>
    /// Setea constantemente la posicion, rotacion y los vertices de el highlight del triangulo seleccionado
    /// </summary>
    void AppendHighlightTriangle()
    {
        if (selectedFace.Count < 3 || !modifiedGameObject || !highlightTriangle_mesh || !highlightTriangle_go || !modifiedMesh) return;
        List<Vector3> highlightTriangle_vertices = new List<Vector3>();
        foreach (var v in selectedFace) highlightTriangle_vertices.Add(modifiedMesh.vertices[v]);

        List<int> highlightTriangle_triangles = new List<int>();
        for(int i = 0; i < selectedFace.Count - 2; i += 3)
        {
            int[] currentTriangle = { i, i + 2, i + 1, i, i + 1, i + 2 };
            highlightTriangle_triangles.AddRange(currentTriangle);
        }

        highlightTriangle_mesh.SetVertices(highlightTriangle_vertices);
        highlightTriangle_mesh.SetTriangles(highlightTriangle_triangles, 0);

        highlightTriangle_go.transform.position = modifiedGameObject.transform.position + new Vector3(0, 0, -0.01f);
        highlightTriangle_go.transform.rotation = modifiedGameObject.transform.rotation;
        highlightTriangle_go.transform.localScale = modifiedGameObject.transform.localScale;

        if (mode != SelectionMode.Faces) highlightTriangle_go.SetActive(false);
        else highlightTriangle_go.SetActive(true);
    }

    Dictionary<List<int>, Vector3> _allCenters = new Dictionary<List<int>, Vector3>();

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

            _allCenters.Add(indices , (point1 + point2 + point3)/3);
        }

        float distance = 0;
        Vector3 selectedCenter = Vector3.zero;
        Vector2 _mouseViewport = previewCamera.ScreenToViewportPoint(Event.current.mousePosition);
        _mouseViewport.y = 1 - _mouseViewport.y;
        foreach(var center in _allCenters)
        {
            Vector3 normal1 = modifiedGameObject.transform.TransformDirection(_normals[center.Key[0]]);
            Vector3 normal2 = modifiedGameObject.transform.TransformDirection(_normals[center.Key[1]]);
            Vector3 normal3 = modifiedGameObject.transform.TransformDirection(_normals[center.Key[2]]);

            Vector3 faceNormal = (normal1 + normal2 + normal3) / 3;
            if (faceNormal.z > 0 || faceNormal.x > 0.5f || faceNormal.x < -0.5f || faceNormal.y > 0.5f || faceNormal.y < -0.5f) continue;

            Vector2 centerViewport = previewCamera.WorldToViewportPoint(modifiedGameObject.transform.TransformPoint(center.Value));
            float _distance = Vector2.Distance(centerViewport, _mouseViewport);
            if (distance == 0 || distance > _distance) { distance = _distance; selectedCenter = center.Value; }
        }

        List<int> result = new List<int>();
        foreach (var center in _allCenters)
        {
            if (selectedCenter == center.Value) { result = center.Key; break; }
        }
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
        foreach(var v in _allVertex)
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

    /// <summary>
    /// Devuelve los puntos de la pantalla donde se encuentran los vértices visibles
    /// </summary>
    /// <returns></returns>
    Vector2[] VertexScreenPoints()
    {
        List<Vector2> result = new List<Vector2>();

            foreach (var ball in balls)
            {
                result.Add(previewCamera.WorldToScreenPoint(ball.transform.position));
            }

        return result.ToArray();
    }

    /// <summary>
    /// En función de la posición del ratón al clickar y las posiciones en pantalla de los vértices, detecta si está lo suficientemente cerca el raton
    /// de algun vertice para asumir que se desea interactuar por el, y en caso afirmativo, devuelve el indice en la lista de vertices del que se asume que se desea interactuar
    /// </summary>
    /// <param name="screenPoints"></param>
    /// <returns></returns>
    int GetClickedVertex(Vector2[] screenPoints)
    {
        int index = 0;
        //Si no hay vértices visibles no nos interesa seguir al bucle ya que daria error
        if(screenPoints.Length > 0)
        foreach(var point in screenPoints)
        {
            //Ajusta el eje y de cada punto para evitar el error que lo invierte, y se calcula la distancia a donde se encuentra el ratón
            Vector2 _point = new Vector2(point.x, Screen.height - point.y);
            float distance = Vector2.Distance(_point, Event.current.mousePosition);

            //En caso de que la distancia sea menor al margen de error y el vértice esté activo, se selecciona ese vertice
            if (distance <= pixelsThreshold && balls[index].activeSelf) return index;
            index++;
                
        }
        return -1;
    }

    //las pelotas que sirven como representación visual de los vertices
    List<GameObject> balls = new List<GameObject>();
    /// <summary>
    /// Creates the balls for showing the vertex
    /// </summary>
    void CreateVertexBalls()
    {
        //Si no hay ninguna malla seleccionada no debe seguir
        if (!modifiedMesh) return;
        //En caso de que haya el mismo numero de bolas que de vértices es que ya están creadas
        if (balls.Count == modifiedMesh.vertices.Length) { 
            //Les ajusta la escala 
            foreach(var ball in balls)
            {
                ball.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f) * scaleMult;
            }
            return;
        } 

        //Malla para aplicar a las esferas
        Mesh sphereMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Packages/com.tesicnor.tesicnorvrcore/Runtime/TesicnorVRCORE/Pseudo-Core/Models/UVSphere.fbx") as Mesh;
        //Material para aplicar a los materiales
        Material ball_mat = AssetDatabase.LoadAssetAtPath(spritePath, typeof(Material)) as Material;
        //Borra todas las esferas
        EmptyBallsPool();

        int index = 0;
        //Crea y setea cada bola
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

    /// <summary>
    /// Borra los vértices seleccionados
    /// </summary>
    void DeleteSelectedVertex()
    {
        //Si no hay vertices seleccionados pa que seguir
        if (selectedVertex.Count < 0) return;

        Vector3[] _vertices = modifiedMesh.vertices;
        List<Vector3> newVertices = new List<Vector3>();

        for(int i = 0; i < _vertices.Length; i++)
        {
            //Rehace la lista de vértices sin los vértices seleccionados
            if (!selectedVertex.Contains(i)) newVertices.Add(_vertices[i]);
        }

        //Buscamos los triangulos que contengan los vertices y se borran de la lista
        DeleteTrianglesContainingOneVertex(selectedVertex);

        //Se aplica a la malla y se deseleccionan los vertices y las caras
        modifiedMesh.SetVertices(newVertices.ToArray());
        selectedVertex.Clear();
        selectedFace.Clear();
    }

    /// <summary>
    /// Borra los triangulos de una malla que contengan los vertices que se aplican a la funcion
    /// </summary>
    /// <param name="containedVertex">vertices para comprobar y borrar sus triangulos</param>
    void DeleteTrianglesContainingOneVertex(List<int> containedVertex)
    {
        //Recogemos los triangulos de la malla
        int[] _triangles = modifiedMesh.triangles;
        List<int> newTriangles = new List<int>();

        //Recorremos cada triangulos (cada triangulo en el array esta compuesto de tres indices por ejemplo: el primer triangulo seria {triangles[0], triangles[1], triangles[2]}
        for(int i = 0; i < _triangles.Length - 3; i += 3)
        {
            //En caso de que el triangulo no contenga ninguno de los vértices de los seleccionados, se añade a la nueva lista de triangulos
            if (!containedVertex.Contains(_triangles[i])&& !containedVertex.Contains(_triangles[i + 1]) && !containedVertex.Contains(_triangles[i + 2]))
            {
                newTriangles.Add(_triangles[i]); newTriangles.Add(_triangles[i + 1]); newTriangles.Add(_triangles[i + 2]);
            }
        }

        //Se le aplican los triangulos a la malla
        modifiedMesh.triangles = newTriangles.ToArray();
    }

    /// <summary>
    /// Borra el poligono seleccionado
    /// </summary>
    void DeleteSelectedTriangle()
    {
        //Si no tiene por lo menos 3 posiciones quiere decir que no tiene un triangulo seleccionado
        if (selectedFace.Count != 3) return;
        int[] _triangles = modifiedMesh.triangles;
        List<int> newTriangles = new List<int>();

        //Recorremos cada triangulo de la malla buscando si coinciden. En caso de no coincidir se añade a la nueva lista de triangulos de la malla
        for(int i = 0; i < _triangles.Length - 3; i += 3)
        {
            if (_triangles[i] != selectedFace[0] || _triangles[i + 1] != selectedFace[1] || _triangles[i + 2] != selectedFace[2])
            {
                newTriangles.Add(_triangles[i]); newTriangles.Add(_triangles[i + 1]); newTriangles.Add(_triangles[i + 2]);
            }
        }

        //Se deselecciona la cara
        selectedFace.Clear();
       
        //Se aplican los cambios a la malla
        modifiedMesh.triangles = newTriangles.ToArray();
    }

    //Crea una extrusión cubica en las caras seleccionadas
    void Extrude()
    {
        //Si no estamos en el modo de modificar poligonos no nos interesa seguir
        if (mode != SelectionMode.Faces) return;

        //Si no tiene seleccionado por lo menos 3 puntos, quiere decir que no tiene ni un poligono seleccionado y no nos interesa seguir
        if (selectedFace.Count < 3) return;

        //Inicializamos las listas necesarias para guardar datos
        List<int> alreadyAddedVertex = new List<int>();
        List<Vector3> newVertex = new List<Vector3>();
        List<int> newTriangles = new List<int>();
        List<int> newSelectedTriangles = new List<int>();

        //Añadimos los vertices y triangulos que ya existen en la geometria a las nuevas listas que se usarán
        newVertex.AddRange(modifiedMesh.vertices);
        newTriangles.AddRange(modifiedMesh.triangles);

        for (int i = 0; i < selectedFace.Count - 2; i += 3)
        {
            //Crea los nuevos puntos que compartirán posición con los originales pero les añadimos un pequeño offset por si acaso
            Vector3 v00_v3 = modifiedMesh.vertices[selectedFace[i]] + modifiedMesh.normals[selectedFace[i]] * 0.1f;
            Vector3 v01_v3 = modifiedMesh.vertices[selectedFace[i + 1]] + modifiedMesh.normals[selectedFace[i]] * 0.1f;
            Vector3 v02_v3 = modifiedMesh.vertices[selectedFace[i + 2]] + modifiedMesh.normals[selectedFace[i]] * 0.1f;

            //Comprueba si los nuevos vértices ya se han añadido a la geometría, y en caso negativo los añade
            bool already0 = false, already1 = false, already2 = false;
            if (!alreadyAddedVertex.Contains(selectedFace[i])) newVertex.Add(v00_v3);
            else already0 = true;
            if (!alreadyAddedVertex.Contains(selectedFace[i + 1])) newVertex.Add(v01_v3);
            else already1 = true;
            if (!alreadyAddedVertex.Contains(selectedFace[i + 2])) newVertex.Add(v02_v3);
            else already2 = true;

            //Guarda los vertices que ya se han usado para no repetirse al crear los poligonos
            int[] usedIndexes = { selectedFace[i], selectedFace[i + 1], selectedFace[i + 2] };
            alreadyAddedVertex.AddRange(usedIndexes);

            //Establecemos los indices de los vertices seleccionados y los creados para mayor facilidad a la hora de operar
            int v0 = selectedFace[i]; int v1 = selectedFace[i + 1]; int v2 = selectedFace[i + 2];
            int v00 = newVertex.Count - 3; int v01 = newVertex.Count - 2; int v02 = newVertex.Count - 1;

            //Se comprueba si ya se han añadido por si acaso para setear el indice en consecuencia y no generar errores por ello
            for (int j = 0; j < newVertex.Count; j++)
            {
                if (already0)
                {
                    if (newVertex[j] == v00_v3)
                    {
                        v00 = j;
                    }
                }
                if (already1)
                {
                    if (newVertex[j] == v01_v3)
                    {
                        v01 = j;
                    }
                }
                if (already2)
                {
                    if (newVertex[j] == v02_v3)
                    {
                        v02 = j;
                    }
                }
            }

            //Creamos una lista con los nuevos poligonos definidos
            //Si no se puede ver gráficamente como funciona, seguir el siguiente esquema
            //TRIANGULO SUPERIOR O EXTRUIDO
            //
            //|\ v00
            //| \
            //|  \
            //|   \
            //|    \
            //|_____\
            //v01\     v02
            //
            //TRIANGULO ORIGINAL O INFERIOR
            //|\v0    
            //| \     
            //|  \    
            //|   \   
            //|    \  
            //|     \ 
            //|______\v2
            //v1
            //
            //
            int[] createdTriangles = { v00, v01, v02, v00, v02, v01,
            v0, v00, v02, v0, v02, v00,
            v2, v0, v02, v2, v02, v0,
            v1, v0, v00, v1, v00, v0,
            v1, v01, v00, v1, v00, v01,
            v1, v2, v02, v1, v02, v2,
            v01, v02, v1, v01, v1, v02};

            //Se le aplican estos cambios a las listas que hemos creado anteriormente
            newTriangles.AddRange(createdTriangles);
            int[] createdSelection = { newVertex.Count - 1, newVertex.Count - 2, newVertex.Count - 3 };
            newSelectedTriangles.AddRange(createdSelection);
        }
        //Seleccionamos las nuevas caras
        selectedFace = newSelectedTriangles;

        //Le aplicamos los cambios a la malla
        modifiedMesh.vertices = newVertex.ToArray(); modifiedMesh.SetTriangles(newTriangles, 0);
        modifiedMesh.RecalculateNormals();
        modifiedMesh.UploadMeshData(false);
        modifiedMesh.Optimize();
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
