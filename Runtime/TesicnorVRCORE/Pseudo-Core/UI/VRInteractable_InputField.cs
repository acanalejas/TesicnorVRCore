using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class VRInteractable_InputField : VR_Interactable
{
    #region PARAMETERS
    [Header("El texto sobre el que se va a escribir")]
    public TextMeshProUGUI writeText;

    [Header("El texto que se usará como preview")]
    public TextMeshProUGUI sampleText;
    #endregion

    #region FUNCTIONS

    public override void Awake()
    {
        base.Awake();
        sampleText.color = writeText.color * 0.75f;
        this.canBePressed = true;
        StartCoroutine("update");
    }

    public override void OnClick()
    {
        base.OnClick();
        VRInteractable_Keyboard.setInputField(this);
        if (VRInteractable_Keyboard.Instance) VRInteractable_Keyboard.Instance.EnableKeyboard(true);
    }

    WaitForEndOfFrame frame = new WaitForEndOfFrame();
    IEnumerator update()
    {
        while (true)
        {
            if (writeText.text != "") sampleText.gameObject.SetActive(false);
            else sampleText.gameObject.SetActive(true);
            yield return frame;
        }
    }

#if UNITY_EDITOR
    [MenuItem("Tesicnor/VR UI/VR_InputField")]
    public new static void Create()
    {
        Transform parent = null;
        if(Selection.gameObjects.Length > 0) parent = Selection.gameObjects[0].transform;

        GameObject inputField_go = new GameObject("VRInputField", typeof(VRInteractable_InputField));
        inputField_go.transform.parent = parent;
        inputField_go.transform.localPosition = Vector3.zero;
        inputField_go.transform.localRotation = Quaternion.identity;
        inputField_go.transform.localScale = Vector3.one;

        GameObject inputField_writeText = new GameObject("WriteText", typeof(TextMeshProUGUI));
        inputField_writeText.transform.parent = inputField_go.transform;
        inputField_writeText.transform.localPosition = Vector3.zero;
        inputField_writeText.transform.localRotation = Quaternion.identity;
        inputField_writeText.transform.localScale = Vector3.one;

        GameObject inputField_sampleText = new GameObject("SampleText", typeof(TextMeshProUGUI));
        inputField_sampleText.transform.parent = inputField_go.transform;
        inputField_sampleText.transform.localPosition = Vector3.zero;
        inputField_sampleText.transform.localRotation = Quaternion.identity;
        inputField_sampleText.transform.localScale = Vector3.one;

        VRInteractable_InputField inputField = inputField_go.GetComponent<VRInteractable_InputField>();
        inputField.writeText = inputField_writeText.GetComponent<TextMeshProUGUI>();
        inputField.sampleText = inputField_sampleText.GetComponent<TextMeshProUGUI>();


        inputField.sampleText.color = Color.black;
        inputField.sampleText.alignment = TextAlignmentOptions.Center;
        inputField.sampleText.autoSizeTextContainer = true;
        inputField.writeText.color = Color.black;
        inputField.writeText.alignment = TextAlignmentOptions.Center;
        inputField.writeText.autoSizeTextContainer = true;
        inputField.sampleText.text = "sample@sample.com";

        inputField_go.GetComponent<Image>().rectTransform.sizeDelta = new Vector2(1000, 100);
    }
#endif
#endregion
}
