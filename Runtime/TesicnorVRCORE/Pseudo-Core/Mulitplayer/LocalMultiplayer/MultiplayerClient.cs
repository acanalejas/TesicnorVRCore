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
    public static string IP = "192.168.20.40";
    [Header("El puerto por el que se va a establecer la conexión")]
    public static int Port = 8080;

    private static HttpClient httpClient;

    public bool initializeOnStart = false;

    static bool alreadySent = false;

    string response_string;
    string last_response;
    string last_content;
    #endregion

    #region FUNCTIONS

    private void Start()
    {
        if (initializeOnStart) StartClient();
        StartCoroutine("update");
    }

    private IEnumerator update()
    {
        while (true)
        {
            //if (last_content != MultiplayerManager.Instance.FindReplicatedGameObjects_str())
            //{
                yield return SendData(MultiplayerManager.Instance.FindReplicatedGameObjects_str());
            //}

            if (MultiplayerManager.Instance.isValidString(response_string.Split("%")[0]) && last_response != response_string)
            {
                MultiplayerManager.Instance.FindReplicatedGameObjects(response_string);
                last_response = response_string;
            }
            yield return new WaitForSeconds(1 / 30);
        }
        
    }
    public void StartClient()
    {
        httpClient = new HttpClient();
        httpClient.BaseAddress = new System.Uri("http://" + IP + ":" + Port.ToString());
        httpClient.DefaultRequestHeaders.Accept.Clear();
    }

    public async Task SendData(string data)
    {
        var cts = new System.Threading.CancellationTokenSource();

        using (ByteArrayContent sc = new ByteArrayContent(Encoding.UTF8.GetBytes(data)))
        using (HttpResponseMessage request = httpClient.PostAsync("http://" + IP + ":8080", sc).Result)
        {
            request.EnsureSuccessStatusCode();

            last_content = data;
            
            ManageResponse(request);

            request.Content?.Dispose();
            request.Content = null;

            while (!request.IsSuccessStatusCode) continue;
        }
    }

    public async void ManageResponse(HttpResponseMessage response)
    {
        Debug.Log("Receiving a response");
        string response_str = await response.Content.ReadAsStringAsync();
        Debug.Log("Response is : " + response_str);
        try
        {
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
