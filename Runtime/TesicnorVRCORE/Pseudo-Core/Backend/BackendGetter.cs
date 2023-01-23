using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Http;
using System.Net;
using Oculus.Platform;
using Oculus.Platform.Models;
using TMPro;
using System.Text;

[System.Serializable]
public class VRExperience
{
    public int id;
    public string code;
    public string name;
    public bool visibleForAll;
    public string description;
}
[System.Serializable]
public class VRProperty
{
    public long id;
    public string propertyName;
    public string propertyValue;
}


[System.Serializable]
public class BackendData
{
    [System.Serializable]
    public class VRClient
    {
        public int id;
        public string name;
        public string code;
        public string description;
        public string vrusers;
    }

    public int id;
    public string name;
    public string description;
    public VRProperty[] vRProperties;
    public VRExperience[] vrExperiences;
    public VRClient client;
    public VRProperty[] vrproperties;
}

public class BackendGetter : MonoBehaviour
{
    #region PARAMETERS
    HttpClient httpClient;
    public static  BackendData backendData = new BackendData();
    public TextMeshProUGUI username;
    string username_str;
    public static int appCode { get { return 1; } }
    public static string urlNoParams { get { return "https://app.e-xtinguisher.com/api/vr-users/public"; } }

    public static string BackendDataKey { get { return "BackendData"; } }
    #endregion

    #region FUNCTIONS

    public virtual void Awake()
    {
        httpClient = new HttpClient();
        GetBackendData(appCode.ToString());
    }
    public virtual void Start()
    {
        backendData = JsonUtility.FromJson<BackendData>(PlayerPrefs.GetString(BackendDataKey));
    }

    #region Connecting and getting the data
    public async virtual void GetBackendData(string appCode)
    {
        var cts = new System.Threading.CancellationTokenSource();

        username_str = PlayerPrefs.GetString("Username");
        Debug.Log(username_str);
        using (HttpRequestMessage hrm = new HttpRequestMessage(HttpMethod.Get, BackendGetter.urlNoParams + "?applicationId=" + appCode + "&" + "userName=" + username_str))
        {
            using (HttpResponseMessage response = await httpClient.SendAsync(hrm, cts.Token))
            {
                BackendDataFromResponse(response);
            }
        }
    }

    private async void BackendDataFromResponse(HttpResponseMessage response)
    {
        string buffer = await response.Content.ReadAsStringAsync();
        if (buffer == "" || buffer == null) return;
        if (response.StatusCode != HttpStatusCode.OK) return;

        try
        {
            backendData = JsonUtility.FromJson<BackendData>(buffer);
            
            Debug.Log("Number of experiences is : " + backendData.vrExperiences.Length);

            if (PlayerPrefs.HasKey(BackendDataKey))
            {
                string oldData = PlayerPrefs.GetString(BackendDataKey);

                if(oldData != buffer)
                {
                    PlayerPrefs.SetString(BackendDataKey, buffer);
                }
            }
            else { PlayerPrefs.SetString(BackendDataKey, buffer); }
        }
        catch
        {
            Debug.Log("Coudn´t parse the string from the backend to the struct");
        }
    }
    #endregion

    #endregion
}
