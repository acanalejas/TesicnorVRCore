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
    static float timer;

    List<byte[]> buffer = new List<byte[]>();

    List<HttpListenerContext> contexts = new List<HttpListenerContext>();

    #endregion

    #region FUNCTIONS
    private void Start()
    {
        if (initializeOnStart) CreateLocalSession();
        StartCoroutine("update");
    }

    IEnumerator update()
    {
        while (true)
        {
            manageRequest();
            yield return new WaitForSeconds(1 / 30);
        }
    }

    private void Update()
    {
        try
        {
            if (content != lastContent && content != "") 
            { MultiplayerManager.Instance.FindReplicatedGameObjects(content); }
        }
        catch
        {
            Debug.LogError("Coudn't replicate");
        }

        try
        {
            response = MultiplayerManager.Instance.FindReplicatedGameObjects_str();
            if (response == content || response == lastContent) response = "";
        }
        catch
        {
            Debug.LogError("Couldn't get the string for the response");
        }
        lastContent = content;
    }

    public void CreateLocalSession()
    {
        host = new HttpListener();
        Debug.Log(IP);
        host.Prefixes.Add("http://" + this.IP + ":" + port.ToString() + "/");
        //CloseLocalSession();
        if (!host.IsListening) { host.Start(); }
        host.BeginGetContext(new AsyncCallback(HttpCallback), host);
    }

    private void HttpCallback(IAsyncResult result)
    {
        var context = host.EndGetContext(result);
        
        MemoryStream ms = new MemoryStream();
        context.Request.InputStream.CopyTo(ms);
        context.Request.InputStream.Close();
        
        byte[] _buff = ms.ToArray();
        ms.Close();
        if (_buff.Length > 0)
            buffer.Add(_buff);

        manageResponse(context.Response, Encoding.UTF8.GetBytes(response));

        host.BeginGetContext(new AsyncCallback(HttpCallback), host);
    }

    private void manageRequest()
    {
        if (buffer.Count <= 0) return;

        content = Encoding.UTF8.GetString(buffer[0]);
        buffer.RemoveAt(0);

        MultiplayerManager.Instance.actionsData.Clear();
        MultiplayerManager.Instance.fieldDatas.Clear();

    }

    private async void manageResponse(HttpListenerResponse response, byte[] bytes)
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
