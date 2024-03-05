using UnityEngine;
using System.Net.Http;
using System.Net;
using TMPro;
using System;

public class BackendGetTimeUse : MonoBehaviour
{
    #region PARAMETERS
    public HttpClient httpClient;
    public static BackendTimeData backendDataTime = new BackendTimeData();

    protected const string url = "https://pre.app.e-xtinguisher.com/api/public/";

    const string apiURL = url + "client-time-uses?";

    public static string BackendDataKey { get { return "BackendKeyTime"; } }

    public float reloadTime = 10f;
    #endregion

    #region FUNCTIONS

    public virtual void Awake()
    {
        httpClient = new HttpClient();
        InvokeRepeating("GetBackendDataTime", 0.1f, reloadTime);
    }

    public virtual void Start()
    {
        BackendTimeData backendDataTime = JsonUtility.FromJson<BackendTimeData>(PlayerPrefs.GetString(BackendDataKey));
    }

    #region Connecting and getting the data
    /// <summary>
    /// Realiza la petición al backend para recoger los datos de tiempo del usuario
    /// </summary>
    public async virtual void GetBackendDataTime()
    {
        string jsonString = PlayerPrefs.GetString(BackendGetter.BackendDataKey);
        BackendData dataUser = JsonUtility.FromJson<BackendData>(jsonString);

        string username = PlayerPrefs.GetString("Username");

        //Recomendable, no se por que pero creandole el source para asignar el token funciona mejor, mierdas de .net
        var cts = new System.Threading.CancellationTokenSource();

        using (HttpRequestMessage hrm = new HttpRequestMessage(HttpMethod.Get, BackendGetTimeUse.apiURL + "clientId=" + dataUser.client.id + "&" + "name=" + username))
        {
            using (HttpResponseMessage response = await httpClient.SendAsync(hrm, cts.Token))
            {
                if (response.IsSuccessStatusCode)
                {
                    //Debug.Log($"Solicitud Exitosa: {response.StatusCode}");
                    BackendDataFromResponse(response);
                }
                else
                {
                    //Debug.Log($"Error en la solicitud: {response.StatusCode}");
                    PlayerPrefs.SetString(BackendDataKey, "");
                }
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
            backendDataTime = JsonUtility.FromJson<BackendTimeData>(buffer);

            //Guarda el resultado en local para cuando no haya conexión
            if (PlayerPrefs.HasKey(BackendDataKey))
            {
                string oldData = PlayerPrefs.GetString(BackendDataKey);

                if (oldData != buffer)
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
