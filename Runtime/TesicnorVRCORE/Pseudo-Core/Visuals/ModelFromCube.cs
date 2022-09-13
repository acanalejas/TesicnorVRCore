using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Face
{
    /// <summary>
    /// Lista de los 4 vértices de la cara
    /// </summary>
    public int[] faceVertex = new int[4];
    public int section = 0;
}

public class Section_Class : MonoBehaviour
{
    public Section section;
}

#if UNITY_EDITOR
[CustomEditor(typeof(Section_Class), true)]
public class Section_editor : Editor
{
    public void OnSceneGUI()
    {
        Section_Class section = (Section_Class)target;
        ModelFromCube mfc = section.GetComponentInParent<ModelFromCube>();

        if (mfc)
        {
            Event e = Event.current;
            switch (e.type)
            {
                case EventType.KeyDown:
                    if(e.keyCode == KeyCode.C)
                        mfc.SelectionObject(e.mousePosition);
                    break;
            }
        }
    }
}
#endif

public class Section_collider : MeshCollider
{
    #region PARAMETERS

    #endregion

    #region FUNCTIONS
    void UpdateCollider()
    {
        this.isTrigger = true;
    }
    #endregion
}

public class SelectionObject : MonoBehaviour
{
    public ModelFromCube cube;
    public int section;
    public int[] selectedVertices;

    Vector3 lastPos = Vector3.zero;
    private void Update()
    {
        if(lastPos != Vector3.zero)
        {
            Vector3 dif = this.transform.position - lastPos;
            
            for(int i = 0; i < selectedVertices.Length; i++)
            {
                cube.sectionsVertices[section][selectedVertices[i]] += dif;
            }
        }
        lastPos = this.transform.position;
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(SelectionObject), true)]
public class SelectionObject_Editor : Editor
{
    public void OnSceneGUI()
    {
        SelectionObject obj = target as SelectionObject;

        obj.cube.UpdateSection(obj.section);
    }
}
#endif

public class ModelFromCube : ProceduralGeometry
{
    #region PARAMETERS
    [HideInInspector] public int section = 0;
    [HideInInspector] public float initialRadius = 1;
    Section currentSection;

    [HideInInspector] public List<List<Vector3>> sectionsVertices = new List<List<Vector3>>();
    [HideInInspector] public List<List<int>> sectionsTriangles = new List<List<int>>();
    [HideInInspector] public List<List<Face>> sectionsFaces = new List<List<Face>>();

    [HideInInspector] public SelectionObject selectedObject;
    #endregion

    #region FUNCTIONS
    /// <summary>
    /// Crea el cubo desde el cual empezaremos a modelar
    /// </summary>
    public void CreateInitialCube()
    {
        Transform[] allChilds = this.transform.GetComponentsInChildren<Transform>();

        if (section > allChilds.Length - 2)
        {
            while (section > allChilds.Length - 2)
            {
                GameObject go = new GameObject("Section", typeof(MeshRenderer), typeof(MeshFilter), typeof(Section_Class), typeof(BoxCollider));
                go.transform.parent = this.transform;
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
                allChilds = this.transform.GetComponentsInChildren<Transform>();
            }
        }

        if (section > Sections.Count - 1)
        {
            while(section > Sections.Count - 1)
            {
                currentSection = new Section();
                Sections.Add(currentSection);
            }
        }
        else
        {
            currentSection = Sections[section];
        }

        if (section > sectionsVertices.Count - 1)
        {
            while(section > sectionsVertices.Count - 1)
            {
                sectionsVertices.Add(new List<Vector3>());
            }
        }
        if(section > sectionsTriangles.Count - 1)
        {
            while(section > sectionsTriangles.Count - 1)
            {
                sectionsTriangles.Add(new List<int>());
            }
        }
        if(section > sectionsFaces.Count - 1)
        {
            while(section > sectionsFaces.Count - 1)
            {
                sectionsFaces.Add(new List<Face>());
            }
        }

        Sections[section].vertices.Clear();
        Sections[section].triangles.Clear();
        sectionsTriangles[section].Clear();
        sectionsVertices[section].Clear();

        Vector3[] initialVertex = new Vector3[8];

        //h2 = a2 + b2 => a = b => h2 = b2 + b2 => h2 = 2b2 => h2/2 = b2
        float halfSide = Mathf.Sqrt((initialRadius * initialRadius) / 2);

        //PARA LA BASE

        Vector3 v0 = Vector3.forward * halfSide + Vector3.up * halfSide - Vector3.right * halfSide;
        Vector3 v1 = Vector3.forward * halfSide - Vector3.up * halfSide - Vector3.right * halfSide;
        Vector3 v2 = Vector3.forward * halfSide + Vector3.up * halfSide + Vector3.right * halfSide;
        Vector3 v3 = Vector3.forward * halfSide - Vector3.up * halfSide + Vector3.right * halfSide;

        initialVertex[0] = v0; initialVertex[1] = v1; initialVertex[2] = v2; initialVertex[3] = v3;

        int[] firstTriangle_b = { 0, 1, 2, 0, 2, 1 };
        int[] secondTriangle_b = { 2, 1, 3, 2, 3, 1 };

        Face _base = new Face();
        _base.faceVertex[0] = 0; _base.faceVertex[1] = 1; _base.faceVertex[2] = 2; _base.faceVertex[3] = 3;
        _base.section = section;
        sectionsFaces[section].Add(_base);

        //PARA LA TAPA

        Vector3 v4 = -Vector3.forward * halfSide + Vector3.up * halfSide - Vector3.right * halfSide;
        Vector3 v5 = -Vector3.forward * halfSide - Vector3.up * halfSide - Vector3.right * halfSide;
        Vector3 v6 = -Vector3.forward * halfSide + Vector3.up * halfSide + Vector3.right * halfSide;
        Vector3 v7 = -Vector3.forward * halfSide - Vector3.up * halfSide + Vector3.right * halfSide;

        initialVertex[4] = v4; initialVertex[5] = v5; initialVertex[6] = v6; initialVertex[7] = v7;

        Face _tapa = new Face();
        _tapa.faceVertex[0] = 4; _tapa.faceVertex[1] = 5; _tapa.faceVertex[2] = 6; _tapa.faceVertex[3] = 7;
        _tapa.section = section;
        sectionsFaces[section].Add(_tapa);

        int[] firstTriangle_t = { 4, 5, 6, 4, 6, 5 };
        int[] secondTriangle_t = { 6, 5, 7, 6, 7, 5 };

        //PARA EL LADO DERECHO

        int[] firstTriangle_r = { 2, 3, 6, 2, 6, 3 };
        int[] secondTriangle_r = { 6, 3, 7, 6, 7, 3 };

        Face _right = new Face();
        _right.faceVertex[0] = 2; _right.faceVertex[1] = 3; _right.faceVertex[2] = 6; _right.faceVertex[3] = 7;
        _right.section = section;
        sectionsFaces[section].Add(_right);

        //PARA EL LADO IZQUIERDO

        int[] firstTriangle_l = { 0, 5, 1, 0, 1, 5 };
        int[] secondTriangle_l = { 4, 5, 0, 4, 0, 5 };

        Face _left = new Face();
        _left.faceVertex[0] = 4; _left.faceVertex[1] = 5; _left.faceVertex[2] = 0; _left.faceVertex[3] = 1;
        _left.section = section;
        sectionsFaces[section].Add(_left);

        // PARA LA CARA INFERIOR

        int[] firstTriangle_d = { 1, 5, 3, 1, 3, 5 };
        int[] secondTriangle_d = { 3, 5, 7, 3, 7, 5 };

        Face _low = new Face();
        _low.faceVertex[0] = 1; _low.faceVertex[1] = 5; _low.faceVertex[2] = 3; _low.faceVertex[3] = 7;
        _low.section = section;
        sectionsFaces[section].Add(_low);

        //PARA LA CARA SUPERIOR

        int[] firstTriangle_u = { 4, 0, 6, 4, 6, 0 };
        int[] secondTriangle_u = { 6, 0, 2, 6, 2, 0 };

        Face _up = new Face();
        _up.faceVertex[0] = 4; _up.faceVertex[1] = 0; _up.faceVertex[2] = 6; _up.faceVertex[3] = 2;
        _up.section = section;
        sectionsFaces[section].Add(_up);

        sectionsVertices[section].AddRange(initialVertex);
        sectionsTriangles[section].AddRange(firstTriangle_b); sectionsTriangles[section].AddRange(secondTriangle_b);
        sectionsTriangles[section].AddRange(firstTriangle_t); sectionsTriangles[section].AddRange(secondTriangle_t);
        sectionsTriangles[section].AddRange(firstTriangle_r); sectionsTriangles[section].AddRange(secondTriangle_r);
        sectionsTriangles[section].AddRange(firstTriangle_l); sectionsTriangles[section].AddRange(secondTriangle_l);
        sectionsTriangles[section].AddRange(firstTriangle_d); sectionsTriangles[section].AddRange(secondTriangle_d);
        sectionsTriangles[section].AddRange(firstTriangle_u); sectionsTriangles[section].AddRange(secondTriangle_u);

        SetSection(section, sectionsVertices[section], sectionsTriangles[section], Sections[section].normals, Sections[section].tangents, Sections[section].uvs, allChilds[section + 1].GetComponent<MeshFilter>());
    }

    /// <summary>
    /// Elimina la seccion cuyo index coincida con el parametro "section"
    /// </summary>
    public void RemoveSection()
    {
        sectionsVertices.RemoveAt(section);
        sectionsTriangles.RemoveAt(section);
        sectionsFaces.RemoveAt(section);
        Sections.RemoveAt(section);

        Transform[] allChilds = this.GetComponentsInChildren<Transform>();
        DestroyImmediate(allChilds[section + 1].gameObject);
    }

    /// <summary>
    /// Elige una cara en el editor de la escena
    /// </summary>
    #if UNITY_EDITOR
    public Face SelectFace(Vector3 mousePosition)
    {
        Debug.Log("Clicked");

        Ray ray = SceneView.currentDrawingSceneView.camera.ScreenPointToRay(mousePosition);
        RaycastHit hit = new RaycastHit();

        Physics.Raycast(ray, out hit);
        Vector3 clickPoint = hit.point;
        Debug.Log(clickPoint);

        int i = 0;
        int section = 0;
        Face selectedFace = new Face();
        float minDistance = 0;
        foreach(List<Face> lf in sectionsFaces)
        {
            foreach(Face f in lf)
            {
                if (Sections[i].vertices.Count < 4) break;
                Vector3 v0 = Sections[i].vertices[0];
                Vector3 v1 = Sections[i].vertices[1];
                Vector3 v2 = Sections[i].vertices[2];
                Vector3 v3 = Sections[i].vertices[3];

                float distance0 = Vector3.Distance(clickPoint, v0);
                float distance1 = Vector3.Distance(clickPoint, v1);
                float distance2 = Vector3.Distance(clickPoint, v2);
                float distance3 = Vector3.Distance(clickPoint, v3);

                float mDistance = (distance0 + distance1 + distance2 + distance3) / 4;

                if (mDistance < minDistance || minDistance == 0) { selectedFace = f; section = i; }
            }
            i++;
        }
        selectedFace.section = section;
        return selectedFace;
    }

    public SelectionObject SelectionObject(Vector3 mousePosition)
    {
        Face selectedFace = SelectFace(mousePosition);

        if(selectedFace.faceVertex.Length == 0 || sectionsVertices.Count- 1 < selectedFace.section || sectionsVertices[selectedFace.section].Count < 4) { if (this.selectedObject.gameObject != null) DestroyImmediate(selectedObject.gameObject); return null; }

        GameObject go = new GameObject("XYZ", typeof(SelectionObject));

        Vector3 v0 = this.transform.TransformPoint(this.sectionsVertices[selectedFace.section][selectedFace.faceVertex[0]]);
        Vector3 v1 = this.transform.TransformPoint(this.sectionsVertices[selectedFace.section][selectedFace.faceVertex[1]]);
        Vector3 v2 = this.transform.TransformPoint(this.sectionsVertices[selectedFace.section][selectedFace.faceVertex[2]]);
        Vector3 v3 = this.transform.TransformPoint(this.sectionsVertices[selectedFace.section][selectedFace.faceVertex[3]]);

        Vector3 center = (v0 + v1 + v2 + v3) / 4;

        SelectionObject so = go.GetComponent<SelectionObject>();
        
        so.section = selectedFace.section;
        so.selectedVertices = selectedFace.faceVertex;

        so.cube = this;

        return so;
    }
#endif
#endregion
}

#if UNITY_EDITOR
[CustomEditor(typeof(ModelFromCube), true)]
public class ModelFromCubeEditor : Editor
{
    #region PARAMETERS

    #endregion

    #region FUNCTIONS
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ModelFromCube model = target as ModelFromCube;

        GUILayout.Label("El índice de la sección que se está editando", EditorStyles.boldLabel);
        model.section = EditorGUILayout.IntField("Current Section", model.section);

        GUILayout.Space(10);
        GUILayout.Label("La distancia del cubo inicial desde el centro a los vértices", EditorStyles.boldLabel);
        model.initialRadius = EditorGUILayout.FloatField("Initial Radius", model.initialRadius);

        GUILayout.Space(20);
        if(GUILayout.Button("CREATE INITIAL CUBE"))
        {
            model.CreateInitialCube();
        }

        GUILayout.Space(20);
        if(GUILayout.Button("REMOVE SELECTED SECTION"))
        {
            model.RemoveSection();
        }
    }

    public void OnSceneGUI()
    {
        ModelFromCube cube = target as ModelFromCube;
        Physics.Simulate(0.01f);
        Event e = Event.current;
        switch (e.type)
        {
            case EventType.KeyDown:
                e.Use();
                if(e.keyCode == KeyCode.C)
                cube.SelectionObject(e.mousePosition);
                break;
        }
    }
    #endregion
}
#endif