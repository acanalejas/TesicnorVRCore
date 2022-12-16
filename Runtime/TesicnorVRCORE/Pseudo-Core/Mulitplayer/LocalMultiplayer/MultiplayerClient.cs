using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Http;
using System.Text;
using System.IO;
using System.Threading.Tasks;

public class MultiplayerClient : MonoBehaviour
{
    #region PARAMETERS
    [Header("La IP a la que se va a conectar")]
    public static string IP = "192.168.20.73";
    [Header("El puerto por el que se va a establecer la conexión")]
    public static int Port = 8080;

    private static HttpClient httpClient;

    public bool initializeOnStart = false;
    #endregion

    #region FUNCTIONS

    private void Start()
    {
        if (initializeOnStart) StartClient();
    }

    private void Update()
    {
        SendData(MultiplayerManager.Instance.FindReplicatedGameObjects_str());
    }
    public void StartClient()
    {
        httpClient = new HttpClient();
        httpClient.BaseAddress = new System.Uri("http://" + IP + ":" + Port.ToString());
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Add("application", "text");
    }

    public static async Task SendData(string data)
    {
        if (IP == "") return;

        var cts = new System.Threading.CancellationTokenSource();

        using(StringContent sc = new StringContent(data))
        using(HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, IP))
        {
            request.Content = sc;
            using(HttpResponseMessage response = await httpClient.SendAsync(request, cts.Token))
            {
                ManageResponse(response);
            }
            request.Content?.Dispose();
            request.Content = null;
        }
    }

    public static async void ManageResponse(HttpResponseMessage response)
    {
        Debug.Log("Receiving a response");
        string response_str = await response.Content.ReadAsStringAsync();
        try
        {
            MultiplayerManager.Instance.FindReplicatedGameObjects(response_str);
        }
        catch
        {
            Debug.Log("Invalid string format to parse" + " : " + response_str);
        }
        response.Content?.Dispose();
        response.Content = null;
    }
    #endregion
}
