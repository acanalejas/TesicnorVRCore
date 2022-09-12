using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class Triangle
{
    //Todos los tri�ngulos se almacenar�n segun el siguiente esquema

    /// 0--------2
    /// |       / 
    /// |      /  
    /// |     /
    /// |    /
    /// |   /
    /// |  /
    ///  1  
    
    public int vertice0;
    public int vertice1;
    public int vertice2;

    public List<int> triangle = new List<int>();
}

public struct Square
{
    //Aqui almacenamos los dos tri�ngulos que forman cada rect�ngulo

    public Triangle firstTriangle;
    public Triangle secondTriangle;
}

[System.Serializable]
public class Section
{
    public List<Vector3> vertices;
    public List<int> triangles;
    public List<Triangle> triangles_data;
    public List<Square> squares;
    public List<Vector3> normals;
    public List<Vector4> tangents;
    public List<Vector2> uvs;

    public Section()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        triangles_data = new List<Triangle>();
        squares = new List<Square>();
        normals = new List<Vector3>();
        tangents = new List<Vector4>();
        uvs = new List<Vector2>();
    }

}

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class ProceduralGeometry : MonoBehaviour
{
    #region PARAMETERS
    [Header("Lista de secciones renderizables de la geometr�a")]
    [SerializeField]
    public List<Section> Sections = new List<Section>();

    [HideInInspector] public MeshFilter meshFilter;

    #endregion

    #region FUNCTIONS

    public async void SetSectionData(int index, List<Vector3> _vertices, List<int> _triangles, List<Vector3> _normals, List<Vector4> _tangents, List<Vector2> _uvs)
    {
        //Recoge la secci�n que vamos a modificar con un poco de seguridad para no acceder a una posicion inexistente de la lista
        Section _section = new Section();
        if (index >= Sections.Count)
        {
            Sections.Add(_section);
            index = Sections.Count - 1;
        }
        else
        {
            _section = Sections[index];
        }

        //Recoge parametros que puedan estar ya seteados en la seccion
        //bfVertices = Sections[index].vertices;
        //bfTriangles = Sections[index].triangles;
        //bfNormals = Sections[index].normals;
        //bfTangents = Sections[index].tangents;
        //bfUvs = Sections[index].uvs;

        Sections[index].vertices.Clear();
        Sections[index].triangles.Clear();
        Sections[index].normals.Clear();
        Sections[index].tangents.Clear();
        Sections[index].uvs.Clear();

        //A�ade los datos que le habiamos metido a la funci�n
        Sections[index].vertices.AddRange(_vertices);
        Sections[index].triangles.AddRange(_triangles);
        Sections[index].normals.AddRange(_normals);
        Sections[index].tangents.AddRange(_tangents);
        Sections[index].uvs.AddRange(_uvs);
    }

    /// <summary>
    /// Setea una secci�n haci�ndola renderizable
    /// </summary>
    /// <param name="_vertices">Lista completa de v�rtices de la secci�n</param>
    /// <param name="_triangles">Lista completa de tri�ngulos de la secci�n</param>
    /// <param name="_normals">Lista completa de las normales de la secci�n</param>
    /// <param name="_uvs">Lista completa de las UVs de la secci�n</param>
    public virtual void SetSection(int index, List<Vector3> _vertices , List<int> _triangles, List<Vector3> _normals ,List<Vector4> _tangents , List<Vector2> _uvs, MeshFilter _meshFilter = null)
    {
        if (!meshFilter) meshFilter = GetComponent<MeshFilter>();

        //All parameters that could be setted before modifying
        meshFilter.mesh = null;

        List<Vector3> bfVertices = new List<Vector3>();
        List<int> bfTriangles = new List<int>();
        List<Vector3> bfNormals = new List<Vector3>();
        List<Vector4> bfTangents = new List<Vector4>();
        List<Vector2> bfUvs = new List<Vector2>();

        //Recoge la secci�n que vamos a modificar con un poco de seguridad para no acceder a una posicion inexistente de la lista
        Section _section = new Section();
        if (index >= Sections.Count)
        {
            Sections.Add(_section);
            index = Sections.Count - 1;
        }
        else
        {
            _section = Sections[index];
        }

        //Recoge parametros que puedan estar ya seteados en la seccion
        //bfVertices = Sections[index].vertices;
        //bfTriangles = Sections[index].triangles;
        //bfNormals = Sections[index].normals;
        //bfTangents = Sections[index].tangents;
        //bfUvs = Sections[index].uvs;

        Sections[index].vertices.Clear();
        Sections[index].triangles.Clear();
        Sections[index].normals.Clear();
        Sections[index].tangents.Clear();
        Sections[index].uvs.Clear();

        //A�ade los datos que ya teniamos de antes a la secci�n
        Sections[index].vertices.AddRange(bfVertices);
        Sections[index].triangles.AddRange(bfTriangles);
        Sections[index].normals.AddRange(bfNormals);
        Sections[index].tangents.AddRange(bfTangents);
        Sections[index].uvs.AddRange(bfUvs);

        //A�ade los datos que le habiamos metido a la funci�n
        Sections[index].vertices.AddRange(_vertices);
        Sections[index].triangles.AddRange(_triangles);
        //Sections[index].normals.AddRange(_normals);
        Sections[index].tangents.AddRange(_tangents);
        Sections[index].uvs.AddRange(_uvs);

        //Setea la nueva maya que usaremos para este objeto
        Mesh newMesh = new Mesh();
        newMesh.Clear();
        newMesh.vertices = Sections[index].vertices.ToArray();
        newMesh.triangles = Sections[index].triangles.ToArray();
        newMesh.SetNormals(GetNormals(_triangles, _vertices));
        //newMesh.RecalculateTangents();
        newMesh.uv = Sections[index].uvs.ToArray();
        //newMesh.RecalculateNormals();
        //newMesh.RecalculateTangents();

        if (_meshFilter == null) meshFilter.mesh = newMesh;
        else _meshFilter.mesh = newMesh;
        //meshFilter.mesh.RecalculateUVDistributionMetrics();
        //meshFilter.mesh.RecalculateNormals();
        //meshFilter.mesh.RecalculateTangents();
        //meshFilter.mesh.Optimize();

        JsonUtility.ToJson(Sections[index]);
        string jsonName = "Assets/Models/"+this.gameObject.name + "Section" + index.ToString() + ".json";
        if (!File.Exists(jsonName))
        {
            File.Create(jsonName);
        }
        string json = JsonUtility.ToJson(Sections[index]);
        File.WriteAllText(jsonName, json);
    }

    /// <summary>
    /// Devuelve la secci�n en caso de encontrarla, sino una vac�a
    /// </summary>
    /// <param name="index">El �ndice de la secci�n que queremos encontrar</param>
    public virtual Section GetSection(int index)
    {
        Section result = new Section();
        string jsonName = "Assets/Models/" + this.gameObject.name + "Section" + index.ToString() + ".json";
        if (File.Exists(jsonName))
        {
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
        }
        SetSection(index, result.vertices, result.triangles, result.normals, result.tangents, result.uvs);
        return result;
    }

    /// <summary>
    /// A�ade una cara rectangular a la secci�n indicada. Mirar la funci�n para ver el orden necesario de los vertices
    /// </summary>
    /// <param name="sectionIndex"></param>
    /// <param name="_squareVertices"></param>
    public virtual List<int> AddSquare(Section _section, Vector3[] _squareVertices)
    {
        /// 0-----------------2
        /// |                 |
        /// |                 |
        /// |                 |
        /// |                 |
        /// |                 |
        /// 1-----------------3

        //Recogemos los �ndices que marcar�an los v�rtices nuevos
        _section.vertices.AddRange(_squareVertices);
        this.Sections[0].vertices.AddRange(_squareVertices);
        int _0 = _section.vertices.Count - 4;
        int _1 = _section.vertices.Count - 3;
        int _2 = _section.vertices.Count - 2;
        int _3 = _section.vertices.Count - 1;

        //Seteamos el primer tri�ngulo en ambas direcciones

        ///0-------2
        ///|      /
        ///|     /
        ///|    /
        ///|   /
        ///|  /
        /// 1
        /// 

        Vector3[] firstTriangle = { _section.vertices[_0], _section.vertices[_1], _section.vertices[_2] };
        List<int> _firstTriangle = AddTriangle(_section, firstTriangle);

        //Seteamos el segundo tri�ngulo en ambas direcciones

        /// 2/0
        ///| \
        ///|  \
        ///|   \
        ///|    \
        ///|     \
        ///1----- 3/2
        ///

        Vector3[] secondTriangle = { _section.vertices[_2], _section.vertices[_1], _section.vertices[_3] };
        List<int> _secondTriangle = AddTriangle(_section, secondTriangle);

        List<int> allTriangles = new List<int>();
        allTriangles.AddRange(_firstTriangle);
        allTriangles.AddRange(_secondTriangle);
        //Recogemos en la secci�n el cuadrado que acabamos de crear por si en alg�n momento necesitamos hacer algo con �l
        /*Square square = new Square();
        square.firstTriangle = currentSection.triangles_data[currentSection.triangles_data.Count - 2];
        square.secondTriangle = currentSection.triangles_data[currentSection.triangles_data.Count - 1];*/

        //currentSection.squares.Add(square);
        SetSection(0, _section.vertices, _section.triangles, _section.normals, _section.tangents, _section.uvs);

        return allTriangles;
        //Debug.Log(section.vertices.Count);
    }

    /// <summary>
    /// A�ade un tri�ngulo a la secci�n seleccionada
    /// </summary>
    /// <param name="_section">La secci�n a la que vamos a a�adir el tri�ngulo</param>
    /// <param name="_triangleVertices">Los v�rtices que componen el tri�ngulo</param>
    public virtual List<int> AddTriangle(Section _section, Vector3[] _triangleVertices)
    {
        //Se a�adiran siempre de la siguiente manera
        ///0----2
        ///|   /
        ///|  /
        ///| /
        ///1
        ///

        Section currentSection = _section;


        //A�adimos los vertices a la secci�n y lo preparamos todo para formar los tri�ngulos
        //_section.vertices.AddRange(_triangleVertices);

        int _0 = _section.vertices.Count - 3;
        int _1 = _section.vertices.Count - 2;
        int _2 = _section.vertices.Count - 1;

        //Formamos el tri�ngulo
        int[] triangle = { _0, _1, _2, _0, _2, _1 };
        currentSection.triangles.AddRange(triangle);
        this.Sections[0].triangles.AddRange(triangle);

        //Guardamos los datos del tri�ngulo en la secci�n
        /*Triangle _triangle = new();
        _triangle.triangle = triangle;
        _triangle.vertice0 = _section.vertices[_0];
        _triangle.vertice1 = _section.vertices[_1];
        _triangle.vertice2 = _section.vertices[_2];*/
        

        //Ahora vamos con las normales
        Vector3 cross = Vector3.Cross(_section.vertices[_1] - _section.vertices[_0], _section.vertices[_2] - _section.vertices[_0]);
        Vector3 normal_0 = _section.vertices[_0] + cross;
        Vector3 normal_1 = _section.vertices[_1] + cross;
        Vector3 normal_2 = _section.vertices[_2] + cross;

        normal_0 = normal_0.normalized;
        normal_1 = normal_1.normalized;
        normal_2 = normal_2.normalized;

        _section.normals.Add(normal_0); _section.normals.Add(normal_1); _section.normals.Add(normal_2);
        this.Sections[0].normals.Add(normal_0); this.Sections[0].normals.Add(normal_1); this.Sections[0].normals.Add(normal_2);

        return new List<int>(triangle);

        //currentSection.triangles_data.Add(_triangle);
    }

    /// <summary>
    /// A�ade un tri�ngulo a la secci�n seleccionada
    /// </summary>
    /// <param name="sectionIndex">El indice de la secci�n seleccionada en la lista de secciones de esta geometr�a </param>
    /// <param name="_triangleVertices">Los v�rtices que componen el tri�ngulo</param>
    public virtual List<int> AddTriangle(int sectionIndex, Vector3[] _triangleVertices)
    {
        Section currentSection = new Section();

        if(sectionIndex < Sections.Count)
        {
            currentSection = Sections[sectionIndex];
        }
        else
        {
            Sections.Add(currentSection);
            sectionIndex = Sections.Count - 1;
        }

        return AddTriangle(currentSection, _triangleVertices);
    }

    public virtual void UpdateSection(int sectionIndex, Vector3[] newVertexPositions)
    {
        if (sectionIndex >= Sections.Count || sectionIndex < 0) return;

        if (newVertexPositions.Length != Sections[sectionIndex].vertices.Count) return;

        for(int i = 0; i < newVertexPositions.Length; i++)
        {
            Sections[sectionIndex].vertices[i] = newVertexPositions[i];
        }

        Mesh mesh = new Mesh();
        mesh.SetVertices(Sections[sectionIndex].vertices);
        mesh.RecalculateBounds();
        mesh.SetTriangles(Sections[sectionIndex].triangles, 0);
    }

    public virtual void UpdateSection(int sectionIndex)
    {
        Section section = new Section();


        if (sectionIndex < Sections.Count && sectionIndex > 0) section = Sections[sectionIndex];
        SetSection(sectionIndex, section.vertices, section.triangles, section.normals, section.tangents, section.uvs);
    }

    public Vector3[] GetNormals(List<int> triangles, List<Vector3> vertices)
    {
        //Just for security
        if (triangles.Count == 0 || vertices.Count == 0) return null;

        Vector3[] result = new Vector3[vertices.Count];
        int[] alreadyChecked = new int[vertices.Count];

        foreach(Triangle tri in Sections[0].triangles_data)
        {
            bool containsAny = alreadyChecked.Contains(tri.vertice0) || alreadyChecked.Contains(tri.vertice1) || alreadyChecked.Contains(tri.vertice2);
            if (containsAny) return null;

            Vector3 v0 = vertices[tri.vertice0];
            Vector3 v1 = vertices[tri.vertice1];
            Vector3 v2 = vertices[tri.vertice2];

            Vector3 cross = Vector3.Cross(v1 - v0, v2 - v0).normalized;
            bool contains = alreadyChecked.Contains(tri.vertice0);
            if (!contains)
            {
                result[tri.vertice0] = cross;
            }

            bool contains_1 = alreadyChecked.Contains(tri.vertice1);
            if (!contains_1)
            {
                result[tri.vertice1] = cross;
            }

            bool contains_2 = alreadyChecked.Contains(tri.vertice2);
            if (!contains_2)
            {
                result[tri.vertice2] = cross;
            }
        }

        return result;
    }

    public Vector3 GetFaceNormal(Vector3 vertice_0, Vector3 vertice_1, Vector3 vertice_2)
    {
        Vector3 cross = Vector3.Cross(vertice_1 - vertice_0, vertice_2 - vertice_0);

        return cross.normalized;
    }
    #endregion
}
