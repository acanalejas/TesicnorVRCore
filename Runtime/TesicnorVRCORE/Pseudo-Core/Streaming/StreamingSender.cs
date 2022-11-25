using UnityEngine;
using StreamingCSharp;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using System.Drawing;
using System.IO.Compression;
using System.IO;

public class StreamingSender : MonoBehaviour
{
    #region PARAMETERS
    private static StreamingSender instance;
    public RenderTexture captured;
    public string path;
    private Camera capturadora;
    public Camera playerCamera;
    private static ImageConverter converter;
    #endregion

    #region FUNCTIONS
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        HttpClient_Custom.IntializeClient();
        DontDestroyOnLoad(this);

        SceneManager.sceneLoaded += OnSceneChanged;
    }
    private void Start()
    {
        SetTextureForCamera();
        StartCoroutine("update");
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
        capturadora.Render();

        parse = new Texture2D(640, 480, TextureFormat.RGB24, false);
    }

    private IEnumerator update()
    {
        while (true)
        {
            this.WriteTXTFile();
            yield return new WaitForSecondsRealtime(1 / 50);

        }
        
    }
    Texture2D parse;
    Rect rect = new Rect(0, 0, 640, 480);
    private async void GetTextureTraduction()
    {
        RenderTexture.active = captured;
        parse.ReadPixels(rect,0,0,false);
        //_data = parse.GetRawTextureData();
        _data = parse.GetRawTextureData();

        //Compress the byte[]
        MemoryStream ms = new MemoryStream();
        await Task.Run(() =>
        {
            using (DeflateStream deflate = new DeflateStream(ms, System.IO.Compression.CompressionLevel.Fastest, false))
            {
                deflate.Write(_data, 0, _data.Length);
                deflate.Close();
            }
            _data = ms.ToArray();
            ms.Close();
        }); 
        await HttpClient_Custom.SendData(_data);
        alreadySent = true;
    }


    byte[] _data;
    bool alreadySent = true;
    void WriteTXTFile()
    {
        if (!alreadySent) return;
        alreadySent = false;
        GetTextureTraduction();
    }

    private bool hasCamera()
    {
        if (!capturadora)
        {
            SetTextureForCamera();
        }

        return true;
    }

    public void OnSceneChanged(Scene scene, LoadSceneMode mode)
    {
        //CancelInvoke(nameof(update));
        if (CameraGetter.Instance)
        {
            playerCamera = CameraGetter.Instance.playerCamera;
            this.Start();
        }
    }
    #endregion
}
