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
            if (MultiplayerManager.Instance.isValidString(resp) && last_response != response_string)
            {
                MultiplayerManager.Instance.FindReplicatedGameObjects(response_string);
                last_response = response_string;
            }

            Debug.Log("Number of actions to replicate this frame : " + MultiplayerManager.Instance.actionsData.Count);
            
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
        //if (data == last_content) return;
        var cts = new System.Threading.CancellationTokenSource();

        string _data = data;
        if (data == last_content) _data = "";

        using (ByteArrayContent sc = new ByteArrayContent(Encoding.UTF8.GetBytes(_data)))
        using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, "http://" + IP + ":" + Port.ToString()))
        {
            request.Content = sc;
            using (HttpResponseMessage response = await httpClient.SendAsync(request, cts.Token))
            {

                //response.EnsureSuccessStatusCode();

                last_content = data;

                ManageResponse(response);

                request.Content?.Dispose();
                request.Content = null;

                //while (!request.IsSuccessStatusCode) continue;
            }

        }

        MultiplayerManager.Instance.actionsData.Clear();
        MultiplayerManager.Instance.fieldDatas.Clear();
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
