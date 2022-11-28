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

        parse = new Texture2D(640, 480, TextureFormat.RGB565, false);
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
        Debug.Log(parse.format);

        //Compress the byte[]
        MemoryStream ms = new MemoryStream();
        await Task.Run(() =>
        {
            using (DeflateStream deflate = new DeflateStream(ms, System.IO.Compression.CompressionLevel.Optimal, false))
            {
                deflate.Write(_data, 0, _data.Length);
                deflate.Close();
            }
            _data = ms.ToArray();
            ms.Close();
        });

        if (needsResize(_data))
        {
            captured.width = 480;
            captured.height = 360;
            parse.Reinitialize(480, 360);
            
            parse.ReadPixels(rect, 0, 0, false);
            //_data = parse.GetRawTextureData();
            _data = parse.GetRawTextureData();
            Debug.Log(parse.format);
            
            //Compress the byte[]
            MemoryStream _ms = new MemoryStream();
            await Task.Run(() =>
            {
                using (DeflateStream deflate = new DeflateStream(_ms, System.IO.Compression.CompressionLevel.Optimal, false))
                {
                    deflate.Write(_data, 0, _data.Length);
                    deflate.Close();
                }
                _data = _ms.ToArray();
                _ms.Close();
            });
            parse.Reinitialize(640, 480);
            captured.width = 640;
            captured.height = 480;
        }
        await HttpClient_Custom.SendData(_data);
        alreadySent = true;
    }

    bool needsResize(byte[] img)
    {
        if(img.Length > 50000)
        {
            return true;
        }

        return false;
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
