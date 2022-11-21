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
    private static StreamingSender instance;
    public RenderTexture captured;
    public string path;
    private Camera capturadora;
    #endregion

    #region FUNCTIONS
    private void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        SetTextureForCamera();
        
        HttpClient_Custom.IntializeClient();

        DontDestroyOnLoad(this.gameObject);
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
        captured.width = 1920;
        captured.height = 1080;
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
        Destroy(parse);
        return jpg;
    }

    bool alreadySended = true;
    async Task WriteTXTFile()
    {
        if (!alreadySended) return;
        alreadySended = false;
        byte[] jpg = GetTextureTraduction();
        //File.WriteAllBytes(path, jpg);
        var task = Task.Run(() =>
        {
            HttpClient_Custom.SendData(jpg).Wait();
        });

        alreadySended = true;
    }
    #endregion
}
