using UnityEngine;
using System.Net;
using System.IO;
using StreamingCSharp;

public class StreamingSender : MonoBehaviour
{
    #region PARAMETERS
    public RenderTexture captured;
    public string path;
    private Texture2D traduced;
    private Camera capturadora;
    #endregion

    #region FUNCTIONS
    private void Start()
    {
        SetTextureForCamera();
        
        traduced = new Texture2D(1920,1080);
    }
    private void SetTextureForCamera()
    {
        Camera mainCamera = Camera.main;

        GameObject capturadora_go = new GameObject("capturadora", typeof(Camera));
        capturadora_go.transform.parent = mainCamera.gameObject.transform;
        capturadora_go.transform.localPosition = Vector3.zero;
        capturadora_go.transform.localRotation = Quaternion.Euler(Vector3.zero);
        capturadora_go.transform.localScale = Vector3.one;

        capturadora = capturadora_go.GetComponent<Camera>();
        capturadora.targetTexture = captured;
        capturadora.Render();
    }

    private void Update()
    {
        WriteTXTFile();
    }
    private byte[] GetTextureTraduction()
    {
        Texture2D parse = new Texture2D(captured.width,captured.height, TextureFormat.ARGB32, false);
        RenderTexture.active = captured;
        parse.ReadPixels(new Rect(0, 0, parse.width, parse.height), 0, 0);
        parse.Apply();
        byte[] jpg = parse.EncodeToJPG();
        return jpg;
    }

    void WriteTXTFile()
    {
        byte[] jpg = GetTextureTraduction();
        //File.WriteAllBytes(path, jpg);
        HttpClient_Custom.SendData(jpg).Wait();
    }
    #endregion
}
