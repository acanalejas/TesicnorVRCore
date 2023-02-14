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
    public static string IP = "192.168.20.49";
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

    WaitForSeconds seconds = new WaitForSeconds(1 / 30);
    private IEnumerator update()
    {
        while (true)
        {
            //if (last_content != MultiplayerManager.Instance.FindReplicatedGameObjects_str())
            //{
                yield return SendData(MultiplayerManager.Instance.FindReplicatedGameObjects_str());
            //}
            string resp = response_string;
            
            try
            {
                resp = response_string.Split("%")[0];
            }
            catch
            {

            }
            if (MultiplayerManager.Instance.isValidString(resp) && last_response != response_string && response_string != last_content)
            {
                MultiplayerManager.Instance.FindReplicatedGameObjects(response_string);
                last_response = response_string;
            }

            yield return seconds;
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
        using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://" + IP + ":" + Port.ToString()))
        {
            request.Content = sc;
            using (HttpResponseMessage response = await httpClient.SendAsync(request, cts.Token))
            {
                last_content = data;

                ManageResponse(response);

                request.Content?.Dispose();
                request.Content = null;
            }
        }
        MultiplayerManager.Instance.actionsData.Clear();
        MultiplayerManager.Instance.fieldDatas.Clear();
    }

    public async void ManageResponse(HttpResponseMessage response)
    {
        string response_str = await response.Content.ReadAsStringAsync();
        try
        {
            if (response_str != last_response && response_str != last_content)
                response_string = response_str;
            else response_string = "";
        }
        catch
        {
            Debug.LogError("Invalid string format to parse" + " : " + response_str);
        }
        response.Content?.Dispose();
        response.Content = null;
    }

    public void SetIP(string ip)
    {
        IP = ip;
    }

    #endregion
}
