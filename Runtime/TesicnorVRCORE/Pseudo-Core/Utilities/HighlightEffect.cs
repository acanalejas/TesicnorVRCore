using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class HighlightEffect : MonoBehaviour
{
    //In order to custom editor to work properly all parameters must have the macro [SerializeField]
    #region PARAMETERS
    [SerializeField]
    [HideInInspector] public Color highlightColor = Color.green;

    [SerializeField]
    [HideInInspector] public float scaleMultiplier 
    { 
        get { return scale; } 
        set { float _value = value; 
            _value = Mathf.Clamp(_value, 1f, 2f); 
            scale = _value; } }

    [SerializeField]
    [HideInInspector]
    private float scale = 0;

    [SerializeField]
    [HideInInspector] public MeshFilter mf;

    private GameObject highlight;
    #endregion

    #region FUNCTIONS
    private void Start()
    {
        if (transform.Find("highlight"))
        {
            highlight = transform.Find("highlight").gameObject;
            highlight.SetActive(false);
        }
    }

    public void SetHighlight(bool value)
    {
        highlight.SetActive(value);
    }
    #endregion
}

#if UNITY_EDITOR
[CustomEditor(typeof(HighlightEffect), true)]
[CanEditMultipleObjects]
public class HighlightEffectEditor: Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        HighlightEffect effect = target as HighlightEffect;

        GUILayout.Label("El color del efecto", EditorStyles.boldLabel);
        effect.highlightColor = EditorGUILayout.ColorField(effect.highlightColor);

        GUILayout.Space(10);

        GUILayout.Label("La escala del efecto", EditorStyles.boldLabel);
        effect.scaleMultiplier = EditorGUILayout.Slider(effect.scaleMultiplier, 1f, 2);

        effect.mf = effect.GetComponent<MeshFilter>();

        Transform highlight_transform = effect.transform.Find("highlight");
        GameObject highlight;
        //Creando el hijo que hara el efecto
        if (!highlight_transform)
        {
            highlight = new GameObject("highlight", typeof(MeshFilter), typeof(MeshRenderer));
            highlight.transform.parent = effect.transform;
            highlight.transform.localPosition = Vector3.zero;
            highlight.transform.localRotation = Quaternion.Euler(Vector3.zero);
            highlight.transform.localScale = Vector3.one;
            //highlight.hideFlags = HideFlags.HideInHierarchy;

            if (effect.GetComponent<MeshFilter>())
            {
                highlight.GetComponent<MeshFilter>().sharedMesh = effect.GetComponent<MeshFilter>().sharedMesh;
            }

            MeshRenderer hmr = highlight.GetComponent<MeshRenderer>();
            hmr.sharedMaterial = new Material((Material)AssetDatabase.LoadAssetAtPath("Packages/com.tesicnor.tesicnorvrcore/Runtime/TesicnorVRCORE/Pseudo-Core/Materials/Highlight_mat.mat", typeof(Material)));
            hmr.sharedMaterial.color = effect.highlightColor;
            
        }

        else {
            highlight = highlight_transform.gameObject;
            highlight.transform.localScale = Vector3.one;

            if (!highlight.GetComponent<MeshFilter>().sharedMesh)
            {
                if (effect.GetComponent<MeshFilter>())
                {
                    highlight.GetComponent<MeshFilter>().sharedMesh = effect.GetComponent<MeshFilter>().sharedMesh;
                }
            }

            if(highlight.GetComponent<MeshFilter>().sharedMesh && effect.GetComponent<MeshFilter>())
            {
                if(highlight.GetComponent<MeshFilter>().sharedMesh != effect.GetComponent<MeshFilter>().sharedMesh)
                {
                    highlight.GetComponent<MeshFilter>().sharedMesh = effect.GetComponent<MeshFilter>().sharedMesh;
                }
            }
        }

        AdaptGeometryOutline(highlight, effect.scaleMultiplier - 1);
        highlight.GetComponent<MeshRenderer>().sharedMaterial.color = effect.highlightColor;
    }

    void AdaptGeometryOutline(GameObject highlight, float outlineThickness)
    {
        Mesh mesh = highlight.GetComponent<MeshFilter>().sharedMesh;

        

        List<Vector3> vertices      = new List<Vector3>();
        List<Vector3> normals       = new List<Vector3>();

        List<Vector3> newVertices   = new List<Vector3>();
        List<Vector3> newNormals    = new List<Vector3>();
        List<Vector4> newTangents   = new List<Vector4>();
        List<int> newTriangles      = new List<int>();

        if (!mesh) goto Done;

        mesh.GetVertices(vertices);
        mesh.GetNormals(normals);

        for(int i = 0; i < vertices.Count; i++)
        {
            Vector3 normal = normals[i].normalized;
            Vector3 offset = normal * outlineThickness;
            Vector3 position = vertices[i] + offset;

            newVertices.Add(position);
        }

        newNormals.AddRange(mesh.normals);
        newTangents.AddRange(mesh.tangents);
        newTriangles.AddRange(mesh.triangles);

        Done:

        Transform parent = highlight.transform.parent;
        MeshFilter[] childrenMeshes = parent.gameObject.GetComponentsInChildren<MeshFilter>();

        int l = 0;
        foreach(var child in childrenMeshes)
        {
            if (child.gameObject == highlight.gameObject || child.gameObject == parent.gameObject) { childrenMeshes[l] = null; }
                l++;
        }
        if(childrenMeshes.Length > 0)
        {
            int currentCount = newVertices.Count;
            if(childrenMeshes.Length == 1 && childrenMeshes[0] != null)
            {
                List<Vector3> child_vertices = new List<Vector3>();
                List<Vector3> child_normals = new List<Vector3>();

                childrenMeshes[0].sharedMesh.GetVertices(child_vertices);
                childrenMeshes[0].sharedMesh.GetNormals(child_normals);

                for(int k = 0; k < child_vertices.Count; k++)
                {
                    Vector3 normal = child_normals[k].normalized;
                    Vector3 offset = normal * outlineThickness;
                    Vector3 vertex = childrenMeshes[0].transform.TransformPoint(child_vertices[k]);
                    vertex = highlight.transform.InverseTransformPoint(vertex);
                    Vector3 position = vertex + offset;

                    newVertices.Add(position);
                }

                newNormals.AddRange(childrenMeshes[0].sharedMesh.normals);
                newTangents.AddRange(childrenMeshes[0].sharedMesh.tangents);

                foreach(int j in childrenMeshes[0].sharedMesh.triangles)
                {
                    newTriangles.Add(j + currentCount);
                }
            }
            else if(childrenMeshes.Length > 1)
            {
                foreach(MeshFilter child in childrenMeshes)
                {
                    if (child != null && child.sharedMesh != null) {
                        currentCount = newVertices.Count;

                        List<Vector3> child_vertices = new List<Vector3>();
                        List<Vector3> child_normals = new List<Vector3>();

                        child.sharedMesh.GetVertices(child_vertices);
                        child.sharedMesh.GetNormals(child_normals);

                        for (int i = 0; i < child_vertices.Count; i++)
                        {
                            Vector3 normal = child_normals[i].normalized;
                            Vector3 offset = normal * outlineThickness;
                            Vector3 vertex = child.transform.TransformPoint(child_vertices[i]);
                            vertex = highlight.transform.InverseTransformPoint(vertex);
                            Vector3 position = vertex + offset ;

                            newVertices.Add(position);
                        }
                        newNormals.AddRange(child.sharedMesh.normals);
                        newTangents.AddRange(child.sharedMesh.tangents);

                        foreach (int i in child.sharedMesh.triangles)
                        {
                            newTriangles.Add(i + currentCount);
                        }
                    }
                }
            }
        }

        Mesh newMesh = new Mesh();
        newMesh.SetVertices(newVertices);
        newMesh.SetTriangles(newTriangles, 0);
        newMesh.SetTangents(newTangents);
        newMesh.SetNormals(newNormals);
        highlight.GetComponent<MeshFilter>().sharedMesh = newMesh;
    }

    public void OnDestroy()
    {
        var effect = target as HighlightEffect;

        Transform highlight = effect.transform.Find("highlight");
        //if(highlight) DestroyImmediate(highlight.gameObject);
    }

    public void OnDisable()
    {
        var effect = target as HighlightEffect;

        Transform highlight = effect.transform.Find("highlight");
        //if (highlight) highlight.gameObject.SetActive(false);
    }

    public void OnEnable()
    {
        var effect = target as HighlightEffect;

        Transform highlight = effect.transform.Find("highlight");
        if (highlight) highlight.gameObject.SetActive(true);
    }
}
#endif
