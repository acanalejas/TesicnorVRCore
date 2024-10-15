using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Plugin.BLE;
//using Plugin.BluetoothLE;

public class BluetoothDetector : MonoBehaviour
{
    #region SINGLETON
    private static BluetoothDetector instance;
    public static BluetoothDetector Instance {get {return instance;}}

    void CheckSingleton()
    {
        if (!instance) instance = this;
        else Destroy(this);
    }
    #endregion

    #region PARAMETERS
    [Header("Evento usado cuando se conecta un nuevo dispositivo")]
    public UnityEvent<string> onDeviceConnected;

    [Header("Evento usado cuando se desconecta un dispositivo")]
    public UnityEvent<string> onDeviceDisconnected;

    #endregion

    #region METHODS
    private void Awake()
    {
        CheckSingleton();
    }

    private void BindBTEvents()
    {
        CrossBluetoothLE.Current.Adapter.DeviceConnected += OnConnected;
        CrossBluetoothLE.Current.Adapter.DeviceDisconnected += OnDisconnected;
    }

    protected void OnConnected(object _sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
    {
        onDeviceConnected.Invoke(e.Device.Name);
    }

    protected void OnDisconnected(object _sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
    {
        onDeviceDisconnected.Invoke(e.Device.Name);
    }

    public bool IsDeviceConnected(string _deviceName)
    {
        if (!CrossBluetoothLE.Current.IsOn) return false;
        
        var _connectedDevices = CrossBluetoothLE.Current.Adapter.ConnectedDevices;
        
        foreach(var device in _connectedDevices)
        {
            if (device.Name.Contains(_deviceName))
            {
                return true;
            }
        }
        return false;
    }

    public void ConnectToDevice(string _deviceName)
    {
        if (!CrossBluetoothLE.Current.IsOn) return;
        if(!CrossBluetoothLE.Current.IsAvailable) return;

        
    }
    public int ConnectedDevicesCount { get { return CrossBluetoothLE.Current.Adapter.ConnectedDevices.Count; } }
    #endregion
}
