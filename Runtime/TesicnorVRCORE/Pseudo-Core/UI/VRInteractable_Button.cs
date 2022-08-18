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

    #endregion

    #region FUNCTIONS
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
