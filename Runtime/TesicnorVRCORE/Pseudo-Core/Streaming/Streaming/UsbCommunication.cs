using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Management;

#if UNITY_STANDALONE_WIN

//System.Diagnostics.Process proc = new System.Diagnostics.Process();
//proc.EnableRaisingEvents = false;
//proc.StartInfo.FileName = "EXECUTABLE";
//proc.StartInfo.CreateNoWindow = true;
//proc.StartInfo.UseShellExecute = false;
//proc.StartInfo.RedirectStandardOutput = true;
//proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
//proc.Start();
//string fingerprint = proc.StandardOutput.ReadLine();
//proc.WaitForExit();

public static class UsbCommunication
{
#region PARAMETERS

#endregion

#region METHODS

    public static List<USBDeviceInfo> GetUSBDevicesFromDescription(string content)
    {
        List<USBDeviceInfo> devices = new List<USBDeviceInfo>();

        var usbDevices = GetUSBDevices();

        foreach(var device in usbDevices)
        {
            if(device.Description.Contains(content)) devices.Add(device);
        }

        return devices;
    }

    public static USBDeviceInfo GetUSBDeviceFromDescription(string content)
    {
        var usbDevices = GetUSBDevices();

        foreach(var device in usbDevices)
        {
            if (device.Description.Contains(content)) return device;
        }

        return null;
    }

    public static List<string> GetUSBDevicesIDs()
    {
        List<string> list = new List<string>();

        var usbDevices = GetUSBDevices();

        foreach(var usbDevice in usbDevices) {
            list.Add(usbDevice.DeviceID);
        }

        return list;
    }

    public static USBDeviceInfo GetDeviceFromID(string ID)
    {
        var usbDevices = GetUSBDevices();

        foreach(var usbDevice in usbDevices)
        {
            if(usbDevice.DeviceID == ID) return usbDevice;
        }

        return null;
    }

    public static List<USBDeviceInfo> GetUSBDevices()
    {
        List<USBDeviceInfo> devices = new List<USBDeviceInfo>();

        using (var searcher = new ManagementObjectSearcher(@"Select * From Win32_USBHub"))
        using (ManagementObjectCollection collection = searcher.Get())
        {
            foreach (var device in collection)
            {
                devices.Add(new USBDeviceInfo(
                    (string)device.GetPropertyValue("DeviceID"),
                    (string)device.GetPropertyValue("PNPDeviceID"),
                    (string)device.GetPropertyValue("Desciption")));
            }
        }

        return devices;
    }
#endregion
}

public class USBDeviceInfo
{
    public USBDeviceInfo(string deviceID, string pnpDeviceID, string description)
    {
        this.DeviceID = deviceID;
        this.PnpDeviceID = pnpDeviceID;
        this.Description = description;
    }
    public string DeviceID { get; private set; }
    public string PnpDeviceID { get; private set; }
    public string Description { get; private set; }
}

#endif
