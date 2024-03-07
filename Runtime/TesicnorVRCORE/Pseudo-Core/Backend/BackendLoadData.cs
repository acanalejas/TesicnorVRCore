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
    public string hwInfo;
    public string start;
    public string end;
    public string period;
    public string? clientId;
    public string vrApplicationId;
    public string vrExperienceId;
    public string? userName;
}

public class BackendLoadData : BackendGetter
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

    [HideInInspector] public float reloadTime = 5;                                  // Tiempo que tarda en volver a checkear el tiempo de uso

    [HideInInspector] public bool timeOut = false;                                  // Se ha acabado el tiempo de espera?

    private string inputTime;

    [HideInInspector] public int vrExperienceId = 0;

    private int timeInSeconds;

    //Spend time parameters

    System.DateTime InitialDate;

    #endregion

    #region Working Parameters

    public enum WorkingMethod { RetrieveTime, SpendTime, CheckUserInfo}                 //Enum for setting the work method of the instanced object
    [HideInInspector] public WorkingMethod workingMethod = WorkingMethod.RetrieveTime;  //Current state of work of the instanced object

    #endregion

    #endregion


    public override void Start()
    {
        base.Start();                                               // Llamamos el método start de BackendGetTimeUse.
        StartCoroutine(StartScene());                               // Llamamos la corrutina que actualizara los datos cada 1seg.

        switch (workingMethod)
        {
            case WorkingMethod.RetrieveTime:

                break;

            case WorkingMethod.SpendTime:

                InitialDate = System.DateTime.Now;

                ApplicationEventsManager.Instance.onSceneLoaded.AddListener(SpendTime);
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
                    RetrieveTimeUpdate();
                    break;

                case WorkingMethod.SpendTime:

                    break;
                    default: break;
            }
            

            yield return new WaitForSeconds(reloadTime);
        }
    }

    private void RetrieveTimeUpdate()
    {
        if (PlayerPrefs.GetString("Username") != "")
        {
            string jsonString = PlayerPrefs.GetString(BackendConstants.BackendTimeDataKey);                     // Cargar y procesar los datos guardados del tiempo de uso desde PlayerPrefs

            if (!string.IsNullOrEmpty(jsonString))
            {
                backendDataTime = JsonUtility.FromJson<BackendTimeData>(jsonString);                            // Convertir el JSON a un objeto de la clase BackendDataTime

                if (panelTime) panelTime.SetActive(backendDataTime.usageType == BackendConstants.TimeType);      // Comparamos el tipo de tiempo de uso que tiene el usuario.
                if (backendDataTime.usageType == BackendConstants.TimeType) ValidateTimeLeft();
            }
        }
        else
        {
            if (panelTime) panelTime.SetActive(false);
            if (buttonChargeTime) buttonChargeTime.SetActive(false);
        }

        LoadDataOnDisable();
    }

    private void SpendTime()
    {
        System.DateTime currentDate = System.DateTime.Now;

        System.TimeSpan elapsedTime = currentDate - InitialDate;

        timeInSeconds = (int)elapsedTime.TotalSeconds;

        PlayerPrefs.SetString(BackendConstants.DataOnDisableKey, DataTime());
    }

    public void LoadDataOnDisable()
    {
        LoadPendingData();

        string dataOnDisable = PlayerPrefs.GetString(BackendConstants.DataOnDisableKey, "");                          // Guardamos los datos almacenados en el PlayerPrefs "DataOnDisable".

        if (!string.IsNullOrEmpty(dataOnDisable))
        {
            AddList(dataOnDisable);
        }

        if (BackendConstants.bHasInternetConnection)                                                                // Validamos si hay conexión a Internet.
        {
            foreach (string jsString in dataToUpload)
            {
                //TODO quitar la deserializacion que luego se vuelve a serialiar inmediaamente sin sentido alguno
                //BackendPostTime data = JsonConvert.DeserializeObject<BackendPostTime>(jsString);  
                //Deserializar cada cadena JSON en un objeto BackendPostTime.
                SendDataToAPI(/*jsonSerialize(data)*/ jsString);                                                 // Enviamos los datos por medio de la API.                                     
            }
        }

        dataToUpload.Clear();       // Limpiamos la lista.
    }

    // Método para agregar un elemento a la lista.
    public void AddList(string data)
    {
        if (dataToUpload.Contains(data)) return;
        
        dataToUpload.Add(data);                                             // Agregar los datos a la lista de datos pendientes para cargar.
        string jsString = JsonUtility.ToJson(dataToUpload);                 // Convertir la lista de datos a formato JSON.
        PlayerPrefs.SetString(BackendConstants.TimeQueueKey, jsString);                       // Guardar el JSON en PlayerPrefs.
        PlayerPrefs.DeleteKey(BackendConstants.DataOnDisableKey);                             // Limpiar los datos guardados de la experiencia.
    }

    // Método para convertir y formatear el tiempo restante en horas:minutos:segundos
    private void FormatTime(float timeInSeconds)
    {
        int hours = Mathf.FloorToInt(timeInSeconds / 3600);
        int minutes = Mathf.FloorToInt((timeInSeconds % 3600) / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);

        if(txtTime) this.txtTime.text = $"{hours:00}:{minutes:00}:{seconds:00}";

        if (timeInSeconds < 0 && txtTime)
        {
            this.txtTime.text = "00:00:00";
        }
    }

    // Método para convertir el tiempo y actualizar el texto del tiempo.
    private void ValidateTimeLeft()
    {
        FormatTime(backendDataTime.timeLeft);  // Uso del método para formatear el tiempo.

        if (backendDataTime.timeLeft < 300)
        {
            txtTime.color = Color.red;
            if(buttonChargeTime) buttonChargeTime.SetActive(true);
            StartCoroutine(nameof(BlinkText));
        }
        else
        {
            txtTime.color = Color.white;
        }

        if (backendDataTime.timeLeft < 0f)
        {
            Object[] allObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject));                               // Buscar todos los objetos desactivados con el tag especificado

            foreach (GameObject obj in BlockGameObjects)
            {
                GameObject gameObj = obj;
                if (!gameObj.activeSelf)    // Verificar si el objeto tiene el tag que estás buscando y si no está activo
                {
                    gameObj.SetActive(true);                                                                    // Activar todos los demás objetos desactivados
                    this.timeOut = true;
                }
            }
        }
    }

    public async virtual void SendDataToAPI(string jsonData)
    {

        //TODO quitar deserializacion completamente inutil
        //BackendPostTime dataUser = JsonConvert.DeserializeObject<BackendPostTime>(jsonData);

        var cts = new System.Threading.CancellationTokenSource();

        //string jsonPostData = JsonUtility.ToJson(dataUser);                               // Convierte el objeto a formato JSON
        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");       // Convierte los datos a un StringContent con tipo de medio "application/json"

        // Realiza la solicitud POST con los datos en el cuerpo
        using (HttpResponseMessage response = await httpClient.PostAsync(BackendConstants.urlForTime, content, cts.Token))
        {
            if (response.IsSuccessStatusCode)
            {
                Debug.LogError($"Solicitud Enviada: {response.StatusCode}");
                PlayerPrefs.SetString(BackendConstants.TimeQueueKey, "");
            }
            else
            {
                Debug.LogError($"Error en la solicitud: {response.StatusCode}");
            }
        }
    }

    // Función para cargar los datos pendientes a una lista.
    void LoadPendingData()
    {
        string jsonDatosString = PlayerPrefs.GetString(BackendConstants.TimeQueueKey, "");                                        // Obtener la cadena JSON desde PlayerPrefs

        if (!string.IsNullOrEmpty(jsonDatosString))                                                             // Verificar si la cadena no es nula o vacía
        {
            List<string> data = JsonUtility.FromJson<List<string>>(jsonDatosString);//JsonConvert.DeserializeObject<List<string>>(jsonDatosString);                   // Deserializar la cadena JSON a una lista de cadenas

            if (data != null)                                                                                   // Verificar si la lista no es nula antes de continuar
            {
                foreach (string jsonString in data)                                                             // Ahora puedes manipular la lista de cadenas según tus necesidades
                {
                    //TODO quitar la deserializacion completamente innecesaria
                    BackendPostTime dataBackend = JsonConvert.DeserializeObject<BackendPostTime>(jsonString);   // Deserializar cada cadena JSON en un objeto o manipular según sea necesario
                    dataToUpload.Add(jsonString);                                                               // Realizar manipulaciones necesarias con el objeto 'dataBackend' y rellenamos la lista nuevamente.
                }
            }
        }
    }

    public string jsonSerialize(BackendPostTime data)
    {
        string jsonData = "{\"hwInfo\": \"" + data.hwInfo + "\", " +
                            "\"start\": \"" + data.start + "\", " +
                            "\"end\": \"" + data.end + "\", " +
                            "\"period\": " + data.period + ", " +
                            "\"clientId\": \"" + data.clientId + "\", " +
                            "\"vrApplicationId\": " + data.vrApplicationId + ", " +
                            "\"vrExperienceId\": " + data.vrExperienceId + ", " +
                            "\"userName\": \"" + data.userName + "\"}";

        return jsonData;
    }

    IEnumerator BlinkText()
    {
        txtTime.color = new Color(1f, 0f, 0f, 1f);
        yield return new WaitForSeconds(0.5f);
        txtTime.color = new Color(1f, 0f, 0f, 0.2f);
        yield return new WaitForSeconds(0.5f);
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

    private string DataTime()
    {
        string jsonData;

        var hwInfoaux = SystemInfo.deviceUniqueIdentifier;
        string startDateTime = inputTime;
        string endDateTime = System.DateTime.Now.ToString("yyyy-MM-dd" + "T" + "HH:mm:ss", CultureInfo.InvariantCulture);
        int duration = (int)timeInSeconds;
        string clientId;
        string user;

        if (PlayerPrefs.GetString("Username", "") != "")
        {
            clientId = backendDataTime.clientId;
            user = PlayerPrefs.GetString("Username");

            // Corregido el formato JSON y manejo de tipos de datos
            jsonData = "{\"hwInfo\": \"" + hwInfoaux.ToString() + "\", " +
                              "\"start\": \"" + startDateTime + "\", " +
                              "\"end\": \"" + endDateTime + "\", " +
                              "\"period\": " + duration + ", " +
                              "\"clientId\": " + clientId + ", " +
                              "\"vrApplicationId\": " + appCode + ", " +
                              "\"vrExperienceId\": " + vrExperienceId + ", " +
                              "\"userName\": \"" + user + "\"}";
        }
        else
        {
            // Corregido el formato JSON y manejo de tipos de datos
            jsonData = "{\"hwInfo\": \"" + hwInfoaux.ToString() + "\", " +
                              "\"start\": \"" + startDateTime + "\", " +
                              "\"end\": \"" + endDateTime + "\", " +
                              "\"period\": " + duration + ", " +
                              "\"clientId\": \"" + null + "\", " +
                              "\"vrApplicationId\": " + appCode + ", " +
                              "\"vrExperienceId\": " + vrExperienceId + ", " +
                              "\"userName\": \"" + "\"}";
        }

        return jsonData;
    }
}

[CustomEditor(typeof(BackendLoadData), true)]
[CanEditMultipleObjects]
public class BackendLoadDataEditor : Editor
{
    BackendLoadData Target;
    private void OnEnable()
    {
        Target = (BackendLoadData)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(20);

        GUILayout.Label("Esta clase se usa para la comunicación con el backend de Tesicnor", EditorStyles.boldLabel);

        GUILayout.Space(20);

        GUILayout.Label("Selecciona el modo de funcionamiento del objecto", EditorStyles.boldLabel);
        Target.workingMethod = (BackendLoadData.WorkingMethod)EditorGUILayout.EnumPopup(Target.workingMethod);

        GUILayout.Space(15);

        if(Target.workingMethod == BackendLoadData.WorkingMethod.RetrieveTime)
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

        if(Target.workingMethod == BackendLoadData.WorkingMethod.SpendTime)
        {
            GUILayout.Label("Periodo de tiempo entre calculo de tiempo restante");
            Target.reloadTime = EditorGUILayout.FloatField(Target.reloadTime, EditorStyles.miniTextField);
        }

        #region Unity Parameters
        
        #endregion
    }
}