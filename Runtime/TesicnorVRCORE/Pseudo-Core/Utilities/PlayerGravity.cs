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
        BodyColl.radius = 0.3f;

        BodyColl.isTrigger = true;
    }

    void TransferVelocity()
    {

    }

    private void Start()
    {
        if (!IsGravtyActive) return;
        SetupBodyGO();
        StartCoroutine(nameof(CustomUpdate));

        
    }

    WaitForEndOfFrame Frame = new WaitForEndOfFrame();
    IEnumerator CustomUpdate()
    {
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
                    Vector3 velocity = Physics.gravity * timer;
                    this.transform.position += (velocity * Time.deltaTime);
                    timer += Time.deltaTime;
                }
                else if(timer != 0)
                {
                    if(timer >= 0.025f) OnFallEnd.Invoke(IsPlayerAnchored());
                    timer = 0;
                }
                if (HMD_pd && this.BodyColl) this.BodyColl.height = HMD_pd.positionInput.action.ReadValue<Vector3>().y + 0.2f;
            }
            else if(timer != 0) { timer = 0;}

            yield return Frame;
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
        if (!IsPlayerAnchored()) return true;

        foreach(var a in Anchors)
        {
            if (a.IsAnchored() && !a.bCanFallAnchored) return false;
        }
        return ShouldContinueFalling();
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
