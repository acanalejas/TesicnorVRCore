using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using TMPro;
using System.Net.Http;
using Newtonsoft.Json;
using Object = UnityEngine.Object;

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
    [SerializeField] private GameObject panelTime;          // Variable para el panel donde se muestra el tiempo disponible.
    [SerializeField] private TMP_Text txtTime;              // Variable para almacenar el componente text del tiempo.
    [SerializeField] private GameObject buttonChargeTime;   // Array para guardar los botones de bloqueo de cada escena.
    [SerializeField] private GameObject popUp;              // Variable para almacenar el popUp.

    #endregion

    #region Constantes

    const string NoLimitType = "NO_LIMIT_TYPE";             // Tipo de limite indefinido.
    const string TimeType = "TIME_TYPE";                    // Tipo de limite de tiempo.

    #endregion


    #region Backend Parameters

    private List<string> dataToUpload = new List<string>(); // Lista con los datos pendientes de envio.
    
    public int reloadTime = 5;                              // Tiempo que tarda en volver a checkear el tiempo de uso

    [HideInInspector] public bool timeOut = false;          // Se ha acabado el tiempo de espera?

    #endregion

    #endregion
    

    public override void Start()
    {
        base.Start();                                               // Llamamos el método start de BackendGetTimeUse.
        StartCoroutine(StartScene());                         // Llamamos la corrutina que actualizara los datos cada 1seg.
    }

    private IEnumerator StartScene()
    {
        while (true)
        {
            if(PlayerPrefs.GetString("Username") != "")
            {
                string jsonString = PlayerPrefs.GetString(BackendGetTimeUse.BackendDataKey);       // Cargar y procesar los datos guardados del tiempo de uso desde PlayerPrefs

                if (!string.IsNullOrEmpty(jsonString))
                {
                    backendDataTime = JsonUtility.FromJson<BackendTimeData>(jsonString);          // Convertir el JSON a un objeto de la clase BackendDataTime

                    if(panelTime) panelTime.SetActive(backendDataTime.usageType == TimeType);     // Comparamos el tipo de tiempo de uso que tiene el usuario.
                    if (backendDataTime.usageType == TimeType) ValidateTimeLeft();
                }
            }
            else
            {
                if(panelTime) panelTime.SetActive(false);
                if(buttonChargeTime) buttonChargeTime.SetActive(false);
            }

            LoadDataOnDisable();

            yield return new WaitForSeconds(reloadTime);
        }
    }

    public void LoadDataOnDisable()
    {
        LoadPendingData();

        string dataOnDisable = PlayerPrefs.GetString("DataOnDisable", "");      // Guardamos los datos almacenados en el PlayerPrefs "DataOnDisable".

        if (!string.IsNullOrEmpty(dataOnDisable))
        {
            AddList(dataOnDisable);
        }

        if (HasInternetConnection())                                            // Validamos si hay conexión a Internet.
        {
            foreach (string jsString in dataToUpload)
            {
                BackendPostTime data = JsonConvert.DeserializeObject<BackendPostTime>(jsString);  
                // Deserializar cada cadena JSON en un objeto BackendPostTime.
                SendDataToAPI(jsonSerialize(data));                                                 // Enviamos los datos por medio de la API.                                     
            }
        }

        dataToUpload.Clear();       // Limpiamos la lista.
    }

    // Método para agregar un elemento a la lista.
    public void AddList(string data)
    {
        if (dataToUpload.Contains(data)) return;
        
        dataToUpload.Add(data);                                             // Agregar los datos a la lista de datos pendientes para cargar.
        string jsString = JsonConvert.SerializeObject(dataToUpload);        // Convertir la lista de datos a formato JSON.
        PlayerPrefs.SetString("jsonDatos", jsString);                       // Guardar el JSON en PlayerPrefs.
        PlayerPrefs.DeleteKey("DataOnDisable");                             // Limpiar los datos guardados de la experiencia.
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
            StartCoroutine(BlinkText());
        }
        else
        {
            txtTime.color = Color.white;
        }

        if (backendDataTime.timeLeft < 0f)
        {
            Object[] allObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject));   // Buscar todos los objetos desactivados con el tag especificado

            foreach (Object obj in allObjects)
            {
                if (obj is GameObject)
                {
                    GameObject gameObj = (GameObject)obj;
                    if ((gameObj.CompareTag("Block") || gameObj.CompareTag("DemoBlock")) && !gameObj.activeSelf)    // Verificar si el objeto tiene el tag que estás buscando y si no está activo
                    {
                        gameObj.SetActive(true);                                // Activar todos los demás objetos desactivados
                        this.timeOut = true;
                    }
                }
            }
        }
    }

    public async virtual void SendDataToAPI(string jsonData)
    {
        BackendPostTime dataUser = JsonConvert.DeserializeObject<BackendPostTime>(jsonData);

        var cts = new System.Threading.CancellationTokenSource();

        string jsonPostData = JsonUtility.ToJson(dataUser);                              // Convierte el objeto a formato JSON
        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");    // Convierte los datos a un StringContent con tipo de medio "application/json"

        // Realiza la solicitud POST con los datos en el cuerpo
        using (HttpResponseMessage response = await httpClient.PostAsync(urlForTime, content, cts.Token))
        {
            if (response.IsSuccessStatusCode)
            {
                Debug.LogError($"Solicitud Enviada: {response.StatusCode}");
                PlayerPrefs.SetString("jsonDatos", "");
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
        string jsonDatosString = PlayerPrefs.GetString("jsonDatos", "");                                        // Obtener la cadena JSON desde PlayerPrefs

        if (!string.IsNullOrEmpty(jsonDatosString))                                                             // Verificar si la cadena no es nula o vacía
        {
            List<string> data = JsonConvert.DeserializeObject<List<string>>(jsonDatosString);                   // Deserializar la cadena JSON a una lista de cadenas

            if (data != null)                                                                                   // Verificar si la lista no es nula antes de continuar
            {
                foreach (string jsonString in data)                                                             // Ahora puedes manipular la lista de cadenas según tus necesidades
                {
                    BackendPostTime dataBackend = JsonConvert.DeserializeObject<BackendPostTime>(jsonString);   // Deserializar cada cadena JSON en un objeto o manipular según sea necesario
                    dataToUpload.Add(jsonSerialize(dataBackend));                                               // Realizar manipulaciones necesarias con el objeto 'dataBackend' y rellenamos la lista nuevamente.
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

    // Función para verificar la conectividad a Internet.
    public bool HasInternetConnection()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            return true;
        }
        else
        {
            return false;
        }
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
}

[CustomEditor(typeof(BackendLoadData), true)]
[CanEditMultipleObjects]
public class BackendLoadDataEditor : Editor
{
    
}