using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Http;
using System.Net;
using Oculus.Platform;
using Oculus.Platform.Models;
using TMPro;

public struct VRExperience
{
    public int id;
    public string code;
    public string name;
    public bool visibleForAll;
    public string description;
}

public struct VRProperty
{
    public long id;
    public string propertyName;
    public string propertyValue;
}

public struct BackendData
{
    public int id;
    public string name;
    public string description;
    public VRProperty[] vRProperties;
    public VRExperience[] vrExperiences;
    public VRProperty[] vrproperties;
}

public class BackendGetter : MonoBehaviour
{
    #region PARAMETERS
    HttpClient httpClient;
    public TextMeshProUGUI username;
    #endregion

    #region FUNCTIONS
    public void Start()
    {
        httpClient = new HttpClient();
        Core.Initialize("5770002119716955");
        Oculus.Platform.Users.GetLoggedInUser().OnComplete(GetLoggedInUserCallback);
        

        DontDestroyOnLoad(this.gameObject);
    }
    private void GetLoggedInUserCallback(Message msg)
    {
        try
        {
            if (!msg.IsError)
            {
                User user = msg.GetUser();
                string userName = user.OculusID;
                string displayName = user.DisplayName;
                username.text = userName + displayName;
            }
            else
            {
                username.text = "Error detected while getting username";
            }
        }
        catch
        {
            username.text = "Error on try";
        }
    }
    #endregion
}
