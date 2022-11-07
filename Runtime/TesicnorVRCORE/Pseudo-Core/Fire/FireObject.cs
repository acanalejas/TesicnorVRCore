using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Linq;

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
    [RequireComponent(typeof(Rigidbody))]
    public sealed class FireObject : MonoBehaviour, FireUtils
    {
        #region PARAMETERS
        /// <summary>
        /// Prefab de las partículas de fuego
        /// </summary>
        [SerializeField][HideInInspector] public FireParticles fire_SystemPrefab;
        /// <summary>
        /// El sistema de partículas del fuego
        /// </summary>
        [SerializeField][HideInInspector] public ParticleSystem fire_System;

        /// <summary>
        /// El GameObject de las partículas del fuego
        /// </summary>
        [SerializeField][HideInInspector] public GameObject fire_GO;

        /// <summary>
        /// El meshRenderer del fuego
        /// </summary>
        [SerializeField][HideInInspector] public MeshRenderer fire_MR;
        /// <summary>
        /// El sistema de partículas del humo
        /// </summary>
        [SerializeField][HideInInspector] public ParticleSystem smoke_System;

        /// <summary>
        /// El GameObject de las partículas del humo
        /// </summary>
        [SerializeField][HideInInspector] public GameObject smoke_GO;

        [SerializeField]
        [HideInInspector] public ParticleSystem sparks_System;

        [HideInInspector] public AudioSource fire_Source;

        [SerializeField][HideInInspector] public bool InitialFire = false;
        [SerializeField][HideInInspector] public bool UsesSmoke = true;
        [SerializeField][HideInInspector] public bool UsesSparks = true;
        #region Para el fuego
        [SerializeField][HideInInspector] public float Delay = 4;
        [SerializeField][HideInInspector] public float FireSpeed { get { return fireSpeed; } set { fireSpeed = value; fireSpeed = Mathf.Clamp(fireSpeed, 0.05f, 10f); } }
        [SerializeField][HideInInspector] private float fireSpeed = 1;
        [SerializeField][HideInInspector] public float PropDistance = 1;
        [SerializeField][HideInInspector] public float MaxTimeToExtinguish = 2;
        [SerializeField][HideInInspector] public float TimeToExtinguish = 2;
        [SerializeField][HideInInspector] public float MaxEmission = 80;
        [SerializeField][HideInInspector] public float MaxSize = 4;
        [SerializeField][HideInInspector] public Vector3 PropOffset = Vector3.zero;

        [HideInInspector] public BoxCollider trigger;

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
        #region Para el humo
        [SerializeField][HideInInspector] public float Density = 1;
        [SerializeField][HideInInspector] public Color Smoke_Color = Color.black;
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
            fire_GO.GetComponent<ParticleSystem>().Stop();
            if (smoke_GO && !smoke_System) smoke_System = smoke_GO.GetComponent<ParticleSystem>();
            if(smoke_System)smoke_System.Stop();
            if (sparks_System) sparks_System.Stop();

            fire_MR = fire_GO.GetComponent<MeshRenderer>();
            InitializeStructures();

            BoxCollider[] bcs = GetComponents<BoxCollider>();
            BoxCollider bc = new BoxCollider();
            bool isTrigger = false;
            foreach (BoxCollider col in bcs) { if (col.isTrigger) { isTrigger = true; bc = col; trigger = col; } else { col.enabled = false; } }
            if (!isTrigger) bc = bcs[0];
            //bc.size = fire_MR.localBounds.size;
            bc.isTrigger = true;
            //bc.center = Vector3.zero;
            //bc.enabled = false;

            GetComponent<Rigidbody>().isKinematic = true;

            //Just for security in sound
            fire_Source.playOnAwake = false;
            fire_Source.Stop();
        }
        private void Start()
        {
            mesh_original = GetComponent<MeshFilter>().mesh;
            if (InitialFire) BeginFire(/*GetComponent<MeshRenderer>().localBounds.center*/ mesh_original.bounds.center);
        }

        IEnumerator construct()
        {
            if(completeFire) { StopCoroutine("construct"); }
            yield return FireMesh(initialFirePoint, 4);
            yield return FireMesh(initialFirePoint, 3);
            yield return FireMesh(initialFirePoint, 2);
            yield return FireMesh(initialFirePoint, 1f);
            fire_mesh.Add(GetComponent<MeshFilter>().mesh);

            yield return new WaitForSeconds(Delay);
            
            fire_System.Play();
            fire_Source.loop = true;
            fire_Source.Play();
            if (UsesSmoke && smoke_System) smoke_System.Play();
            if (UsesSparks && sparks_System) sparks_System.Play();
            StartCoroutine("burning");
            //reconstructing = false;

            StopCoroutine("construct");
        }

        [HideInInspector]public bool reconstructing = false;
        int index = 0;
        void reconstruct()
        {
            Debug.Log("Index is : " + index);
            if (index == fire_mesh.Count) return;
            reconstructing = true;
            MeshFilter mf = fire_GO.GetComponent<MeshFilter>();

            float timePerSection = MaxTimeToExtinguish / fire_mesh.Count;

                if (mesh_original.isReadable)
                {
                    mf.mesh = fire_mesh[index];
                    var shape = fire_System.shape;
                    shape.mesh = fire_mesh[index];
                }
                else
                {
                    var shape = fire_System.shape;
                    shape.scale = (GetComponent<MeshRenderer>().bounds.extents) / (fire_mesh.Count - index);
                }
                ParticleSize();
                AdaptSmoke();
                AdaptSparks();
                Propagate();

                TimeToExtinguish = timePerSection * (index);
                if (index == fire_mesh.Count - 1) completeFire = true;
                else completeFire = false;
            index++;
        }
        public void BeginFire(Vector3 initialPoint)
        {
            //Vacia la lista de puntos actuales
            meshData_current.vertex.Clear ();
            meshData_current.triangles.Clear();

            //Obtenemos la mesh del objeto
            Mesh mesh = GetComponent<MeshFilter>().mesh;
            mesh_original = mesh;

            var sol = fire_System.sizeOverLifetime;
            sol.sizeMultiplier = 0;

            var emission = fire_System.emission;
            emission.rateOverTime = 0;

            //var main = fire_System.main;
            //main.startSize = 0;

            if (mesh.isReadable)
            {
                //Almacenamos sus vértices y triángulos
                meshData_original.vertex.Clear();
                mesh.GetVertices(meshData_original.vertex);

                meshData_original.triangles.Clear();
                meshData_original.triangles.AddRange(mesh.triangles);

                meshData_original.normals.Clear();
                meshData_original.normals.AddRange(mesh.normals);

                var shape = fire_System.shape;
                shape.scale = Vector3.one;
            }
            else
            {
                var shape = fire_System.shape;
                shape.shapeType = ParticleSystemShapeType.Box;
            }

            onFire = true;
            initialFirePoint = initialPoint;

            TimeToExtinguish = MaxTimeToExtinguish;

            StartCoroutine("construct");

            ParticleSize(); Propagate(); AdaptSmoke(); AdaptSparks();

            BoxCollider[] bcs = GetComponents<BoxCollider>();
            BoxCollider bc = new BoxCollider();
            foreach (BoxCollider col in bcs) if (col.isTrigger) {bc = col; }
            bc.enabled = true;
        }

        WaitForEndOfFrame frame = new WaitForEndOfFrame();
        IEnumerator burning()
        {
            while (this.onFire)
            {
                ParticleSize();
                if (this.IsExtinguising()) CancelInvoke(nameof(reconstruct));
                else Reconstruct();
                if (this.Extinguished()) GetComponent<BoxCollider>().enabled = false;

                yield return frame;
            }
            StopCoroutine("burning");
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

            //var shape = fire_System.shape;
            //shape.scale = Vector3.Lerp(shape.scale, fire_MR.bounds.size, Time.deltaTime);
            BoxCollider[] bcs = GetComponents<BoxCollider>();
            BoxCollider bc = new BoxCollider();
            foreach (BoxCollider col in bcs) { if (col.isTrigger) { bc = col; trigger = col; } else col.enabled = true; }

            if (mesh_original.isReadable)
            {
                bc.size = fire_MR.localBounds.size + PropOffset;
            }
            else
            {
                var shape = fire_System.shape;
                bc.size = shape.scale + PropOffset;
            }
            if (fire_Source)
            {
                fire_Source.volume = fire_MR.localBounds.extents.magnitude;
                fire_Source.maxDistance = fire_MR.localBounds.extents.magnitude;
            }
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
                if (TimeToExtinguish >= currentSectionTime) { fire_GO.GetComponent<MeshFilter>().mesh = fire_mesh[i]; index = i; }
                ParticleSize();
                AdaptSmoke();
                AdaptSparks();
                Propagate();
            }

            if (TimeToExtinguish <= 0)
            {
                extinguished = true;
                fire_System.Stop();
                if (smoke_System) smoke_System.Stop();
                if(sparks_System) sparks_System.Stop();
                if (fire_Source) fire_Source.Stop();

                Collider[] colliders = GetComponents<Collider>();
                foreach(Collider col in colliders) col.enabled = false;
            }

            StopCoroutine("reconstruct");
        }

        public void ExtinguishWithRaycast(Ray raycast)
        {
            RaycastHit hit;
            extinguishing = true;
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

        public void ExtinguishWithCone(Vector3 origin, Vector3 forward)
        {
            Vector3 destiny = this.transform.position;
            Vector3 distance = destiny - origin;

            float dot = Vector3.Dot(distance.normalized, forward);
            Debug.Log("DOT : " + dot);

            Ray ray = new Ray(origin, distance.normalized);
            RaycastHit hit;
            Physics.Raycast(ray, out hit, 7);

            if(dot > 0.8f && hit.collider == GetComponent<Collider>())
            {
                ExtinguishFire();
            }
            else { Reconstruct(); }
        }


        byte framesChecked = 0;
        float valueToCheck = 0;
        public bool IsExtinguising()
        {
            if (valueToCheck == TimeToExtinguish) framesChecked++;
            else framesChecked = 0;

            if (framesChecked > 10) extinguishing = false;

            valueToCheck = TimeToExtinguish;

            return extinguishing;
        }

        public void Reconstruct()
        {
            if (reconstructing) return;
            extinguishing = false;

            InvokeRepeating(nameof(reconstruct), 0.0f, 2/FireSpeed);
        }

        struct vertex { public float distance; public int index; }
        int numVertex;
        Vector3 center;
        public IEnumerator FireMesh(Vector3 initialFirePoint, float radiusMultiplier)
        {

            Mesh fireMesh = new Mesh();

            //if (!mesh_original.isReadable)  return fireMesh;

            //Creating the sphere to detect the points
            center = initialFirePoint;

            yield return checkVertices(center, radiusMultiplier);
            fireMesh.SetVertices(meshData_current.vertex);
            fireMesh.SetTriangles(meshData_current.triangles, 0);
            fireMesh.UploadMeshData(false);
            fireMesh.name = "FireMesh  :  " + radiusMultiplier;
            if (meshData_current.normals.Count == meshData_current.vertex.Count) fireMesh.SetNormals(meshData_current.normals);

            fire_mesh.Add(fireMesh);
            //return fireMesh;

        }

        
        IEnumerator checkVertices(Vector3 center, float radiusMultiplier)
        {
            numVertex = (int)(mesh_original.vertices.Length / radiusMultiplier);
            List<vertex> distances = new List<vertex>();
            int i = 0;
            List<int> VertexInside = new List<int>();
            //distances = distances.OrderByDescending(x => x.distance).ToList();
            distances = distances.OrderBy(x => x.distance).ToList();
            Debug.Log(meshData_current.vertex.Count);
            int eachLength = (int)(meshData_original.vertex.Count / 10);
            for (int j = 0; j < meshData_original.vertex.Count; j++)
            {
                vertex _vertex = new vertex();
                _vertex.index = j;
                _vertex.distance = Vector3.Distance(center, transform.TransformPoint(meshData_original.vertex[j]));
                distances.Add(_vertex);

                i++;
            }
            distances = distances.OrderBy(x => x.distance).ToList();
            yield return new WaitForEndOfFrame();
            for (int k = numVertex - 1; k >= 0; k--)
            {
                meshData_current.vertex.Add(meshData_original.vertex[distances[k].index]);
                VertexInside.Add(distances[k].index);
            }
            yield return new WaitForEndOfFrame();
            int h = 0;
            foreach (Vector3 v in meshData_current.vertex)
            {
                if (h < VertexInside.Count)
                    if (VertexInside[h] < meshData_original.normals.Count) meshData_current.normals.Add(meshData_original.normals[VertexInside[h]]);
                h++;
            }
            StopCoroutine("checkVertices");
        }

        public bool OnFire()
        {
            return onFire;
        }

        public bool FireStarted()
        {
            return fire_System.particleCount > 0;
        }
        
        public bool CompleteFire()
        {
            return completeFire;
        }

        public Vector2 ParticleSize()
        {
            MeshRenderer mr = fire_MR;
            var sbl = fire_System.sizeOverLifetime;
            var shape = fire_System.shape;
            if (GetComponent<MeshFilter>().mesh.isReadable)
            {
                sbl.sizeMultiplier = Mathf.Lerp(sbl.sizeMultiplier, mr.bounds.size.magnitude / 1.5f, Time.deltaTime);
            }
            else
            {
                sbl.sizeMultiplier = Mathf.Lerp(sbl.sizeMultiplier, shape.scale.magnitude / 1.5f, Time.deltaTime);
            }
            sbl.sizeMultiplier = Mathf.Clamp(sbl.sizeMultiplier, 0.4f, 1.5f);

            var emission = fire_System.emission;
            Vector3 size = Vector3.zero;

            size = fire_GO.GetComponent<MeshRenderer>().bounds.size;
            float maxSize = MaxSize;
            float maxEmission = MaxEmission;
            float currentSize = size.magnitude;
            float currentEmission = (maxEmission * currentSize) / maxSize;

            //var main = fire_System.main;
            //main.startSize = Mathf.Lerp(main.startSize.constant, GetComponent<BoxCollider>().size.magnitude, Time.deltaTime);

            emission.rateOverTime = Mathf.Lerp(emission.rateOverTime.constant, currentEmission, Time.deltaTime);
            
            return Vector2.zero;
        }

        public void AdaptSmoke()
        {
            if (!smoke_System || !UsesSmoke) return;

            var fire_shape = fire_System.shape;
            var smoke_shape = smoke_System.shape;

            //smoke_shape.scale = fire_shape.scale;
            smoke_shape.shapeType = ParticleSystemShapeType.Box;
            if (fire_shape.mesh)
            {
                smoke_shape.scale = fire_MR.bounds.size;
            }
            smoke_System.transform.position = fire_MR.bounds.center;

            var fire_sol = fire_System.sizeOverLifetime;
            var smoke_sol = smoke_System.sizeOverLifetime;
            var fire_emission = fire_System.emission;
            var smoke_emission = smoke_System.emission;

            if (fire_emission.rateMultiplier == 0) smoke_emission.rateMultiplier = 0;
            else smoke_emission.rateMultiplier = 7;
            smoke_sol.sizeMultiplier = fire_sol.sizeMultiplier*1.5f;

            smoke_emission.rateMultiplier *= Density;

            var smoke_main = smoke_System.main;
            smoke_main.startColor = Smoke_Color;

            smoke_System.gameObject.GetComponent<ParticleSystemRenderer>().material.color = Smoke_Color;

            var smoke_col = smoke_System.colorOverLifetime;
            smoke_col.color = Smoke_Color;

            
        }
        public void AdaptSparks()
        {
            if (!sparks_System || !UsesSparks) return;

            var fire_shape = fire_System.shape;
            var sparks_shape = sparks_System.shape;

            sparks_shape.scale = fire_shape.scale;
            sparks_shape.shapeType = fire_shape.shapeType;
            sparks_shape.meshRenderer = fire_shape.meshRenderer;
        }

        public void OnDrawGizmos()
        {
            if (!fire_MR) return;
            Vector3 size = fire_MR.localBounds.size + PropOffset; Vector3 scale = transform.lossyScale;
            Matrix4x4 rotationMatrix = transform.localToWorldMatrix;
            //Gizmos.matrix = rotationMatrix;
            Gizmos.DrawWireCube(fire_MR.bounds.center, transform.InverseTransformDirection(new Vector3(size.x * scale.x, size.y * scale.y, size.z * scale.z)));
        }

        public void OnTriggerStay(Collider other)
        {
            FireUtils fireUtils = other.GetComponent<FireUtils>();

            if (fireUtils != null)
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
            BoxCollider[] cols = manager.gameObject.GetComponents<BoxCollider>();
            foreach(BoxCollider col in cols){ if(col.isTrigger) manager.trigger = col;}
            if(!manager.trigger) {
                manager.trigger = manager.gameObject.AddComponent<BoxCollider>();
                manager.trigger.isTrigger = true;
                }
            OnInspectorGUI();   
        }

        [InitializeOnEnterPlayMode]
        public override void OnInspectorGUI()
        {
            if (Application.isPlaying) return;
            base.OnInspectorGUI();
            
            FireObject manager = (FireObject)target;

            manager.fire_MR = manager.GetComponent<MeshRenderer>();

            if(!manager.trigger){
               BoxCollider[] cols = manager.gameObject.GetComponents<BoxCollider>();
            foreach(BoxCollider col in cols){ if(col.isTrigger) manager.trigger = col;}
            if(!manager.trigger) {
                manager.trigger = manager.gameObject.AddComponent<BoxCollider>();
                manager.trigger.isTrigger = true;
                }
            }

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
                SerializedProperty firePrefab = serializedObject.FindProperty("fire_SystemPrefab");
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

                GUILayout.Space(10);

                GUILayout.Label("La emissión de referencia", EditorStyles.boldLabel);
                manager.MaxEmission = EditorGUILayout.FloatField(manager.MaxEmission, EditorStyles.miniTextField);

                GUILayout.Space(10);

                GUILayout.Label("El tamaño de referencia", EditorStyles.boldLabel);
                manager.MaxSize = EditorGUILayout.FloatField(manager.MaxSize, EditorStyles.miniTextField);

                GUILayout.Space(20);

                if(GUILayout.Button("Reset Fire", EditorStyles.miniButton))
                {
                    Transform fire = manager.transform.Find("Fire_GO");
                    if (!fire) fire = manager.transform.Find("Fire");
                    if (fire)
                    {
                        DestroyImmediate(fire.gameObject);
                    }
                }

            }
    #region CheckIfHasFire
            Transform[] _children = manager.GetComponentsInChildren<Transform>();

            bool hasFire = false;
            foreach(Transform child in _children)
            {
                if(child != manager.transform)
                {
                    if (child.name == "Fire_GO" && child.parent == manager.transform) hasFire = true;
                }
            }

            if (!hasFire)
            {
                GameObject fire = new GameObject("Fire_GO", typeof(ParticleSystem), typeof(MeshFilter), typeof(MeshRenderer), typeof(AudioSource));
                fire.transform.parent = manager.transform;
                fire.transform.localPosition = Vector3.zero;
                fire.transform.localRotation = Quaternion.Euler(Vector3.zero);
                fire.transform.localScale = Vector3.one;
                //fire.transform.forward = Vector3.up;
                fire.GetComponent<MeshRenderer>().enabled = false;
                manager.fire_System = fire.GetComponent<ParticleSystem>();
                manager.fire_GO = fire;
                manager.fire_SystemPrefab = AssetDatabase.LoadAssetAtPath("Packages/com.tesicnor.tesicnorvrcore/Runtime/TesicnorVRCORE/Pseudo-Core/Fire/ScriptableObjects/Fire Particles.asset", typeof(FireParticles)) as FireParticles;
                manager.fire_Source = fire.GetComponent<AudioSource>();
                manager.fire_Source.clip = AssetDatabase.LoadAssetAtPath("Packages/com.tesicnor.tesicnorvrcore/Runtime/TesicnorVRCORE/Pseudo-Core/Fire/Sounds/Big Fire.wav", typeof(AudioClip)) as AudioClip;
                manager.fire_Source.playOnAwake = false;

                CopyParticles(manager.fire_SystemPrefab.fire_System, manager.fire_System);
            }

    #endregion
    #endregion

    #region Ajustes del humo
            if (smokeSettings)
            {
                GUILayout.Label("Se usa el humo?", EditorStyles.boldLabel);
                manager.UsesSmoke = EditorGUILayout.Toggle(manager.UsesSmoke, EditorStyles.toggle);

                GUILayout.Space(10);

                GUILayout.Label("La densidad del humo", EditorStyles.boldLabel);
                manager.Density = EditorGUILayout.Slider(manager.Density, 1, 3);

                GUILayout.Space(10);

                GUILayout.Label("El color del humo", EditorStyles.boldLabel);
                manager.Smoke_Color = EditorGUILayout.ColorField(manager.Smoke_Color);

                GUILayout.Space(20);
                
                if(GUILayout.Button("Reset Smoke"))
                {
                    Transform smoke = manager.transform.Find("Smoke");
                    if (smoke) DestroyImmediate(smoke.gameObject);
                }
            }

            Transform[] children = manager.gameObject.GetComponentsInChildren<Transform>();
            bool smokeCreated = false;
            foreach (Transform child in children) { if (child.gameObject.name == "Smoke" && child.parent == manager.transform) smokeCreated = true; }

            if (!smokeCreated && manager.UsesSmoke)
            {
                GameObject smoke_go = new GameObject("Smoke", typeof(ParticleSystem));
                smoke_go.transform.parent = manager.transform;
                smoke_go.transform.localRotation = Quaternion.Euler(Vector3.zero);
                smoke_go.transform.localScale = Vector3.one;
                //smoke_go.hideFlags = HideFlags.NotEditable;
                manager.smoke_System = smoke_go.GetComponent<ParticleSystem>();

                CopyParticles(manager.fire_SystemPrefab.smoke_System, manager.smoke_System);
            }
            else if (smokeCreated && !manager.UsesSmoke)
            {
                Transform smoke_go = manager.transform.Find("Smoke");
                DestroyImmediate(smoke_go.gameObject);
            }
    #endregion

    #region Ajustes de VFX
            if (otherVFX)
            {
                GUILayout.Label("Se usan chispas?", EditorStyles.boldLabel);
                manager.UsesSparks = EditorGUILayout.Toggle(manager.UsesSparks, EditorStyles.toggle);

                GUILayout.Space(20);

                GUILayout.Button("Reset Sparks", EditorStyles.miniButton);
            }

            Transform sparks = manager.transform.Find("Sparks");
            if(sparks)
            if(sparks.parent != manager.transform) sparks = null;

            if(manager.UsesSparks && !sparks)
            {
                GameObject sparks_go = new GameObject("Sparks", typeof(ParticleSystem));
                sparks_go.transform.parent = manager.transform;
                sparks_go.transform.localPosition = Vector3.zero;
                sparks_go.transform.localRotation = Quaternion.identity;
                sparks_go.transform.localScale = Vector3.one;
                sparks_go.hideFlags = HideFlags.HideInHierarchy;
                manager.sparks_System = sparks_go.GetComponent<ParticleSystem>();

                CopyParticles(manager.fire_SystemPrefab.sparks_System, manager.sparks_System);
            }
            else if(!manager.UsesSparks && sparks)
            {
                DestroyImmediate(sparks.gameObject);
            }

    #endregion

            GUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();
    #endregion
        }
        
        void CopyParticles(ParticleSystem ps, ParticleSystem _target)
        {
            FireObject manager = (FireObject)target;

            var main = _target.main;
            var main_p = ps.main;
            main.duration = main_p.duration;
            main.startLifetime = main_p.startLifetime;
            main.simulationSpace = main_p.simulationSpace;
            main.cullingMode = main_p.cullingMode;
            main.emitterVelocityMode = main_p.emitterVelocityMode;
            main.emitterVelocity = main_p.emitterVelocity;
            main.gravityModifier = main_p.gravityModifier;
            main.maxParticles = main_p.maxParticles;
            main.gravityModifierMultiplier = main_p.gravityModifierMultiplier;
            main.playOnAwake = main_p.playOnAwake;
            main.prewarm = main_p.prewarm;
            main.startRotation = main_p.startRotation;
            main.loop = main_p.loop;
            main.flipRotation = main_p.flipRotation;
            main.scalingMode = main_p.scalingMode;
            main.startColor = main_p.startColor;
            main.startDelay = main_p.startDelay;
            main.startSize = main_p.startSize;
            main.startSpeed = main_p.startSpeed;


            var shape = _target.shape;
            shape.shapeType = ParticleSystemShapeType.Mesh;
            shape.meshShapeType = ParticleSystemMeshShapeType.Edge;
            shape.meshRenderer = _target.GetComponent<MeshRenderer>();

            var col = _target.colorOverLifetime;
            var col_p = ps.colorOverLifetime;
            col.color = col_p.color;
            col.enabled = col_p.enabled;

            var emission = _target.emission;
            var emission_p = ps.emission;
            emission.burstCount = emission_p.burstCount;
            emission.rateMultiplier = emission_p.rateMultiplier;
            emission.rateOverTimeMultiplier = emission_p.rateOverTimeMultiplier;
            emission.rateOverDistanceMultiplier = emission_p.rateOverDistanceMultiplier;
            emission.rate = emission_p.rate;
            emission.rateOverDistance = emission_p.rateOverDistance;
            emission.rateOverTime = emission_p.rateOverTime;
            emission.type = emission_p.type;

            var sol = _target.sizeOverLifetime;
            var sol_p = ps.sizeOverLifetime;
            sol.size = sol_p.size;
            sol.sizeMultiplier = sol_p.sizeMultiplier;
            sol.separateAxes = sol_p.separateAxes;
            sol.x = sol_p.x; sol.y = sol_p.y; sol.z = sol_p.z;
            sol.xMultiplier = sol_p.zMultiplier; sol.yMultiplier = sol_p.yMultiplier; sol.zMultiplier = sol_p.zMultiplier;
            sol.enabled = sol_p.enabled;

            Renderer renderer = _target.GetComponent<Renderer>();
            Renderer renderer_p = ps.GetComponent<Renderer>();
            renderer.sharedMaterial = renderer_p.sharedMaterial;

            var tsa = _target.textureSheetAnimation;
            var tsa_p = ps.textureSheetAnimation;
            tsa.fps = tsa_p.fps;
            tsa.animation = tsa_p.animation;
            tsa.mode = tsa_p.mode;
            tsa.rowMode = tsa_p.rowMode;
            tsa.cycleCount = tsa_p.cycleCount;
            tsa.frameOverTime = tsa_p.frameOverTime;
            tsa.startFrame = tsa_p.startFrame;
            tsa.speedRange = tsa_p.speedRange;
            tsa.enabled = tsa_p.enabled;
            tsa.numTilesX = tsa_p.numTilesX;
            tsa.numTilesY = tsa_p.numTilesY;

            var noise = _target.noise;
            var noise_p = ps.noise;
            noise.damping = noise_p.damping;
            noise.frequency = noise_p.frequency;
            noise.octaveScale = noise_p.octaveScale;
            noise.octaveMultiplier = noise_p.octaveMultiplier;
            noise.quality = noise_p.quality;
            noise.enabled = noise_p.enabled;
            noise.strength = noise_p.strength;

            var vol = _target.velocityOverLifetime;
            var vol_p = ps.velocityOverLifetime;
            vol.orbitalX = vol_p.orbitalX;
            vol.orbitalY = vol_p.orbitalY;
            vol.orbitalZ = vol_p.orbitalZ;
            vol.space = vol_p.space;
            vol.x = vol_p.x; vol.y = vol_p.y; vol.z = vol_p.z;
            vol.radial = vol_p.radial;
            vol.orbitalOffsetX = vol_p.orbitalOffsetX;
            vol.orbitalOffsetY = vol_p.orbitalOffsetY;
            vol.orbitalOffsetZ = vol_p.orbitalOffsetZ;
            vol.speedModifier = vol_p.speedModifier;
            vol.enabled = vol_p.enabled;
        }
    }
#endif
}
