using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.SceneManagement;
using OVR;

public class TimeInExperience : BackendLoadData
{
    // Almacena el la fecha y hora de inicio de una experiencia.
    string inputTime;
    // Almacena el tiempo que transcurre en la experiencia en segundos.
    float timeInSeconds;

    private int vrApplicationId = 1;
    public int vrExperienceId = 1;

    private void Start()
    {
        // Seteamos los datos guardados en el PlayerPrefs (Se hizo de est� forma porque poniendo directamente el PlayerPrefs no convertia bien a VRTimeUse).
        string jsonString = PlayerPrefs.GetString(BackendConstants.BackendDataKey);

        // Convierte el JSON a un objeto de la clase VRTimeUse.
        backendDataTime = JsonUtility.FromJson<BackendTimeData>(jsonString);

        inputTime = System.DateTime.Now.ToString("yyyy-MM-dd" + "T" + "HH:mm:ss", CultureInfo.InvariantCulture);

        var startDate = System.DateTime.Now;
        var testDate = System.DateTime.Now;

        var timeSpan = startDate - testDate;
        int secondsElapsed = (int)timeSpan.TotalSeconds; 
    }

    private void Update()
    {
        timeInSeconds += Time.deltaTime;

        bool internet = false;

        PlayerPrefs.SetString("DataOnDisable", DataTime());
    }
    
    /*public void SendDataReset()
    {
        if (HasInternetConnection())
        {
            if (PlayerPrefs.GetString("DataOnDisable", "") != "")
            {
                SendDataToAPI(PlayerPrefs.GetString("DataOnDisable", ""));
                Debug.Log("Data enviada");
                PlayerPrefs.DeleteKey("DataOnDisable");
            }
        }
        else
        {
            string dataOnDisable = PlayerPrefs.GetString("DataOnDisable", "");

            if (!string.IsNullOrEmpty(dataOnDisable))
            {
                AddList(dataOnDisable);
            }
        }
    }*/

    private string DataTime()
    {
        string jsonData;

        var hwInfoaux = SystemInfo.deviceUniqueIdentifier;
        string startDateTime = inputTime;
        string endDateTime = System.DateTime.Now.ToString("yyyy-MM-dd" + "T" + "HH:mm:ss", CultureInfo.InvariantCulture);
        int duration = (int)timeInSeconds;
        string clientId;
        string user;

        if (PlayerPrefs.GetString("Username", "") != "")
        {
            clientId = backendDataTime.clientId;
            user = PlayerPrefs.GetString("Username");

            // Corregido el formato JSON y manejo de tipos de datos
            jsonData = "{\"hwInfo\": \"" + hwInfoaux.ToString() + "\", " +
                              "\"start\": \"" + startDateTime + "\", " +
                              "\"end\": \"" + endDateTime + "\", " +
                              "\"period\": " + duration + ", " +
                              "\"clientId\": " + clientId + ", " +
                              "\"vrApplicationId\": " + vrApplicationId + ", " +
                              "\"vrExperienceId\": " + vrExperienceId + ", " +
                              "\"userName\": \"" + user + "\"}";
        }
        else
        {
            // Corregido el formato JSON y manejo de tipos de datos
            jsonData = "{\"hwInfo\": \"" + hwInfoaux.ToString() + "\", " +
                              "\"start\": \"" + startDateTime + "\", " +
                              "\"end\": \"" + endDateTime + "\", " +
                              "\"period\": " + duration + ", " +
                              "\"clientId\": \"" + null + "\", " +
                              "\"vrApplicationId\": " + vrApplicationId + ", " +
                              "\"vrExperienceId\": " + vrExperienceId + ", " +
                              "\"userName\": \"" + "\"}";
        }

        return jsonData;
    }
}
