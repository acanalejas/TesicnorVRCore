using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;

namespace TesicFire
{
    [Serializable]
    public class MeshData
    {
        public List<Vector3> vertex = new List<Vector3>();
        public List<int> triangles = new List<int>();
        public List<Vector3> normals = new List<Vector3>();
    }

    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(BoxCollider))]
    public sealed class FireObject : MonoBehaviour, FireUtils
    {
        #region PARAMETERS
        /// <summary>
        /// Prefab de las partículas de fuego
        /// </summary>
        [HideInInspector] public ParticleSystem fire_SystemPrefab;
        /// <summary>
        /// El sistema de partículas del fuego
        /// </summary>
        [HideInInspector] public ParticleSystem fire_System;

        /// <summary>
        /// El GameObject de las partículas del fuego
        /// </summary>
        [HideInInspector] public GameObject fire_GO;

        /// <summary>
        /// El meshRenderer del fuego
        /// </summary>
        [HideInInspector] public MeshRenderer fire_MR;
        /// <summary>
        /// El sistema de partículas del humo
        /// </summary>
        [HideInInspector] public ParticleSystem smoke_System;

        /// <summary>
        /// El GameObject de las partículas del humo
        /// </summary>
        [HideInInspector] public GameObject smoke_GO;

        [HideInInspector] public bool InitialFire = false;
        [HideInInspector] public bool UsesSmoke = true;
        [HideInInspector] public bool UsesSparks = true;
        #region Para el fuego
        [HideInInspector] public float Delay = 4;
        [HideInInspector] public float FireSpeed { get { return fireSpeed; } set { fireSpeed = value; fireSpeed = Mathf.Clamp(fireSpeed, 0.05f, 10f); } }
        private float fireSpeed = 1;
        [HideInInspector] public float PropDistance = 1;
        [HideInInspector] public float MaxTimeToExtinguish = 2;
        [HideInInspector] public float TimeToExtinguish = 2;
        [HideInInspector] public Vector3 PropOffset = Vector3.zero;

        private float timeOnFire = 0;

        private bool onFire = false;

        private bool extinguished = false, extinguishing = false;

        private bool completeFire = false;

        private Mesh mesh_original;
        MeshData meshData_current = new MeshData();

        MeshData meshData_original = new MeshData();

        List<Mesh> fire_mesh = new List<Mesh>();

        private Vector3 initialFirePoint;
        #endregion
        #endregion

        #region FUNCTIONS
        #region Interface

        void InitializeStructures()
        {
            meshData_current.vertex         = new List<Vector3>();
            meshData_current.triangles      = new List<int>();
            meshData_current.normals        = new List<Vector3>();

            meshData_original.vertex        = new List<Vector3>();
            meshData_original.triangles     = new List<int>();
            meshData_original.normals       = new List<Vector3>();
        }
        private void Awake()
        {
            fire_GO.GetComponent<ParticleSystem>().playOnAwake = false;
            fire_GO.GetComponent<ParticleSystem>().Stop();

            fire_MR = fire_GO.GetComponent<MeshRenderer>();
            InitializeStructures();

            BoxCollider bc = GetComponent<BoxCollider>();
            //bc.size = fire_MR.localBounds.size;
            bc.isTrigger = true;
            bc.center = fire_MR.localBounds.center;
        }
        private void Start()
        {
            //BeginFire(GetComponent<MeshFilter>().mesh.bounds.center);
            mesh_original = GetComponent<MeshFilter>().mesh;
            if (InitialFire) BeginFire(Vector3.zero);
        }

        IEnumerator construct()
        {
            fire_mesh.Add(FireMesh(Vector3.zero, "FireMesh50", 20));
            //yield return new WaitForSeconds(0.3f);
            fire_mesh.Add(FireMesh(Vector3.zero, "FireMesh40", 10));
            //yield return new WaitForSeconds(0.3f);
            fire_mesh.Add(FireMesh(Vector3.zero, "FireMesh30", 8));
            //yield return new WaitForSeconds(0.3f);
            fire_mesh.Add(FireMesh(Vector3.zero, "FireMesh20", 1));
            //yield return new WaitForSeconds(0.3f);
            fire_mesh.Add(FireMesh(Vector3.zero, "FireMesh", 0.01f));

            fire_mesh.Add(GetComponent<MeshFilter>().mesh);

            yield return new WaitForSeconds(0.1f);
            //string read = File.ReadAllText("Assets/FireMesh20.txt");
            //Mesh mesh = FireUtilsMethods.StringToMesh(read);
            //AssetDatabase.CreateAsset(mesh, "Assets/mesh.asset");
            reconstructing = true;
            fire_GO.GetComponent<MeshFilter>().mesh = fire_mesh[0];
            fire_System.Play();
            yield return new WaitForSeconds(2/FireSpeed);
            fire_GO.GetComponent<MeshFilter>().mesh = fire_mesh[1];
            yield return new WaitForSeconds(2/FireSpeed);
            fire_GO.GetComponent<MeshFilter>().mesh = fire_mesh[2];
            yield return new WaitForSeconds(2/FireSpeed);
            fire_GO.GetComponent<MeshFilter>().mesh = fire_mesh[3];
            yield return new WaitForSeconds(2/FireSpeed);
            fire_GO.GetComponent<MeshFilter>().mesh = fire_mesh[4];
            yield return new WaitForSeconds(2/FireSpeed);
            fire_GO.GetComponent<MeshFilter>().mesh = fire_mesh[5];

            reconstructing = false;
            completeFire = true;

        }

        bool reconstructing = false;
        IEnumerator reconstruct()
        {
            reconstructing = true;
            MeshFilter mf = fire_GO.GetComponent<MeshFilter>();
            if (mf.mesh == GetComponent<MeshFilter>()) yield return null;

            int index = 0;
            for(int i = 0; i < fire_mesh.Count; i++)
            {
                if (mf.mesh.GetHashCode() == fire_mesh[i].GetHashCode()) index = i;
            }

            float timePerSection = MaxTimeToExtinguish / fire_mesh.Count;

            for(int i = index; i < fire_mesh.Count - 1; i++)
            {
                yield return new WaitForSeconds(2/FireSpeed);
                mf.mesh = fire_mesh[i + 1];
                TimeToExtinguish = timePerSection * (i + 1);
            }

            StopCoroutine("reconstruct");
        }
        public void BeginFire(Vector3 initialPoint)
        {
            //Vacia la lista de puntos actuales
            meshData_current.vertex.Clear ();
            meshData_current.triangles.Clear();

            //Obtenemos la mesh del objeto
            Mesh mesh = GetComponent<MeshFilter>().mesh;
            mesh_original = mesh;
            //Almacenamos sus vértices y triángulos
            meshData_original.vertex.Clear();
            meshData_original.vertex.AddRange(mesh.vertices);

            meshData_original.triangles.Clear();
            meshData_original.triangles.AddRange(mesh.triangles);

            meshData_original.normals.Clear();
            meshData_original.normals.AddRange(mesh.normals);

            onFire = true;
            initialFirePoint = initialPoint;
            FireSpeed = 3;

            StartCoroutine("construct");
            StartCoroutine("burning");

            fire_System.Play();
            if (UsesSmoke && smoke_System) smoke_System.Play();
        }

        WaitForEndOfFrame frame = new WaitForEndOfFrame();
        IEnumerator burning()
        {
            while (this.onFire)
            {
                UpdateFire(initialFirePoint);
                if(this.OnFire())ParticleSize();
                if(this.OnFire())Propagate();
                if (this.OnFire())
                {
                    if (!this.IsExtinguising() && fire_GO.GetComponent<MeshFilter>().mesh != GetComponent<MeshFilter>().mesh)
                    {
                        Reconstruct();
                    }
                }

                if (this.IsExtinguising()) StopCoroutine("reconstruct");
                if (this.Extinguished()) GetComponent<BoxCollider>().enabled = false;

                yield return frame;
            }
        }
        public void UpdateFire(Vector3 initialPoint)
        {
            timeOnFire += Time.deltaTime;
        }

        public void EndFire()
        {
            onFire = false;
            timeOnFire = 0;
            StopCoroutine("burning");

            fire_System.Stop();
            smoke_System.Stop();
        }

        bool maxSize = false;
        public void Propagate()
        {
            PropDistance = fire_MR.localBounds.size.magnitude/1.2f;

            var shape = fire_System.shape;
            shape.scale = fire_MR.bounds.size;

            if (maxSize) return;

            BoxCollider bc = GetComponent<BoxCollider>();
            bc.size = fire_MR.localBounds.size + PropOffset;

            if (completeFire) maxSize = true;
        }

        public bool Extinguished()
        {
            return extinguished;
        }

        public void ExtinguishFire()
        {
            if (this.Extinguished() || !this.OnFire()) return;

            extinguishing = true;
            reconstructing = false;

            TimeToExtinguish -= Time.deltaTime;

            float timePerSection = MaxTimeToExtinguish / fire_mesh.Count;
            for(int i = 0; i < fire_mesh.Count; i++)
            {
                float currentSectionTime = timePerSection * i;
                if(TimeToExtinguish >= currentSectionTime) fire_GO.GetComponent<MeshFilter>().mesh = fire_mesh[i];
            }

            if(TimeToExtinguish <= 0)
            {
                extinguished = true;
                fire_System.Stop();
            }

            StopCoroutine("reconstruct");
        }

        public void ExtinguishWithRaycast(Ray raycast)
        {
            RaycastHit hit;
            
            if (Physics.Raycast(raycast, out hit, 8))
            {
                if (hit.collider == GetComponent<Collider>())
                {
                    ExtinguishFire();
                }
                else Reconstruct();
            }
            else Reconstruct();
        }

        public void ExtinguishWithParticles()
        {
            ExtinguishFire();
        }


        byte framesChecked = 0;
        float valueToCheck = 0;
        public bool IsExtinguising()
        {
            return extinguishing;
            
            if (framesChecked > 2) return false;

            if (valueToCheck == TimeToExtinguish) framesChecked++;

            valueToCheck = TimeToExtinguish;

            return true;
        }

        public void Reconstruct()
        {
            if (reconstructing) return;
            extinguishing = false;

            StartCoroutine("reconstruct");
        }

        public Mesh FireMesh(Vector3 initialFirePoint)
        {
            Mesh fireMesh = new Mesh();

            //Creating the sphere to detect the points
            Vector3 center = initialFirePoint;
            float radius = mesh_original.bounds.size.magnitude/2 / 30 + timeOnFire * FireSpeed;

            foreach (Vector3 p in meshData_original.vertex)
            {
                //(x?cx)2+(y?cy)2+(z?cz)2<r2 .
                //Check if point is inside a sphere

                bool inside = (p.x - center.x) * (p.x - center.x) + (p.y - center.y) * (p.y - center.y) + (p.z - center.z) * (p.z - center.z) < radius;

                Action<object> action = (object obj) => {
                    meshData_current.vertex.Add(p);
                };

                Task addVertex = new(action, "alpha");
                if (inside) addVertex.Wait();
            }

            //Checkea todos los triangulos asegurandose de que ninguno pase de la lungitud de vertices
            for (int i = 0; i < meshData_original.triangles.Count - 3; i += 3)
            {
                bool validTriangle = true;

                if (meshData_original.triangles[i] > meshData_current.vertex.Count - 1 || meshData_original.triangles[i + 1] > meshData_current.vertex.Count - 1 || meshData_original.triangles[i + 2] > meshData_current.vertex.Count - 1) validTriangle = false;

                if (validTriangle) { meshData_current.triangles.Add(meshData_original.triangles[i]); meshData_current.triangles.Add(meshData_original.triangles[i + 1]); meshData_current.triangles.Add(meshData_original.triangles[i + 2]); }
            }

            int h = 0;
            foreach(Vector3 v in meshData_current.vertex)
            {
                meshData_current.normals.Add(meshData_original.normals[h]);
                h++;
            }

            fireMesh.SetVertices(meshData_current.vertex);
            fireMesh.SetTriangles(meshData_current.triangles, 0);
            if(meshData_current.normals.Count == meshData_current.vertex.Count) fireMesh.SetNormals(meshData_current.normals);
            return fireMesh;
        }

        public Mesh FireMesh(Vector3 initialPoint, string assetName)
        {
            Mesh mesh = new Mesh();
            Mesh fireMesh = new Mesh();

            //Creating the sphere to detect the points
            Vector3 center = initialFirePoint;
            float radius = mesh_original.bounds.size.magnitude / 30 + timeOnFire * FireSpeed;

            foreach (Vector3 p in meshData_original.vertex)
            {
                //(x?cx)2+(y?cy)2+(z?cz)2<r2 .
                //Check if point is inside a sphere

                bool inside = (p.x - center.x) * (p.x - center.x) + (p.y - center.y) * (p.y - center.y) + (p.z - center.z) * (p.z - center.z) < radius;

                Action<object> action = (object obj) => {
                    meshData_current.vertex.Add(p);
                };

                Task addVertex = new(action, "alpha");
                if (inside) addVertex.Wait();
            }

            //Checkea todos los triangulos asegurandose de que ninguno pase de la lungitud de vertices
            for (int i = 0; i < meshData_original.triangles.Count - 3; i += 3)
            {
                bool validTriangle = true;

                if (meshData_original.triangles[i] > meshData_current.vertex.Count - 1 || meshData_original.triangles[i + 1] > meshData_current.vertex.Count - 1 || meshData_original.triangles[i + 2] > meshData_current.vertex.Count - 1) validTriangle = false;

                if (validTriangle) { meshData_current.triangles.Add(meshData_original.triangles[i]); meshData_current.triangles.Add(meshData_original.triangles[i + 1]); meshData_current.triangles.Add(meshData_original.triangles[i + 2]); }
            }

            fireMesh.SetVertices(meshData_current.vertex);
            fireMesh.SetTriangles(meshData_current.triangles, 0);
            return fireMesh;
        }

        public Mesh FireMesh(Vector3 initialFirePoint, string assetName, float radiusMultiplier)
        {
            Mesh mesh = new Mesh();
            Mesh fireMesh = new Mesh();

            //Creating the sphere to detect the points
            Vector3 center = initialFirePoint;
            float radius = mesh_original.bounds.size.magnitude / radiusMultiplier + timeOnFire * FireSpeed;

            foreach (Vector3 p in meshData_original.vertex)
            {
                //(x?cx)2+(y?cy)2+(z?cz)2<r2 .
                //Check if point is inside a sphere

                bool inside = (p.x - center.x) * (p.x - center.x) + (p.y - center.y) * (p.y - center.y) + (p.z - center.z) * (p.z - center.z) < radius;

                if (inside) meshData_current.vertex.Add(p);
            }

            //Checkea todos los triangulos asegurandose de que ninguno pase de la lungitud de vertices
            for (int i = 0; i < meshData_original.triangles.Count - 3; i += 3)
            {
                bool validTriangle = true;

                if (meshData_original.triangles[i] > meshData_current.vertex.Count - 1 || meshData_original.triangles[i + 1] > meshData_current.vertex.Count - 1 || meshData_original.triangles[i + 2] > meshData_current.vertex.Count - 1) validTriangle = false;

                if (validTriangle) { meshData_current.triangles.Add(meshData_original.triangles[i]); meshData_current.triangles.Add(meshData_original.triangles[i + 1]); meshData_current.triangles.Add(meshData_original.triangles[i + 2]); }
            }

            int h = 0;
            foreach (Vector3 v in meshData_current.vertex)
            {
                if(h < meshData_original.normals.Count) meshData_current.normals.Add(meshData_original.normals[h]);
                h++;
            }

            fireMesh.SetVertices(meshData_current.vertex);
            fireMesh.SetTriangles(new int[0], 0);
            //if (meshData_current.normals.Count == meshData_current.vertex.Count) fireMesh.SetNormals(meshData_current.normals);

            return fireMesh;
        }

        public bool OnFire()
        {
            return onFire;
        }
        
        public bool CompleteFire()
        {
            return completeFire;
        }

        public Vector2 ParticleSize()
        {
            MeshRenderer mr = fire_MR;
            var sbl = fire_System.sizeOverLifetime;
            sbl.sizeMultiplier = Mathf.Lerp(sbl.sizeMultiplier, mr.localBounds.size.magnitude/1.8f , Time.deltaTime);
            sbl.sizeMultiplier = Mathf.Clamp(sbl.sizeMultiplier, 0.4f, 1.5f);
            
            return Vector2.zero;
        }

        public void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(GetComponent<MeshRenderer>().bounds.center, GetComponent<MeshRenderer>().bounds.size + PropOffset);
        }
        public void OnTriggerEnter(Collider other)
        {
            FireUtils fireUtils = other.GetComponent<FireUtils>();

            if(fireUtils != null)
            {
                if (!fireUtils.OnFire() && this.OnFire() && this.CompleteFire())
                fireUtils.BeginFire(other.ClosestPoint(fire_MR.bounds.center));
            }
        }

        public void OnTriggerStay(Collider other)
        {
            FireUtils fireUtils = other.GetComponent<FireUtils>();

            if(fireUtils != null)
            {
                if (!fireUtils.OnFire() && this.OnFire() && this.CompleteFire())
                {
                    fireUtils.BeginFire(other.ClosestPoint(fire_MR.bounds.center));
                }
            }
        }

        #endregion
        #endregion
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(FireObject), true)]
    [CanEditMultipleObjects]
    public class FireObjectEditor : Editor
    {
        bool fireSettings = true;
        bool smokeSettings = false;
        bool otherVFX = false;

        void OnEnable()
        {
            FireObject manager = (FireObject)target;
        }

        [InitializeOnEnterPlayMode]
        public override void OnInspectorGUI()
        {
            if (Application.isPlaying) return;
            base.OnInspectorGUI();
            
            FireObject manager = (FireObject)target;

            #region Titulo
            GUILayout.BeginHorizontal();
            GUILayout.Label("FIRE OBJECT", EditorStyles.miniButtonMid, GUILayout.ExpandHeight(true));
            GUILayout.EndHorizontal();
            #endregion

            GUILayout.Space(20);

            #region Botones
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Fire Settings", EditorStyles.miniButtonLeft))
            {
                fireSettings = true;
                smokeSettings = false;
                otherVFX = false;
            }
            if (GUILayout.Button("Smoke Settings", EditorStyles.miniButtonMid))
            {
                fireSettings = false;
                smokeSettings = true;
                otherVFX = false;
            }
            if (GUILayout.Button("Other VFX", EditorStyles.miniButtonRight))
            {
                fireSettings = false;
                smokeSettings = false;
                otherVFX = true;
            }
            
            GUILayout.EndHorizontal();
            #endregion

            GUILayout.Space(20);

            #region Zona de parámetros
            GUILayout.BeginVertical(EditorStyles.helpBox);

            #region Ajustes del fuego
            if (fireSettings)
            {
                GUILayout.Label("El prefab de las partículas de fuego");
                SerializedObject obj = new SerializedObject(manager);
                SerializedProperty firePrefab = obj.FindProperty("fire_SystemPrefab");
                EditorGUILayout.PropertyField(firePrefab);

                GUILayout.Space(10);

                GUILayout.Label("Es el fuego inicial?", EditorStyles.boldLabel);
                manager.InitialFire = EditorGUILayout.Toggle(manager.InitialFire, EditorStyles.toggle);

                GUILayout.Space(10);

                GUILayout.Label("El tiempo que tarda en empezar el fuego", EditorStyles.boldLabel);
                manager.Delay = EditorGUILayout.FloatField(manager.Delay, EditorStyles.miniTextField);

                GUILayout.Space(10);

                GUILayout.Label("La velocidad a la que se propaga el fuego");
                manager.FireSpeed = EditorGUILayout.FloatField(manager.FireSpeed, EditorStyles.miniTextField);

                GUILayout.Space(10);

                GUILayout.Label("El tiempo que tarda en apagarse el fuego", EditorStyles.boldLabel);
                manager.MaxTimeToExtinguish = EditorGUILayout.FloatField(manager.MaxTimeToExtinguish, EditorStyles.miniTextField);

                GUILayout.Space(10);

                GUILayout.Label("Solo para testear");
                EditorGUILayout.FloatField(manager.TimeToExtinguish, EditorStyles.label);

                GUILayout.Space(10);

                GUILayout.Label("El offset en el collider para la propagación del fuego", EditorStyles.boldLabel);
                manager.PropOffset = EditorGUILayout.Vector3Field("Propagation Offset", manager.PropOffset);

            }
            #region CheckIfHasFire
            Transform[] _children = manager.GetComponentsInChildren<Transform>();

            bool hasFire = false;
            foreach(Transform child in _children)
            {
                if(child != manager.transform)
                {
                    if (child.name == "Fire") hasFire = true;
                }
            }

            if (!hasFire)
            {
                GameObject fire = new GameObject("Fire", typeof(ParticleSystem), typeof(MeshFilter), typeof(MeshRenderer));
                fire.transform.parent = manager.transform;
                fire.transform.localPosition = Vector3.zero;
                fire.transform.localRotation = Quaternion.identity;
                fire.transform.localScale = Vector3.one;
                manager.fire_System = fire.GetComponent<ParticleSystem>();
                manager.fire_GO = fire;
                var shape = manager.fire_System.shape;
                shape.shapeType = ParticleSystemShapeType.Box;
                shape.meshRenderer = fire.GetComponent<MeshRenderer>();
            }
            var _shape = manager.fire_System.shape;
            if (_shape.meshRenderer != manager.fire_GO.GetComponent<MeshRenderer>()) _shape.meshRenderer = manager.fire_GO.GetComponent<MeshRenderer>();

            #endregion
            #endregion

            #region Ajustes del humo
            if (smokeSettings)
            {
                GUILayout.Label("Se usa el humo?", EditorStyles.boldLabel);
                manager.UsesSmoke = EditorGUILayout.Toggle(manager.UsesSmoke, EditorStyles.toggle);

                Transform[] children = manager.gameObject.GetComponentsInChildren<Transform>();
                bool smokeCreated = false;
                foreach(Transform child in children) { if (child.gameObject.name == "Smoke") smokeCreated = true; }

                if(!smokeCreated && manager.UsesSmoke)
                {
                    GameObject smoke_go = new GameObject("Smoke", typeof(ParticleSystem));
                    smoke_go.transform.parent = manager.transform;
                    smoke_go.hideFlags = HideFlags.NotEditable;
                    manager.smoke_System = smoke_go.GetComponent<ParticleSystem>();
                    manager.smoke_System.hideFlags = HideFlags.HideInInspector;
                }
            }
            #endregion

            #region Ajustes de VFX
            if (otherVFX)
            {
                GUILayout.Label("Se usan chispas?", EditorStyles.boldLabel);
                manager.UsesSparks = EditorGUILayout.Toggle(manager.UsesSparks, EditorStyles.toggle);
            }
            #endregion

            if(GUILayout.Button("DELETE SMOKE"))
            {
                Transform[] children = manager.GetComponentsInChildren<Transform>();

                foreach(Transform child in children) { if (child.gameObject.name == "Smoke") DestroyImmediate(child.gameObject); }
            }

            GUILayout.EndVertical();
            #endregion
        }

        public void OnSceneGUI()
        {
            FireObject manager = (FireObject)target;

            if (Selection.gameObjects.Length > 0 && manager.smoke_System)
            {
                if (Selection.gameObjects[0] == manager.gameObject) manager.smoke_System.Simulate(0.001f, true, false);
            }
        }
    }
#endif
}
