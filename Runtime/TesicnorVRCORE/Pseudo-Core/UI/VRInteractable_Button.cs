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

    [Header("Imagen a usar para en el efecto")]
    [SerializeField, HideInInspector] protected Image ExternalImage;

    private GameObject effectObject;
    private Image effectImage;

    private float timeHovered;

    private float timeElapsed;

    private bool alreadyClicked;
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
    }

    public virtual void SetupHover()
    {

        if (bUsesDefaultHoverEffect)
            CreateGameObjectForEffect();
        else
        {
            effectImage = this.ExternalImage;
            effectObject = effectImage.gameObject;
        }

        timeElapsed = Time.deltaTime;

        this.onHover.AddListener(this.CheckHoverClick);
        this.onHoverExit.AddListener(this.ResetHoverClick);
        this.onHoverExit.AddListener(() => { alreadyClicked = false; });
        this.onClick.AddListener(this.ResetHoverClick);
    }

    public override void Update()
    {
        base.Update();
        if (!GetIsHovered() && this.effectImage && this.effectImage.fillAmount != 0) ResetHoverClick();
    }
    protected virtual void CheckHoverClick()
    {
        timeHovered += timeElapsed;

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
        effectImage.fillAmount = 0;
    }

    protected virtual void CreateGameObjectForEffect()
    {
        effectObject = new GameObject("HoverEffect", typeof(Image));
        effectObject.transform.parent = this.transform.parent;
        effectImage = effectObject.GetComponent<Image>();
        RectTransform objectRect = effectObject.GetComponent<RectTransform>();
        RectTransform selfRect = this.GetComponent<RectTransform>();
        objectRect.sizeDelta = selfRect.sizeDelta;

        effectObject.transform.localPosition = this.transform.localPosition;
        effectObject.transform.localRotation = this.transform.localRotation;
        effectObject.transform.localScale = this.transform.localScale * 1.2f;

        this.transform.parent = effectObject.transform;

        if (this.transform.GetSiblingIndex() > 0)
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
        effectImage.fillAmount = timeHovered / fTimeToClickByHover;

        if(bUsesDefaultHoverEffect)effectImage.rectTransform.sizeDelta = GetComponent<Image>().rectTransform.sizeDelta;
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
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif