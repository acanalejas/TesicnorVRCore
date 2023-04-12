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

public class BackendGetter : MonoBehaviour
{
    #region PARAMETERS
    HttpClient httpClient;
    public static  BackendData backendData = new BackendData();
    public TextMeshProUGUI username;
    string username_str;
    public static int appCode = 1;
    public static string urlNoParams { get { return "https://app.e-xtinguisher.com/api/vr-users/public?"; } }

    public static string BackendDataKey { get { return "BackendData"; } }
    #endregion

    #region FUNCTIONS

    public virtual void Awake()
    {
        httpClient = new HttpClient();
        GetBackendData(appCode.ToString());
    }
    public virtual void Start()
    {
        backendData = JsonUtility.FromJson<BackendData>(PlayerPrefs.GetString(BackendDataKey));
    }

    #region Connecting and getting the data
    /// <summary>
    /// Realiza la petición al backend para recoger los datos
    /// </summary>
    /// <param name="appCode">Código de la aplicación en la que estemos</param>
    public async virtual void GetBackendData(string appCode)
    {
        //Recomendable, no se por que pero creandole el source para asignar el token funciona mejor, mierdas de .net
        var cts = new System.Threading.CancellationTokenSource();

        username_str = PlayerPrefs.GetString("Username");

        using (HttpRequestMessage hrm = new HttpRequestMessage(HttpMethod.Get, BackendGetter.urlNoParams + "applicationId=" + appCode + "&" + "userName=" + username_str))
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
    private async void BackendDataFromResponse(HttpResponseMessage response)
    {
        //Recogemos los datos en un string
        string buffer = await response.Content.ReadAsStringAsync();
        //En caso de que no haya nada en el string de los datos no nos interesa seguir
        if (buffer == "" || buffer == null) return;
        //En caso de que la petición no haya salido bien tampoco nos interesa seguir
        if (response.StatusCode != HttpStatusCode.OK) return;

        //Por si peta algo o se ha corrompido algo en el mensaje y no se puede parsear, mejor un try
        try
        {
            //Parseamos el string donde guardamos el contenido de la respuesta a la estructura de datos del backend
            backendData = JsonUtility.FromJson<BackendData>(buffer);

            //Guarda el resultado en local para cuando no haya conexión
            if (PlayerPrefs.HasKey(BackendDataKey))
            {
                string oldData = PlayerPrefs.GetString(BackendDataKey);

                if(oldData != buffer)
                {
                    PlayerPrefs.SetString(BackendDataKey, buffer);
                }
            }
            else { PlayerPrefs.SetString(BackendDataKey, buffer); }
        }
        catch
        {
            
        }
    }
    #endregion

    #endregion
}
