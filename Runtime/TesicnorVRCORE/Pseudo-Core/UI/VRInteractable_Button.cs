using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

[RequireComponent(typeof(Image))]
public class VRInteractable_Button : VR_Interactable
{
    #region PARAMETERS
    [Header("Se pulsa al mantener el cursor encima?")]
    [SerializeField, HideInInspector] protected bool bClickByHover = false;

    [Header("El tiempo que se debe mantener el cursor encima para activar el click")]
    [SerializeField, HideInInspector] protected float fTimeToClickByHover = 1.5f;

    [Header("Se usa el efecto por defecto de pulsar por tiempo?")]
    [SerializeField, HideInInspector] protected bool bUsesDefaultHoverEffect = true;

    [Header("La direccion de rellenado que tiene")]
    [SerializeField, HideInInspector] protected Image.FillMethod fillMethod = Image.FillMethod.Horizontal;

    [Header("El objeto esta por delante o atras?")]
    [SerializeField, HideInInspector] protected bool IsBehind = false;

    [Header("La escala del objeto de Hover")]
    [SerializeField, HideInInspector] protected float hoverScale = 1;

    [Header("Sigue el anchor del padre?")]
    [SerializeField, HideInInspector] protected bool bUsesParentAnchor = true;

    [Header("El anchor min")]
    [SerializeField, HideInInspector] protected Vector2 anchorMin;

    [Header("El anchor max")]
    [SerializeField, HideInInspector] protected Vector2 anchorMax;

    [Header("La posicion del anchor")]
    [SerializeField, HideInInspector] protected Vector2 anchorPosition;

    [Header("El tamaño en pixeles")]
    [SerializeField, HideInInspector] protected Vector2 sizeDelta;

    [Header("Imagen a usar para en el efecto")]
    [SerializeField, HideInInspector] protected Image ExternalImage;

    public bool ShouldSetSiblingIndex = false;

    private GameObject effectObject;
    private Image effectImage;

    private float timeHovered;

    private float timeElapsed;

    private bool alreadyClicked;

    float lastTimeChecked;
    #endregion

    #region FUNCTIONS
    #region Functionality
    public override void Awake()
    {
        base.Awake();
        
    }
    protected virtual void Start()
    {
        if (this.bClickByHover)
        {
            SetupHover();
        }

        if(this.ShouldSetSiblingIndex)
            this.transform.SetSiblingIndex(0);
    }

    public virtual void SetupHover()
    {

        if (bUsesDefaultHoverEffect)
            CreateGameObjectForEffect();
        else
        {
            if(ExternalImage != null)
                effectImage = this.ExternalImage;
            if(effectImage)
                effectObject = effectImage.gameObject;
        }

        timeElapsed = 0.03f;
        if(effectImage != null && effectImage != ExternalImage)
            effectImage.color = PressedColor;

        this.onHover.AddListener(this.CheckHoverClick);
        this.onHoverExit.AddListener(this.ResetHoverClick);
        this.onHoverExit.AddListener(() => { alreadyClicked = false; });
        this.onClick.AddListener(this.ResetHoverClick);
    }

    public override void Update()
    {
        base.Update();
        if (!GetIsHovered() && this.effectImage && this.effectImage.fillAmount != 0) ResetHoverClick();

        if (ShouldSetSiblingIndex) this.transform.SetSiblingIndex(0);
    }
    protected virtual void CheckHoverClick()
    {
        if (Time.time - lastTimeChecked < timeElapsed) return;
        lastTimeChecked = Time.time;
        timeHovered += timeElapsed;
        Debug.Log(timeElapsed);

        if(timeHovered >= fTimeToClickByHover && !alreadyClicked)
        {
            OnClick();
            OnRelease();
            alreadyClicked = true;
        }

        if (!alreadyClicked)
        {
            UpdateDefaultEffect();
        }
    }

    protected virtual void ResetHoverClick()
    {
        timeHovered = 0;
        if(effectImage != null)
            effectImage.fillAmount = 0;
    }

    protected virtual void CreateGameObjectForEffect()
    {
        effectObject = new GameObject("HoverEffect", typeof(Image));
        effectObject.transform.parent = this.transform.parent;
        if (effectObject.GetComponent<Image>()) effectImage = effectObject.GetComponent<Image>();
        else effectImage = effectObject.AddComponent<Image>();

        effectImage.raycastTarget = false;
        RectTransform objectRect = effectObject.GetComponent<RectTransform>();
        RectTransform selfRect = this.GetComponent<RectTransform>();
        if (bUsesParentAnchor)
        {
            objectRect.anchorMin = selfRect.anchorMin;
            objectRect.anchorMax = selfRect.anchorMax;
            objectRect.anchoredPosition = selfRect.anchoredPosition;
        }
        
        objectRect.sizeDelta = selfRect.sizeDelta;

        effectObject.transform.localPosition = this.transform.localPosition;
        effectObject.transform.localRotation = this.transform.localRotation;
        effectObject.transform.localScale = this.transform.localScale * hoverScale;

        if (IsBehind)
            this.transform.parent = effectObject.transform;
        else { effectObject.transform.SetParent(this.transform); effectObject.transform.SetSiblingIndex(0); }

        if (this.transform.GetSiblingIndex() > 0 && IsBehind)
            effectObject.transform.SetSiblingIndex(this.transform.GetSiblingIndex() - 1);
        else effectObject.transform.SetSiblingIndex(0);
        
        effectImage.sprite = this.GetComponent<Image>().sprite;

        effectImage.type = Image.Type.Filled;
        effectImage.fillMethod = Image.FillMethod.Radial360;
        effectImage.fillAmount = 0;

        effectImage.color = new Color(0, 0, 0, 0.85f);
        
    }

    protected virtual void UpdateDefaultEffect()
    {
        if (effectImage)
        {
            effectImage.fillMethod = fillMethod;
            effectImage.fillAmount = timeHovered / fTimeToClickByHover;
        }
        
        if(bUsesDefaultHoverEffect)effectImage.rectTransform.sizeDelta = GetComponent<Image>().rectTransform.sizeDelta;
        //if (bUsesDefaultHoverEffect) effectImage.transform.localScale = this.transform.localScale * hoverScale;
    }

    #endregion
#if UNITY_EDITOR
    [MenuItem("Tesicnor/VR UI/VRButton")]
    public static void Create()
    {
        GameObject self = new GameObject("VRButton", typeof(VRInteractable_Button));
        if (Selection.gameObjects.Length > 0) self.transform.parent = Selection.gameObjects[0].transform;
        self.transform.localPosition = Vector3.zero;
        self.transform.localScale = Vector3.one;
        self.transform.localRotation = Quaternion.Euler(Vector3.zero);

        Image selfIMG = self.GetComponent<Image>();
        selfIMG.rectTransform.sizeDelta = new Vector2(300, 100);

        GameObject tmp = new GameObject("TMP", typeof(TextMeshProUGUI));
        tmp.transform.parent = self.transform;
        tmp.transform.localPosition = Vector3.zero;
        tmp.transform.localScale = Vector3.one;
        tmp.transform.localRotation = Quaternion.Euler(Vector3.zero);

        TextMeshProUGUI text = tmp.GetComponent<TextMeshProUGUI>();
        text.text = "New Button";
        text.rectTransform.sizeDelta = new Vector2(250, 80);
        text.fontSize = 24;
        text.color = Color.black;

        VRInteractable_Button button = self.GetComponent<VRInteractable_Button>();
        button.HoverColor *= 0.75f;
        button.PressedColor *= 0.5f;
    }

    [MenuItem("Component/UI/VRButton")]
    public static void Create_Component()
    {
        VRInteractable_Button button = Selection.gameObjects[0].AddComponent<VRInteractable_Button>();

        GameObject tmp = new GameObject("TMP", typeof(TextMeshProUGUI));
        tmp.transform.parent = button.transform;
        tmp.transform.localPosition = Vector3.zero;
        tmp.transform.localRotation = Quaternion.Euler(Vector3.zero);
        tmp.transform.localScale = Vector3.one;

        TextMeshProUGUI text = tmp.GetComponent<TextMeshProUGUI>();
        text.text = "New Button";
        text.rectTransform.sizeDelta = new Vector2(button.GetComponent<Image>().rectTransform.sizeDelta.x * 0.8f, button.GetComponent<Image>().rectTransform.sizeDelta.y * 0.80f);
        text.fontSize = 24;
        text.color = Color.black;

        button.HoverColor *= 0.75f;
        button.PressedColor *= 0.5f;
    }
#endif
#endregion
}

#if UNITY_EDITOR
[CustomEditor(typeof(VRInteractable_Button), true)]
[CanEditMultipleObjects]
public class VRInteractableButtonEditor: InteractableEditor
{
    VRInteractable_Button Target;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Target = target as VRInteractable_Button;

        SerializedProperty bUsesTimeByHover = serializedObject.FindProperty("bClickByHover");
        GUILayout.Label("Se puede clickar tambien al mantener en hover durante un tiempo?", EditorStyles.boldLabel);
        bUsesTimeByHover.boolValue = GUILayout.Toggle(bUsesTimeByHover.boolValue, "Click by hover time");

        GUILayout.Space(10);

        if (bUsesTimeByHover.boolValue)
        {
            SerializedProperty timeToClick = serializedObject.FindProperty("fTimeToClickByHover");
            SerializedProperty usesDefaultEffect = serializedObject.FindProperty("bUsesDefaultHoverEffect");

            GUILayout.Label("El tiempo que tarda en clickarse por hover", EditorStyles.boldLabel);
            timeToClick.floatValue = EditorGUILayout.FloatField(timeToClick.floatValue, EditorStyles.miniTextField);

            GUILayout.Space(10);

            GUILayout.Label("Se usa el efecto por defecto programado para el click por hover?", EditorStyles.boldLabel);
            usesDefaultEffect.boolValue = GUILayout.Toggle(usesDefaultEffect.boolValue, "Uses default hover effect");

            GUILayout.Space(10);

            if(usesDefaultEffect.boolValue == false)
            {
                SerializedProperty externalImage = serializedObject.FindProperty("ExternalImage");

                GUILayout.Label("La imagen que se va a usar para el efecto", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(externalImage);
            }
            else
            {
                SerializedProperty fillMethod = serializedObject.FindProperty("fillMethod");

                GUILayout.Label("El método de rellenado de la imagen del hover", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(fillMethod);

                GUILayout.Space(10);

                GUILayout.Label("La escala de la imagen que se va a usar", EditorStyles.boldLabel);
                SerializedProperty scale = serializedObject.FindProperty("hoverScale");
                scale.floatValue = EditorGUILayout.FloatField(scale.floatValue, EditorStyles.miniTextField);

                GUILayout.Space(10);

                GUILayout.Label("Se posiciona detras la imagen?", EditorStyles.boldLabel);
                SerializedProperty isBehind = serializedObject.FindProperty("IsBehind");
                isBehind.boolValue = GUILayout.Toggle(isBehind.boolValue, "Is Behind");

                GUILayout.Space(10);

                GUILayout.Label("Usa el anchor del padre?", EditorStyles.boldLabel);
                SerializedProperty parentAnchor = serializedObject.FindProperty("bUsesParentAnchor");
                parentAnchor.boolValue = GUILayout.Toggle(parentAnchor.boolValue, "Parent Anchor");

                if (!parentAnchor.boolValue)
                {
                    GUILayout.Space(10);

                    GUILayout.Label("El anchor min", EditorStyles.boldLabel);
                    SerializedProperty anchorMin = serializedObject.FindProperty("anchorMin");
                    anchorMin.vector2Value = EditorGUILayout.Vector2Field("Anchor Min", anchorMin.vector2Value);

                    GUILayout.Space(10);

                    GUILayout.Label("El anchor max", EditorStyles.boldLabel);
                    SerializedProperty anchorMax = serializedObject.FindProperty("anchorMax");
                    anchorMax.vector2Value = EditorGUILayout.Vector2Field("Anchor Max", anchorMax.vector2Value);

                    GUILayout.Space(10);

                    GUILayout.Label("La posicion del anchor", EditorStyles.boldLabel);
                    SerializedProperty anchorPosition = serializedObject.FindProperty("anchorPosition");
                    anchorPosition.vector2Value = EditorGUILayout.Vector2Field("Anchor Position", anchorPosition.vector2Value);

                    GUILayout.Space(10);

                    GUILayout.Label("El tamaño del objeto en píxeles", EditorStyles.boldLabel);
                    SerializedProperty sizeDelta = serializedObject.FindProperty("sizeDelta");
                    sizeDelta.vector2Value = EditorGUILayout.Vector2Field("sizeDelta", sizeDelta.vector2Value);
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif