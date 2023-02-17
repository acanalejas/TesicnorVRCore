using UnityEngine;


public class PC_DEBUG : MonoBehaviour
{
#if UNITY_EDITOR
    #region SINGLETON
    private static PC_DEBUG instance;
    public static PC_DEBUG Instance { get { return instance; } }

    void CheckSingleton()
    {
        if (instance == null) instance = this;
        else Destroy(this.gameObject);
    }
    #endregion
    #region PARAMETERS
    [Header("Se la a usar PC_DEBUG_PLAYER?")]
    public bool usingDebugPlayer = true;

    [Header("El player que se va a usar para testear")]
    [SerializeField] private PC_DEBUG_PLAYER player;
    #endregion

    #region FUNCTIONS
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        if (usingDebugPlayer) SetTestPlayer();
        else player.camera.enabled = false;
    }

    private void Update()
    {
        SetTestPlayer();
    }
    void SetTestPlayer()
    {
        foreach (Camera cam in Camera.allCameras) cam.enabled = false;
        player.camera.enabled = true;
    }
    #endregion
#endif
}
