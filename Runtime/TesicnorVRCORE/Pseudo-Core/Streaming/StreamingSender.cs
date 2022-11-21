using UnityEngine;
using System.Net;
using System.IO;
using StreamingCSharp;
using System.Collections;
using System.Threading.Tasks;
using System.Text;

public class StreamingSender : MonoBehaviour
{
    #region PARAMETERS
    public RenderTexture captured;
    public string path;
    private Texture2D traduced;
    private Camera capturadora;
    HttpClient_Custom client;
    #endregion

    #region FUNCTIONS
    private async void Start()
    {
        SetTextureForCamera();
        
        traduced = new Texture2D(1920,1080);
        client = new HttpClient_Custom();
        HttpClient_Custom.IntializeClient();
        await WriteTXTFile();
        
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

    private async void Update()
    {
        await WriteTXTFile();
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

    async Task WriteTXTFile()
    {
        byte[] jpg = GetTextureTraduction();
        //File.WriteAllBytes(path, jpg);
        await HttpClient_Custom.SendData(jpg);
    }
    #endregion
}
