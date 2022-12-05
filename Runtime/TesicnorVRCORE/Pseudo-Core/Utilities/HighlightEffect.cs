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
                if (effect.GetComponent<MeshFilter>().sharedMesh)
                {
                    highlight.GetComponent<MeshFilter>().sharedMesh = effect.GetComponent<MeshFilter>().sharedMesh;
                }
            }

            if(highlight.GetComponent<MeshFilter>().sharedMesh && effect.GetComponent<MeshFilter>().sharedMesh)
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

        if (!mesh) return;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();

        List<Vector3> newVertices = new List<Vector3>();

        mesh.GetVertices(vertices);
        mesh.GetNormals(normals);

        for(int i = 0; i < vertices.Count; i++)
        {
            Vector3 normal = normals[i].normalized;
            Vector3 offset = normal * outlineThickness;
            Vector3 position = vertices[i] + offset;

            newVertices.Add(position);
        }

        Mesh newMesh = new Mesh();
        newMesh.SetVertices(newVertices);
        newMesh.SetTriangles(mesh.triangles, 0);
        newMesh.SetTangents(mesh.tangents);
        newMesh.SetNormals(normals);
        highlight.GetComponent<MeshFilter>().sharedMesh = newMesh;
    }
}
