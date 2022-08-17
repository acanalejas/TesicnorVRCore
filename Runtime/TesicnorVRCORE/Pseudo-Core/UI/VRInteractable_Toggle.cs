using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

[RequireComponent(typeof(Image))]
public class VRInteractable_Toggle : VR_Interactable
{
    #region PARAMETERS
    [Header("La ruta donde está la imagen del check")]
    //[SerializeField] protected string checkPath = "Assets/Images/UI/Check.png";
    [SerializeField] protected string checkPath = "Assets/Scripts/Pseudo-Core/UI/Sprites/Check.png";

    [Header("El evento que se invoca al cambiar el valor")]
    public UnityEvent<bool> onValueChanged;

    [HideInInspector] public GameObject check;
    #endregion

    #region FUNCTIONS
#if UNITY_EDITOR
    [MenuItem("GameObject/UI/VRToggle")]
    static void Create()
    {
        //Crea el GameObject principal
        GameObject self = new GameObject("VRToggle", typeof(VRInteractable_Toggle));
        if (Selection.gameObjects[0]) self.transform.parent = Selection.gameObjects[0].transform;
        self.transform.localPosition = Vector3.zero;
        self.transform.localScale = Vector3.one;

        VRInteractable_Toggle toggle = self.GetComponent<VRInteractable_Toggle>();

        //Crea el GameObject que llevará la imagen del check
        GameObject check = new GameObject("Check", typeof(Image));
        check.transform.parent = self.transform;
        check.transform.localPosition = Vector3.zero;
        check.transform.localScale = Vector3.one;

        //Crea el GameObject que llevará el texto
        GameObject tmp = new GameObject("TMP", typeof(TextMeshProUGUI));
        tmp.transform.parent = self.transform;
        tmp.transform.localPosition = Vector3.zero + new Vector3(30 + 75, 0);
        tmp.transform.localScale = Vector3.one;

        //Accedemos a la imagen principal y le asignamos valores
        Image selfIMG = self.GetComponent<Image>();
        selfIMG.rectTransform.sizeDelta = new Vector2(60, 60);

        //Accedemos a la imagen del check y le asignamos valores
        Image checkIMG = check.GetComponent<Image>();
        checkIMG.sprite = (Sprite)AssetDatabase.LoadAssetAtPath(toggle.checkPath, typeof(Sprite));
        checkIMG.rectTransform.sizeDelta = new Vector2(60, 60);
        checkIMG.raycastTarget = false;

        //Accedemos al texto y le seteamos los valores necesarios
        TextMeshProUGUI text = tmp.GetComponent<TextMeshProUGUI>();
        text.text = "Toggle text";
        text.fontSize = 30;
        text.color = Color.black;
        text.rectTransform.sizeDelta = new Vector2(150, 60);

        toggle.check = check;
        toggle.HoverColor *= 0.75f;
        toggle.PressedColor *= 0.5f;
        
    }
#endif

    private void ChangeValue()
    {
        if (check.activeSelf) check.SetActive(false);
        else check.SetActive(true);

        onValueChanged.Invoke(check.activeSelf);
    }

    public override void OnRelease()
    {
        base.OnRelease();
        ChangeValue();
    }

    #endregion
}
