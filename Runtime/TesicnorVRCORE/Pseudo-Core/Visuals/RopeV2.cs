using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(LineRenderer))]
public class RopeV2 : MonoBehaviour
{
#region PARAMETERS
    /// <summary>
    /// Numero de secciones en las que se divide la cuerda
    /// </summary>
    [Header("Número de secciones en las que se divide la cuerda")]
    [SerializeField] private int ropeSections = 10;

    /// <summary>
    /// Punto de anclaje al principio de la cuerda
    /// </summary>
    [Header("Punto de anclaje del principio de la cuerda")]
    [SerializeField] private Transform initialTransform;

    /// <summary>
    /// Punto de anclaje al final de la cuerda
    /// </summary>
    [Header("Punto de anclaje del final de la cuerda")]
    [SerializeField] private Transform finalTransform;

    /// <summary>
    /// Si el transform al final esta sujeto a la cuerda o viceversa
    /// </summary>
    [Header("Si el transform al final esta sujeto a la cuerda o viceversa")]
    [SerializeField] private bool isFinalAttached = true;

    /// <summary>
    /// Si no son suficientes para la distancia, añadir secciones? sino se suma distancia maxima por seccion
    /// </summary>
    [Header("Si no son suficientes para la distancia, añadir secciones? \n sino se suma distancia máxima por seccion")]
    [SerializeField] private bool addSections = true;

    /// <summary>
    /// Masa en kg de cada seccion
    /// </summary>
    [Header("Masa en kg que tiene cada seccion")]
    [SerializeField] private float sectionMass = 0.2f;

    /// <summary>
    /// Coeficiente elastico de la cuerda
    /// </summary>
    [Header("Coeficiente elástico de la cuerda")]
    [SerializeField] private float krope = 100;

    /// <summary>
    /// Coeficiente de rozamiento con el aire de la cuerda
    /// </summary>
    [Header("Coeficiente de rozamiento con el aire")]
    [SerializeField] private float coeficent = 0.15f;

    /// <summary>
    /// Longitud maxima de cada seccion de la cuerda
    /// </summary>
    [Header("Longitud máxima de cada seccion")]
    [SerializeField] private float maxSectionLenght = 0.1f;

    /// <summary>
    /// Factor de compresion minimo de la cuerda
    /// </summary>
    [Header("Factor de compresion minimo de la cuerda")]
    [SerializeField] private float minCompress = 0.9f;

    /// <summary>
    /// Factor de estiramiento maximo de la cuerda
    /// </summary>
    [Header("Factor de estiramiento máximo de la cuerda")]
    [SerializeField] private float maxStretch = 1.1f;

    /// <summary>
    /// Line renderer que renderiza la cuerda
    /// </summary>
    [Header("LineRenderer de este GameObject")]
    [SerializeField] private LineRenderer line;

    /// <summary>
    /// Lista que guarda las secciones de la cuerda
    /// </summary>
    private List<RopeSection> Sections = new List<RopeSection>();

    private List<List<Vector3>> allVertexPositions = new List<List<Vector3>>();

#endregion

#region FUNCTIONS

    public void Start()
    {
        coeficent = Mathf.Clamp(coeficent, 1, 10);
        SetupRope();
    }

    public void Update()
    {
        CheckStrecthAndCompression();
        AttachLineToSections();
    }

    private void FixedUpdate()
    {
        GetAllPositions();
    }

    #region Creation

#if UNITY_EDITOR

    [MenuItem("Tesicnor/Visuals/Cuerda")]
    public static void CreateRope()
    {
        GameObject parent = new GameObject("Rope", typeof(RopeV2));
        if (Selection.gameObjects.Length > 0) parent.transform.parent = Selection.gameObjects[0].transform;

        parent.transform.localPosition =    Vector3.zero;
        parent.transform.localRotation =    Quaternion.identity;
        parent.transform.localScale =       Vector3.one;

        GameObject it = new GameObject("Initial Transform Rope");
        it.transform.parent =           parent.transform;
        it.transform.localPosition =    Vector3.zero;
        it.transform.localRotation =    Quaternion.identity;
        it.transform.localScale =       Vector3.one;

        GameObject ft = new GameObject("Final Transform Rope");
        ft.transform.parent =           parent.transform;
        ft.transform.localPosition =    new Vector3(0,-1,0);
        ft.transform.localRotation =    Quaternion.identity;
        ft.transform.localScale =       Vector3.one;

        LineRenderer lr = parent.GetComponent<LineRenderer>();
        lr.startWidth = 0.01f; lr.endWidth = 0.02f;

        RopeV2 rope = parent.GetComponent<RopeV2>();
        rope.initialTransform = it.transform;
        rope.finalTransform = ft.transform;
        rope.line = lr;
    }

#endif

#endregion

    #region Setup
    /// <summary>
    /// Prepara todas las visuales de la cuerda
    /// Usar en el start o Awake
    /// </summary>
    private void SetupRope()
    {
        //Checkea si son suficientes secciones para la distancia que tiene que cubrir la cuerda
        if (!areEnoughSections())
        {
            if (addSections) ropeSections = getRecommendedSections();
            else maxSectionLenght = getRecommendedLength();
        }

        Vector3 totalDistance = finalTransform.position - initialTransform.position;
        Vector3 distanceBetweenSections = totalDistance / ropeSections;

        for (int i = 0; i < ropeSections; i++)
        {
            RopeSection section = this.gameObject.AddComponent<RopeSection>();
            Sections.Add(section);

            Sections[i].position = initialTransform.position + distanceBetweenSections * i;
            Sections[i].velocity = Vector3.zero;
            Sections[i].mass = sectionMass;
            if (i == ropeSections - 1) Sections[i].mass *= 5;
        }

        line.positionCount = ropeSections;
        for (int i = 0; i < Sections.Count; i++) line.SetPosition(i, Sections[i].position);
    }

    /// <summary>
    /// Son suficientes secciones para la distancia que hay de principio a fin?
    /// </summary>
    /// <returns></returns>
    bool areEnoughSections()
    {
        float totalDistance = Vector3.Distance(initialTransform.position, finalTransform.position);
        float simulatedDistance = ropeSections * maxSectionLenght;

        if (totalDistance <= simulatedDistance) return true;

        return false;
    }

    /// <summary>
    /// Devuelve el número de secciones necesarias para recorrer la distancia
    /// </summary>
    /// <returns></returns>
    int getRecommendedSections()
    {
        float totalDistance = Vector3.Distance(initialTransform.position, finalTransform.position);

        return (int)(totalDistance / maxSectionLenght);
    }

    /// <summary>
    /// Devuelve la longitud necesaria por seccion para cubrir la distancia
    /// </summary>
    /// <returns></returns>
    float getRecommendedLength()
    {
        float totalDistance = Vector3.Distance(initialTransform.position, finalTransform.position);

        return totalDistance / ropeSections;
    }

    #endregion


    #region Simulation
    /// <summary>
    /// Devuelve el sumatorio de fuerzas de cada seccion
    /// </summary>
    /// <returns></returns>
    List<Vector3> allForces = new List<Vector3>();
    public List<Vector3> GetAllForces()
    {
        allForces.Clear();

        for(int i = 0; i < Sections.Count; i++)
        {
            float positionDiference = 0;
            Vector3 kDirection = Vector3.zero;
            float previousDiference = 0;
            Vector3 previousKDirection = Vector3.zero;
            if (i > 0) { positionDiference = Vector3.Distance(Sections[i - 1].position, Sections[i].position) - maxSectionLenght; kDirection = (Sections[i - 1].position - Sections[i].position).normalized * (positionDiference); }
            if(i < Sections.Count - 1) { previousDiference = Vector3.Distance(Sections[i].position, Sections[i + 1].position) - maxSectionLenght; previousKDirection = (Sections[i].position - Sections[i + 1].position).normalized; }
            allForces.Add(RopePhysics.gravityForce(sectionMass) + RopePhysics.dampForce(coeficent, Sections[i].velocity.magnitude, -Sections[i].velocity.normalized) + 
                (RopePhysics.elasticForce(krope, positionDiference, previousDiference, previousKDirection, kDirection)));
        }

        return allForces;
    }

    /// <summary>
    /// Devuelve la aceleracion de cada seccion
    /// </summary>
    /// <returns></returns>
    List<Vector3> result_a = new List<Vector3>();
    public List<Vector3> GetAllAccelerations()
    {
        result_a.Clear();
        List<Vector3> allForces = GetAllForces();
        int index = 0;
        foreach (Vector3 force in allForces)
        {
            result_a.Add(force / Sections[index].mass);
            //if (isFinalAttached && index == Sections.Count - 1) result[index] = Vector3.zero;
            index++;
        }
        return result;
    }

    /// <summary>
    /// Devuelve la velocidad de cada seccion
    /// </summary>
    /// <returns></returns>
    List<Vector3> result_v = new List<Vector3>();
    public List<Vector3> GetAllVelocities()
    {
        result_v.Clear();
        List<Vector3> allAccelerations = GetAllAccelerations();

        int index = 0;
        foreach (var acceleration in allAccelerations)
        {
            Vector3 velocity = Sections[index].velocity + Time.deltaTime * acceleration;
            result_v.Add(velocity);
            Sections[index].velocity = velocity;
            index++;
        }
        return result;
    }

    /// <summary>
    /// Devuelve la posicion de cada seccion
    /// </summary>
    /// <returns></returns>
     List<Vector3> result = new List<Vector3>();
    public List<Vector3> GetAllPositions()
    {
        result.Clear();
        List<Vector3> allVelocities = GetAllVelocities();

        //Euler's method
        //vel = currentVel + acceleration * time;  -> this is made in step GetAllVelocities
        //pos = currentPos + vel * time;
        int i = 0;
        foreach (var velocity in allVelocities)
        {
            Vector3 position = Sections[i].position + velocity * Time.deltaTime;
            //if (isFinalAttached && i == Sections.Count - 1) position = finalTransform.position;
            //if (IsInCollision(Sections[i].position)) position = Sections[i].position;
            result.Add(position);
            Sections[i].position = position;
            i++;
        }

        return result;
    }

    /// <summary>
    /// Comprueba si cada segmento se estira o contrae mas de lo permitido y lo soluciona
    /// </summary>
    public void CheckStrecthAndCompression()
    {
        for (int i = 0; i < Sections.Count - 1; i++)
        {
            RopeSection topSection = Sections[i];
            RopeSection bottomSection = Sections[i + 1];

            float distanceBetween = Vector3.Distance(topSection.position, bottomSection.position);

            float stretch = distanceBetween / maxSectionLenght;

            if (stretch > maxStretch)
            {
                float compressLength = distanceBetween - maxSectionLenght * maxStretch;

                Vector3 dir = (topSection.position - bottomSection.position).normalized;

                Vector3 change = compressLength * dir;

                bottomSection.position += change;
            }
            if (stretch < minCompress)
            {
                float stretchLength = maxSectionLenght * minCompress - distanceBetween;

                Vector3 dir = (bottomSection.position - topSection.position).normalized;

                Vector3 change = stretchLength * dir;

                bottomSection.position += change;
            }
        }

        RopeSection finalSection = Sections[Sections.Count - 1];
        RopeSection previousSection = Sections[Sections.Count - 2];

        float distance = Vector3.Distance(finalSection.position, previousSection.position);

        float lastStretch = distance / maxSectionLenght;

        if(lastStretch > maxStretch)
        {
            float compressDist = distance - maxSectionLenght * maxStretch;

            Vector3 dir = (previousSection.position - finalSection.position).normalized;

            Vector3 change = compressDist * dir;

            finalSection.position += change;
        }
        if(lastStretch < minCompress)
        {
            float stretchLength = maxSectionLenght * minCompress - distance;

            Vector3 dir = (finalSection.position - previousSection.position).normalized;

            Vector3 change = stretchLength * dir;

            finalSection.position += change;
        }
    }

    
#endregion

#region Visuals
    /// <summary>
    /// Setea la posicion de cada punto del LineRenderer para que cuadre con la posicion de cada seccion
    /// </summary>
    private void AttachLineToSections()
    {
        if(Sections.Count > 0)
        {
            for (int i = 1; i < Sections.Count; i++)
            {
                //Sections[i].position = (Sections[i - 1].position - Sections[i].position).normalized * maxSectionLenght;
                line.SetPosition(i, Sections[i].position);
            }
            Sections[0].position = initialTransform.position;
            line.SetPosition(0, initialTransform.position);

            if (!isFinalAttached)
            {
                Sections[Sections.Count - 1].position = finalTransform.position;
                line.SetPosition(Sections.Count - 1, Sections[Sections.Count - 1].position);
            }
            else
            {
                finalTransform.position = Sections[Sections.Count - 1].position;
                finalTransform.up = (Sections[Sections.Count - 2].position - Sections[Sections.Count - 1].position).normalized;
            }
        }
        
    }
    List<List<Vector3>> GetAllVertexPositions()
    {
        List<List<Vector3>> result = new List<List<Vector3>>();

        GameObject[] allGo = GameObject.FindObjectsOfType<GameObject>();

        List<Mesh> allMeshes = new List<Mesh>();
        List<MeshFilter> allFilters = new List<MeshFilter>();

        foreach(GameObject go in allGo)
        {
            if (go.GetComponent<MeshFilter>())
            {
                allMeshes.Add(go.GetComponent<MeshFilter>().mesh);
                allFilters.Add(go.GetComponent<MeshFilter>());
            }
        }

        
        foreach (Mesh mesh in allMeshes)
        {
            Vector3[] vertex = mesh.vertices;
            List<Vector3> lvertex = new List<Vector3>();

            foreach (Vector3 _vertex in vertex)
            {
                lvertex.Add(_vertex);
            }
            result.Add(lvertex);
        }

        //Only if positions given are local

        int i = 0;
        foreach(List<Vector3> list in result)
        {
            foreach(Vector3 v in list)
            {
                Vector3 position = allFilters[i].transform.position;
                list[i] += position;
            }
        }
        return result;
    }

    bool IsInCollision(Vector3 position)
    {
        bool result = false;

        foreach(List<Vector3> list in allVertexPositions)
        {
            foreach(Vector3 v in list)
            {
                Vector3 distance = v - position;
                if (distance.magnitude <= 0.04f) result = true;
            }
        }

        return result;
    }
#endregion
#endregion
}
