using UnityEngine;
using StreamingCSharp;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class StreamingSender : MonoBehaviour
{
    #region PARAMETERS
    private static StreamingSender instance;
    public RenderTexture captured;
    public string path;
    private Camera capturadora;
    #endregion

    #region FUNCTIONS
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        DontDestroyOnLoad(this.gameObject);

        parse = new Texture2D(1920, 1080, TextureFormat.ARGB32, false);

        HttpClient_Custom.IntializeClient();
    }
    private void Start()
    {
        SetTextureForCamera();
    }
    private void SetTextureForCamera()
    {
        Camera mainCamera = GameObject.FindObjectOfType<Camera>();

        GameObject capturadora_go = new GameObject("capturadora", typeof(Camera));
        capturadora_go.transform.parent = mainCamera.gameObject.transform;
        capturadora_go.transform.localPosition = Vector3.zero;
        capturadora_go.transform.localRotation = Quaternion.Euler(Vector3.zero);
        capturadora_go.transform.localScale = Vector3.one;

        capturadora = capturadora_go.GetComponent<Camera>();
        capturadora.targetTexture = captured;
        capturadora.Render();

        parse = new Texture2D(captured.width, captured.height, TextureFormat.ARGB32, false);
    }

    private async void Update()
    {
        hasCamera();
        await WriteTXTFile();
    }
    Texture2D parse;
    private byte[] GetTextureTraduction()
    {
        RenderTexture.active = captured;
        parse.ReadPixels(new Rect(0, 0, captured.width, captured.height), 0, 0);
        parse.Apply();
        jpg = parse.EncodeToJPG();
        return jpg;
    }

    bool alreadySended = true;
    byte[] jpg;
    async Task WriteTXTFile()
    {
        if (!alreadySended) return;
        alreadySended = false;
        jpg = GetTextureTraduction();
        //File.WriteAllBytes(path, jpg);
        try
        {
            await HttpClient_Custom.SendData(jpg);
        }
        catch
        {
            HttpClient_Custom.IntializeClient();
            await HttpClient_Custom.SendData(jpg);
        }

        alreadySended = true;
    }

    private void OnLevelWasLoaded(int level)
    {
        Start();
    }

    private bool hasCamera()
    {
        if (!capturadora)
        {
            capturadora = GameObject.FindObjectOfType<Camera>();
            SetTextureForCamera();
        }

        return true;
    }
    #endregion
}
