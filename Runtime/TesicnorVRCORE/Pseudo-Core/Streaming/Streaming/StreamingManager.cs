using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreamingManager : MonoBehaviour
{
    #region PARAMETERS
    public enum ConnectionType { Receiver, Sender}
    [Header("Como se comunica esta aplicación?")]
    public ConnectionType connectionType = ConnectionType.Sender;

    public string FilePath;
    public RenderTexture renderTexture;
    public Material screen_mat;
    #endregion

    #region FUNCTIONS
    private void Start()
    {
        //FilePath = Application.persistentDataPath + "/streamignFrame.jpg";
        Component toAdd = connectionType == ConnectionType.Sender? this.gameObject.AddComponent<StreamingSender>() : this.gameObject.AddComponent<StreamingReceiver>();

        if (connectionType == ConnectionType.Sender)
        {
            StreamingSender sender = toAdd as StreamingSender;
            sender.captured = renderTexture;
            sender.path = FilePath;
        }
        else
        {
            StreamingReceiver receiver = toAdd as StreamingReceiver;
            receiver.path = FilePath;
            receiver.screen = screen_mat;
        }
    }
    #endregion
}
