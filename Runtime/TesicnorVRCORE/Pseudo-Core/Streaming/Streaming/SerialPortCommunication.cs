using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using UnityEditor.PackageManager;
using System;

public class SerialPortCommunication : MonoBehaviour
{
    #region PARAMETERS
    SerialPort Port;

    bool portDetected;

    string checkString = "Streaming";
    #endregion

    #region METHODS
    
    public virtual void SearchConnectedPort(string id = "Arduino")
    {
        USBDeviceInfo info = UsbCommunication.GetUSBDeviceFromDescription(id);
        if (info == null) return;

        string[] portNames = SerialPort.GetPortNames();

        foreach(string name in portNames)
        {
            if(name.Contains(info.DeviceID) || info.DeviceID.Contains(name))
            {
                Debug.Log("Serial port for arduino found");
                return;
            }
        }
        Debug.Log("Unable to find an arduino");

    }

    //void GetPortByName(string name)
    //{
    //    string[] names = SerialPort.GetPortNames();
    //    foreach(string _name in names)
    //    {
    //        Debug.Log(_name);
    //        arduinoPort = new SerialPort(_name);
    //        arduinoPort.Open();
    //        arduinoPort.WriteLine("Streaming");
    //        Debug.Log(arduinoPort.ReadLine());
    //    }
    //}


    WaitForSeconds Seconds = new WaitForSeconds(1);
    IEnumerator LookForStreamingUSB()
    {
        while (!portDetected)
        {
            string[] names = SerialPort.GetPortNames();
            foreach (string name in names)
            {
                Port = new SerialPort(name, 9600);
                Port.Open();
                Port.WriteLine(checkString);
                if(Port.ReadLine() == checkString)
                {
                    portDetected = true;
                    Debug.Log("Arduino detected via USB");
                }
            }
            yield return Seconds;
        }
    }

    WaitForEndOfFrame Frame = new WaitForEndOfFrame();
    IEnumerator ReceiveUSBData()
    {
        Port.Disposed += (Object, EventHandler) => { portDetected = false; };
        while (portDetected)
        {
            
            yield return Frame;
        }

        StartCoroutine(nameof(LookForStreamingUSB));
    }

    private void Start()
    {
        //SearchConnectedPort();
        //GetPortByName("");

        StartCoroutine(nameof(LookForStreamingUSB));
    }
    #endregion
}
