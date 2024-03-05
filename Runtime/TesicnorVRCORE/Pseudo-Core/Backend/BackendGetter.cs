using System;
using UnityEngine;
using System.Net.Http;
using System.Net;
using TMPro;

[System.Serializable]
public class VRExperience
{
    public int id;
    public string code;
    public string name;
    public bool visibleForAll;
    public string description;
}
[System.Serializable]
public class VRProperty
{
    public long id;
    public string propertyName;
    public string propertyValue;
}


[System.Serializable]
public class BackendData
{
    [System.Serializable]
    public class VRClient
    {
        public int id;
        public string name;
        public string code;
        public string description;
        public string vrusers;
    }

    public int id;
    public string name;
    public string description;
    public VRProperty[] vRProperties;
    public VRExperience[] vrExperiences;
    public VRClient client;
    public VRProperty[] vrproperties;
}

public class BackendTimeData
{
    public string usageType;
    public float timeLeft;
    public string updated;
    public string clientId;
}

public enum BackendDataType {UserData, TimeData}

public class BackendGetter : MonoBehaviour
{
    #region PARAMETERS
    protected HttpClient httpClient;
    public static  BackendData backendData = new BackendData();
    public static BackendTimeData backendDataTime = new BackendTimeData();
    public TextMeshProUGUI username;
    string username_str;
    public static int appCode = 1;
    #endregion

    #region FUNCTIONS

    public virtual void Awake()
    {
        httpClient = new HttpClient();
        GetBackendData(appCode.ToString());
    }
    public virtual void Start()
    {
        backendData = JsonUtility.FromJson<BackendData>(PlayerPrefs.GetString(BackendConstants.BackendDataKey));
        backendDataTime = JsonUtility.FromJson<BackendTimeData>(PlayerPrefs.GetString(BackendConstants.BackendTimeDataKey));
    }

    #region Connecting and getting the data
    /// <summary>
    /// Realiza la petici�n al backend para recoger los datos
    /// </summary>
    /// <param name="appCode">C�digo de la aplicaci�n en la que estemos</param>
    public async virtual void GetBackendData(string appCode)
    {
        //Recomendable, no se por que pero creandole el source para asignar el token funciona mejor, mierdas de .net
        var cts = new System.Threading.CancellationTokenSource();

        username_str = PlayerPrefs.GetString("Username");

        using (HttpRequestMessage hrm = new HttpRequestMessage(HttpMethod.Get, BackendConstants.urlNoParams + "applicationId=" + appCode + "&" + "userName=" + username_str))
        {
            using (HttpResponseMessage response = await httpClient.SendAsync(hrm, cts.Token))
            {
                if (response.IsSuccessStatusCode) 
                BackendDataFromResponse(response);
            }
        }
    }

    /// <summary>
    /// Parsea el contenido de la respuesta HTTP del servidor para obtener los datos que nos envia el backend
    /// </summary>
    /// <param name="response"></param>
    protected async void BackendDataFromResponse(HttpResponseMessage response, BackendDataType backendDataType = BackendDataType.UserData)
    {
        //Recogemos los datos en un string
        string buffer = await response.Content.ReadAsStringAsync();
        //En caso de que no haya nada en el string de los datos no nos interesa seguir
        if (buffer == "" || buffer == null) return;
        //En caso de que la petici�n no haya salido bien tampoco nos interesa seguir
        if (response.StatusCode != HttpStatusCode.OK) return;
        
        //Elegir que claves usar
        string currentKey = "";

        //Por si peta algo o se ha corrompido algo en el mensaje y no se puede parsear, mejor un try
        try
        {
            //Parseamos el string donde guardamos el contenido de la respuesta a la estructura de datos del backend

            switch (backendDataType)
            {
                case BackendDataType.UserData:
                    backendData = JsonUtility.FromJson<BackendData>(buffer);
                    currentKey = BackendConstants.BackendDataKey;
                    break;
                case BackendDataType.TimeData:
                    backendDataTime = JsonUtility.FromJson<BackendTimeData>(buffer);
                    currentKey = BackendConstants.BackendTimeDataKey;
                    break;
                default:
                    currentKey = BackendConstants.IncorrectKey;
                    break;
            }

            //Guarda el resultado en local para cuando no haya conexi�n
            if (PlayerPrefs.HasKey(currentKey))
            {
                string oldData = PlayerPrefs.GetString(currentKey);

                if(oldData != buffer)
                {
                    PlayerPrefs.SetString(currentKey, buffer);
                }
            }
            else { PlayerPrefs.SetString(currentKey, buffer); }
        }
        catch
        {
            
        }
    }
    #endregion

    #endregion
}
