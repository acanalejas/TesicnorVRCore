using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using TMPro;
using System.Net.Http;
using Newtonsoft.Json;
using Object = UnityEngine.Object;
using System.Globalization;

[System.Serializable]
public class BackendPostTime
{
    // Propiedades para almacenar la información del tiempo.
    public string hwInfo = "Test Oculus";
    public string start;
    public string end;
    public string period;
    public string? clientId;
    public string vrApplicationId;
    public string vrExperienceId;
    public string? userName;

    public BackendPostTime(string hwInfo, string start, string end, string period, string clientId, string vrApplicationId, string vrExperienceId, string userName)
    {
        this.hwInfo = hwInfo;
        this.start = start;
        this.end = end;
        this.period = period;
        this.clientId = clientId;
        this.vrApplicationId = vrApplicationId;
        this.vrExperienceId = vrExperienceId;
        this.userName = userName;
    }
}

[System.Serializable]
public class BackendTimeManager : BackendGetter
{
    #region PARAMETERS

    #region Unity Objects

    // Variables de Unity
    [HideInInspector][SerializeField] private GameObject panelTime;          // Variable para el panel donde se muestra el tiempo disponible.
    [HideInInspector][SerializeField] private TMP_Text txtTime;              // Variable para almacenar el componente text del tiempo.
    [HideInInspector][SerializeField] private GameObject buttonChargeTime;   // Array para guardar los botones de bloqueo de cada escena.
    [HideInInspector][SerializeField] private GameObject popUp;
    [HideInInspector][SerializeField] List<GameObject> BlockGameObjects = new List<GameObject>();
        

    #endregion


    #region Backend Parameters

    private List<string> dataToUpload = new List<string>();                         // Lista con los datos pendientes de envio.

    [HideInInspector] public float reloadTime = 30;                                  // Tiempo que tarda en volver a checkear el tiempo de uso

    [HideInInspector] public bool timeOut = false;                                  // Se ha acabado el tiempo de espera?

    private string inputTime;

    [HideInInspector] public int vrExperienceId = 0;

    private int timeInSeconds;
    private int secondsToSubstractLocal;

    //Spend time parameters

    System.DateTime InitialDate;

    #endregion

    #region Working Parameters

    public enum WorkingMethod { RetrieveTime, SpendTime, CheckUserInfo}                 //Enum for setting the work method of the instanced object
    public WorkingMethod workingMethod = WorkingMethod.RetrieveTime;                    //Current state of work of the instanced object

    #endregion

    #endregion

    private void Awake()
    {
#if UNITY_EDITOR
        PlayerPrefs.SetString("Username", "vr@tesicnor.com");
#endif
        base.Awake();
    }
 
    public override void Start()
    {
        base.Start();                                               // Llamamos el método start de BackendGetTimeUse.
        //StartCoroutine(StartScene());                               // Llamamos la corrutina que actualizara los datos cada 1seg.

        //Lo que se haga aqui dentro variara dependiendo del modo de funcionamiento
        switch (workingMethod)
        {
            case WorkingMethod.RetrieveTime:
                StartCoroutine(nameof(RetrieveTimeUpdate));
                //PlayerPrefs.DeleteKey(BackendConstants.TimeQueueKey);
                break;

            case WorkingMethod.SpendTime:

                InitialDate = System.DateTime.Now;

                //ApplicationEventsManager.Instance.onSceneLoaded.AddListener(SpendTime);
                ApplicationEventsManager.Instance.onActiveSceneChange.AddListener(SpendTime);
                ApplicationEventsManager.Instance.onSceneUnloaded.AddListener(SpendTime);
                ApplicationEventsManager.Instance.onApplicationQuit.AddListener(SpendTime);
                ApplicationEventsManager.Instance.onApplicationFocus.AddListener((bool b) => { SpendTime(); });
                break;
            default: break;
        }
    }

    private IEnumerator StartScene()
    {
        while (true)
        {
            switch (workingMethod)
            {
                case WorkingMethod.RetrieveTime:
                    //RetrieveTimeUpdate();
                    break;

                case WorkingMethod.SpendTime:

                    break;
                    default: break;
            }
            

            yield return new WaitForSeconds(reloadTime);
        }
    }

    /// <summary>
    /// Update usado cuando se tiene que recoger el tiempo de uso
    /// 
    /// TODO mirar si realmente se necesita un update, se podria cambiar todo a eventos o poner un bool para saber si debe o no usar update
    /// </summary>
    private IEnumerator RetrieveTimeUpdate()
    {
        if (PlayerPrefs.GetString("Username") != "")
        {
            string jsonString = PlayerPrefs.GetString(BackendConstants.BackendTimeDataKey);                     // Cargar y procesar los datos guardados del tiempo de uso desde PlayerPrefs

            if (!string.IsNullOrEmpty(jsonString))
            {
                backendDataTime = JsonUtility.FromJson<BackendTimeData>(jsonString);                            // Convertir el JSON a un objeto de la clase BackendDataTime

                if (panelTime) panelTime.SetActive(backendDataTime.usageType == BackendConstants.TimeType);      // Comparamos el tipo de tiempo de uso que tiene el usuario.
                //if (backendDataTime.usageType == BackendConstants.TimeType) ValidateTimeLeft();
            }
        }
        else
        {
            if (panelTime) panelTime.SetActive(false);
            if (buttonChargeTime) buttonChargeTime.SetActive(false);
        }

        LoadDataOnDisable();

        yield return new WaitForSeconds(reloadTime);
        if (backendDataTime.usageType == BackendConstants.TimeType) ValidateTimeLeft();
    }

    /// <summary>
    /// Guarda los datos sobre el tiempo que se ha usado la experincia
    /// Este método es el que se añade a los eventos correspondientes de la aplicación.
    /// Los datos se sustituyen para asegurarnos de que no se envian varios registros que den un resultado erroneo en la resta de tiempo
    /// </summary>
    public void SpendTime()
    {
        System.DateTime currentDate = System.DateTime.Now;

        System.TimeSpan elapsedTime = currentDate - InitialDate;

        timeInSeconds = (int)elapsedTime.TotalSeconds;
        
        PlayerPrefs.SetString(BackendConstants.DataOnDisableKey, DataTime());
    }

    /// <summary>
    /// Carga los datos que esten guardados en memoria para mandarlos al backend
    /// </summary>
    public void LoadDataOnDisable()
    {
        LoadPendingData();

        string dataOnDisable = PlayerPrefs.GetString(BackendConstants.DataOnDisableKey, "");                          // Guardamos los datos almacenados en el PlayerPrefs "DataOnDisable".

        //Checkea si los datos guardados no estan vacios para añadir a la lista
        if (!string.IsNullOrEmpty(dataOnDisable))
        {
            AddList(dataOnDisable);
        }

        if (BackendConstants.bHasInternetConnection)                                                                // Validamos si hay conexión a Internet.
        {
            Debug.Log("Amount of usage records to send is : " + dataToUpload.Count);
            //Envia cada dato guardado al backend
            foreach (string jsString in dataToUpload)
            {
                SendDataToAPI(jsString, BackendConstants.urlForPostTime);                                                 // Enviamos los datos por medio de la API.
            }
            PlayerPrefs.DeleteKey(BackendConstants.TimeQueueKey);
            StartCoroutine(nameof(UpdateTimeData));
        }
        else
        {
            Debug.Log("Queue to save is : " + JsonConvert.SerializeObject(dataToUpload));
            PlayerPrefs.SetString(BackendConstants.TimeQueueKey, JsonConvert.SerializeObject(dataToUpload));
            foreach(string jsString in dataToUpload)
            {
                BackendPostTime timeData = JsonUtility.FromJson<BackendPostTime>(jsString);
                secondsToSubstractLocal += int.Parse(timeData.period);
            }
        }

        //Limpia la lista de datos a mandar
        dataToUpload.Clear();       // Limpiamos la lista.
    }

    IEnumerator UpdateTimeData()
    {
        yield return new WaitForSeconds(1);
        GetBackendTimeData(appCode.ToString());
        yield return new WaitForSeconds(reloadTime);
        ValidateTimeLeft();
        StopCoroutine(nameof(UpdateTimeData));
    }

    // Método para agregar un elemento a la lista.
    public void AddList(string data)
    {
        if (dataToUpload.Contains(data)) return;
        
        dataToUpload.Add(data);                                             // Agregar los datos a la lista de datos pendientes para cargar.
        //string jsString = JsonUtility.ToJson(dataToUpload);                 // Convertir la lista de datos a formato JSON.
        //PlayerPrefs.SetString(BackendConstants.TimeQueueKey, jsString);                       // Guardar el JSON en PlayerPrefs.
        PlayerPrefs.DeleteKey(BackendConstants.DataOnDisableKey);                             // Limpiar los datos guardados de la experiencia.
    }

    // Método para convertir y formatear el tiempo restante en horas:minutos:segundos
    private void FormatTime(float timeInSeconds)
    {
        float _timeInSeconds = timeInSeconds - secondsToSubstractLocal;
        int hours = Mathf.FloorToInt(_timeInSeconds / 3600);
        int minutes = Mathf.FloorToInt((_timeInSeconds % 3600) / 60);
        int seconds = Mathf.FloorToInt(_timeInSeconds % 60);

        if(txtTime) this.txtTime.text = $"{hours:00}:{minutes:00}:{seconds:00}";

        if (_timeInSeconds < 0 && txtTime)
        {
            this.txtTime.text = "00:00:00";
        }
    }

    // Método para convertir el tiempo y actualizar el texto del tiempo.
    private void ValidateTimeLeft()
    {
        GetBackendTimeData(appCode.ToString());
        if(backendDataTime == null) { Debug.LogError("No data from the backend was retrieved for time, it is null"); return; } 
        FormatTime(backendDataTime.timeLeft);  // Uso del método para formatear el tiempo.

        if (backendDataTime.timeLeft < 300)
        {
            txtTime.color = Color.red;
            if(buttonChargeTime) buttonChargeTime.SetActive(true);
            StartCoroutine(nameof(BlinkText));
        }
        else
        {
            StopCoroutine(nameof(BlinkText));
            txtTime.color = Color.white;
        }

        if (backendDataTime.timeLeft < 0f)
        {
            Object[] allObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject));                               // Buscar todos los objetos desactivados con el tag especificado

            foreach (GameObject obj in allObjects)
            {
                GameObject gameObj = obj;
                if (!gameObj.activeSelf && gameObj.tag.Contains("Block"))    // Verificar si el objeto tiene el tag que estás buscando y si no está activo
                {
                    gameObj.SetActive(true);                                                                    // Activar todos los demás objetos desactivados
                    this.timeOut = true;
                }
            }
        }
    }

    

    // Función para cargar los datos pendientes a una lista.
    void LoadPendingData()
    {
        string jsonDatosString = PlayerPrefs.GetString(BackendConstants.TimeQueueKey);                                        // Obtener la cadena JSON desde PlayerPrefs

        if (!string.IsNullOrEmpty(jsonDatosString))                                                             // Verificar si la cadena no es nula o vacía
        {
            Debug.Log(jsonDatosString);
            dataToUpload = JsonConvert.DeserializeObject<List<string>>(jsonDatosString);                   // Deserializar la cadena JSON a una lista de cadenas

            //if (data != null)                                                                                   // Verificar si la lista no es nula antes de continuar
            //{
            //    foreach (string jsonString in data)                                                             // Ahora puedes manipular la lista de cadenas según tus necesidades
            //    {
            //        //TODO quitar la deserializacion completamente innecesaria
            //        //BackendPostTime dataBackend = JsonConvert.DeserializeObject<BackendPostTime>(jsonString);   // Deserializar cada cadena JSON en un objeto o manipular según sea necesario
            //        dataToUpload.Add(jsonString);                                                               // Realizar manipulaciones necesarias con el objeto 'dataBackend' y rellenamos la lista nuevamente.
            //    }
            //}
        }
    }

    IEnumerator BlinkText()
    {
        while (true)
        {
            txtTime.color = new Color(1f, 0f, 0f, 1f);
            yield return new WaitForSeconds(0.5f);
            txtTime.color = new Color(1f, 0f, 0f, 0.2f);
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void ShowPopUp()
    {
        if(popUp) popUp.SetActive(true);
        else Debug.LogError("Pop up not assigned in BackendLoadData Class");
    }

    public void HidePopUp()
    {
        if(popUp) popUp.SetActive(false);
        else Debug.LogError("Pop up not assigned in BackendLoadData Class");
    }

    /// <summary>
    /// Retrieve the needed info about the elapsed time using the experience in a json
    /// </summary>
    /// <returns></returns>
    private string DataTime()
    {
        //Set the base variables for constructing the data object
        var hwInfoaux = SystemInfo.deviceUniqueIdentifier;
        string startDateTime = InitialDate.ToString("yyyy-MM-dd" + "T" + "HH:mm:ss", CultureInfo.InvariantCulture);
        string endDateTime = System.DateTime.Now.ToString("yyyy-MM-dd" + "T" + "HH:mm:ss", CultureInfo.InvariantCulture);
        int duration = (int)timeInSeconds;
        string clientId = "";
        string user = "";

        //Check for the stored data about the user
        if (PlayerPrefs.GetString("Username", "") != "")
        {
            clientId = backendData.client.id.ToString();
            user = PlayerPrefs.GetString("Username");
        }
        //Construct the data object
        BackendPostTime PostTime = new BackendPostTime(hwInfoaux, startDateTime, endDateTime, duration.ToString(), clientId, appCode.ToString(), vrExperienceId.ToString(), user);

        //Parse the object to Json for sending
        string jsonData = JsonUtility.ToJson(PostTime);

        return jsonData;
    }
}
#if UNITY_EDITOR

[CustomEditor(typeof(BackendTimeManager), true)]
//[CanEditMultipleObjects]
public class BackendLoadDataEditor : Editor
{
    BackendTimeManager Target;
    private void OnEnable()
    {
        Target = (BackendTimeManager)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(20);

        GUILayout.Label("Esta clase se usa para la comunicación con el backend de Tesicnor", EditorStyles.boldLabel);

        GUILayout.Space(20);

       //GUILayout.Label("Selecciona el modo de funcionamiento del objecto", EditorStyles.boldLabel);
       //Target.workingMethod = (BackendLoadData.WorkingMethod)EditorGUILayout.EnumPopup(Target.workingMethod);
       //
       //GUILayout.Space(15);

        if(Target.workingMethod == BackendTimeManager.WorkingMethod.RetrieveTime)
        {
            GUILayout.Label("Panel donde se muestra el timepo restante");
            SerializedProperty timePanel = serializedObject.FindProperty("panelTime");
            EditorGUILayout.PropertyField(timePanel);

            GUILayout.Space(15);

            GUILayout.Label("Texto donde se muestra el tiempo restante");
            SerializedProperty timeText = serializedObject.FindProperty("txtTime");
            EditorGUILayout.PropertyField(timeText);

            GUILayout.Space(15);

            GUILayout.Label("Botón de recargar tiempo");
            SerializedProperty buttonChargeTIme = serializedObject.FindProperty("buttonChargeTime");
            EditorGUILayout.PropertyField(buttonChargeTIme);

            GUILayout.Space(15);

            GUILayout.Label("Pop up para recargar el tiempo");
            SerializedProperty timePopUp = serializedObject.FindProperty("popUp");
            EditorGUILayout.PropertyField(timePopUp);

            GUILayout.Space(15);

            GUILayout.Label("Lista de objectos bloqueantes para activar cuando se acaba el tiempo de uso");
            SerializedProperty blockingObjects = serializedObject.FindProperty("BlockGameObjects");
            EditorGUILayout.PropertyField(blockingObjects);

            GUILayout.Space(15);
        }

        if(Target.workingMethod == BackendTimeManager.WorkingMethod.SpendTime)
        {
            GUILayout.Label("Periodo de tiempo entre calculo de tiempo restante");
            Target.reloadTime = EditorGUILayout.FloatField(Target.reloadTime, EditorStyles.miniTextField);

            GUILayout.Space(15);

            GUILayout.Label("Id correspondiente al backend de la experiencia actual");
            Target.vrExperienceId = EditorGUILayout.IntField(Target.vrExperienceId, EditorStyles.miniTextField);
        }

        #region Unity Parameters
        
        #endregion

        serializedObject.ApplyModifiedProperties();
    }

}
#endif