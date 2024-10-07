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

    [Header("El tamaño inicial del pincel")]
    [SerializeField] private float initialBrushScale = 0.2f;

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
    private int maxBrushCount = 30;

    /// <summary>
    /// La lista de pinceles spawneados
    /// </summary>
    private List<GameObject> spawnedBrushes = new List<GameObject>();

    /// <summary>
    /// Lista de pinceles activos
    /// </summary>
    private List<GameObject> activeBrushes = new List<GameObject>();

    /// <summary>
    /// El ultimo objeto que se pinto
    /// </summary>
    private GameObject lastPainted;

    Dictionary<GameObject, Texture2D> _textures = new Dictionary<GameObject, Texture2D>();

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
        CreateBrushPool(30);
        CurrentBrushScale = initialBrushScale;
    }

    private void CreateBrushPool(int amount)
    {
        for(int i = 0; i < amount; i++)
        {
            GameObject newBrush = GameObject.Instantiate(brushPrefab, brushContainer.transform);
            newBrush.transform.localPosition = Vector3.zero;
            newBrush.transform.localScale = Vector3.one;

            newBrush.SetActive(false);
            spawnedBrushes.Add(newBrush);
        }
    }

    private GameObject GetBrushFromPool(Vector3 worldPosition, Vector3 localScale)
    {
        if (spawnedBrushes.Count <= 0) CreateBrushPool(10);

        GameObject brush = spawnedBrushes[0];

        brush.SetActive(true);
        brush.transform.localPosition = worldPosition;
        brush.transform.localScale = localScale;

        spawnedBrushes.Remove(brush);
        activeBrushes.Add(brush);

        return brush;
    }

    private void ReturnToPool()
    {
        foreach(var brush in activeBrushes)
        {
            brush.SetActive(false);
            spawnedBrushes.Add(brush);
        }
        activeBrushes.Clear();
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

        if (!img) return;
        if(brushSprite)
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
        if (!GO || uvCoordinates == Vector3.zero) return;
        if (lastPainted == null || (lastPainted && lastPainted.name != GO.name))
        {
            if(!_textures.ContainsKey(GO))
            _textures.Add(GO, new Texture2D(1024, 1024, TextureFormat.RGB565, false));
            Debug.Log(_textures.Count);
            Debug.Log(lastPainted);

            if (GO.GetComponent<MeshRenderer>())
            {
                proyectionRenderer.material.mainTexture = GO.GetComponent<MeshRenderer>().material.mainTexture;
            }
            else if (GO.GetComponent<SkinnedMeshRenderer>())
            {
                proyectionRenderer.materials[0].mainTexture = GO.GetComponent<SkinnedMeshRenderer>().materials[0].mainTexture;
            }
            lastPainted = GO;

            DestroyBrushes();
            Debug.Log(_textures.Count);
            //if (lastPainted == proyectionRenderer.gameObject) lastPainted = null;
        }

        Vector3 worldPosition = Vector3.zero;
        worldPosition.x = uvCoordinates.x - proyectionCamera.orthographicSize;
        worldPosition.y = uvCoordinates.y - proyectionCamera.orthographicSize;
        worldPosition.z = 0.0f;

        Vector3 localScale = new Vector3(CurrentBrushScale, CurrentBrushScale, CurrentBrushScale);

        GameObject newBrush = GetBrushFromPool(worldPosition, localScale);
        //newBrush.transform.localPosition = worldPosition;
        //newBrush.transform.localScale = localScale;
        //spawnedBrushes.Add(newBrush);

        proyectionCamera.targetTexture = proyectionRT;
        RenderTexture.active = proyectionRT;
        proyectionCamera.Render();

        _textures[GO].ReadPixels(rect, 0, 0, false);
        _textures[GO].Apply();

        //proyectionRenderer.sharedMaterial.mainTexture = _texture;
        if (GO && GO.GetComponent<MeshRenderer>())
        {
            GO.GetComponent<MeshRenderer>().material.mainTexture = _textures[GO];
        }
        else if (GO && GO.GetComponent<SkinnedMeshRenderer>())
        {
            GO.GetComponent<SkinnedMeshRenderer>().sharedMaterial.mainTexture = _textures[GO];
        }

        //Destroy(newBrush);
        if(activeBrushes.Count > maxBrushCount)
        {
            proyectionRenderer.material.mainTexture = _textures[GO];
            DestroyBrushes();
        }
        //StartCoroutine(ParseTextureAndApply(GO));
    }

    private void DestroyBrushes()
    {
        ReturnToPool();
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
