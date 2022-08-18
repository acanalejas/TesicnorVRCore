using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public static class MainMenuCreation
{
#if UNITY_EDITOR

    static string sideSpritePath = "Packages/com.tesicnor.tesicnorvrcore/Runtime/TesicnorVRCORE/Pseudo-Core/UI/Sprites/Back.png";
    static string menuMatPath = "Packages/com.tesicnor.tesicnorvrcore/Runtime/TesicnorVRCORE/Pseudo-Core/MainMenu/Room Materials/MainMenuRoom_Mat.mat";
    static string gridMatPath = "Packages/com.tesicnor.tesicnorvrcore/Runtime/TesicnorVRCORE/Pseudo-Core/MainMenu/Room Materials/Grid.mat";

    [MenuItem("Tesicnor/MENU/MainMenu")]
    public static void Create_GameObject()
    {
        GameObject self = new GameObject("Main Menu Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(MainMenuRCP));
        if (Selection.gameObjects.Length > 0) self.transform.parent = Selection.gameObjects[0].transform;
        self.transform.localPosition = Vector3.zero;
        self.transform.localRotation = Quaternion.Euler(Vector3.zero);
        self.transform.localScale = Vector3.one;

        Canvas canvas = self.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(1920, 1080);

        GameObject mask = new GameObject("Mask", typeof(Image), typeof(Mask));
        mask.transform.parent = self.transform;
        mask.transform.localPosition = Vector3.zero;
        mask.transform.localRotation = Quaternion.Euler(Vector3.zero);
        mask.transform.localScale = Vector3.one;

        Image maskIMG = mask.GetComponent<Image>();
        maskIMG.rectTransform.sizeDelta = Vector2.zero;
        maskIMG.rectTransform.anchorMin = new Vector2(0, 0);
        maskIMG.rectTransform.anchorMax = new Vector2(1, 1);
        maskIMG.color *= 0.60f;

        //========================= TOP SECTION ======================================

        GameObject topSection = new GameObject("Top Section", typeof(VerticalLayoutGroup));
        topSection.transform.parent = mask.transform;
        topSection.transform.localPosition = new Vector3(0, -151, 0);
        topSection.transform.localRotation = Quaternion.Euler(Vector3.zero);
        topSection.transform.localScale = Vector3.one;

        RectTransform topSection_rect = topSection.GetComponent<RectTransform>();
        topSection_rect.anchorMin = new Vector2(0.5f, 1);
        topSection_rect.anchorMax = new Vector2(0.5f, 1);

        VerticalLayoutGroup t_vl = topSection.GetComponent<VerticalLayoutGroup>();
        t_vl.childAlignment = TextAnchor.MiddleCenter;

        GameObject title = new GameObject("Title", typeof(TextMeshProUGUI));
        title.transform.parent = topSection.transform;
        title.transform.localPosition = Vector3.zero;
        title.transform.localRotation = Quaternion.Euler(Vector3.zero);
        title.transform.localScale = Vector3.one;

        TextMeshProUGUI title_text = title.GetComponent<TextMeshProUGUI>();
        title_text.fontSize = 65;
        title_text.text = "TITLE";
        title_text.color = Color.white;
        title_text.rectTransform.sizeDelta = new Vector2(1880, 80);
        title_text.verticalAlignment = VerticalAlignmentOptions.Middle;
        title_text.horizontalAlignment = HorizontalAlignmentOptions.Center;
        title_text.fontStyle = FontStyles.Bold;

        //=============================================================================

        //=============================== MIDDLE SECTION ===================================

        GameObject middleSection = new GameObject("Middle Section", typeof(HorizontalLayoutGroup));
        middleSection.transform.parent = mask.transform;
        middleSection.transform.localPosition = Vector3.zero;
        middleSection.transform.localRotation = Quaternion.Euler(Vector3.zero);
        middleSection.transform.localScale = Vector3.one;

        MainMenuRCP mainMenu = self.GetComponent<MainMenuRCP>();
        GameObject[] newList = new GameObject[1];
        newList[0] = middleSection;
        mainMenu.escenarios = newList;

        HorizontalLayoutGroup m_hl = middleSection.GetComponent<HorizontalLayoutGroup>();
        m_hl.childAlignment = TextAnchor.MiddleCenter;
        m_hl.padding.left = -700;
        m_hl.spacing = 100;

        GameObject Experience001 = new GameObject("Experience001", typeof(MenuButton));
        Experience001.transform.parent = middleSection.transform;
        Experience001.transform.localPosition = Vector3.zero;
        Experience001.transform.localRotation = Quaternion.Euler(Vector3.zero);
        Experience001.transform.localScale = Vector3.one;

        Image Experience001IMG = Experience001.GetComponent<Image>();
        Experience001IMG.color = Color.white;
        Experience001IMG.rectTransform.sizeDelta = new Vector2(300, 400);

        GameObject Experience002 = new GameObject("Experience002", typeof(MenuButton));
        Experience002.transform.parent = middleSection.transform;
        Experience002.transform.localPosition = Vector3.zero;
        Experience002.transform.localRotation = Quaternion.Euler(Vector3.zero);
        Experience002.transform.localScale = Vector3.one;

        Image Experience002IMG = Experience002.GetComponent<Image>();
        Experience002IMG.color = Color.white;
        Experience002IMG.rectTransform.sizeDelta = new Vector2(300, 400);

        GameObject Experience003 = new GameObject("Experience003", typeof(MenuButton));
        Experience003.transform.parent = middleSection.transform;
        Experience003.transform.localPosition = Vector3.zero;
        Experience003.transform.localRotation = Quaternion.Euler(Vector3.zero);
        Experience003.transform.localScale = Vector3.one;

        Image Experience003IMG = Experience003.GetComponent<Image>();
        Experience003IMG.color = Color.white;
        Experience003IMG.rectTransform.sizeDelta = new Vector2(300, 400);

        GameObject Experience004 = new GameObject("Experience004", typeof(MenuButton));
        Experience004.transform.parent = middleSection.transform;
        Experience004.transform.localPosition = Vector3.zero;
        Experience004.transform.localRotation = Quaternion.Euler(Vector3.zero);
        Experience004.transform.localScale = Vector3.one;

        Image Experience004IMG = Experience004.GetComponent<Image>();
        Experience004IMG.color = Color.white;
        Experience004IMG.rectTransform.sizeDelta = new Vector2(300, 400);

        GameObject Experience001Text = new GameObject("TMP", typeof(TextMeshProUGUI));
        Experience001Text.transform.parent = Experience001.transform;
        Experience001Text.transform.localPosition = new Vector3(0, -334, 0);
        Experience001Text.transform.localRotation = Quaternion.Euler(Vector3.zero);
        Experience001Text.transform.localScale = Vector3.one;

        TextMeshProUGUI Experience001_text = Experience001Text.GetComponent<TextMeshProUGUI>();
        Experience001_text.color = Color.white;
        Experience001_text.rectTransform.sizeDelta = new Vector2(300, 200);
        Experience001_text.fontSize = 45;
        Experience001_text.text = "EXPERIENCE NAME";

        GameObject Experience002Text = new GameObject("TMP", typeof(TextMeshProUGUI));
        Experience002Text.transform.parent = Experience002.transform;
        Experience002Text.transform.localPosition = new Vector3(0, -334, 0);
        Experience002Text.transform.localRotation = Quaternion.Euler(Vector3.zero);
        Experience002Text.transform.localScale = Vector3.one;

        TextMeshProUGUI Experience002_text = Experience002Text.GetComponent<TextMeshProUGUI>();
        Experience002_text.color = Color.white;
        Experience002_text.rectTransform.sizeDelta = new Vector2(300, 200);
        Experience002_text.fontSize = 45;
        Experience002_text.text = "EXPERIENCE NAME";

        GameObject Experience003Text = new GameObject("TMP", typeof(TextMeshProUGUI));
        Experience003Text.transform.parent = Experience003.transform;
        Experience003Text.transform.localPosition = new Vector3(0, -334, 0);
        Experience003Text.transform.localRotation = Quaternion.Euler(Vector3.zero);
        Experience003Text.transform.localScale = Vector3.one;

        TextMeshProUGUI Experience003_text = Experience003Text.GetComponent<TextMeshProUGUI>();
        Experience003_text.color = Color.white;
        Experience003_text.rectTransform.sizeDelta = new Vector2(300, 200);
        Experience003_text.fontSize = 45;
        Experience003_text.text = "EXPERIENCE NAME";

        GameObject Experience004Text = new GameObject("TMP", typeof(TextMeshProUGUI));
        Experience004Text.transform.parent = Experience004.transform;
        Experience004Text.transform.localPosition = new Vector3(0, -334, 0);
        Experience004Text.transform.localRotation = Quaternion.Euler(Vector3.zero);
        Experience004Text.transform.localScale = Vector3.one;

        TextMeshProUGUI Experience004_text = Experience004Text.GetComponent<TextMeshProUGUI>();
        Experience004_text.color = Color.white;
        Experience004_text.rectTransform.sizeDelta = new Vector2(300, 200);
        Experience004_text.fontSize = 45;
        Experience004_text.text = "EXPERIENCE NAME";

        //==================================================================================

        //============================ LEFT BUTTON =========================================

        GameObject leftButton = new GameObject("Left Button", typeof(ArrowButton));
        leftButton.transform.parent = mask.transform;

        Image leftButtonIMG = leftButton.GetComponent<Image>();
        leftButtonIMG.sprite = (Sprite)AssetDatabase.LoadAssetAtPath(sideSpritePath, typeof(Sprite));
        leftButtonIMG.rectTransform.sizeDelta = new Vector2(200, 200);
        leftButtonIMG.rectTransform.anchorMin = new Vector2(0, 0.5f);
        leftButtonIMG.rectTransform.anchorMax = new Vector2(0, 0.5f);
        leftButtonIMG.rectTransform.anchoredPosition = new Vector3(106, 0, 0);
        leftButtonIMG.rectTransform.localRotation = Quaternion.Euler(Vector3.zero);
        leftButtonIMG.rectTransform.localScale = Vector3.one;

        GameObject rightButton = new GameObject("Right Button", typeof(ArrowButton));
        rightButton.transform.parent = mask.transform;

        Image rightButtonIMG = rightButton.GetComponent<Image>();
        rightButtonIMG.sprite = (Sprite)AssetDatabase.LoadAssetAtPath(sideSpritePath, typeof(Sprite));
        rightButtonIMG.rectTransform.sizeDelta = new Vector2(200, 200);
        rightButtonIMG.rectTransform.anchorMin = new Vector2(1, 0.5f);
        rightButtonIMG.rectTransform.anchorMax = new Vector2(1, 0.5f);
        rightButtonIMG.rectTransform.anchoredPosition = new Vector3(-106, 0, 0);
        rightButtonIMG.rectTransform.localRotation = Quaternion.Euler(Vector3.zero);
        rightButtonIMG.rectTransform.localScale = new Vector3(-1, 1, 1);

        // ============================== SETTING MAIN MENU SCRIPT ============================

        GameObject centerPosition = new GameObject("Center Position Holder");
        centerPosition.transform.parent = mask.transform;
        centerPosition.transform.localPosition = Vector3.zero;
        centerPosition.transform.localRotation = Quaternion.Euler(Vector3.zero);
        centerPosition.transform.localScale = Vector3.one;

        mainMenu.centerPosition = centerPosition.transform;

        GameObject rightPosition = new GameObject("Right Position Holder");
        rightPosition.transform.parent = mask.transform;
        rightPosition.transform.localPosition = new Vector3(2300, 0, 0);
        rightPosition.transform.localRotation = Quaternion.Euler(Vector3.zero);
        rightPosition.transform.localScale = Vector3.one;

        mainMenu.rightPosition = rightPosition.transform;

        GameObject leftPosition = new GameObject("Left Position Holder");
        leftPosition.transform.parent = mask.transform;
        leftPosition.transform.localPosition = new Vector3(-2300, 0, 0);
        leftPosition.transform.localRotation = Quaternion.Euler(Vector3.zero);
        leftPosition.transform.localScale = Vector3.one;

        mainMenu.leftPosition = leftPosition.transform;
        mainMenu.initialMenu = self;

        // ====================================================================================
    }

    [MenuItem("Tesicnor/MENU/MainMenu_Room")]
    public static void Create_MainMenu_Room()
    {
        GameObject self = new GameObject("Main Menu Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(MainMenuRCP));
        if (Selection.gameObjects.Length > 0) self.transform.parent = Selection.gameObjects[0].transform;
        self.transform.localPosition = Vector3.zero;
        self.transform.localRotation = Quaternion.Euler(Vector3.zero);
        self.transform.localScale = new Vector3(0.002f,0.002f,0.002f);

        Canvas canvas = self.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(1920, 1080);

        GameObject mask = new GameObject("Mask", typeof(Image), typeof(Mask));
        mask.transform.parent = self.transform;
        mask.transform.localPosition = Vector3.zero;
        mask.transform.localRotation = Quaternion.Euler(Vector3.zero);
        mask.transform.localScale = Vector3.one;

        Image maskIMG = mask.GetComponent<Image>();
        maskIMG.rectTransform.sizeDelta = Vector2.zero;
        maskIMG.rectTransform.anchorMin = new Vector2(0, 0);
        maskIMG.rectTransform.anchorMax = new Vector2(1, 1);
        maskIMG.color *= 0.60f;

        //========================= TOP SECTION ======================================

        GameObject topSection = new GameObject("Top Section", typeof(VerticalLayoutGroup));
        topSection.transform.parent = mask.transform;
        topSection.transform.localPosition = new Vector3(0, -151, 0);
        topSection.transform.localRotation = Quaternion.Euler(Vector3.zero);
        topSection.transform.localScale = Vector3.one;

        RectTransform topSection_rect = topSection.GetComponent<RectTransform>();
        topSection_rect.anchorMin = new Vector2(0.5f, 1);
        topSection_rect.anchorMax = new Vector2(0.5f, 1);

        VerticalLayoutGroup t_vl = topSection.GetComponent<VerticalLayoutGroup>();
        t_vl.childAlignment = TextAnchor.MiddleCenter;

        GameObject title = new GameObject("Title", typeof(TextMeshProUGUI));
        title.transform.parent = topSection.transform;
        title.transform.localPosition = Vector3.zero;
        title.transform.localRotation = Quaternion.Euler(Vector3.zero);
        title.transform.localScale = Vector3.one;

        TextMeshProUGUI title_text = title.GetComponent<TextMeshProUGUI>();
        title_text.fontSize = 65;
        title_text.text = "TITLE";
        title_text.color = Color.white;
        title_text.rectTransform.sizeDelta = new Vector2(1880, 80);
        title_text.verticalAlignment = VerticalAlignmentOptions.Middle;
        title_text.horizontalAlignment = HorizontalAlignmentOptions.Center;
        title_text.fontStyle = FontStyles.Bold;

        //=============================================================================

        //=============================== MIDDLE SECTION ===================================

        GameObject middleSection = new GameObject("Middle Section", typeof(HorizontalLayoutGroup));
        middleSection.transform.parent = mask.transform;
        middleSection.transform.localPosition = Vector3.zero;
        middleSection.transform.localRotation = Quaternion.Euler(Vector3.zero);
        middleSection.transform.localScale = Vector3.one;

        MainMenuRCP mainMenu = self.GetComponent<MainMenuRCP>();
        GameObject[] newList = new GameObject[1];
        newList[0] = middleSection;
        mainMenu.escenarios = newList;

        HorizontalLayoutGroup m_hl = middleSection.GetComponent<HorizontalLayoutGroup>();
        m_hl.childAlignment = TextAnchor.MiddleCenter;
        m_hl.padding.left = -700;
        m_hl.spacing = 100;

        GameObject Experience001 = new GameObject("Experience001", typeof(MenuButton));
        Experience001.transform.parent = middleSection.transform;
        Experience001.transform.localPosition = Vector3.zero;
        Experience001.transform.localRotation = Quaternion.Euler(Vector3.zero);
        Experience001.transform.localScale = Vector3.one;

        Image Experience001IMG = Experience001.GetComponent<Image>();
        Experience001IMG.color = Color.white;
        Experience001IMG.rectTransform.sizeDelta = new Vector2(300, 400);

        GameObject Experience002 = new GameObject("Experience002", typeof(MenuButton));
        Experience002.transform.parent = middleSection.transform;
        Experience002.transform.localPosition = Vector3.zero;
        Experience002.transform.localRotation = Quaternion.Euler(Vector3.zero);
        Experience002.transform.localScale = Vector3.one;

        Image Experience002IMG = Experience002.GetComponent<Image>();
        Experience002IMG.color = Color.white;
        Experience002IMG.rectTransform.sizeDelta = new Vector2(300, 400);

        GameObject Experience003 = new GameObject("Experience003", typeof(MenuButton));
        Experience003.transform.parent = middleSection.transform;
        Experience003.transform.localPosition = Vector3.zero;
        Experience003.transform.localRotation = Quaternion.Euler(Vector3.zero);
        Experience003.transform.localScale = Vector3.one;

        Image Experience003IMG = Experience003.GetComponent<Image>();
        Experience003IMG.color = Color.white;
        Experience003IMG.rectTransform.sizeDelta = new Vector2(300, 400);

        GameObject Experience004 = new GameObject("Experience004", typeof(MenuButton));
        Experience004.transform.parent = middleSection.transform;
        Experience004.transform.localPosition = Vector3.zero;
        Experience004.transform.localRotation = Quaternion.Euler(Vector3.zero);
        Experience004.transform.localScale = Vector3.one;

        Image Experience004IMG = Experience004.GetComponent<Image>();
        Experience004IMG.color = Color.white;
        Experience004IMG.rectTransform.sizeDelta = new Vector2(300, 400);

        GameObject Experience001Text = new GameObject("TMP", typeof(TextMeshProUGUI));
        Experience001Text.transform.parent = Experience001.transform;
        Experience001Text.transform.localPosition = new Vector3(0, -334, 0);
        Experience001Text.transform.localRotation = Quaternion.Euler(Vector3.zero);
        Experience001Text.transform.localScale = Vector3.one;

        TextMeshProUGUI Experience001_text = Experience001Text.GetComponent<TextMeshProUGUI>();
        Experience001_text.color = Color.white;
        Experience001_text.rectTransform.sizeDelta = new Vector2(300, 200);
        Experience001_text.fontSize = 45;
        Experience001_text.text = "EXPERIENCE NAME";

        GameObject Experience002Text = new GameObject("TMP", typeof(TextMeshProUGUI));
        Experience002Text.transform.parent = Experience002.transform;
        Experience002Text.transform.localPosition = new Vector3(0, -334, 0);
        Experience002Text.transform.localRotation = Quaternion.Euler(Vector3.zero);
        Experience002Text.transform.localScale = Vector3.one;

        TextMeshProUGUI Experience002_text = Experience002Text.GetComponent<TextMeshProUGUI>();
        Experience002_text.color = Color.white;
        Experience002_text.rectTransform.sizeDelta = new Vector2(300, 200);
        Experience002_text.fontSize = 45;
        Experience002_text.text = "EXPERIENCE NAME";

        GameObject Experience003Text = new GameObject("TMP", typeof(TextMeshProUGUI));
        Experience003Text.transform.parent = Experience003.transform;
        Experience003Text.transform.localPosition = new Vector3(0, -334, 0);
        Experience003Text.transform.localRotation = Quaternion.Euler(Vector3.zero);
        Experience003Text.transform.localScale = Vector3.one;

        TextMeshProUGUI Experience003_text = Experience003Text.GetComponent<TextMeshProUGUI>();
        Experience003_text.color = Color.white;
        Experience003_text.rectTransform.sizeDelta = new Vector2(300, 200);
        Experience003_text.fontSize = 45;
        Experience003_text.text = "EXPERIENCE NAME";

        GameObject Experience004Text = new GameObject("TMP", typeof(TextMeshProUGUI));
        Experience004Text.transform.parent = Experience004.transform;
        Experience004Text.transform.localPosition = new Vector3(0, -334, 0);
        Experience004Text.transform.localRotation = Quaternion.Euler(Vector3.zero);
        Experience004Text.transform.localScale = Vector3.one;

        TextMeshProUGUI Experience004_text = Experience004Text.GetComponent<TextMeshProUGUI>();
        Experience004_text.color = Color.white;
        Experience004_text.rectTransform.sizeDelta = new Vector2(300, 200);
        Experience004_text.fontSize = 45;
        Experience004_text.text = "EXPERIENCE NAME";

        //==================================================================================

        //============================ LEFT BUTTON =========================================

        GameObject leftButton = new GameObject("Left Button", typeof(ArrowButton));
        leftButton.transform.parent = mask.transform;

        Image leftButtonIMG = leftButton.GetComponent<Image>();
        leftButtonIMG.sprite = (Sprite)AssetDatabase.LoadAssetAtPath(sideSpritePath, typeof(Sprite));
        leftButtonIMG.rectTransform.sizeDelta = new Vector2(200, 200);
        leftButtonIMG.rectTransform.anchorMin = new Vector2(0, 0.5f);
        leftButtonIMG.rectTransform.anchorMax = new Vector2(0, 0.5f);
        leftButtonIMG.rectTransform.anchoredPosition = new Vector3(106, 0, 0);
        leftButtonIMG.rectTransform.localRotation = Quaternion.Euler(Vector3.zero);
        leftButtonIMG.rectTransform.localScale = Vector3.one;

        // =========================== RIGHT BUTTON =========================================

        GameObject rightButton = new GameObject("Right Button", typeof(ArrowButton));
        rightButton.transform.parent = mask.transform;

        Image rightButtonIMG = rightButton.GetComponent<Image>();
        rightButtonIMG.sprite = (Sprite)AssetDatabase.LoadAssetAtPath(sideSpritePath, typeof(Sprite));
        rightButtonIMG.rectTransform.sizeDelta = new Vector2(200, 200);
        rightButtonIMG.rectTransform.anchorMin = new Vector2(1, 0.5f);
        rightButtonIMG.rectTransform.anchorMax = new Vector2(1, 0.5f);
        rightButtonIMG.rectTransform.anchoredPosition = new Vector3(-106, 0, 0);
        rightButtonIMG.rectTransform.localRotation = Quaternion.Euler(Vector3.zero);
        rightButtonIMG.rectTransform.localScale = new Vector3(-1, 1, 1);

        // ====================================================================================

        // ============================== SETTING MAIN MENU SCRIPT ============================

        GameObject centerPosition = new GameObject("Center Position Holder");
        centerPosition.transform.parent = mask.transform;
        centerPosition.transform.localPosition = Vector3.zero;
        centerPosition.transform.localRotation = Quaternion.Euler(Vector3.zero);
        centerPosition.transform.localScale = Vector3.one;

        mainMenu.centerPosition = centerPosition.transform;

        GameObject rightPosition = new GameObject("Right Position Holder");
        rightPosition.transform.parent = mask.transform;
        rightPosition.transform.localPosition = new Vector3(2300, 0, 0);
        rightPosition.transform.localRotation = Quaternion.Euler(Vector3.zero);
        rightPosition.transform.localScale = Vector3.one;

        mainMenu.rightPosition = rightPosition.transform;

        GameObject leftPosition = new GameObject("Left Position Holder");
        leftPosition.transform.parent = mask.transform;
        leftPosition.transform.localPosition = new Vector3(-2300, 0, 0);
        leftPosition.transform.localRotation = Quaternion.Euler(Vector3.zero);
        leftPosition.transform.localScale = Vector3.one;

        mainMenu.leftPosition = leftPosition.transform;
        mainMenu.initialMenu = self;

        // ====================================================================================

        // ============================== ROOM CREATION =======================================

        GameObject room = new GameObject("Room");
        room.transform.localPosition = Vector3.zero;
        room.transform.localRotation = Quaternion.Euler(Vector3.zero);
        room.transform.localScale = Vector3.one;

        GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Plane);
        leftWall.name = "Left Wall";
        leftWall.transform.parent = room.transform;
        leftWall.transform.localPosition = new Vector3(-5, 3.5f, 0);
        leftWall.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -90));
        leftWall.transform.localScale = Vector3.one;

        MeshRenderer leftMesh = leftWall.GetComponent<MeshRenderer>();
        leftMesh.material = (Material)AssetDatabase.LoadAssetAtPath(menuMatPath, typeof(Material));

        GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Plane);
        rightWall.name = "Right Wall";
        rightWall.transform.parent = room.transform;
        rightWall.transform.localPosition = new Vector3(5, 3.5f);
        rightWall.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
        rightWall.transform.localScale = Vector3.one;

        MeshRenderer rightMesh = rightWall.GetComponent<MeshRenderer>();
        rightMesh.material = (Material)AssetDatabase.LoadAssetAtPath(menuMatPath, typeof(Material));

        GameObject afterWall = GameObject.CreatePrimitive(PrimitiveType.Plane);
        afterWall.name = "After Wall";
        afterWall.transform.parent = room.transform;
        afterWall.transform.localPosition = new Vector3(0, 3.5f, 5);
        afterWall.transform.localRotation = Quaternion.Euler(new Vector3(-90, 0, 0));
        afterWall.transform.localScale = Vector3.one;

        MeshRenderer afterMesh = afterWall.GetComponent<MeshRenderer>();
        afterMesh.material = (Material)AssetDatabase.LoadAssetAtPath(menuMatPath, typeof(Material));

        GameObject behindWall = GameObject.CreatePrimitive(PrimitiveType.Plane);
        behindWall.name = "Behind Wall";
        behindWall.transform.parent = room.transform;
        behindWall.transform.localPosition = new Vector3(0, 3.5f, -5);
        behindWall.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));
        behindWall.transform.localScale = Vector3.one;

        MeshRenderer behindMesh = behindWall.GetComponent<MeshRenderer>();
        behindMesh.material = (Material)AssetDatabase.LoadAssetAtPath(menuMatPath, typeof(Material));

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.parent = room.transform;
        floor.transform.localPosition = new Vector3(0, -1.47f, 0);
        floor.transform.localRotation = Quaternion.Euler(Vector3.zero);
        floor.transform.localScale = Vector3.one;

        MeshRenderer floorMesh = floor.GetComponent<MeshRenderer>();
        floorMesh.material = (Material)AssetDatabase.LoadAssetAtPath(gridMatPath, typeof(Material));

        GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ceiling.name = "Ceiling";
        ceiling.transform.parent = room.transform;
        ceiling.transform.localPosition = new Vector3(0, 8.45f, 0);
        ceiling.transform.localRotation = Quaternion.Euler(new Vector3(180, 0, 0));
        ceiling.transform.localScale = Vector3.one;

        MeshRenderer ceilingMesh = ceiling.GetComponent<MeshRenderer>();
        ceilingMesh.material = (Material)AssetDatabase.LoadAssetAtPath(menuMatPath, typeof(Material));
    }

    [MenuItem("Tesicnor/MENU/EscenesScreen")]
    public static void Create_EscenesScreen()
    {
        GameObject middleSection = new GameObject("Middle Section", typeof(HorizontalLayoutGroup));
        if (Selection.gameObjects.Length > 0) middleSection.transform.parent = Selection.gameObjects[0].transform;
        middleSection.transform.localPosition = Vector3.zero;
        middleSection.transform.localRotation = Quaternion.Euler(Vector3.zero);
        middleSection.transform.localScale = Vector3.one;

        HorizontalLayoutGroup m_hl = middleSection.GetComponent<HorizontalLayoutGroup>();
        m_hl.childAlignment = TextAnchor.MiddleCenter;
        m_hl.padding.left = -700;
        m_hl.spacing = 100;

        GameObject Experience001 = new GameObject("Experience001", typeof(MenuButton));
        Experience001.transform.parent = middleSection.transform;
        Experience001.transform.localPosition = Vector3.zero;
        Experience001.transform.localRotation = Quaternion.Euler(Vector3.zero);
        Experience001.transform.localScale = Vector3.one;

        Image Experience001IMG = Experience001.GetComponent<Image>();
        Experience001IMG.color = Color.white;
        Experience001IMG.rectTransform.sizeDelta = new Vector2(300, 400);

        GameObject Experience002 = new GameObject("Experience002", typeof(MenuButton));
        Experience002.transform.parent = middleSection.transform;
        Experience002.transform.localPosition = Vector3.zero;
        Experience002.transform.localRotation = Quaternion.Euler(Vector3.zero);
        Experience002.transform.localScale = Vector3.one;

        Image Experience002IMG = Experience002.GetComponent<Image>();
        Experience002IMG.color = Color.white;
        Experience002IMG.rectTransform.sizeDelta = new Vector2(300, 400);

        GameObject Experience003 = new GameObject("Experience003", typeof(MenuButton));
        Experience003.transform.parent = middleSection.transform;
        Experience003.transform.localPosition = Vector3.zero;
        Experience003.transform.localRotation = Quaternion.Euler(Vector3.zero);
        Experience003.transform.localScale = Vector3.one;

        Image Experience003IMG = Experience003.GetComponent<Image>();
        Experience003IMG.color = Color.white;
        Experience003IMG.rectTransform.sizeDelta = new Vector2(300, 400);

        GameObject Experience004 = new GameObject("Experience004", typeof(MenuButton));
        Experience004.transform.parent = middleSection.transform;
        Experience004.transform.localPosition = Vector3.zero;
        Experience004.transform.localRotation = Quaternion.Euler(Vector3.zero);
        Experience004.transform.localScale = Vector3.one;

        Image Experience004IMG = Experience004.GetComponent<Image>();
        Experience004IMG.color = Color.white;
        Experience004IMG.rectTransform.sizeDelta = new Vector2(300, 400);

        GameObject Experience001Text = new GameObject("TMP", typeof(TextMeshProUGUI));
        Experience001Text.transform.parent = Experience001.transform;
        Experience001Text.transform.localPosition = new Vector3(0, -334, 0);
        Experience001Text.transform.localRotation = Quaternion.Euler(Vector3.zero);
        Experience001Text.transform.localScale = Vector3.one;

        TextMeshProUGUI Experience001_text = Experience001Text.GetComponent<TextMeshProUGUI>();
        Experience001_text.color = Color.white;
        Experience001_text.rectTransform.sizeDelta = new Vector2(300, 200);
        Experience001_text.fontSize = 45;
        Experience001_text.text = "EXPERIENCE NAME";

        GameObject Experience002Text = new GameObject("TMP", typeof(TextMeshProUGUI));
        Experience002Text.transform.parent = Experience002.transform;
        Experience002Text.transform.localPosition = new Vector3(0, -334, 0);
        Experience002Text.transform.localRotation = Quaternion.Euler(Vector3.zero);
        Experience002Text.transform.localScale = Vector3.one;

        TextMeshProUGUI Experience002_text = Experience002Text.GetComponent<TextMeshProUGUI>();
        Experience002_text.color = Color.white;
        Experience002_text.rectTransform.sizeDelta = new Vector2(300, 200);
        Experience002_text.fontSize = 45;
        Experience002_text.text = "EXPERIENCE NAME";

        GameObject Experience003Text = new GameObject("TMP", typeof(TextMeshProUGUI));
        Experience003Text.transform.parent = Experience003.transform;
        Experience003Text.transform.localPosition = new Vector3(0, -334, 0);
        Experience003Text.transform.localRotation = Quaternion.Euler(Vector3.zero);
        Experience003Text.transform.localScale = Vector3.one;

        TextMeshProUGUI Experience003_text = Experience003Text.GetComponent<TextMeshProUGUI>();
        Experience003_text.color = Color.white;
        Experience003_text.rectTransform.sizeDelta = new Vector2(300, 200);
        Experience003_text.fontSize = 45;
        Experience003_text.text = "EXPERIENCE NAME";

        GameObject Experience004Text = new GameObject("TMP", typeof(TextMeshProUGUI));
        Experience004Text.transform.parent = Experience004.transform;
        Experience004Text.transform.localPosition = new Vector3(0, -334, 0);
        Experience004Text.transform.localRotation = Quaternion.Euler(Vector3.zero);
        Experience004Text.transform.localScale = Vector3.one;

        TextMeshProUGUI Experience004_text = Experience004Text.GetComponent<TextMeshProUGUI>();
        Experience004_text.color = Color.white;
        Experience004_text.rectTransform.sizeDelta = new Vector2(300, 200);
        Experience004_text.fontSize = 45;
        Experience004_text.text = "EXPERIENCE NAME";
    }

    [MenuItem("Component/UI/EscenesScreen")]
    public static void Create_EscenesScreen_Component()
    {
        GameObject middleSection = Selection.gameObjects[0];
        middleSection.AddComponent<HorizontalLayoutGroup>();
        middleSection.transform.localPosition = Vector3.zero;
        middleSection.transform.localRotation = Quaternion.Euler(Vector3.zero);
        middleSection.transform.localScale = Vector3.one;

        HorizontalLayoutGroup m_hl = middleSection.GetComponent<HorizontalLayoutGroup>();
        m_hl.childAlignment = TextAnchor.MiddleCenter;
        m_hl.padding.left = -700;
        m_hl.spacing = 100;

        GameObject Experience001 = new GameObject("Experience001", typeof(MenuButton));
        Experience001.transform.parent = middleSection.transform;
        Experience001.transform.localPosition = Vector3.zero;
        Experience001.transform.localRotation = Quaternion.Euler(Vector3.zero);
        Experience001.transform.localScale = Vector3.one;

        Image Experience001IMG = Experience001.GetComponent<Image>();
        Experience001IMG.color = Color.white;
        Experience001IMG.rectTransform.sizeDelta = new Vector2(300, 400);

        GameObject Experience002 = new GameObject("Experience002", typeof(MenuButton));
        Experience002.transform.parent = middleSection.transform;
        Experience002.transform.localPosition = Vector3.zero;
        Experience002.transform.localRotation = Quaternion.Euler(Vector3.zero);
        Experience002.transform.localScale = Vector3.one;

        Image Experience002IMG = Experience002.GetComponent<Image>();
        Experience002IMG.color = Color.white;
        Experience002IMG.rectTransform.sizeDelta = new Vector2(300, 400);

        GameObject Experience003 = new GameObject("Experience003", typeof(MenuButton));
        Experience003.transform.parent = middleSection.transform;
        Experience003.transform.localPosition = Vector3.zero;
        Experience003.transform.localRotation = Quaternion.Euler(Vector3.zero);
        Experience003.transform.localScale = Vector3.one;

        Image Experience003IMG = Experience003.GetComponent<Image>();
        Experience003IMG.color = Color.white;
        Experience003IMG.rectTransform.sizeDelta = new Vector2(300, 400);

        GameObject Experience004 = new GameObject("Experience004", typeof(MenuButton));
        Experience004.transform.parent = middleSection.transform;
        Experience004.transform.localPosition = Vector3.zero;
        Experience004.transform.localRotation = Quaternion.Euler(Vector3.zero);
        Experience004.transform.localScale = Vector3.one;

        Image Experience004IMG = Experience004.GetComponent<Image>();
        Experience004IMG.color = Color.white;
        Experience004IMG.rectTransform.sizeDelta = new Vector2(300, 400);

        GameObject Experience001Text = new GameObject("TMP", typeof(TextMeshProUGUI));
        Experience001Text.transform.parent = Experience001.transform;
        Experience001Text.transform.localPosition = new Vector3(0, -334, 0);
        Experience001Text.transform.localRotation = Quaternion.Euler(Vector3.zero);
        Experience001Text.transform.localScale = Vector3.one;

        TextMeshProUGUI Experience001_text = Experience001Text.GetComponent<TextMeshProUGUI>();
        Experience001_text.color = Color.white;
        Experience001_text.rectTransform.sizeDelta = new Vector2(300, 200);
        Experience001_text.fontSize = 45;
        Experience001_text.text = "EXPERIENCE NAME";

        GameObject Experience002Text = new GameObject("TMP", typeof(TextMeshProUGUI));
        Experience002Text.transform.parent = Experience002.transform;
        Experience002Text.transform.localPosition = new Vector3(0, -334, 0);
        Experience002Text.transform.localRotation = Quaternion.Euler(Vector3.zero);
        Experience002Text.transform.localScale = Vector3.one;

        TextMeshProUGUI Experience002_text = Experience002Text.GetComponent<TextMeshProUGUI>();
        Experience002_text.color = Color.white;
        Experience002_text.rectTransform.sizeDelta = new Vector2(300, 200);
        Experience002_text.fontSize = 45;
        Experience002_text.text = "EXPERIENCE NAME";

        GameObject Experience003Text = new GameObject("TMP", typeof(TextMeshProUGUI));
        Experience003Text.transform.parent = Experience003.transform;
        Experience003Text.transform.localPosition = new Vector3(0, -334, 0);
        Experience003Text.transform.localRotation = Quaternion.Euler(Vector3.zero);
        Experience003Text.transform.localScale = Vector3.one;

        TextMeshProUGUI Experience003_text = Experience003Text.GetComponent<TextMeshProUGUI>();
        Experience003_text.color = Color.white;
        Experience003_text.rectTransform.sizeDelta = new Vector2(300, 200);
        Experience003_text.fontSize = 45;
        Experience003_text.text = "EXPERIENCE NAME";

        GameObject Experience004Text = new GameObject("TMP", typeof(TextMeshProUGUI));
        Experience004Text.transform.parent = Experience004.transform;
        Experience004Text.transform.localPosition = new Vector3(0, -334, 0);
        Experience004Text.transform.localRotation = Quaternion.Euler(Vector3.zero);
        Experience004Text.transform.localScale = Vector3.one;

        TextMeshProUGUI Experience004_text = Experience004Text.GetComponent<TextMeshProUGUI>();
        Experience004_text.color = Color.white;
        Experience004_text.rectTransform.sizeDelta = new Vector2(300, 200);
        Experience004_text.fontSize = 45;
        Experience004_text.text = "EXPERIENCE NAME";
    }
#endif
}
