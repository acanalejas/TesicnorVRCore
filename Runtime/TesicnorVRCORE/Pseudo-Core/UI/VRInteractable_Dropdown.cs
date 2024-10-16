using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// Un dropdown compatible con VR y con el core de Tesicnor 
/// </summary>
[RequireComponent(typeof(Image))]
public class VRInteractable_Dropdown : VRInteractable_Button
{
    
    #region PARAMETERS
    /// <summary>
    /// Las imagenes de las opciones que se despliegan
    /// </summary>
    [Header(" =========== DROPDOWN PARAMETERS ==============")][Space(10)]
    
    [Header("Las imágenes que se despliegarán")]
    public Sprite[] options_Sprites;

    /// <summary>
    /// Los textos de las opciones que se desplegaran
    /// </summary>
    [Header("OPCIONAL : Los textos de las opciones")]
    public string[] options_Texts;

    /// <summary>
    /// El espacio que habrá entre opciones
    /// Medido en pixeles del canvas
    /// </summary>
    [Header("El espacio entre el Dropdown y las opciones")]
    [SerializeField] float spacing = 10;

    /// <summary>
    /// Acción que se lanza cuando cambia el valor del dropdown
    /// </summary>
    [Header("Cuando se cambia el valor del dropdown")]
    public UnityEvent<int> onValueChanged;

    /// <summary>
    /// El GameObject que contiene el layout donde aparecerán las opciones
    /// </summary>
    public GameObject vl;
    /// <summary>
    /// La imagen del dropdown
    /// </summary>
    public Image image;
    /// <summary>
    /// El texto del dropdown
    /// </summary>
    public TextMeshProUGUI text;

    /// <summary>
    /// La opción que está ahora seleccionada
    /// </summary>
    [HideInInspector] public int currentOption = 0;

    /// <summary>
    /// La clase que se añade a cada opción del dropdown
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class Dropdown_Item : VRInteractable_Button
    {
        public VRInteractable_Dropdown dropdown;
        public Image image;
        public TextMeshProUGUI text;
        public int position;

        protected override void Start()
        {
            
        }

        public override void Awake()
        {
            this.bClickByHover = true;
            this.fTimeToClickByHover = 6;

            base.Awake();
            
            image = GetComponent<Image>();
            image.maskable = false;

            GameObject text_GO = new GameObject("TMP", typeof(TextMeshProUGUI));
            text_GO.transform.parent = this.gameObject.transform;
            text_GO.transform.localPosition = Vector3.zero;
            text_GO.transform.localRotation = Quaternion.Euler(Vector3.zero);
            text_GO.transform.localScale = Vector3.one;

            text = text_GO.GetComponent<TextMeshProUGUI>();
            text.color = Color.black;
            text.maskable = false;
            text.raycastTarget = false;

            PressedColor = Color.white * 0.5f;
            HoverColor = Color.white * 0.75f;
        }

        public void SetValues(Sprite _image = null, string _text = "", int _position = 0)
        {
            image.sprite = _image;
            text.text = _text;
            position = _position;

            Transform vl = this.gameObject.transform.parent;
            RectTransform rect = vl.GetComponentInParent<Image>().rectTransform;
            dropdown = vl.GetComponentInParent<VRInteractable_Dropdown>();

            image.rectTransform.sizeDelta = rect.sizeDelta;

            PressedColor = dropdown.PressedColor;
            HoverColor = dropdown.HoverColor;
            NormalColor = dropdown.NormalColor;
        }

        public override void OnRelease()
        {
            base.OnRelease();
            if(this.gameObject.activeSelf || this.gameObject.activeInHierarchy)dropdown.ChangeValue(position);
        }
    }

    /// <summary>
    /// La lista de opciones creadas
    /// </summary>
    private List<Dropdown_Item> items = new List<Dropdown_Item>();
    #endregion

    #region FUNCTIONS
#if UNITY_EDITOR
    /// <summary>
    /// Funcion para crear dropdown desde el menu de GameObjects
    /// </summary>
    [MenuItem("Tesicnor/VR UI/VRDropdown")]
    static void Create()
    {
        GameObject empty = null;
        GameObject thisGO = new GameObject("Dropdown", typeof(VRInteractable_Dropdown));
        GameObject vl = new GameObject("Layout", typeof(VerticalLayoutGroup));
        vl.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperCenter;
        GameObject textGo = new GameObject("TMP", typeof(TextMeshProUGUI));

        if(Selection.gameObjects.Length > 0)thisGO.transform.parent = Selection.gameObjects[0].transform;
        thisGO.transform.localPosition = Vector3.zero;
        thisGO.transform.localScale = Vector3.one;
        thisGO.transform.localRotation = Quaternion.Euler(Vector3.zero);
        thisGO.GetComponent<Rigidbody>().useGravity = false;
        thisGO.isStatic = true;

        vl.transform.parent = thisGO.transform;
        vl.transform.localPosition = Vector3.zero;
        vl.transform.localRotation = Quaternion.Euler(Vector3.zero);
        vl.transform.localScale = Vector3.one;

        textGo.transform.parent = thisGO.transform;
        textGo.transform.localPosition = Vector3.zero;
        textGo.transform.localRotation = Quaternion.Euler(Vector3.zero);
        textGo.transform.localScale = Vector3.one;

        VRInteractable_Dropdown dropdown = thisGO.GetComponent<VRInteractable_Dropdown>();
        dropdown.HoverColor = Color.white * 0.75f;
        dropdown.PressedColor = Color.white * 0.5f;
        dropdown.image = thisGO.GetComponent<Image>();
        dropdown.text = textGo.GetComponent<TextMeshProUGUI>();
        dropdown.vl = vl;
    }

    /// <summary>
    /// Funcion para crear el dropdown desde el menu de componentes
    /// </summary>
    [MenuItem("Component/UI/VRDropdown")]
    static void Create_Component()
    {
        GameObject empty = null;
        GameObject vl = new GameObject("Layout", typeof(VerticalLayoutGroup));
        vl.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperCenter;
        GameObject textGo = new GameObject("TMP", typeof(TextMeshProUGUI));
        VRInteractable_Dropdown dropdown = Selection.gameObjects[0].AddComponent<VRInteractable_Dropdown>();
        Selection.gameObjects[0].GetComponent<Rigidbody>().useGravity = false;

        textGo.transform.parent = Selection.gameObjects[0].transform;
        textGo.transform.localPosition = Vector3.zero;
        textGo.transform.localRotation = Quaternion.Euler(Vector3.zero);
        textGo.transform.localScale = Vector3.one;

        vl.transform.parent = Selection.gameObjects[0].transform;
        vl.transform.localPosition = Vector3.zero;
        vl.transform.localRotation = Quaternion.Euler(Vector3.zero);
        vl.transform.localScale = Vector3.one;

        dropdown.PressedColor = Color.white * 0.50f;
        dropdown.HoverColor = Color.white * 0.75f;
        dropdown.image = Selection.gameObjects[0].GetComponent<Image>();
        dropdown.text = textGo.GetComponent<TextMeshProUGUI>();
        dropdown.vl = vl;
    }

#endif
    public override void Awake()
    {
        
        SetupDropdown();
        text = GetComponentInChildren<TextMeshProUGUI>();
        text.raycastTarget = false;
        vl.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperCenter;
        vl.GetComponent<VerticalLayoutGroup>().spacing = spacing;
        vl.GetComponent<RectTransform>().localPosition -= new Vector3(0, image.rectTransform.sizeDelta.y, 0);
        vl.GetComponent<RectTransform>().localPosition -= new Vector3(0, spacing, 0);
        vl.SetActive(false);
        vl.transform.parent = this.transform.parent;
        onRelease.AddListener(OpenDropdown);

        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    /// <summary>
    /// Activa o desactiva el dropdown
    /// </summary>
    public void OpenDropdown()
    {
        if (vl.activeSelf) vl.SetActive(false);
        else vl.SetActive(true);

        Debug.Log("OpenDropdown");
    }

    /// <summary>
    /// Setea valores al inicio
    /// </summary>
    void SetupDropdown()
    {
        int optionsCount = 0;
        if (options_Sprites.Length > options_Texts.Length) optionsCount = options_Sprites.Length;
        else optionsCount = options_Texts.Length;

        for(int i = 0; i < optionsCount; i++)
        {
            GameObject option = new GameObject("option", typeof(Dropdown_Item));
            Dropdown_Item item = option.GetComponent<Dropdown_Item>();

            option.transform.parent = vl.transform;
            option.transform.localPosition = Vector3.zero - new Vector3(0, option.GetComponent<Dropdown_Item>().image.rectTransform.rect.size.y * (i +1) + spacing, 0);
            option.transform.localRotation = Quaternion.Euler(Vector3.zero);
            option.transform.localScale = Vector3.one;

            items.Add(item);
            Sprite _sprite = null;
            string _string = "";
            int _position = i;
            if(options_Sprites.Length > i) if (options_Sprites[i]) _sprite = options_Sprites[i];
            if(options_Texts.Length > i) if (options_Texts[i] != "") _string = options_Texts[i];
            
            item.SetValues(_sprite, _string, _position);
            item.image.color = image.color;
            item.text.fontSize = text.fontSize;
            item.text.font = text.font;
            item.text.autoSizeTextContainer = true;
            item.text.enableAutoSizing = true;
            item.text.color = text.color;

            item.SetupUICollider();
            //item.SetupHover();
        }
    }

    /// <summary>
    /// Cuando se cambia el valor del dropdown
    /// </summary>
    /// <param name="position"></param>
    void ChangeValue(int position)
    {
        currentOption = position;
        //text.text = options_Texts[position];
        //OpenDropdown();
        onValueChanged?.Invoke(position);
    }
    #endregion
}
