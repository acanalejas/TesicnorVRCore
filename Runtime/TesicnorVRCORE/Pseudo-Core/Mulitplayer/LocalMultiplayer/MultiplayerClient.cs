using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Http;
using System.Text;
using System.IO;
using System.Threading.Tasks;

[DisallowMultipleComponent]
public class MultiplayerClient : MonoBehaviour
{
    #region PARAMETERS
    [Header("La IP a la que se va a conectar")]
    public static string IP = "192.168.20.73";
    [Header("El puerto por el que se va a establecer la conexión")]
    public static int Port = 8080;

    private static HttpClient httpClient;

    public bool initializeOnStart = false;

    static bool alreadySent = false;

    static string response_string;
    static string last_response;
    #endregion

    #region FUNCTIONS

    private void Start()
    {
        if (initializeOnStart) StartClient();
    }

    private async void Update()
    {
        MultiplayerManager.Instance.FindReplicatedGameObjects(response_string);
        string data = MultiplayerManager.Instance.FindReplicatedGameObjects_str();
        await SendData(data);
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

        alreadySent = true;
        var cts = new System.Threading.CancellationTokenSource();

        using(StringContent sc = new StringContent(data))
        using(HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, "http://" + IP + ":8080"))
        {
            request.Content = sc;
            await Task.Run(async () =>
            {
                var response = await httpClient.SendAsync(request, cts.Token);
                ManageResponse(response);
            });
            //using(var response = await httpClient.SendAsync(request, cts.Token))
            //{
            //    ManageResponse(response);
            //}
            request.Content?.Dispose();
            request.Content = null;
        }
        alreadySent = false;
    }

    public static async void ManageResponse(HttpResponseMessage response)
    {
        Debug.Log("Receiving a response");
        string response_str = await response.Content.ReadAsStringAsync();
        Debug.Log("Response is : " + response_str);
        try
        {
            if(response.StatusCode == HttpStatusCode.OK)
            response_string = response_str;
        }
        catch
        {
            Debug.LogError("Invalid string format to parse" + " : " + response_str);
        }
        response.Content?.Dispose();
        response.Content = null;
    }
    #endregion
}
