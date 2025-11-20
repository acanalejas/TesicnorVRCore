using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGravity : MonoBehaviour
{
    #region PARAMETERS
    [Header("El GameObject del cuerpo")]
    [SerializeField] public GameObject BodyGO;

    [Header("OPCIONAL El gameobject de la camara para calcular la altura")]
    [SerializeField] protected Transform Camera_T;

    [Header("Esta activa la gravedad?")]
    public bool IsGravtyActive = true;

    [Header("Los enganches que pueden afectar a la gravedad")]
    [SerializeField] private Anchor[] Anchors;

    [Header("La tag que se usa para detectar el suelo")]
    [SerializeField] private string FloorTag = "Floor";

    [Header("El evento que se lanza al terminar una caida")]
    public UnityEngine.Events.UnityEvent<bool> OnFallEnd;

    public UnityEngine.InputSystem.XR.TrackedPoseDriver HMD_pd;


    private Rigidbody BodyRB;
    private CapsuleCollider BodyColl;

    private CollisionDetector cd;

    public bool bIsAnchored { get { return IsPlayerAnchored(); } }
    #endregion

    #region CollisionDetector

    public class CollisionDetector : MonoBehaviour
    {
        public GameObject lastCollided;

        private Collider col;

        public float Threshold = 1f;

        public string FloorTag = "Floor";

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == FloorTag)
                lastCollided = other.gameObject;
        }

        private void OnTriggerStay(Collider other)
        {
            if(other.gameObject.tag == FloorTag)
                lastCollided = other.gameObject;
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject == lastCollided) lastCollided = null;
        }

        public float ColliderDistance()
        {
            Vector3 point1 = col.ClosestPoint(this.transform.position);
            Vector3 point2 = this.GetComponent<Collider>().ClosestPoint(point1);

            return Vector3.Distance(point2, point1);
        }
    }

    #endregion

    #region METHODS
    private void SetupBodyGO()
    {
        if(!BodyGO.GetComponent<CapsuleCollider>()) BodyColl = BodyGO.AddComponent<CapsuleCollider>();
        else BodyColl = BodyGO.GetComponent<CapsuleCollider>();

        cd = BodyGO.AddComponent<CollisionDetector>();
        cd.FloorTag = FloorTag;

        BodyColl.height = 1.50f;
        BodyColl.radius = 0.15f;

        BodyColl.isTrigger = true;
    }

    void TransferVelocity()
    {

    }

    private void Start()
    {
        SetupBodyGO();
        if (!IsGravtyActive) return;
        StartCoroutine(nameof(CustomUpdate));

        layerMask = LayerMask.GetMask("Floor");
    }

    public void EnableGravity()
    {
        IsGravtyActive = true;
        StartCoroutine(nameof(CustomUpdate));

        layerMask = LayerMask.GetMask("Floor");
    }

    WaitForEndOfFrame Frame = new WaitForEndOfFrame();
    private int layerMask = 0;
    IEnumerator CustomUpdate()
    {
        float initialHeight = - 10;
        float timer = 0;
        while (true)
        {
            if (CanPlayerFall())
            {
#if UNITY_6000
                //BodyGO.transform.parent.position += BodyRB.linearVelocity * Time.deltaTime;
#else
                //BodyGO.transform.parent.position += BodyRB.velocity * Time.deltaTime;
#endif
                if (!cd.lastCollided || (cd.lastCollided && cd.lastCollided.tag != FloorTag) /*|| (!cd.lastCollided && ShouldContinueFalling())*/)
                {
                    if (timer <= 0) initialHeight = this.transform.position.y;
                    Vector3 velocity = Physics.gravity * timer;
                    this.transform.position += (velocity * Time.deltaTime);
                    timer += Time.deltaTime;
                }
                else if(timer != 0)
                {
                    float finalHeight = this.transform.position.y;
                    if(timer >= 0.05f && initialHeight - finalHeight > 0.4f) OnFallEnd.Invoke(IsPlayerAnchored());
                    timer = 0;
                }
                if (HMD_pd && this.BodyColl) this.BodyColl.height = Mathf.Clamp(HMD_pd.positionInput.action.ReadValue<Vector3>().y + 0.2f, BodyColl.radius * 2, 5);
            }
            // else if(timer != 0) {
//
            //    if (timer >= 0.05f) OnFallEnd.Invoke(IsPlayerAnchored());
            //    timer = 0;
            //}
            AdjustHeight();
            yield return Frame;
        }
    }

    public void AdjustHeight()
    {
        if (!cd.lastCollided || (cd.lastCollided && !cd.lastCollided.CompareTag(FloorTag))) return;
        
        RaycastHit hit;
        Ray ray = new Ray(Camera_T.position, Vector3.down);
        
        
        if (Physics.Raycast(ray, out hit, 2, layerMask))
        {
            
            if(Vector3.Distance(hit.point, Camera_T.transform.position) > HMD_pd.positionInput.action.ReadValue<Vector3>().y + 0.05f || 
               Vector3.Distance(hit.point, Camera_T.position) < HMD_pd.positionInput.action.ReadValue<Vector3>().y - 0.05f)
            {
                float difference = HMD_pd.positionInput.action.ReadValue<Vector3>().y -
                                   Vector3.Distance(hit.point, Camera_T.position);
                this.transform.position += difference * Vector3.up;
            }
            //if (hit.distance < Mathf.Clamp(HMD_pd.positionInput.action.ReadValue<Vector3>().y - 0.1f, 0.5f, 10)) 
            //{
            //    float difference = Mathf.Clamp(HMD_pd.positionInput.action.ReadValue<Vector3>().y - hit.distance - 0.1f, 0.5f, 4);
//
            //    this.transform.position += difference * Vector3.up;
            //}
        }
    }

    private bool IsPlayerAnchored()
    {
        foreach(var a in Anchors)
        {
            Debug.Log(a.gameObject.name + " " + a.IsAnchored());
            if (a != null && a.IsAnchored()) return true;
        }
        return false;
    }

    private bool CanPlayerFall()
    {
        return !IsPlayerAnchored();
        //f (!IsPlayerAnchored()) return true;

        //foreach(var a in Anchors)
        //{
        //    if (a.IsAnchored()) return false;
        //}
        //return ShouldContinueFalling();
    }

    private bool ShouldContinueFalling()
    {
        foreach(var a in Anchors)
        {
            if(a.IsAnchored() && a.bCanFallAnchored)
            {
                Debug.Log("La distancia entre el player y el anclaje" + Vector3.Distance(this.transform.position, a.transform.position));

                if (Vector3.Distance(this.transform.position, a.transform.position) >= a.GetMaxDistance) return false;
            }
        }
        return true;
    }
#endregion
}
