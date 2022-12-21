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
            if (content != lastContent)
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
        host.Prefixes.Clear();
        Debug.Log(IP);
        host.Prefixes.Add("http://" + this.IP + ":" + port.ToString() + "/");
        CloseLocalSession();
        if(!host.IsListening)host.Start();

        host.BeginGetContext(new AsyncCallback(HttpCallback), host);
    }

    private void HttpCallback(IAsyncResult result)
    {
        var context = host.EndGetContext(result);
        var request = context.Request;

        MemoryStream ms = new MemoryStream();
        request.InputStream.CopyTo(ms);
        StreamReader sr = new StreamReader(ms);
        content = Encoding.UTF8.GetString(ms.ToArray());
        Debug.Log("Request content is : " + content);

        using (var _response = context.Response)
        {
            string response_str = "";
            response_str = this.response;
            byte[] response_byte = Encoding.UTF8.GetBytes(response_str);

            _response.OutputStream.Write(response_byte, 0, response_byte.Length);
            _response.Close();
        }

        host.BeginGetContext(new AsyncCallback(HttpCallback), host);
    }

    public void CloseLocalSession()
    {
        if (host.IsListening) host.Stop();
    }
    #endregion
}
