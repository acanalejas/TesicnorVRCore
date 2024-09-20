using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
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
        if (GO)
        {
            Debug.Log("Detecta el gameObject");
            if (GO.GetComponent<MeshRenderer>())
            {
                Debug.Log("Intentando asignar la textura");
                //proyectionRenderer.material.mainTexture = GO.GetComponent<MeshRenderer>().materials[0].mainTexture;
            }
            else if (GO.GetComponent<SkinnedMeshRenderer>())
            {
                proyectionRenderer.materials[0].mainTexture = GO.GetComponent<SkinnedMeshRenderer>().materials[0].mainTexture;
            }

            lastPainted = GO;
        }

        Vector3 worldPosition = Vector3.zero;
        worldPosition.x = uvCoordinates.x - proyectionCamera.orthographicSize;
        worldPosition.y = uvCoordinates.y - proyectionCamera.orthographicSize;
        worldPosition.z = 0.0f;

        GameObject newBrush = GameObject.Instantiate(brushPrefab, brushContainer.transform);
        newBrush.transform.localPosition = worldPosition;
        spawnedBrushes.Add(newBrush);

        RenderTexture.active = proyectionRT;
        //proyectionRT.width = proyectionRenderer.sharedMaterial.mainTexture ? proyectionRenderer.sharedMaterial.mainTexture.width : 512;
        //proyectionRT.height = proyectionRenderer.sharedMaterial.mainTexture ? proyectionRenderer.sharedMaterial.mainTexture.height : 512;
        
        Texture2D _texture = new Texture2D(proyectionRT.width, proyectionRT.height, TextureFormat.ARGB32, false);
        _texture.ReadPixels(new Rect(0, 0, proyectionRT.width, proyectionRT.height), 0, 0);
        //for(int h = 0; h < proyectionRT.height; h++)
        //{
        //    for(int w = 0; w < proyectionRT.width; w++)
        //    {
        //        if (_texture.GetPixel(w, h) == Color.white) _texture.SetPixel(w, h, Color.clear);
        //    }
        //}
        _texture.Apply();

        //proyectionRenderer.sharedMaterial.mainTexture = _texture;
        if (lastPainted && lastPainted.GetComponent<MeshRenderer>())
        {
            lastPainted.GetComponent<MeshRenderer>().sharedMaterial.SetTexture(1, _texture);
        }
        else if (lastPainted && lastPainted.GetComponent<SkinnedMeshRenderer>())
        {
            lastPainted.GetComponent<SkinnedMeshRenderer>().sharedMaterial.mainTexture = _texture;
        }

        //Destroy(newBrush);
        if(spawnedBrushes.Count > maxBrushCount)
        {
            foreach(var brush in spawnedBrushes) Destroy(brush);
            spawnedBrushes.Clear();
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
