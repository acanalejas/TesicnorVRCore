using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public class SerialPortCommunication : MonoBehaviour
{
    #region PARAMETERS
    SerialPort Port;

    bool portDetected;

    string checkString = "Streaming";

    public static byte[] BTBytes;
    #endregion

    #region METHODS
    

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
            string bytes_str = Port.ReadLine();
            List<byte> bytes = new List<byte>();
            foreach(char c in bytes_str)
            {
                byte b = byte.Parse(c.ToString());
                bytes.Add(b);
            }
            BTBytes = bytes.ToArray();
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
