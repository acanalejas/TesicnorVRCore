using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Linq;
using UnityEngine.Events;

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
        [HideInInspector] public bool badExtinguisher = false;

        [HideInInspector] public BoxCollider trigger;
        [HideInInspector] public BoxCollider collider;

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

        #region Multiplayer

        #endregion
        #endregion

        static string id;
        #region FUNCTIONS
        #region Interface
        private void Awake()
        {
            fire_GO.GetComponent<ParticleSystem>().Stop();
            if (smoke_GO && !smoke_System) smoke_System = smoke_GO.GetComponent<ParticleSystem>();
            if(smoke_System)smoke_System.Stop();
            if (sparks_System) sparks_System.Stop();

            fire_MR = fire_GO.GetComponent<MeshRenderer>();

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
            //id = UniqueIDManager.Instance.GetIDFromGameObject(this.gameObject).ToString();
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
            
            if(fireutils != null)
            {
                if (!fireutils.OnFire() || fireutils.Extinguished())
                {
                    onFire = false;
                    extinguished = true;
                    TimeToExtinguish = 0;
                    yield break;
                }
            }

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
            if (index == fire_mesh.Count) return;
            if (extinguishing || extinguished) return;
            reconstructing = true;
            MeshFilter mf = fire_GO.GetComponent<MeshFilter>();
            MeshFilter smf = smoke_System.gameObject.GetComponent<MeshFilter>();

            float timePerSection = MaxTimeToExtinguish / fire_mesh.Count;

            if (mesh_original.isReadable)
            {
                mf.mesh = fire_mesh[index];
                var shape = fire_System.shape;
                shape.mesh = fire_mesh[index];

                var smoke_shape = smoke_System.shape;
                smoke_shape.mesh = fire_mesh[index];
                smf.mesh = fire_mesh[index];
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

        private FireUtils fireutils;

        public void BeginFire(Vector3 initialPoint, FireUtils utils = null)
        {
            //Vacia la lista de puntos actuales
            if (this.onFire || this.extinguished) return;
            fireutils = utils;
            if(utils != null)
            {
                if(!utils.OnFire() || utils.Extinguished()) return; 
            }
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
            while (this.onFire || !this.extinguished)
            {
                ParticleSize();
                if (this.IsExtinguising() || this.extinguished) CancelInvoke(nameof(reconstruct));
                else if(!this.extinguished && !this.IsExtinguising()) Reconstruct();
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
            CancelInvoke(nameof(reconstruct));
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
            BoxCollider bcc = new BoxCollider();
            foreach (BoxCollider col in bcs) { if (col.isTrigger) { bc = col; trigger = col; } else { col.enabled = true; bcc = col; collider = col; } }
            if (!bcc) { bcc = this.gameObject.AddComponent<BoxCollider>(); collider = bcc; }

            if (mesh_original.isReadable)
            {
                bc.size = fire_MR.localBounds.size + PropOffset;
                bcc.size = fire_MR.localBounds.size;

                bc.center = fire_MR.localBounds.center;
                bcc.center = fire_MR.localBounds.center;
            }
            else
            {
                var shape = fire_System.shape;
                bc.size = shape.scale + PropOffset;
                bcc.size = shape.scale;
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

        int _try = 0;
        public void ExtinguishFire()
        {
            if (this.Extinguished() || !this.OnFire()) return;

            extinguishing = true;
            reconstructing = false;

            float timeToSubstract = Time.deltaTime;
            if (_try == 0) _try = 1;
            else if (_try == 1) _try = 0;

            if (badExtinguisher && _try != 0) return;

            TimeToExtinguish -= timeToSubstract;

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

            if (framesChecked > 4) extinguishing = false;

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
                sbl.sizeMultiplier = Mathf.Lerp(sbl.sizeMultiplier, mr.bounds.size.magnitude / 1.8f, Time.deltaTime);
            }
            else
            {
                sbl.sizeMultiplier = Mathf.Lerp(sbl.sizeMultiplier, shape.scale.magnitude / 1.8f, Time.deltaTime);
            }
            sbl.sizeMultiplier = Mathf.Clamp(sbl.sizeMultiplier, 0.4f, 1f);

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
            smoke_shape.shapeType = ParticleSystemShapeType.Mesh;
            if (fire_shape.mesh)
            {
                smoke_shape.scale = Vector3.one;
                //smoke_shape.scale = new Vector3(fire_MR.bounds.size.x / this.transform.lossyScale.x, fire_MR.bounds.size.y / this.transform.lossyScale.y, fire_MR.bounds.size.z / this.transform.lossyScale.z);
            }
            else
            {
                smoke_shape.shapeType = ParticleSystemShapeType.Box;
                smoke_shape.scale = GetComponent<BoxCollider>().size;
            }
            smoke_System.transform.position = fire_System.transform.position;

            var fire_sol = fire_System.sizeOverLifetime;
            var smoke_sol = smoke_System.sizeOverLifetime;
            var fire_emission = fire_System.emission;
            var smoke_emission = smoke_System.emission;

            if (fire_emission.rateMultiplier == 0) smoke_emission.rateMultiplier = 0;
            else smoke_emission.rateMultiplier = 10;
            smoke_sol.sizeMultiplier = fire_sol.sizeMultiplier*1.5f/this.transform.lossyScale.magnitude;

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

            var main = sparks_System.main;
            main.startSize = 0.05f;

            var sol = sparks_System.sizeOverLifetime;
            sol.sizeMultiplier = 0.05f;
        }

        public void OnDrawGizmos()
        {
            if (!fire_MR) return;
            Vector3 size = fire_MR.localBounds.size + PropOffset; Vector3 scale = transform.lossyScale;
            Matrix4x4 rotationMatrix = transform.localToWorldMatrix;
            //Gizmos.matrix = rotationMatrix;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(fire_MR.bounds.center, transform.InverseTransformDirection(new Vector3(size.x * scale.x, size.y * scale.y, size.z * scale.z)));
        }

        public void OnTriggerStay(Collider other)
        {
            FireUtils fireUtils = other.GetComponent<FireUtils>();

            if (fireUtils != null)
            {
                if (!fireUtils.OnFire() && this.OnFire() && this.CompleteFire() && !this.Extinguished()&& !fireUtils.Extinguished())
                {
                    fireUtils.BeginFire(other.ClosestPoint(fire_MR.bounds.center), this);
                }
            }
        }

        /// <summary>
        /// If the object is not in fire, returns (10,10,10)
        /// </summary>
        /// <returns></returns>
        public Vector3 GetClosestPointToFire()
        {
            if (!this.OnFire()) return new Vector3(10,10,10);

            return collider.ClosestPoint(Camera.main.transform.position);
        }
        #endregion
        #endregion
    }
}
