using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Http;
using System;
using System.IO;
using System.Text;

public class MultiplayerHost : MonoBehaviour
{
    #region PARAMETERS
    private string IP { get { return MultiplayerManager.IP; } }
    private int port { get { return MultiplayerManager.Port; } }

    HttpListener host;
    #endregion

    #region FUNCTIONS
    public void CreateLocalSession()
    {
        host = new HttpListener();
        host.Prefixes.Add("http://" + IP + ":" + port.ToString());
        host.Start();

        host.BeginGetContext(new AsyncCallback(HttpCallback), host);
    }

    private void HttpCallback(IAsyncResult result)
    {
        var context = host.EndGetContext(result);
        var request = context.Request;
        var response = context.Response;
        
        using(MemoryStream ms = (MemoryStream)request.InputStream)
        {
            StreamReader sr = new StreamReader(ms);
            string content = Encoding.UTF8.GetString(ms.ToArray());

            MultiplayerManager.Instance.FindReplicatedGameObjects(content);
            ms.Close();
        }

        if (MultiplayerManager.Instance)
        {
            string response_str = MultiplayerManager.Instance.FindReplicatedGameObjects_str();
            byte[] response_byte = Encoding.UTF8.GetBytes(response_str);

            response.OutputStream.Write(response_byte, 0, response_byte.Length);
            response.Close();
        }

        host.BeginGetContext(new AsyncCallback(HttpCallback), host);
    }

    public void CloseLocalSession()
    {
        host.Stop();
        host.Close();
    }
    #endregion
}
