using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Http;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

[DisallowMultipleComponent]
public class MultiplayerHost : MonoBehaviour
{
    #region PARAMETERS
    private string IP { get { return MultiplayerManager.IP; } }
    private int port { get { return MultiplayerManager.Port; } }

    public bool initializeOnStart = true;

    HttpListener host;

    string content;

    string lastContent;

    string response;

    List<byte[]> buffer = new List<byte[]>();

    #endregion

    #region FUNCTIONS
    private void Start()
    {
        if (initializeOnStart) CreateLocalSession();
    }

    private void Update()
    {
        try
        {
            if (content != lastContent && content != "")
            { MultiplayerManager.Instance.FindReplicatedGameObjects(content); }
            lastContent = content;
        }
        catch
        {
            Debug.LogError("Coudn't replicate");
        }

        try
        {
            response = MultiplayerManager.Instance.FindReplicatedGameObjects_str();
            
        }
        catch
        {
            Debug.LogError("Couldn't get the string for the response");
        }
            
    }

    public void CreateLocalSession()
    {
        host = new HttpListener();
        Debug.Log(IP);
        host.Prefixes.Add("http://" + this.IP + ":" + port.ToString() + "/");
        //CloseLocalSession();
        if(!host.IsListening)host.Start();
        host.BeginGetContext(new AsyncCallback(HttpCallback), host);
    }

    private async void HttpCallback(IAsyncResult result)
    {
        var context = host.EndGetContext(result);
        var request = context.Request;
        var _response = context.Response;
        buffer.Add(await manageRequest(request));
        await HandleBuffer(_response);
    }
    private async Task HandleBuffer(HttpListenerResponse response)
    {
        if (buffer.Count <= 0) return;

        byte[] toRead = buffer[0];

        await Task.Run(() =>
        {
            lock (buffer)
            {
                lock (response)
                {
                    manageResponse(response, toRead);
                }
            }
        });
    }

    private async Task<byte[]> manageRequest(HttpListenerRequest request)
    {
        host.BeginGetContext(new AsyncCallback(HttpCallback), host);

        MemoryStream ms = new MemoryStream();
        await request.InputStream.CopyToAsync(ms);
        content = Encoding.UTF8.GetString(ms.ToArray());
        ms.Close();

        string response_str = "";
        response_str = this.response;
        byte[] response_byte = Encoding.UTF8.GetBytes(response);
        MultiplayerManager.Instance.actionsData.Clear();
        MultiplayerManager.Instance.fieldDatas.Clear();

        return response_byte;
    }

    private async Task manageResponse(HttpListenerResponse response, byte[] bytes)
    {
        await response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
        response.Close();
    }

    public void CloseLocalSession()
    {
        if(host != null)
        if (host.IsListening) host.Stop();
    }

    private void OnApplicationQuit()
    {
        CloseLocalSession();
    }
    #endregion
}
