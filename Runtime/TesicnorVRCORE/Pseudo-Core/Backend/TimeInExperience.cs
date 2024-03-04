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

    // Crear una instancia de la clase BackendDataTime
    private BackendDataTime vrTimeUse;

    private int vrApplicationId = 1;
    public int vrExperienceId = 1;

    private void Start()
    {
        // Seteamos los datos guardados en el PlayerPrefs (Se hizo de está forma porque poniendo directamente el PlayerPrefs no convertia bien a VRTimeUse).
        string jsonString = PlayerPrefs.GetString(BackendDataKey);

        // Convierte el JSON a un objeto de la clase VRTimeUse.
        vrTimeUse = JsonUtility.FromJson<BackendDataTime>(jsonString);

        inputTime = System.DateTime.Now.ToString("yyyy-MM-dd" + "T" + "HH:mm:ss", CultureInfo.InvariantCulture);
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
            clientId = vrTimeUse.clientId;
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
