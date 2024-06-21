using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Events;

[RequireComponent(typeof(VideoPlayer))]
public class TutorialVideo : MonoBehaviour
{
    #region PARAMETERS
    /// <summary>
    /// Clip de video para enseñar
    /// </summary>
    [Header("Clip de video para enseñar")]
    public VideoClip clip;

    [Header("El GameObject que contiene todo el reproductor, el padre")]
    [SerializeField] private GameObject Parent;

    [Header("Lista de objetos a desactivar en el start")]
    [SerializeField] private GameObject[] ObjectsToDisable_Start;

    [Header("Lista de objetos a activar en el start")]
    [SerializeField] private GameObject[] ObjectsToEnable_Start;

    [Header("Lista de objetos a desactivar al terminar")]
    [SerializeField] private GameObject[] ObjectsToDisable_End;

    [Header("Lista de objetos a activar al terminar")]
    [SerializeField] private GameObject[] ObjectsToEnable_End;

    /// <summary>
    /// Evento que se lanza cuando se le da a play
    /// </summary>
    [Header("Evento que se lanza cuando se le da a play")]
    public UnityEvent OnPlay;

    /// <summary>
    /// Evento que se lanza cuando se pausa el video
    /// </summary>
    [Header("Evento que se lanza cuando se pausa el video")]
    public UnityEvent OnPause;

    /// <summary>
    /// Evento que se usa cuando se termina de reproducir el video
    /// </summary>
    [Header("Evento que se usa cuando se termina de reproducir el video")]
    public UnityEvent OnFinish;

    /// <summary>
    /// Eventoq que se lanza cuando se cierra el reproductor
    /// </summary>
    [Header("Evento que se lanza cuando se cierra el reproductor")]
    public UnityEvent OnClose;

    /// <summary>
    /// Evento que se usa cuando se enciende el reproductor
    /// </summary>
    [Header("Evento que se usa cuando se enciende el reproductor")]
    public UnityEvent OnBegin;

    [Header("Deberia seguir al jugador?")]
    [SerializeField] private bool bFollowPlayer = true;

    [Header("Deberia preguntar antes de reproducir el video?")]
    [SerializeField] private bool bShouldAskBefore = true;

    [Header("Deberia empezar a reproducir en el start?")]
    [SerializeField] private bool bShouldPlayAtStart = false;

    [Header("La pantalla en la que se pregunta")]
    [SerializeField] private GameObject AskScreen_GO;

    [Header("El GameObject que nos indica la posicion del player para ver el video")]
    [SerializeField] private GameObject PlayerHolder;

    [Header("El GameObject que nos indica a donde devolver el player")]
    [SerializeField] private GameObject PlayerReturnPoint;

    [Header("El GameObject del player en si")]
    [SerializeField] private GameObject PlayerGO;

    [Header("La velocidad de interpolación al moverse")]
    [SerializeField] private float interpSpeed = 1;

    [Header("La escala normal de la pantalla")]
    [SerializeField] private Vector3 normalScale = Vector3.one;

    [Header("La escala cuando se reproduce el video")]
    [SerializeField] private Vector3 playingScale = Vector3.one;

    [Header("La distancia a la que se situa el reproductor de normal")]
    [SerializeField] private float normalDistance = 0.5f;

    [Header("La distancia a la que se situa el reproductor en play")]
    [SerializeField] private float playingDistance = 1f;

    /// <summary>
    /// Reproductor de video
    /// </summary>
    protected VideoPlayer videoPlayer;

    protected Vector3 initialPlayerPosition;

    [Header("El GO de la cabeza del player")]
    [SerializeField] private GameObject Head_GO;
    #endregion

    #region METHODS
    #region Buttons
    public void ToggleVideo()
    {
        if (!videoPlayer) return;
        if (videoPlayer.clip != null) videoPlayer.clip = clip;

        if (!videoPlayer.isPlaying || videoPlayer.isPaused) { videoPlayer.Play(); OnPlay.Invoke(); }
        else if (videoPlayer.isPlaying) { videoPlayer.Pause(); OnPause.Invoke(); }
    }

    public void Close()
    {
        Parent.SetActive(false);
        OnClose.Invoke();
    }

    public void Open()
    {
        Parent.SetActive(true);
        OnBegin.Invoke();
    }

    public void Rewind()
    {
        if (!videoPlayer || !videoPlayer.clip) return;

        videoPlayer.time = 0;
    }
    #endregion
    #region Unity Methods
    private void Awake()
    {
        if(!videoPlayer) videoPlayer = GetComponent<VideoPlayer>();
        if (!Parent) Parent = this.transform.parent.gameObject;
    }
    private void Start()
    {
        if (bFollowPlayer) StartCoroutine(nameof(FollowPlayerHead));
        if (bShouldAskBefore) EnableAskScreen();
        if (bShouldPlayAtStart) ToggleVideo();
    }
    #endregion

    WaitForEndOfFrame Frame = new WaitForEndOfFrame();
    IEnumerator FollowPlayerHead()
    {
        if (!Head_GO) Head_GO = Camera.main.gameObject;

        while (true)
        {
            Vector3 direction = Head_GO.transform.forward.normalized;

            Vector3 position = Head_GO.transform.position + direction * GetCurrentDistance();

            Parent.transform.position = Vector3.Lerp(Parent.transform.position, position, Time.deltaTime * interpSpeed);

            Parent.transform.LookAt(position + direction*GetCurrentDistance());

            Parent.transform.localScale = Vector3.Lerp(Parent.transform.localScale, GetCurrentScale(), Time.deltaTime * interpSpeed);

            yield return Frame;
        }
    }

    private float GetCurrentDistance()
    {
        return videoPlayer.isPlaying ? playingDistance : normalDistance;
    }

    private Vector3 GetCurrentScale()
    {
        return videoPlayer.isPlaying ? playingScale : normalScale;
    }

    public void MovePlayerToVideo()
    {
        if (!PlayerGO) return;

        initialPlayerPosition = PlayerGO.transform.position;
        Vector3 playerPosition = Vector3.zero;
        if (PlayerHolder) playerPosition = PlayerHolder.transform.position;
        else playerPosition = this.transform.position + new Vector3(0, 0, playingDistance);

        PlayerGO.transform.position = playerPosition;
    }

    public void ReturnPlayerToInitialPosition()
    {
        if (!PlayerGO || initialPlayerPosition == Vector3.zero) return;

        PlayerGO.transform.position = PlayerReturnPoint != null ? PlayerReturnPoint.transform.position : initialPlayerPosition;
    }

    public void EnableAskScreen()
    {
        AskScreen_GO.SetActive(true);
    }
    public void DisableAskScreen()
    {
        AskScreen_GO.SetActive(false);
    }

    public void ChangeVideoClip(VideoClip clip)
    {
        if (!videoPlayer) return;
        if (!clip) return;

        videoPlayer.clip = clip;
        this.clip = clip;
    }
    #endregion
}
