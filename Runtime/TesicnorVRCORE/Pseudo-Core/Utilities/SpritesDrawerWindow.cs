using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.IO;

public class SpritesDrawerWindow : EditorWindow
{
    #region FIELDS
    Texture2D sprite;

    bool circle, square, roundSquare, triangle, reset, save;

    float circleRadius = 1, squareX = 1, squareY = 1, squareCornerRadius = 20, polygonSize;

    int polygonSides = 3;

    Color fillColor = Color.black;
    Color backgroundColor = Color.clear;
    #endregion

    #region METHODS
    [MenuItem("Window/Sprites Drawer")]
    public static void ShowWindowPutita()
    {
        EditorWindow window = GetWindow<SpritesDrawerWindow>();

        if (window == null) window = CreateWindow<SpritesDrawerWindow>();

        window.Show();
    }

    private void OnGUI()
    {
        DisplaySprite();
        //this.position.Set(position.x, position.y, 500, 500);
        if (circle) DrawCircleBack(sprite.width/2 *circleRadius, fillColor);
        if (square) DrawSquare(squareX, squareY, fillColor);
        if (roundSquare) DrawRoundedSquare(squareX, squareY, squareCornerRadius, fillColor);
        if (triangle) DrawPolygon(polygonSides, polygonSize, fillColor);
        if (reset) ResetSprite();
        if (save) SaveSprite();
    }

    private void OnEnable()
    {
        if(!sprite)
        sprite = new Texture2D(500, 500);
        ResetSprite();
    }

    void DisplaySprite()
    {
        Vector2 size = GetAreaAdaptativeSize();
        //Zona donde están los botones de guardar y resetear
        GUILayout.BeginHorizontal();
        save = GUILayout.Button("Save", EditorStyles.miniButtonMid);
        reset = GUILayout.Button("Reset", EditorStyles.miniButtonRight);
        GUILayout.EndHorizontal();

        //Zona donde se encuentran los botones laterales
        GUILayout.BeginArea(new Rect(0, 20, size.x * 0.2f, size.y));
        GUI.backgroundColor = Color.yellow;
        circle = GUILayout.Button("Circle", EditorStyles.miniButton);
        square = GUILayout.Button("Square", EditorStyles.miniButton);
        roundSquare = GUILayout.Button("Round Square", EditorStyles.miniButton);
        triangle = GUILayout.Button("Polygon", EditorStyles.miniButton);

        GUILayout.Space(20);

        fillColor = EditorGUILayout.ColorField("Fill Color", fillColor);

        GUILayout.Space(20);

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
        GUI.DrawTexture(new Rect(position.size.x/2 - size.x/2, position.size.y/2 - size.y/2, size.x, size.y), sprite);
    }

    Vector2 GetAreaAdaptativeSize()
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

    float DistanceBetweenPixels(Vector2 pixel1, Vector2 pixel2)
    {
        float xdist = pixel1.x - pixel2.x;
        float ydist = pixel1.y - pixel2.y;

        return Mathf.Sqrt(xdist * xdist + ydist * ydist);
    }

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

    void ResetSprite()
    {
        for(int i = 0; i < sprite.width; i++)
        {
            for(int j = 0; j < sprite.height; j++)
            {
                sprite.SetPixel(i, j, backgroundColor);
            }
        }
        sprite.Apply();
    }

    void SaveSprite()
    {
        SaveWindow sw = EditorWindow.GetWindow<SaveWindow>();
        sw.sprite = sprite;
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
