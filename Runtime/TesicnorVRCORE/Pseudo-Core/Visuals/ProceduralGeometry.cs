using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public struct Section
{
    public List<Vector3> vertices;
    public List<int> triangles;
    public List<Vector3> normals;
    public List<Vector4> tangents;
    public List<Vector2> uvs;
}

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class ProceduralGeometry : MonoBehaviour
{
    #region PARAMETERS
    [SerializeField]
    public List<Section> Sections = new List<Section>();

    private MeshFilter meshFilter;

    private List<Vector3> tryVertices = new List<Vector3>();
    private List<int> tryTriangles = new List<int>();
    #endregion

    #region FUNCTIONS
    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        tryVertices.Add(new Vector3(0, 1, 0));
        tryVertices.Add(new Vector3(0, 0, 0));
        tryVertices.Add(new Vector3(1, 1, 0));
        tryVertices.Add(new Vector3(1, 0, 0));

        tryTriangles.Add(0); tryTriangles.Add(1); tryTriangles.Add(2);
        tryTriangles.Add(2); tryTriangles.Add(1); tryTriangles.Add(3);


        GetSection(0);
    }

    void Update()
    {
        //SetSection(0, tryVertices, tryTriangles, new List<Vector3>(), new List<Vector4>(), new List<Vector2>());
    }

    /// <summary>
    /// Setea una sección haciéndola renderizable
    /// </summary>
    /// <param name="_vertices">Lista completa de vértices de la sección</param>
    /// <param name="_triangles">Lista completa de triángulos de la sección</param>
    /// <param name="_normals">Lista completa de las normales de la sección</param>
    /// <param name="_uvs">Lista completa de las UVs de la sección</param>
    public void SetSection(int index, List<Vector3> _vertices, List<int> _triangles, List<Vector3> _normals,List<Vector4> _tangents, List<Vector2> _uvs)
    {
        
        //All parameters that could be setted before modifying
        meshFilter.mesh = null;

        List<Vector3> bfVertices = new List<Vector3>();
        List<int> bfTriangles = new List<int>();
        List<Vector3> bfNormals = new List<Vector3>();
        List<Vector4> bfTangents = new List<Vector4>();
        List<Vector2> bfUvs = new List<Vector2>();

        //Recoge la sección que vamos a modificar con un poco de seguridad para no acceder a una posicion inexistente de la lista
        Section _section;
        if (index >= Sections.Count)
        {
            Sections.Add(new Section());
            _section = Sections[Sections.Count - 1];
        }
        else
        {
            _section = Sections[index];
        }

        //Recoge parametros que puedan estar ya seteados en la seccion
        bfVertices = _section.vertices;
        bfTriangles = _section.triangles;
        bfNormals = _section.normals;
        bfTangents = _section.tangents;
        bfUvs = _section.uvs;

        _section.vertices.Clear();
        _section.triangles.Clear();
        _section.normals.Clear();
        _section.tangents.Clear();
        _section.uvs.Clear();

        //Añade los datos que ya teniamos de antes a la sección
        _section.vertices.AddRange(bfVertices);
        _section.triangles.AddRange(bfTriangles);
        _section.normals.AddRange(bfNormals);
        _section.tangents.AddRange(bfTangents);
        _section.uvs.AddRange(bfUvs);

        //Añade los datos que le habiamos metido a la función
        _section.vertices.AddRange(_vertices);
        _section.triangles.AddRange(_triangles);
        _section.normals.AddRange(_normals);
        _section.tangents.AddRange(_tangents);
        _section.uvs.AddRange(_uvs);

        //Setea la nueva maya que usaremos para este objeto
        Mesh newMesh = new Mesh();
        newMesh.SetVertices(_section.vertices);
        newMesh.SetTriangles(_section.triangles, index);
        newMesh.SetNormals(_section.normals);
        newMesh.SetTangents(_section.tangents);
        newMesh.SetUVs(index, _section.uvs);

        meshFilter.mesh = newMesh;

        JsonUtility.ToJson(_section);
        string jsonName = "Assets/Models/"+this.gameObject.name + "Section" + index.ToString() + ".json";
        if (!File.Exists(jsonName))
        {
            File.Create(jsonName);
        }
        string json = JsonUtility.ToJson(_section);
        Debug.Log(json);
        File.WriteAllText(jsonName, json);
    }

    /// <summary>
    /// Devuelve la sección en caso de encontrarla, sino una vacía
    /// </summary>
    /// <param name="index">El índice de la sección que queremos encontrar</param>
    public Section GetSection(int index)
    {
        Section result = new Section();
        string jsonName = "Assets/Models/" + this.gameObject.name + "Section" + index.ToString() + ".json";
        if (File.Exists(jsonName))
        {
            Debug.Log("Json Read");
            string json = File.ReadAllText(jsonName);
            Section _section = JsonUtility.FromJson<Section>(json);

            if (index < Sections.Count)
            {
                Sections[index] = _section;
            }
            else
            {
                Sections.Add(_section);
            }
            result = _section;
            SetSection(index, _section.vertices, _section.triangles, _section.normals, _section.tangents, _section.uvs);
        }

        return result;
    }
    #endregion
}
