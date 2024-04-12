using ArduinoBluetoothAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreamingBTSender : MonoBehaviour
{
    #region PARAMETERS
    private BluetoothHelper BluetoothHelper;

    private string DeviceName = "TesicnorStreamingStick";
    bool bIsConnected = false;
    #endregion

    #region METHODS
    private void Start()
    {
        StartCoroutine(nameof(LookForBTDevice));
    }

    WaitForSeconds Seconds = new WaitForSeconds(1);
    IEnumerator LookForBTDevice()
    {
        while (!bIsConnected)
        {
            BluetoothHelper.GetInstance(DeviceName);
            BluetoothHelper.Connect();
            if(BluetoothHelper.isConnected())bIsConnected = true;
            yield return Seconds;
        }
        BluetoothHelper.StartListening();
    }

    public void SendData(string data)
    {
        if(BluetoothHelper.isConnected())
        BluetoothHelper.SendData(data);
    }
    #endregion
}
