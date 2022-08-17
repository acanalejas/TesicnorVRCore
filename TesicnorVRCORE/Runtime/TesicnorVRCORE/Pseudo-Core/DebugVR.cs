using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Clase que sirve para sacar por pantalla mensajes al testear en VR
/// </summary>
[RequireComponent(typeof(Canvas))]
public class DebugVR : MonoBehaviour
{
    #region SINGLETON
    private static DebugVR instance;
    public static DebugVR Instance { get { return instance; } }

    private void CheckSingleton()
    {
        if (instance == null) instance = this;
        else Destroy(this.gameObject);
    }
    #endregion

    #region PARAMETERS

    [RequireComponent(typeof(Text))]
    public class message : MonoBehaviour
    {
        public float lifeTime = 2;

        public string text = "";

        private void Start()
        {
            GetComponent<Text>().rectTransform.sizeDelta = new Vector2(400, 150);
            GetComponent<Text>().fontSize = 25;
            GetComponent<Text>().color = Color.green;
        }
        public void OnEnable()
        {
            if(lifeTime > 0) StartCoroutine("countdown");
        }
        private IEnumerator countdown()
        {
            yield return new WaitForSeconds(lifeTime);
            this.enabled = false;
            this.text = "";
            StopCoroutine("countdown");
        }

        public void SetActive(bool _value, float _lifeTime, string _text, Font _font = null)
        {
            this.gameObject.SetActive(_value);
            lifeTime = _lifeTime;
            text = _text;
            GetComponent<Text>().text = text;
            GetComponent<Text>().font = _font;

            this.enabled = true;
        }
        public void SetActive(bool _value)
        {
            this.gameObject.SetActive(_value);
            this.enabled = _value;
        }
    }
    /// <summary>
    /// Pool de textos que usaremos para enseñar el texto por pantalla, tratando como mensaje separado cada texto
    /// </summary>
    private List<message> textPool = new List<message>();

    /// <summary>
    /// Lista de los textos que tenemos actualmente activos
    /// </summary>
    private List<message> activeTexts = new List<message>();

    [Header("El GameObject donde que actúa como padre de los mensajes")]
    [SerializeField] Transform DebugParent;

    [Header("Distancia en vertical entre los mensajes")]
    [SerializeField] float verticalDistance = 50;

    [Header("La fuente que se va a usar para los mensajes")]
    [SerializeField] Font messageFont;

    [Header("Es una compilación de desarrollo?")]
    [SerializeField] bool isDeveloperBuild = true;

    [Header("Material que se le va a asignar al texto")]
    [SerializeField] Material textMaterial;
    
    #endregion

    #region FUNCTIONS
    private void Awake()
    {
        CheckSingleton();
        FillPool(5);
    }

    private void Start()
    {
        StartCoroutine("update");
    }
    WaitForEndOfFrame frame = new WaitForEndOfFrame();
    private IEnumerator update()
    {
        while (true)
        {
            yield return frame;
            isAnyoneDisabled();
        }
    }

    #region Object Pooling

    /// <summary>
    /// Crea un objeto y lo setea para añadirlo a la pool
    /// </summary>
    /// <returns></returns>
    message PoolObject()
    {
        GameObject createdObject = null;
        if (DebugParent) createdObject = GameObject.Instantiate(new GameObject(), DebugParent);
        else createdObject = GameObject.Instantiate(new GameObject(), this.transform);

        message AddedMessage = createdObject.AddComponent<message>();

        return AddedMessage;
    }

    /// <summary>
    /// Rellena la pool con la cantidad de objetos que le indiquemos
    /// </summary>
    /// <param name="count">Cantidad de objetos a crear</param>
    private void FillPool(int count)
    {
        for(int i = 0; i < count; i++)
        {
            textPool.Add(PoolObject());
            textPool[i].SetActive(false);
        }
    }

    /// <summary>
    /// Recoge el primer objeto disponible que encuentre.
    /// En caso de estar vacia la lista, añade nuevos objetos y recoge el primero que puede.
    /// </summary>
    /// <param name="lifeTime">Tiempo que dura activo 0 -> Siempre</param>
    /// <param name="_text">Texto que se muestra en pantalla</param>
    /// <returns></returns>
    private message GetFromPool(float lifeTime = 2, string _text = "")
    {
        message result = null;

        if (textPool.Count == 0) FillPool(5);

        result = textPool[textPool.Count - 1];
        result.SetActive(true, lifeTime, _text, messageFont);
        textPool.Remove(result);
        activeTexts.Add(result);

        SetPositions();

        return result;
    }

    /// <summary>
    /// Devuelve el objeto que le pasemos a la pool
    /// </summary>
    /// <param name="_message">objeto a devolver</param>
    public void ReturnToPool(message _message)
    {
        _message.SetActive(false);
        if(activeTexts.Contains(_message)) activeTexts.Remove(_message);
        textPool.Add(_message);

        SetPositions();
    }

    /// <summary>
    /// Comprueba en la lista de objetos activos si hay alguno que devolver a la pool de objetos
    /// </summary>
    /// <returns></returns>
    private bool isAnyoneDisabled()
    {
        foreach (message _message in activeTexts)
        {
            if (_message.enabled == false) { ReturnToPool(_message); return true; }
        }
        return false;
    }
    #endregion

    #region Reposition
    /// <summary>
    /// Setea las posiciones de los mensajes en la pantalla
    /// </summary>
    public void SetPositions()
    {
        if (activeTexts.Count == 0) return;

        int h = 0;
        for(int i = activeTexts.Count - 1; i >= 0; i--)
        {
            if (DebugParent) activeTexts[i].gameObject.transform.position = DebugParent.position;
            else activeTexts[i].gameObject.transform.position = this.transform.position;

            activeTexts[i].gameObject.transform.position -= new Vector3(0, h * verticalDistance, 0);
            h++;
        }
    }
    #endregion

    /// <summary>
    /// Funcion pública usada para enseñar un mensaje en pantalla usando el casco VR
    /// </summary>
    /// <param name="text">El mensaje que se va a enseñar</param>
    /// <param name="lifeTime">El tiempo que dura el mensaje en pantalla</param>
    public void Log(string text, float lifeTime = 4)
    {
        if(isDeveloperBuild)GetFromPool(lifeTime, text);
    }
    #endregion
}
