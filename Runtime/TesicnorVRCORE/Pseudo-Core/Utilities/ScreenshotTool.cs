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

    public void OnEnable()
    {
        renderTexture = new RenderTexture(1920,1080, 32, RenderTextureFormat.ARGB32);
        screenshotTexture = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB4444);

        videoPlayer = new VideoPlayer();

        GameObject renderCamera_go = new GameObject("RenderCamera_GO", typeof(Camera));
        GameObject screenshotCamera_go = new GameObject("ScreenshotCamera_GO", typeof(Camera));

        renderCamera = renderCamera_go.GetComponent<Camera>();
        screenShotCamera = screenshotCamera_go.GetComponent<Camera>();

        renderCamera.targetTexture = renderTexture;
        screenShotCamera.targetTexture = screenshotTexture;
    }

    public void OnDisable()
    {
        DestroyImmediate(screenShotCamera.gameObject);
        DestroyImmediate(renderCamera.gameObject);
    }

    bool screenshot;
    public void OnGUI()
    {
        if (screenshot) TakeScreenshot();
        else { DisplayRender();
            GUILayout.BeginArea(new Rect(20, 80, this.position.width / 6 - 20, this.position.height - 160), EditorStyles.helpBox);
            DisplayCameraSettings();

            GUILayout.Space(20);

            DisplaySaveSettings();
            GUILayout.EndArea();
        }

        screenshot = GUILayout.Button("TAKE SCREENSHOT");

        Rect windowRect = new Rect(0, 0, 1920, 1080);
        this.position = windowRect;

        MoveCamera();
        RotateCamera();

        this.Repaint();
    }

    public void DisplayRender()
    {
        renderCamera.Render();

        GUI.DrawTexture(new Rect(0, 0, this.position.width, this.position.height), renderTexture);
    }

    public void TakeScreenshot()
    {
        RenderTexture.active = screenshotTexture;
        screenShotCamera.targetTexture = screenshotTexture;

        screenShotCamera.transform.position = renderCamera.transform.position;
        screenShotCamera.transform.rotation = renderCamera.transform.rotation;
        screenShotCamera.transform.localScale = renderCamera.transform.localScale;

        screenShotCamera.CopyFrom(renderCamera);

        screenShotCamera.Render();
        Texture2D toSave = new Texture2D(1920, 1080, TextureFormat.RGBA32, false);

        toSave.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        toSave.Apply();

        byte[] toSave_jpg = null;
        byte[] toSave_png = null;
        byte[] toSave_tga = null;

        if (jpg) toSave_jpg = toSave.EncodeToJPG();
        if (png) toSave_png = toSave.EncodeToPNG();
        if (tga) toSave_tga = toSave.EncodeToTGA();

        string path = savePath + "/" + fileName;

        if(!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);

        if(toSave_jpg != null)
        {
            FileStream fs = File.Open(path + ".jpg", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            fs.SetLength(0);
            fs.Write(toSave_jpg, 0, toSave_jpg.Length);
            fs.Close();
        }

        if(toSave_png != null)
        {
            FileStream fs = File.Open(path + ".png", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            fs.Write(toSave_png,0, toSave_png.Length);
            fs.Close();
        }

        if(toSave_tga != null)
        {
            FileStream fs = File.Open(path + ".tga", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            fs.Write(toSave_tga,0, toSave_tga.Length);
            fs.Close();
        }
    }

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