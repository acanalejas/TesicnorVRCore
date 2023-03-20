using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using UnityEngine.Video;
#endif

#if UNITY_EDITOR
public class ScreenshotTool : EditorWindow
{
    #region FIELDS
    private static Camera renderCamera;
    private static Camera screenShotCamera;

    private static RenderTexture renderTexture;
    private static RenderTexture screenshotTexture;

    private static VideoPlayer videoPlayer;
    

    public bool jpg = true, png = true, tga = true;

    public string savePath { get { return Path.GetFullPath("Assets/Images/Screenshots"); } }
    public string fileName = "Screenshot";

    //Camera Settings
    float fieldOfView = 60;
    float nearPlane = 0.01f;
    float farPlane = 1000;
    bool ortographic = false;
    float size = 5;
    #endregion

    #region METHODS
    [MenuItem("Window/Photo Mode")]
    public static void ShowWindow()
    {
        EditorWindow window = EditorWindow.GetWindow(typeof(ScreenshotTool));

        if (!window)
        {
            window = EditorWindow.CreateWindow<ScreenshotTool>();
        }
        window.ShowUtility();
    }

    /// <summary>
    /// Cuando la ventana se activa
    /// </summary>
    public void OnEnable()
    {
        //Crea las render textures que necesitamos
        renderTexture = new RenderTexture(1920,1080, 32, RenderTextureFormat.ARGB32);
        screenshotTexture = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB4444);

        //Crea las camaras necesarias
        GameObject renderCamera_go = new GameObject("RenderCamera_GO", typeof(Camera));
        GameObject screenshotCamera_go = new GameObject("ScreenshotCamera_GO", typeof(Camera));

        //Asigna las variables de las camaras para no andar buscando todo el rato en el gameobject
        renderCamera = renderCamera_go.GetComponent<Camera>();
        screenShotCamera = screenshotCamera_go.GetComponent<Camera>();

        //Asigna las rendertextures
        renderCamera.targetTexture = renderTexture;
        screenShotCamera.targetTexture = screenshotTexture;
    }

    /// <summary>
    /// Se lanza cuando la ventana se cierra
    /// </summary>
    public void OnDisable()
    {
        //Destruye las camaras
        DestroyImmediate(screenShotCamera.gameObject);
        DestroyImmediate(renderCamera.gameObject);
    }

    //Bool para el boton de sacar screenshot
    bool screenshot;
    public void OnGUI()
    {
        //En caso de que se haya pulsado el botón de screenshot
        if (screenshot) TakeScreenshot();
        else { 
            DisplayRender();
            //Crea un area a la izquierda con unos márgenes para situar ahi las variables
            GUILayout.BeginArea(new Rect(20, 80, this.position.width / 6 - 20, this.position.height - 160), EditorStyles.helpBox);
            DisplayCameraSettings();

            GUILayout.Space(20);

            DisplaySaveSettings();
            GUILayout.EndArea();
        }

        screenshot = GUILayout.Button("TAKE SCREENSHOT");

        //Establece la posición y tamaño de la ventana para que no cambie
        Rect windowRect = new Rect(this.position.x, this.position.y, 1920, 1080);
        this.position = windowRect;

        MoveCamera();
        RotateCamera();

        this.Repaint();
    }

    /// <summary>
    /// Dibuja lo que renderiza la camara en la ventana
    /// </summary>
    public void DisplayRender()
    {
        renderCamera.Render();

        GUI.DrawTexture(new Rect(0, 0, this.position.width, this.position.height), renderTexture);
    }

    /// <summary>
    /// Renderiza con la camara de screenshot y guarda el resultado en un archivo
    /// </summary>
    public void TakeScreenshot()
    {
        //Establece la renderTexture activa y la asigna a la camara
        RenderTexture.active = screenshotTexture;
        screenShotCamera.targetTexture = screenshotTexture;

        //Copia los valores del transform de la otra cámara
        screenShotCamera.transform.position = renderCamera.transform.position;
        screenShotCamera.transform.rotation = renderCamera.transform.rotation;
        screenShotCamera.transform.localScale = renderCamera.transform.localScale;

        //Copia los valores del componente de la camara de la otra camara
        screenShotCamera.CopyFrom(renderCamera);

        screenShotCamera.Render();
        //Creamos la textura que usaremos para guardar la imagen, debe ser del mismo formato que la render texture o dara error
        Texture2D toSave = new Texture2D(1920, 1080, TextureFormat.RGBA32, false);

        //La textura copia lo que renderiza la otra camara
        toSave.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        toSave.Apply();

        //Creamos los buffer en los que guardaremos la información de la imagen en bytes segun el formato de archivo que queramos guardar
        byte[] toSave_jpg = null;
        byte[] toSave_png = null;
        byte[] toSave_tga = null;

        //Comprueba los formatos de archivo seleccionados y rellena los buffers en consecuencia
        if (jpg) toSave_jpg = toSave.EncodeToJPG();
        if (png) toSave_png = toSave.EncodeToPNG();
        if (tga) toSave_tga = toSave.EncodeToTGA();

        //Recogemos el path del archivo a guardar excepto el formato ej: .png
        string path = savePath + "/" + fileName;

        //En caso de que no exista la carpeta en la que queremos guardar las capturas, la creamos
        if(!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);

        //Guarda la imagen como jpg
        if(toSave_jpg != null)
        {
            FileStream fs = File.Open(path + ".jpg", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            fs.SetLength(0);
            fs.Write(toSave_jpg, 0, toSave_jpg.Length);
            fs.Close();
        }

        //Guarda la imagen como png
        if(toSave_png != null)
        {
            FileStream fs = File.Open(path + ".png", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            fs.Write(toSave_png,0, toSave_png.Length);
            fs.Close();
        }

        //Guarda la imagen como tga
        if(toSave_tga != null)
        {
            FileStream fs = File.Open(path + ".tga", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            fs.Write(toSave_tga,0, toSave_tga.Length);
            fs.Close();
        }

        //El formato EXR se ha dejado fuera dado que daba error directamente
    }

    /// <summary>
    /// Enseña settings de la camara en la ventana para modificarlas a nuestro gusto
    /// </summary>
    public void DisplayCameraSettings()
    {
        GUILayout.Label("Camera Settings", EditorStyles.boldLabel);

        GUILayout.Space(20);

        if (renderCamera.orthographic)
        {
            GUILayout.Label("Size", EditorStyles.boldLabel);
            size = EditorGUILayout.FloatField(size, EditorStyles.miniTextField);
            renderCamera.orthographicSize = size;
        }
        else
        {
            GUILayout.Label("Field of View", EditorStyles.boldLabel);
            fieldOfView = GUILayout.HorizontalSlider(fieldOfView, 0, 179);
            renderCamera.fieldOfView = fieldOfView;
        }

        GUILayout.Space(20);

        GUILayout.Label("Clipping Planes Near", EditorStyles.boldLabel);
        nearPlane = EditorGUILayout.FloatField(nearPlane, EditorStyles.miniTextField);
        renderCamera.nearClipPlane = nearPlane;

        GUILayout.Space(20);

        GUILayout.Label("Clipping Planes Far", EditorStyles.boldLabel);
        farPlane = EditorGUILayout.FloatField(farPlane, EditorStyles.miniTextField);
        renderCamera.farClipPlane = farPlane;
    }

    /// <summary>
    /// Enseña en la ventana las settings que se usan para guardar la foto para que se puedan retocar al gusto
    /// </summary>
    public void DisplaySaveSettings()
    {
        GUILayout.Label("Save Settings", EditorStyles.boldLabel);

        GUILayout.Space(20);
        GUILayout.Label("File formats", EditorStyles.boldLabel);
        jpg = GUILayout.Toggle(jpg, "JPG");
        png = GUILayout.Toggle(png, "PNG");
        tga = GUILayout.Toggle(tga, "TGA");

        GUILayout.Space(20);

        GUILayout.Label("File Name", EditorStyles.boldLabel);
        fileName = GUILayout.TextField(fileName, EditorStyles.miniTextField);
    }

    /// <summary>
    /// Mueve la camara segun el input del teclado, tipico WASD
    /// </summary>
    public void MoveCamera()
    {
        Vector3 direction = Vector3.zero;
        if (Event.current.isKey)
        {
            switch (Event.current.keyCode)
            {
                case KeyCode.A:
                    direction = -renderCamera.transform.right;
                    break;
                case KeyCode.S:
                    direction = -renderCamera.transform.forward;
                    break;
                case KeyCode.D:
                    direction = renderCamera.transform.right;
                    break;
                case KeyCode.W:
                    direction = renderCamera.transform.forward;
                    break;
            }
        }

        renderCamera.transform.position = Vector3.Lerp(renderCamera.transform.position, renderCamera.transform.position + direction, Time.deltaTime *10);
    }

    Vector3 initialMousePosition;
    Vector3 finalMousePosition;
    Vector3 initialCameraRotation;
    Vector3 finalCameraRotation;
    /// <summary>
    /// Rota la camara en funcion del raton cuando se clicka y arrastra
    /// </summary>
    public void RotateCamera()
    {
        if(Event.current.type == EventType.MouseDown)
        {
            initialMousePosition = Event.current.mousePosition;
            initialCameraRotation = renderCamera.transform.rotation.eulerAngles;
        }

        if(Event.current.type == EventType.MouseUp || Event.current.type == EventType.MouseDrag)
        {
            finalMousePosition = Event.current.mousePosition;

            Vector3 mouseDistance = finalMousePosition - initialMousePosition;

            float x = mouseDistance.x / this.position.width * 360;

            float y = mouseDistance.y / this.position.height * 120;

            finalCameraRotation = initialCameraRotation + new Vector3(y, x, 0);
        }

        renderCamera.transform.rotation = Quaternion.Euler(finalCameraRotation);
    }
    #endregion
}
#endif