using UnityEngine;
using StreamingCSharp;
using UnityEngine.SceneManagement;
using System.Drawing;
using System.IO.Compression;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Collections;
using System;

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
    }
    private void Start()
    {
        SetTextureForCamera();
        //StartCoroutine("update");
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
        capturadora.CopyFrom(playerCamera);

        playerCamera.targetTexture = captured;
        //capturadora.targetTexture = captured;
        parse = new Texture2D(640, 480, TextureFormat.RGB565, false);
    }

    private void LateUpdate()
    {
        this.WriteTXTFile();
    }

    Texture2D parse;
    Rect rect = new Rect(0, 0, 640, 480);

    private async void GetTextureTraduction()
    {
        RenderTexture.active = this.playerCamera.targetTexture;
        this.playerCamera.Render();
        this.parse.ReadPixels(rect, 0, 0, false);
        //yield return new WaitForEndOfFrame();
        byte[]  _data = parse.GetRawTextureData();

        //Compress the byte[]
        MemoryStream ms = new MemoryStream();
        using (DeflateStream deflate = new DeflateStream(ms, System.IO.Compression.CompressionLevel.Optimal, false))
        {
            deflate.Write(_data, 0, _data.Length);
            deflate.Close();
        }
        _data = ms.ToArray();
        await HttpClient_Custom.SendData(_data);
        alreadySent = true;
        ms.Close();
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
