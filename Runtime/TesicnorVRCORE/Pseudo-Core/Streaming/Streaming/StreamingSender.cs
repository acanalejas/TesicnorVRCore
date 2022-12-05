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
    Texture2D parse;
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

        playerCamera.targetTexture = captured;
        //capturadora.targetTexture = captured;
        parse = new Texture2D(640, 480, TextureFormat.RGB565, false);
    }


    WaitForEndOfFrame frame = new WaitForEndOfFrame();
    private IEnumerator update()
    {
        while (true)
        {
            this.WriteTXTFile();
            yield return frame;
        }
    }

    
    Rect rect = new Rect(0, 0, 640, 480);

    private async void GetTextureTraduction()
    {
        RenderTexture.active = this.playerCamera.targetTexture;
        parse.ReadPixels(rect, 0, 0, false);
        parse.Apply();
        byte[] _data = parse.EncodeToJPG();

        await HttpClient_Custom.SendData(_data);
        alreadySent = true;
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
