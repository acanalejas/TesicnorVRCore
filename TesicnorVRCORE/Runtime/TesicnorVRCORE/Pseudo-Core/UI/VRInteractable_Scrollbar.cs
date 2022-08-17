using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Events;

public class VRInteractable_Scrollbar : MonoBehaviour
{
    #region PARAMETERS
    [HideInInspector] public GameObject mask;
    [HideInInspector] public GameObject content;
    [HideInInspector] public GameObject slider;
    [HideInInspector] public VRInteractable_Slider slider_inter;

    private Vector3 initialContentPosition;
    #endregion

    #region FUNCTIONS
#if UNITY_EDITOR
    [MenuItem("GameObject/UI/VRScrollbar")]
    static void Create()
    {
        //Creamos el GameObject principal
        GameObject self = new GameObject("Scrollbar", typeof(VRInteractable_Scrollbar), typeof(HorizontalLayoutGroup));
        if (Selection.gameObjects[0]) self.transform.parent = Selection.gameObjects[0].transform;
        self.transform.localPosition = Vector3.zero;
        self.transform.localScale = Vector3.one;

        VRInteractable_Scrollbar scrollbar = self.GetComponent<VRInteractable_Scrollbar>();

        //Creamos el GameObject de la mascara
        GameObject mask = new GameObject("Mask", typeof(Image), typeof(Mask));
        mask.transform.parent = self.transform;
        mask.transform.localPosition = Vector3.zero;
        mask.transform.localScale = Vector3.one;

        //Creamos el GameObject del contenido
        GameObject content = new GameObject("Content", typeof(VerticalLayoutGroup));
        content.transform.parent = mask.transform;
        content.transform.localPosition = Vector3.zero;
        content.transform.localScale = Vector3.one;

        VerticalLayoutGroup vl = content.GetComponent<VerticalLayoutGroup>();
        vl.childAlignment = TextAnchor.UpperCenter;

        //Accedemos a la imagen de la mascara y le seteamos valores
        Image maskIMG = mask.GetComponent<Image>();
        maskIMG.rectTransform.sizeDelta = new Vector2(300, 300);
        maskIMG.color = new Color(maskIMG.color.r, maskIMG.color.g, maskIMG.color.b, 0.01f);

        //Creamos el slider
        GameObject slider = VRInteractable_Slider.Create_GO(self);
        VRInteractable_Slider inter = slider.GetComponent<VRInteractable_Slider>();
        inter.isHorizontal = false;
        inter.selfIMG.rectTransform.sizeDelta = new Vector2(50, 300);
        slider.transform.localPosition = new Vector3(200, 0);
        slider.transform.localScale = Vector3.one;

        scrollbar.slider = slider;
        scrollbar.mask = mask;
        scrollbar.slider_inter = inter;
        scrollbar.content = content;
    }
#endif

    private void Start()
    {
        StartCoroutine("update");
    }

    WaitForEndOfFrame frame = new WaitForEndOfFrame();
    private IEnumerator update()
    {
        initialContentPosition = content.transform.localPosition;
        while (true)
        {
            content.transform.localPosition = scrollPosition();
            yield return frame;
        }
    }

    /// <summary>
    /// Devuelve la posición en la que se debe encontrar en ese frame el contenido
    /// </summary>
    /// <returns></returns>
    private Vector3 scrollPosition()
    {
        float addHeight = allChildsHeight() * slider_inter.currentValue;

        Vector3 result = new Vector3(0,-50,0) + new Vector3(0, addHeight);

        return result;
    }

    /// <summary>
    /// Recoge la altura combinada de todo el contenido del scroll
    /// </summary>
    /// <returns></returns>
    float allChildsHeight()
    {
        RectTransform[] allChildren = content.GetComponentsInChildren<RectTransform>();

        float totalHeight = 0;

        foreach (RectTransform child in allChildren)
        {
            totalHeight += child.sizeDelta.y * child.localScale.y;
        }

        Debug.Log(totalHeight);
        return totalHeight;
    }
#endregion
}
