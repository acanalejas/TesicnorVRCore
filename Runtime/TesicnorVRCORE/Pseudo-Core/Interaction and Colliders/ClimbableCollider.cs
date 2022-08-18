using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbableCollider : VRCollider
{
    #region PARAMETERS
    
    
    Transform player;
    [Header("=========== CLIMBABLE PARAMETERS ============")]
    [Space(10)]
    [Tooltip("Altura del suelo")]
    [Header("La altura mínima a la que puede bajar el jugador")]
    [SerializeField] float minHeight = 0;

    [Header("La altura máxima a la que puede subir el jugador")]
    [SerializeField] float maxHeight = 1;

    [Header("El offset en la altura")]
    [SerializeField] float heightOffset = 0;

    [Header("El threshold para saber si se está arriba")]
    public float threshold_top = 0.3f;

    [Header("El threshold para saber si se está abajo")]
    public float threshold_bottom = 0.4f;

    [Header("Al soltar el jugador se cae?")]
    public bool playerFalls = false;

    [HideInInspector] public bool isPlayerAtTop = false;
    [HideInInspector] public bool isPlayerAtBottom = false;

    private bool canBeClimbed = true;

    /// <summary>
    /// Clase que se añade al personaje cuando se quiere que este caiga.
    /// Asignarle el HeighOffset, y los demás parámetros que puedan ser necesarios
    /// </summary>
    public class Fall : MonoBehaviour
    {
        public float heightOffset;
        public Transform player;
        public float gForce = -9.814f;
        public float mass = 60;
        public float time = 0;
        public Vector3 V0 = Vector3.zero;
        public Vector3 P0 = Vector3.zero;
        public ClimbableCollider escalera;

        private void Start()
        {
            P0 = gameObject.transform.position;
        }
        private void FixedUpdate()
        {
            time += Time.fixedDeltaTime;
            this.transform.position = P();
            if (reachedFloor()) Destroy(this);
        }

        private Vector3 Fg()
        {
            return gForce* mass * Vector3.down;
        }

        private Vector3 V()
        {
            return V0 + gForce * Vector3.down * time;
        }

        private Vector3 P()
        {
            return P0 + V() * time;
        }

        private bool reachedFloor()
        {
            return (player.position.y > heightOffset - 0.1f && player.position.y < heightOffset + 0.1f) || escalera.isGrabbed();
        }

        public void SetValues(ClimbableCollider _escalera, float _height, Transform _player)
        {
            escalera = _escalera;
            heightOffset = _height;
            player = _player;
        }
    }
    #endregion

    #region FUNCTIONS

    public override void Awake()
    {
        base.Awake();
        attachmentMode = AttachmentMode.None;
    }
    public override void Grab(GrippingHand hand)
    {
        lastFrameHeight = 0;
        StopCoroutine("attach");
        base.Grab(hand);

        player = hand.player;

        StartCoroutine("attach");
    }

    float lastFrameHeight = 0;
    WaitForEndOfFrame frame = new WaitForEndOfFrame();
    private IEnumerator attach()
    {
        while (true)
        {
            if(lastFrameHeight != 0)
            {
                float difference = grippingHand.transform.position.y - lastFrameHeight;
                if (!IsPlayerReallyAtTop() && difference < 0 && canBeClimbed) player.position += new Vector3(0, -difference, 0);
                else if (!IsPlayerReallyAtBotton() && difference > 0) player.position += new Vector3(0, -difference, 0);
                //player.position = new Vector3(player.position.x, Mathf.Clamp(player.position.y, minHeight, maxHeight), player.position.z);

                IsPlayerAtBottom();
                IsPlayerAtTop();
            }
            lastFrameHeight = grippingHand.transform.position.y;
            yield return frame;
        }
    }

    public override void Release()
    {
        base.Release();
        lastFrameHeight = 0;
        StopCoroutine("attach");
    }

    /// <summary>
    /// Esta el jugador en lo alto de la escalera?
    /// </summary>
    /// <returns></returns>
    public bool IsPlayerAtTop()
    {
        if(player) isPlayerAtTop = player.position.y >= ((maxHeight + heightOffset) - threshold_top) && (player.position.y <= (maxHeight + heightOffset) + threshold_top);
        return isPlayerAtTop;
    }
    /// <summary>
    /// Esta el jugador en el fondo de la escalera?
    /// </summary>
    /// <returns></returns>
    public bool IsPlayerAtBottom()
    {
        if(player) isPlayerAtBottom = player.position.y >= ((minHeight + heightOffset) - threshold_bottom) && player.position.y <= ((minHeight + heightOffset) + threshold_bottom);
        return isPlayerAtBottom;
    }

    /// <summary>
    /// Esta el jugador realmente al fondo de la escalera?
    /// </summary>
    /// <returns></returns>
    private bool IsPlayerReallyAtBotton()
    {
        if (player) return player.position.y >= ((minHeight + heightOffset) - 0.05f) && player.position.y <= ((minHeight + heightOffset) + 0.05f);

        return false;
    }

    /// <summary>
    /// Esta el jugador realmente en la cima de la escalera?
    /// </summary>
    /// <returns></returns>
    private bool IsPlayerReallyAtTop()
    {
        if (player) return player.position.y >= ((maxHeight + heightOffset) - 0.05f) && player.position.y <= ((maxHeight + heightOffset) + 0.05f);

        return false;
    }

    /// <summary>
    /// Le asigna el valor al bool canBeClimbed
    /// </summary>
    /// <param name="_value"></param>
    public void SetCanBeClimbed(bool _value)
    {
        canBeClimbed = _value;
    }

    /// <summary>
    /// Devuelve el bool canBeClimbed
    /// </summary>
    /// <returns></returns>
    public bool GetCanBeClimbed()
    {
        return canBeClimbed;
    }
    
     /// <summary>
    /// Devuelve la altura actual a la que se encuentra el jugador
    /// </summary>
    /// <returns></returns>
    public float GetCurrentHeight()
    {
        return player.position.y - heightOffset;
    }
    #endregion
}
