using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
public class SpriteLayer
{
    public Texture2D texture;
    public Texture2D displayTexture;
    public Vector2 position;
    public Vector2 size;
    public int id;
    public float opacity;
}
public class SpritesDrawerWindow : EditorWindow
{
    #region FIELDS
    //Textura que se enseña y que luego se guarda
    public Texture2D sprite;
    Texture2D additionalSprite;

    public List<SpriteLayer> spriteLayers = new List<SpriteLayer>();
    public List<Texture2D> displayTextures = new List<Texture2D>();
    public int actualLayer;

    //Bools que sirven para los botones de la ventana
    bool circle, square, roundSquare, triangle, reset, save, import;

    //Modificadores de las figuras
    float circleRadius = 1, squareX = 1, squareY = 1, squareCornerRadius = 20, polygonSize;

    int polygonSides = 3;

    //Colores que se van a usar
    Color fillColor = Color.black;
    Color backgroundColor = Color.clear;

    public Vector2 positionOffset { get { Vector2 size = GetAreaAdaptativeSize(); return new Vector2(this.position.size.x / 2 - size.x / 2, this.position.size.y / 2 - size.y / 2); } }

    LayersWindow lw;
    #endregion

    #region METHODS
    [MenuItem("Window/Delete Player Prefs")]
    public static void DeletePlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
    [MenuItem("Window/Sprites Drawer")]
    public static void ShowWindow()
    {
        EditorWindow window = GetWindow<SpritesDrawerWindow>();

        if (window == null) window = CreateWindow<SpritesDrawerWindow>();

        window.Show();
    }

    private void OnGUI()
    {
        if (!sprite) sprite = new Texture2D(500, 500, TextureFormat.ARGB32, false);
        DisplaySprite();
        //this.position.Set(position.x, position.y, 500, 500);
        if (circle) DrawCircleBack(sprite.width/2 *circleRadius, fillColor);
        if (square) DrawSquare(squareX, squareY, fillColor);
        if (roundSquare) DrawRoundedSquare(squareX, squareY, squareCornerRadius, fillColor);
        if (triangle) DrawPolygon(polygonSides, polygonSize, fillColor);
        if (reset) ResetSprite();
        if (save) SaveSprite();
        if (import) Import();

        lw.Repaint();
        if (!lw) CreateLayerWindow();
        else UpdateLayersWindow();

        if(spriteLayers.Count > 0 && actualLayer < spriteLayers.Count)
        sprite = spriteLayers[actualLayer].texture;
    }

    void OnEnable()
    {
        if(!sprite)
        sprite = new Texture2D(500, 500, TextureFormat.ARGB32, false);
        ResetSprite();

        spriteLayers.Clear();
        SpriteLayer initialLayer = new SpriteLayer();
        initialLayer.texture = sprite;
        Vector2 size = GetAreaAdaptativeSize();
        initialLayer.position = new Vector2(position.size.x / 2 - size.x / 2, position.size.y / 2 - size.y / 2);
        initialLayer.size = size;
        
        spriteLayers.Add(initialLayer);

        DragAndDropWindow.DragAndDropManipulator.onDrop += OnDrop;

        CreateLayerWindow();
    }

    void CreateLayerWindow()
    {
        lw = EditorWindow.GetWindow<LayersWindow>();
        lw.parent = this;
    }

    void UpdateLayersWindow()
    {
        lw.parent = this;
        lw.spriteLayers = spriteLayers.ToArray();
    }
    private void OnDisable()
    {
        lw.Close();
    }
    /// <summary>
    /// Muestra todo en la ventana
    /// </summary>
    void DisplaySprite()
    {
        Vector2 size = GetAreaAdaptativeSize();
        SetLayerValues(new Vector2(position.size.x / 2 - size.x / 2, position.size.y / 2 - size.y / 2), size, 0);
        //Zona donde están los botones de guardar y resetear
        GUILayout.BeginHorizontal();
        save = GUILayout.Button("Save", EditorStyles.miniButtonMid);
        reset = GUILayout.Button("Reset", EditorStyles.miniButtonRight);
        import = GUILayout.Button("Import", EditorStyles.miniButtonLeft);
        GUILayout.EndHorizontal();

        //Zona donde se encuentran los botones laterales
        GUILayout.BeginArea(new Rect(0, 20, size.x * 0.2f, size.y));
        GUI.backgroundColor = Color.yellow;
        //Botones para crear las distintas formas
        circle = GUILayout.Button("Circle", EditorStyles.miniButton);
        square = GUILayout.Button("Square", EditorStyles.miniButton);
        roundSquare = GUILayout.Button("Round Square", EditorStyles.miniButton);
        triangle = GUILayout.Button("Polygon", EditorStyles.miniButton);

        GUILayout.Space(20);

        //Para seleccionar el color de la figura geometrica a dibujar
        fillColor = EditorGUILayout.ColorField("Fill Color", fillColor);

        GUILayout.Space(20);

        //Los parámetros para dibujar las formas
        GUILayout.Label("CircleRadius", EditorStyles.boldLabel);
        circleRadius = GUILayout.HorizontalSlider(circleRadius, 0.2f, 1f);

        GUILayout.Space(10);
        GUILayout.Label("Square Height", EditorStyles.boldLabel);
        squareY = GUILayout.HorizontalSlider(squareY, 0.1f, 1);

        GUILayout.Space(10);
        GUILayout.Label("Square Width", EditorStyles.boldLabel);
        squareX = GUILayout.HorizontalSlider(squareX, 0.1f, 1);

        GUILayout.Space(10);
        GUILayout.Label("Square Corner Radius", EditorStyles.boldLabel);
        squareCornerRadius = GUILayout.HorizontalSlider(squareCornerRadius, 10, 50);

        GUILayout.Space(10);
        GUILayout.Label("Polygon Size", EditorStyles.boldLabel);
        polygonSize = GUILayout.HorizontalSlider(polygonSize, 0.1f, 1);

        GUILayout.Space(10);
        GUILayout.Label("Polygon Side Number", EditorStyles.boldLabel);
        polygonSides = EditorGUILayout.IntField(polygonSides, EditorStyles.miniTextField);
        polygonSides = Mathf.Clamp(polygonSides, 3, 50);

        GUILayout.EndArea();

        //Dibujado de la textura en la ventana
        DisplayLayers();
    }

    public void DisplayLayers()
    {
        GetDisplayTextures();
        foreach(var layer in spriteLayers)
        {
            GUI.DrawTexture(new Rect(layer.position + new Vector2(0,30), layer.size - new Vector2(40,40)), layer.displayTexture);
        }
    }
    void GetDisplayTextures()
    {

        int[] OpacityValues = lw.opacities.ToArray();
        float[] OpacityMultipliers = new float[OpacityValues.Length];

        for(int i = 0;i < OpacityValues.Length; i++)
        {
            OpacityMultipliers[i] = OpacityValues[i] / 100;
        }

        int index = 0;
        foreach(var layer in spriteLayers)
        {
            Color[] texturePixels = layer.texture.GetPixels();
            if (index > OpacityMultipliers.Length) continue;
            for(int i = 0; i < texturePixels.Length; i++)
            {
                if (texturePixels[i].a > layer.opacity) texturePixels[i].a = layer.opacity;
            }
            Texture2D texture = new Texture2D(layer.texture.width, layer.texture.height, TextureFormat.ARGB32, false);
            texture.SetPixels(texturePixels);
            texture.Apply();
            layer.displayTexture = texture;
            index++;
            Debug.Log(layer.opacity);
        }
    }

    private void SetLayerValues(Vector2 position, Vector2 size, int index)
    {
        SpriteLayer layer = spriteLayers[index];
        layer.position = position; layer.size = size;

        spriteLayers[index] = layer;
    }

    //Adapta la previsualización del sprite creado al tamaño de la ventana
    public Vector2 GetAreaAdaptativeSize()
    {
        Vector2 window_size = position.size;

        bool xIsBigger = window_size.x > window_size.y;
        bool yIsBigger = window_size.y > window_size.x;

        float sizeY = 0;
        float sizeX = 0;

        if (xIsBigger)
        {
            sizeY = window_size.y;
            sizeX = window_size.y;
        }
        else
        {
            sizeY = window_size.x;
            sizeX = window_size.x;
        }

        return new Vector2(sizeX, sizeY);
    }

    /// <summary>
    /// Dibuja un círculo en la textura
    /// </summary>
    /// <param name="radius">Radio del circulo</param>
    /// <param name="center">Centro de la circunferencia</param>
    /// <param name="color">Color del que se va a rellenar</param>
    void DrawCircle(float radius, Vector2 center, Color color)
    {
        float _radius = radius;

        for (int i = 0; i < sprite.width; i++)
        {
            for (int j = 0; j < sprite.height; j++)
            {
                float distance = DistanceBetweenPixels(new Vector2(i, j), center);
                if (distance <= radius && sprite.GetPixel(i, j) == backgroundColor || sprite.GetPixel(i,j) == fillColor) sprite.SetPixel(i, j, color);
                //else if (distance > radius) sprite.SetPixel(i, j, backgroundColor);
            }
        }
        sprite.Apply();
        this.SaveChanges();
    }
    /// <summary>
    /// Dibuja un círculo en la textura
    /// </summary>
    /// <param name="radius">Radio de la circunferencia</param>
    /// <param name="color">Color del que se va a rellenar</param>
    void DrawCircle(float radius, Color color)
    {
        Vector2 center = new Vector2((int)sprite.width / 2, (int)sprite.height / 2);
        float _radius = radius;

        for(int i = 0; i < sprite.width; i++)
        {
            for (int j = 0; j < sprite.height; j++)
            {
                float distance = DistanceBetweenPixels(new Vector2(i, j), center);
                if (distance <= radius) sprite.SetPixel(i, j, color);
                else sprite.SetPixel(i, j, backgroundColor);
            }
        }
        sprite.Apply();
        this.SaveChanges();
    }

    /// <summary>
    /// Dibuja un circulo en la textura
    /// </summary>
    /// <param name="radius">Radio del circulo</param>
    /// <param name="color">Color del que se va a rellenar</param>
    void DrawCircleBack(float radius, Color color)
    {
        Vector2 center = new Vector2((int)sprite.width / 2, (int)sprite.height / 2);
        float _radius = radius;

        for (int i = 0; i < sprite.width; i++)
        {
            for (int j = 0; j < sprite.height; j++)
            {
                float distance = DistanceBetweenPixels(new Vector2(i, j), center);
                if (distance <= radius && sprite.GetPixel(i,j) == backgroundColor) sprite.SetPixel(i, j, color);
                else if(distance > radius) sprite.SetPixel(i, j, backgroundColor);
            }
        }
        sprite.Apply();
        this.SaveChanges();
    }

    /// <summary>
    /// Dibuja un cuadrilátero en la textura
    /// </summary>
    /// <param name="x">Acnhura del cuadrilátero</param>
    /// <param name="y">Altura del cuadrilátero</param>
    /// <param name="color">Color del que se va a rellenar</param>
    void DrawSquare(float x, float y, Color color)
    {
        Vector2 center = new Vector2((int)sprite.width / 2, (int)sprite.height / 2);
        float width = sprite.width * x;
        float height = sprite.height * y;

        float extentX = width / 2;
        float extentY = height / 2;

        for(int i = 0; i < sprite.width; i++)
        {
            for(int j = 0; j< sprite.height; j++)
            {
                if (i > center.x - extentX && i < center.x + extentX && j < center.y + extentY && j > center.y - extentY && sprite.GetPixel(i, j) == backgroundColor) sprite.SetPixel(i, j, color);
                else if(i > center.x + extentX || i < center.x - extentX || j < center.y - extentY || j > center.y + extentY) sprite.SetPixel(i, j, backgroundColor);
            }
        }
        sprite.Apply();
        this.SaveChanges();
    }

    /// <summary>
    /// Dibuja un cuadrilátero con las esquinas redondeadas
    /// </summary>
    /// <param name="x">Anchura del cuadrilátero</param>
    /// <param name="y">Altura del cuadrilátero</param>
    /// <param name="radius">Radio de la circunferencia de la esquina</param>
    /// <param name="color">Color del que se va a rellenar</param>
    void DrawRoundedSquare(float x, float y, float radius, Color color)
    {
        DrawSquare(x, y, color);

        float height = sprite.height * y;
        float width = sprite.width * x;

        float extentX = width / 2;
        float extentY = height / 2;

        Vector2 center = new Vector2(sprite.width / 2, sprite.height / 2);
        Vector2 topLeft = center - new Vector2(extentX, extentY);
        Vector2 downLeft = center + new Vector2(-extentX, extentY);
        Vector2 topRight = center + new Vector2(extentX, -extentY);
        Vector2 downRight = center + new Vector2(extentX, extentY);

        Vector2 topLeftCenter = topLeft + new Vector2(radius, radius);
        Vector2 topRightCenter = topRight + new Vector2(-radius, radius);
        Vector2 downLeftCenter = downLeft + new Vector2(radius, -radius);
        Vector2 downRightCenter = downRight + new Vector2(-radius, -radius);

        for(int i = 0; i < sprite.width; i++)
        {
            for(int j = 0; j < sprite.height; j++)
            {
                Vector2 currentPixel = new Vector2(i, j);
                if (DistanceBetweenPixels(topLeftCenter, currentPixel) > radius && i < topLeftCenter.x && j < topLeftCenter.y) sprite.SetPixel(i, j, backgroundColor);
                if (DistanceBetweenPixels(topRightCenter, currentPixel) > radius && i > topRightCenter.x && j < topRightCenter.y) sprite.SetPixel(i, j, backgroundColor);
                if (DistanceBetweenPixels(downLeftCenter, currentPixel) > radius && i < downLeftCenter.x && j > downLeftCenter.y) sprite.SetPixel(i, j, backgroundColor);
                if (DistanceBetweenPixels(downRightCenter, currentPixel) > radius && i > downRightCenter.x && j > downRightCenter.y) sprite.SetPixel(i, j, backgroundColor);
            }
        }
        sprite.Apply();
        this.SaveChanges();
    }

    /// <summary>
    /// Dibuja un polígono regular
    /// </summary>
    /// <param name="sides">Número de lados del polígono</param>
    /// <param name="size">Tamaño del polígono (0 - 1)</param>
    /// <param name="color">Color del que se va a rellenar</param>
    void DrawPolygon(int sides, float size, Color color)
    {
        Vector2 center = new Vector2(sprite.width / 2, sprite.height / 2);

        size = Mathf.Clamp(size, 0.1f, 1f);
        float _size = size * sprite.width;

        float outRadius = _size / 2;

        Vector2[] points = new Vector2[sides];
        //Los angulos salen bien, el problema es el cálculo de los puntos
        float eachAngle = 360 / sides;

        Vector2 initialPoint = new Vector2(0, outRadius);
        points[0] = center + initialPoint;

        for(int i = 1; i < points.Length; i++)
        {
            Vector2 result = Quaternion.Euler(new Vector3(0,0,eachAngle * i)) * initialPoint;

            points[i] = center + result;
        }

        for(int i = 0; i < sprite.width; i++)
        {
            for(int j = 0; j < sprite.height; j++)
            {
                bool inside = IsPointInsidePolygon(new Vector2(i, j), points);
                if (inside && sprite.GetPixel(i, j) == backgroundColor) sprite.SetPixel(i, j, fillColor);
                else if(!inside) sprite.SetPixel(i, j, backgroundColor);
            }
        }
        sprite.Apply();
        this.SaveChanges();
    }

    /// <summary>
    /// Devuelve la distancia entre dos píxeles de la textura
    /// Se calcula por pitágoras
    /// </summary>
    /// <param name="pixel1">Posición del pixel inicial</param>
    /// <param name="pixel2">Posicion del pixel final</param>
    /// <returns></returns>
    float DistanceBetweenPixels(Vector2 pixel1, Vector2 pixel2)
    {
        float xdist = pixel1.x - pixel2.x;
        float ydist = pixel1.y - pixel2.y;

        return Mathf.Sqrt(xdist * xdist + ydist * ydist);
    }

    /// <summary>
    /// Está el punto dentro del polígono?
    /// </summary>
    /// <param name="point">Punto a comprobar</param>
    /// <param name="polygon">Vertices del poligono</param>
    /// <returns></returns>
    bool IsPointInsidePolygon(Vector2 point, Vector2[] polygon)
    {
        int ptNum = polygon.Length;
        if (ptNum < 3) return false;

        int j = ptNum - 1;
        bool oddNodes = false;

        int zeroState = 0;
        for(int i = 0; i < ptNum; i++)
        {
            Vector2 ptI = polygon[i];
            Vector2 ptJ = polygon[j];

            if(((ptI.y > point.y) != (ptJ.y > point.y)) && (point.x < (ptJ.x - ptI.x) *(point.y - ptI.y)/(ptJ.y - ptI.y) + ptI.x))
            {
                oddNodes = !oddNodes;
                if (ptI.y > ptJ.y) zeroState++;
                else zeroState--;
            }
            j = i;
        }

        return oddNodes;
    }

    /// <summary>
    /// Borra el contenido actual de la textura
    /// </summary>
    public void ResetSprite()
    {
        for(int i = 0; i < sprite.width; i++)
        {
            for(int j = 0; j < sprite.height; j++)
            {
                sprite.SetPixel(i, j, backgroundColor);
            }
        }
        sprite.Apply();
        if(spriteLayers.Count > 1)
        spriteLayers.RemoveRange(1, spriteLayers.Count - 1);
    }
    
    public void ResetSprite(bool _)
    {
        for (int i = 0; i < sprite.width; i++)
        {
            for (int j = 0; j < sprite.height; j++)
            {
                sprite.SetPixel(i, j, backgroundColor);
            }
        }
        sprite.Apply();
    }

    public void ResetSprite(Texture2D source)
    {
        for(int i = 0; i < source.width; i++)
        {
            for(int j = 0; j < source.height; j++)
            {
                source.SetPixel(i, j, Color.clear);
            }
        }
        source.Apply();
    }

    /// <summary>
    /// Activa la ventana de guardado
    /// </summary>
    void SaveSprite()
    {
        SaveWindow sw = EditorWindow.GetWindow<SaveWindow>();
        Texture2D forSave = new Texture2D(500, 500, TextureFormat.ARGB32, false);

        System.Drawing.Bitmap b = new System.Drawing.Bitmap(500, 500);
        System.Drawing.Graphics g = System.Drawing.Graphics.FromImage((System.Drawing.Image)b);
        
        ResetSprite(forSave);
        int index = 0;
        foreach(var layer in spriteLayers)
        {
            Texture2D used = resizeImage(layer.displayTexture, new Vector2(500, 500));
            System.Drawing.Image img = Texture2Image(used);
            g.DrawImage(img, 0,0,img.Width,img.Height);
            //Vector2 position = layer.position - positionOffset;
            //int h = 0, k = 0;
            //for(int i = (int)position.x; i < forSave.width; i++)
            //{
            //    for(int j = (int)position.y; j < forSave.height; j++)
            //    {
            //        Color color = used.GetPixel(h, k);
            //        if(color.a > 0)
            //        forSave.SetPixel(i, j, color);
            //        k++;
            //    }
            //    h++;
            //}
            //index++;
        }
        forSave = Image2Texture(b);
        sw.sprite = forSave;
    }

    void Import()
    {
        DragAndDropWindow ddw = EditorWindow.GetWindow<DragAndDropWindow>();
    }

    void OnDrop(Object dropped)
    {
        if (dropped.GetType() != typeof(Texture2D)) return;

        Texture2D texture = (Texture2D)dropped;
        Vector2 size = GetAreaAdaptativeSize();

        Vector2 _position = new Vector2(this.position.size.x / 2 - size.x / 2, this.position.size.y / 2 - size.y / 2);
        int id = spriteLayers.Count;

        SpriteLayer layer = new SpriteLayer();
        layer.texture = texture;
        layer.size = size;
        layer.position = _position;
        layer.id = id;

        spriteLayers.Add(layer);

        DragAndDropWindow ddw = EditorWindow.GetWindow<DragAndDropWindow>();
        ddw.Close();
    }

    public static System.Drawing.Image Texture2Image(Texture2D texture)
    {
        System.Drawing.Image img;
        byte[] png = texture.EncodeToPNG();
        using (MemoryStream MS = new MemoryStream(png))
        {
            //Create the image based on the stream.
            img = System.Drawing.Bitmap.FromStream(MS);
        }
        return img;
    }

    public Texture2D Image2Texture(System.Drawing.Bitmap image)
    {
        Texture2D result = new Texture2D(500,500, TextureFormat.ARGB32, false);

        for(int i = 0; i < image.Width; i++)
        {
            for(int j = 0; j < image.Height; j++)
            {
                System.Drawing.Color color = image.GetPixel(i, j);
                UnityEngine.Color _color = fromSystemColor(color);

                result.SetPixel(i,j,_color);
            }
        }
        result.Apply();
        return result;
    }

    Texture2D resizeImage(Texture2D toResize, Vector2 size)
    {
        System.Drawing.Image image = Texture2Image(toResize);

        int sourceWidth = image.Width;
        int sourceHeight = image.Height;

        float nPercent = 0;
        float nPercentW = 0;
        float nPercentH = 0;

        nPercentW = ((float)size.x / (float)sourceWidth);
        nPercentH = size.y / (float)sourceHeight;

        if (nPercentH < nPercentW) nPercent = nPercentW;
        else nPercent = nPercentH;

        int dstWidth = (int)(sourceWidth * nPercentW);
        int dstHeight = (int)(sourceHeight * nPercentH);

        System.Drawing.Bitmap b = new System.Drawing.Bitmap(500, 500);
        System.Drawing.Graphics g = System.Drawing.Graphics.FromImage((System.Drawing.Image)b);
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        g.DrawImage(image, 0,0,dstWidth,dstHeight);
        g.Dispose();

        //b.RotateFlip(System.Drawing.RotateFlipType.Rotate180FlipX);

        Texture2D resized = Image2Texture(b);
        return resized;
    }

    System.Drawing.Color fromUnityColor(UnityEngine.Color source)
    {
        float r = source.r, g = source.g, b = source.b, a = source.a;

        int _r = fromUnityValueToRGB(r);
        int _g = fromUnityValueToRGB(g);
        int _b = fromUnityValueToRGB(b);
        int _a = fromUnityValueToRGB(a);

        return System.Drawing.Color.FromArgb(_a, _r, _g, _b);
    }

    UnityEngine.Color fromSystemColor(System.Drawing.Color source)
    {
        int r = source.R, g = source.G, b = source.B, a = source.A;

        float _r = fromRGBToUnityValue(r);
        float _g = fromRGBToUnityValue(g);
        float _b = fromRGBToUnityValue(b);
        float _a = fromRGBToUnityValue(a);

        return new Color(_r,_g,_b,_a);
    }

    int fromUnityValueToRGB(float value)
    {
        return (int)(value / 1 * 255);
    }
    float fromRGBToUnityValue(int value)
    {
        return (float)value / 255;
    }
    #endregion
}

public class SaveWindow : EditorWindow
{
    string name;
    string path;

    public Texture2D sprite;

    bool save;
    private void OnGUI()
    {
        GUILayout.Label("Nombre del archivo :", EditorStyles.boldLabel);
        name = GUILayout.TextField(name);

        GUILayout.Space(20);

        save = GUILayout.Button("Save", EditorStyles.miniButton);

        if (save) Save();
    }

    async void Save()
    {
        if(!Directory.Exists("Assets/Images/Creadas en Unity"))
        {
            Directory.CreateDirectory("Assets/Images/Creadas en Unity");
        }

        if (name.Length <= 0) return;

        if (!sprite) return;

        byte[] data = sprite.EncodeToPNG();
        string path = "Assets/Images/Creadas en Unity/" + name + ".png";
        FileStream fs = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);

        await fs.WriteAsync(data, 0, data.Length);
        fs.Close();

        this.Close();
    }
}

public class LayersWindow : EditorWindow
{
    public SpriteLayer[] spriteLayers;
    List<bool> layersButtons = new List<bool>();
    public List<int> opacities = new List<int>();
    public EditorWindow parent;
    Vector2 scrollPosition;

    bool newLayer;
    private void OnGUI()
    {
        if(parent != null)
        this.position = new Rect(parent.position.position - new Vector2(parent.position.size.x/5,0), new Vector2(parent.position.size.x / 5, parent.position.size.y));

        DisplayContent();
        CheckButtons();
        parent.Repaint();
    }

    void DisplayContent()
    {
        GUILayout.BeginArea(new Rect(0, 0, this.position.size.x, this.position.size.y - this.position.size.y / 6));
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        DisplayLayers();
        GUILayout.EndScrollView();
        GUILayout.EndArea();
        GUILayout.BeginArea(new Rect(0, this.position.size.y - this.position.size.y / 6, this.position.size.x, this.position.size.y / 6));
        newLayer = GUILayout.Button("Create new layer");
        GUILayout.EndArea();
    }

    void DisplayLayers()
    {
        if (spriteLayers == null) return;
        if (layersButtons.Count > spriteLayers.Length)
        {
            int difference = layersButtons.Count - spriteLayers.Length;
            layersButtons.RemoveRange(layersButtons.Count - difference, difference);
        }
        if(opacities.Count > spriteLayers.Length)
        {
            int differences = opacities.Count - spriteLayers.Length;
            opacities.RemoveRange(opacities.Count - differences, differences);
        }

        if(spriteLayers.Length > 0)
            for(int j = 0; j < spriteLayers.Length; j++)
            {
                GUILayout.BeginHorizontal();
                bool value = GUILayout.Button("Layer " + spriteLayers[j].id.ToString());
                if (j >= opacities.Count) opacities.Add(100);
                opacities[j] = EditorGUILayout.IntField(opacities[j]);
                opacities[j] = Mathf.Clamp(opacities[j], 0, 100);
                SpritesDrawerWindow sd = (SpritesDrawerWindow)parent;
                sd.spriteLayers[j].opacity = (float)opacities[j]/100;
                GUILayout.EndHorizontal();
                if (j >= layersButtons.Count) layersButtons.Add(value);
                else layersButtons[j] = value;
            }
    }

    void CheckButtons()
    {
        if (newLayer) CreateNewLayer();

        for(int i = 0; i < layersButtons.Count; i++)
        {
            if (layersButtons[i])
            {
                SpritesDrawerWindow sd = parent as SpritesDrawerWindow;
                sd.actualLayer = i;
                Debug.Log(i);
            }
        }
    }

    void CreateNewLayer()
    {
        SpritesDrawerWindow sd = EditorWindow.GetWindow<SpritesDrawerWindow>();
        SpriteLayer layer = new SpriteLayer();
        layer.size = sd.GetAreaAdaptativeSize();
        layer.position = sd.positionOffset;
        layer.id = sd.spriteLayers.Count;
        layer.texture = new Texture2D(500, 500, TextureFormat.ARGB32, false);
        layer.opacity = 1;

        sd.spriteLayers.Add(layer);
        sd.actualLayer = layer.id;
        sd.sprite = layer.texture;
        sd.ResetSprite(true);
        spriteLayers = sd.spriteLayers.ToArray();
    }
}
#endif