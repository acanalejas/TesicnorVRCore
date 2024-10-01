using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectPainter : MonoBehaviour
{
    #region PARAMETERS
    [Header("La camara que se encarga de proyectar el color")]
    [SerializeField] private Camera proyectionCamera;

    [Header("El objeto que va a servir de origen para pintar")]
    [SerializeField] private GameObject brushContainer;

    [Header("El objeto que sirve como prefab del pincel")]
    [SerializeField] private GameObject brushPrefab;

    [Header("La Render Texture sobre la que se va a proyectar la textura pintada")]
    [SerializeField] private RenderTexture proyectionRT;

    [Header("El meshrenderer del Quad que actua como lienzo")]
    [SerializeField] private MeshRenderer proyectionRenderer;

    [Header("El color del que se va a pintar")]
    [SerializeField] private Color brushColor;

    [Header("El sprite que se va a usar como pincel")]
    [SerializeField] private Sprite brushSprite;

    [Header("La opacidad del pincel")]
    [SerializeField] private float brushOpacity = 0.75f;

    [Header("El tamaño máximo del pincel (escala)")]
    [SerializeField] private float maxBrushScale = 2;

    [Header("El tamaño mínimo del pincel (escala)")]
    [SerializeField] private float minBrushScale = 1;

    /// <summary>
    /// Valor de almacenamiento, no usar
    /// </summary>
    private float currentBrushScale = 1;
    /// <summary>
    /// El tamaño actual del pincel
    /// </summary>
    public float CurrentBrushScale { get { return currentBrushScale; } set { currentBrushScale = Mathf.Clamp(value, minBrushScale, maxBrushScale); } }

    /// <summary>
    /// El material que esta siendo pintado (Comprobar que no sea null)
    /// </summary>
    private Material paintedMaterial;

    /// <summary>
    /// La cantidad de brushes instanciados en el momento, para que no sean infinitos
    /// </summary>
    private int brushCount = 0;

    /// <summary>
    /// La cantidad máxima de brushes que se pueden llegar a instanciar antes de guardar
    /// </summary>
    private int maxBrushCount = 100;

    /// <summary>
    /// La lista de pinceles spawneados
    /// </summary>
    private List<GameObject> spawnedBrushes = new List<GameObject>();

    /// <summary>
    /// El ultimo objeto que se pinto
    /// </summary>
    private GameObject lastPainted;

    private Texture2D _texture;

    Rect rect = new Rect(0, 0, 1024, 1024);
    #endregion

    #region METHODS

    private void Awake()
    {
        CheckSingleton();
    }

    private void Start()
    {
        CreateBrushPrefab();
        ConfigureCamera();
        _texture = new Texture2D(1024, 1024, TextureFormat.RGB565, false);
    }

    private void CreateBrushPrefab()
    {
        if (brushPrefab != null) return;

        brushPrefab = new GameObject("brushPrefab DO NOT TOUCH", typeof(SpriteRenderer));
        SpriteRenderer img = brushPrefab.GetComponent<SpriteRenderer>();

        img.sprite = brushSprite;
        img.color = new Color(brushColor.r, brushColor.g, brushColor.b, brushOpacity);

        brushPrefab.transform.localScale = Vector3.one * CurrentBrushScale;
    }

    private void UpdateBrushPrefab()
    {
        if (!brushPrefab) { CreateBrushPrefab(); return; }

        brushPrefab.transform.localEulerAngles = Vector3.one * CurrentBrushScale;

        Image img = brushPrefab.GetComponent<Image>();
        img.sprite = brushSprite;
        img.color = new Color(brushColor.r, brushColor.g, brushColor.b, brushOpacity);
    }

    #region Set parameters
    public void SetBrushSize(float size)
    {
        CurrentBrushScale = size;
        UpdateBrushPrefab();
    }

    public void SetBrushColor(Color color)
    {
        brushColor = color;
        UpdateBrushPrefab();
    }

    public void SetBrushOpacity(float opacity)
    {
        brushOpacity = opacity;
        UpdateBrushPrefab();
    }

    private void ConfigureCamera()
    {
        proyectionCamera.orthographic = true;
        proyectionCamera.orthographicSize = 0.5f;
        proyectionCamera.nearClipPlane = 0.3f;
        proyectionCamera.farClipPlane = 5;
    }
    #endregion
    public void Paint(Vector3 uvCoordinates, GameObject GO)
    {
        goto NormalLoop;

        NormalLoop:
        if (GO && lastPainted != GO)
        {
            _texture = new Texture2D(1024, 1024, TextureFormat.RGB565, false);

            Debug.Log("Detecta el gameObject");
            if (GO.GetComponent<MeshRenderer>())
            {
                Debug.Log("Intentando asignar la textura");
                proyectionRenderer.material.mainTexture = GO.GetComponent<MeshRenderer>().material.mainTexture;
            }
            else if (GO.GetComponent<SkinnedMeshRenderer>())
            {
                proyectionRenderer.materials[0].mainTexture = GO.GetComponent<SkinnedMeshRenderer>().materials[0].mainTexture;
            }
            lastPainted = GO;

            goto DestroyBrushes;
            //if (lastPainted == proyectionRenderer.gameObject) lastPainted = null;
        }

        Vector3 worldPosition = Vector3.zero;
        worldPosition.x = uvCoordinates.x - proyectionCamera.orthographicSize;
        worldPosition.y = uvCoordinates.y - proyectionCamera.orthographicSize;
        worldPosition.z = 0.0f;

        GameObject newBrush = GameObject.Instantiate(brushPrefab, brushContainer.transform);
        newBrush.transform.localPosition = worldPosition;
        spawnedBrushes.Add(newBrush);

        proyectionCamera.targetTexture = proyectionRT;
        RenderTexture.active = proyectionRT;
        proyectionCamera.Render();

        _texture.ReadPixels(rect, 0, 0, false);
        _texture.Apply();

        //proyectionRenderer.sharedMaterial.mainTexture = _texture;
        if (GO && GO.GetComponent<MeshRenderer>())
        {
            Debug.Log("Aplicando textura nueva");
            GO.GetComponent<MeshRenderer>().material.mainTexture = _texture;
        }
        else if (GO && GO.GetComponent<SkinnedMeshRenderer>())
        {
            GO.GetComponent<SkinnedMeshRenderer>().sharedMaterial.mainTexture = _texture;
        }

        //Destroy(newBrush);
        if(spawnedBrushes.Count > maxBrushCount)
        {
            proyectionRenderer.material.mainTexture = _texture;
            goto DestroyBrushes;
        }

        DestroyBrushes:
        foreach (var brush in spawnedBrushes) Destroy(brush);
        spawnedBrushes.Clear();

        //StartCoroutine(ParseTextureAndApply(GO));
    }

    IEnumerator ParseTextureAndApply(GameObject GO)
    {
        yield return new WaitForEndOfFrame();

        _texture.ReadPixels(rect, 0, 0, false);
        _texture.Apply();

        //proyectionRenderer.sharedMaterial.mainTexture = _texture;
        if (GO && GO.GetComponent<MeshRenderer>())
        {
            Debug.Log("Aplicando textura nueva");
            GO.GetComponent<MeshRenderer>().material.mainTexture = _texture;
        }
        else if (GO && GO.GetComponent<SkinnedMeshRenderer>())
        {
            GO.GetComponent<SkinnedMeshRenderer>().sharedMaterial.mainTexture = _texture;
        }
    }
    #endregion

    #region SINGLETON
    private static ObjectPainter instance;
    public static ObjectPainter Instance { get { return instance; } }

    void CheckSingleton()
    {
        if (!instance) instance = this;
        else Destroy(this);
    }
    #endregion
}
