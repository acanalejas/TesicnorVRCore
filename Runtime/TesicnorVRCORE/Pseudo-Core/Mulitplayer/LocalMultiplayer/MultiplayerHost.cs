using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Http;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

public class MultiplayerHost : MonoBehaviour
{
    #region PARAMETERS
    private string IP { get { return MultiplayerManager.IP; } }
    private int port { get { return MultiplayerManager.Port; } }

    public bool initializeOnStart = true;

    HttpListener host;
    #endregion

    #region FUNCTIONS
    private void Start()
    {
        if (initializeOnStart) CreateLocalSession();
    }

    public void CreateLocalSession()
    {
        host = new HttpListener();
        host.Prefixes.Clear();
        Debug.Log(IP);
        host.Prefixes.Add("http://" + this.IP + ":" + port.ToString() + "/");
        host.Start();

        host.BeginGetContext(new AsyncCallback(HttpCallback), host);
    }

    private async void HttpCallback(IAsyncResult result)
    {
        Debug.Log("Receiving a request");
        var context = host.EndGetContext(result);
        var request = context.Request;

        Debug.Log("Before entering the request");
        MemoryStream ms = new MemoryStream();
        request.InputStream.CopyTo(ms);
        StreamReader sr = new StreamReader(ms);
        string content = Encoding.UTF8.GetString(ms.ToArray());
        Debug.Log("Request content is : " + content);

        Debug.Log("Before finding replicated go");
        try
        {
            MultiplayerManager.Instance.FindReplicatedGameObjects(content);
        }
        finally
        {
            Debug.Log("Before response");
            using (var response = context.Response)
            {
                string response_str = "";
                try
                {
                    response_str = MultiplayerManager.Instance.FindReplicatedGameObjects_str();
                }
                catch { Debug.Log("Couldn´t get string"); }
                Debug.Log("response string is: " + response_str);
                byte[] response_byte = Encoding.UTF8.GetBytes(response_str);

                response.OutputStream.Write(response_byte, 0, response_byte.Length);
                response.Close();
            }

            Debug.Log("After response");

            host.BeginGetContext(new AsyncCallback(HttpCallback), host);
        }
       
        //ms.Close();


    }

    public void CloseLocalSession()
    {
        host.Stop();
        host.Close();
    }
    #endregion
}
