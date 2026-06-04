using System;
using UnityEngine;
using StreamingCSharp;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using UnityEngine.Rendering;

public class StreamingSender : MonoBehaviour
{
    #region PARAMETERS
    public static StreamingSender Instance
    {
        get { return instance; }
    }
    private static StreamingSender instance;
    public RenderTexture captured;
    public string path;
    private Camera capturadora;
    public Camera playerCamera;
    private static ImageConverter converter;
    Texture2D parse;
    //private StreamingBTSender BTSender;

    public bool isStreaming
    {
        get { return streaming; }
    }

    private static bool streaming = false;

    //Objetos necesarios para la funcionalidad multihilo
    private Thread uploadThread;
    private Queue<byte[]> rawFrameQueue = new Queue<byte[]>();
    private readonly object queueLock = new object();
    #endregion

    #region FUNCTIONS
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
        //HttpClient_Custom.IntializeClient();
        //UDPClient.InitializeClient();
    }
    private void Start()
    {
        //BTSender = this.gameObject.AddComponent<StreamingBTSender>();
        if (!streaming) return;
        UDPClient.InitializeClient();
        CheckCamera();
        SetTextureForCamera();
        PrepareThread();
        StartCoroutine("update");
    }

    public void InitializeStreaming()
    {
        streaming = true;
        UDPClient.InitializeClient();
        CheckCamera();
        SetTextureForCamera();
        PrepareThread();
        StartCoroutine("update");
    }

    public void EndStreaming()
    {
        UDPClient.CloseSocket();
        StopCoroutine(nameof(update));
    }

    void PrepareThread()
    {
        uploadThread = new Thread(new ThreadStart(SendData));
        uploadThread.IsBackground = true;
        uploadThread.Start();
    }

    private void CheckCamera()
    {
        if (this.playerCamera) return;

        TesicnorPlayer player = TesicnorPlayer.Instance;

        Camera _camera = player.Camera_GO.GetComponent<Camera>();
        if (player.Camera_GO && _camera) this.playerCamera = _camera;
    }

    private void SetTextureForCamera()
    {
        Camera mainCamera = playerCamera;

        if (!playerCamera) return;

        GameObject capturadora_go = new GameObject("capturadora", typeof(Camera));
        capturadora_go.transform.parent = mainCamera.gameObject.transform;
        capturadora_go.transform.localPosition = Vector3.zero;
        capturadora_go.transform.localRotation = Quaternion.Euler(Vector3.zero);
        capturadora_go.transform.localScale = Vector3.one;

        capturadora = capturadora_go.GetComponent<Camera>();

        capturadora.targetTexture = captured;
        //capturadora.targetTexture = captured;
        parse = new Texture2D(640, 360, TextureFormat.RGB24, false);
    }


    WaitForSeconds frame = new WaitForSeconds(1 / 30);
    private IEnumerator update()
    {
        while (true)
        {
            yield return frame;
            this.WriteTXTFile();
        }
    }

    
    Rect rect = new Rect(0, 0, 640, 360);

    private void GetTextureTraduction()
    {
        //RenderTexture.active = captured;
        //AsyncGPUReadback.Request(captured, 0, TextureFormat.RGB565, OnCompletedReadback);
        try
        {
            //Capture the screen
            if(!parse) parse = new Texture2D(640, 360, TextureFormat.RGB24, false);
            RenderTexture.active = this.capturadora.targetTexture;
            parse.ReadPixels(rect, 0, 0, false);
            parse.Apply(false);
        
            //Get the jpg byte array
            //byte[] _data = parse.EncodeToJPG(50);
            byte[] _data = parse.EncodeToJPG(50);
        
            lock (queueLock)
            {
                if (rawFrameQueue.Count < 2)
                {
                    rawFrameQueue.Enqueue(_data);
                }
            }
        
            //Send it to the receiver
            //await UDPClient.SendBytes(_data);
            //await HttpClient_Custom.SendData(_data);
            
            alreadySent = true;
        }
        catch(Exception e)
        {
            Debug.LogError("No se ha podido enviar la imagen debido a : " + e.Message);
        }
    }

    /*void OnCompletedReadback(AsyncGPUReadbackRequest request)
    {
        if (request.hasError)
        {
            Debug.LogError("Ha habido un error con la lectura de GPU");
            return;
        }
        if(!parse) parse = new Texture2D(1280, 720, TextureFormat.RGB565, false);

        var nativeBytes = request.GetData<byte>();
        byte[] rawBytes = nativeBytes.ToArray();
        
        parse.LoadRawTextureData(rawBytes);
        parse.Apply(false);
        byte[] jpg = parse.EncodeToJPG();

        lock (queueLock)
        {
            if (rawFrameQueue.Count < 2)
            {
                rawFrameQueue.Enqueue(jpg);
            }
        }

        alreadySent = true;
    }*/

    private void SendData()
    {
        while (true)
        {
            byte[] frameToSend = null;
            lock (queueLock)
            {
                if (rawFrameQueue.Count > 0)
                {
                    frameToSend = rawFrameQueue.Dequeue();
                }
            }

            if (frameToSend != null)
            {
                try
                {
                    UDPClient.SendBytes(frameToSend);
                }
                catch (Exception e)
                {
                    Debug.LogError("Error en el hilo de red " + e.Message);
                }
            }
            else
            {
                Thread.Sleep(5);
            }
        }
    }

    //byte[] _data;
    bool alreadySent = true;
    void WriteTXTFile()
    {
        if (!alreadySent) return;
        alreadySent = false;
        GetTextureTraduction();
    }
    #endregion
}
