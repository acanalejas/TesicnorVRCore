using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEditor;

[RequireComponent(typeof(Image))]
public class VRInteractable_Slider : MonoBehaviour
{
    #region PARAMETERS
    /// <summary>
    /// El evento que se dispara cuando cambia de valor
    /// </summary>
    [Header("El evento que se dispara cuando cambia de valor")]
    [SerializeField] private UnityEvent<float> onValueChanged;

    /// <summary>
    /// Es un slider vertical u horizontal?
    /// </summary>
    [Header("Es un slider vertical u horizontal?")]
    [SerializeField] public bool isHorizontal = true;

    /// <summary>
    /// El componente VR_Interactable del punto que se mueve
    /// </summary>
    [HideInInspector] public dot dot_Interactable;

    /// <summary>
    /// La imagen propia de este componente, el BG
    /// </summary>
    [HideInInspector] public Image selfIMG;

    /// <summary>
    /// El valor actual del slider entre 0 y 1
    /// </summary>
    public float currentValue = 0;

    /// <summary>
    /// Clase que controla el punto que se mueve del slider
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class dot : VR_Interactable
    {
        [HideInInspector] public VRInteractable_Slider slider;

        public Vector3 minPoint;
        public Vector3 maxPoint;

        public override void Awake()
        {
            base.Awake();
            // Se setean los puntos minimo y maximo entre los que se puede mover el punto dependiendo del tamaño del padre
            if (slider.isHorizontal)
            {
                minPoint = slider.transform.position - new Vector3(slider.selfIMG.rectTransform.sizeDelta.x * slider.transform.lossyScale.x / 2, 0);
                maxPoint = slider.transform.position + new Vector3(slider.selfIMG.rectTransform.sizeDelta.x * slider.transform.lossyScale.x / 2, 0);
            }
            else
            {
                minPoint = slider.transform.position - new Vector3(0, slider.selfIMG.rectTransform.sizeDelta.y * slider.transform.lossyScale.y / 2);
                maxPoint = slider.transform.position + new Vector3(0, slider.selfIMG.rectTransform.sizeDelta.y * slider.transform.lossyScale.y / 2);
            }
        }
        private void Start()
        {
            CheckValue();
        }
        public override void OnClick()
        {
            base.OnClick();
        }

        public override void OnRelease()
        {
            base.OnRelease();
        }

        //Hace de update mientras se pulsa el boton
        WaitForEndOfFrame frame = new WaitForEndOfFrame();
        private void Update()
        {
            if (GetIsClicking())
            {
                Attach();
            }
        }


        private Vector3 lastPoint;
        /// <summary>
        /// Setea la posicion del punto dependiendo de los puntos ya establecidos, y el rayo con el que apuntamos al objeto
        /// </summary>
        private void Attach()
        {
            VRInteractionInterface interaction = hand.GetComponent<VRInteractionInterface>();
            Vector3 hitPoint = interaction.GetRaycastHit(interaction.GetOrigin()).point;
            if (lastPoint != Vector3.zero && Vector3.Distance(hitPoint, lastPoint) > 0.5f) hitPoint = lastPoint;

            if (slider.isHorizontal) this.transform.position = new Vector3(Mathf.Clamp(hitPoint.x, minPoint.x, maxPoint.x), this.transform.position.y, this.transform.position.z);
            else this.transform.position = new Vector3(this.transform.position.x, Mathf.Clamp(hitPoint.y, minPoint.y, maxPoint.y), this.transform.position.z);

            CheckValue();
            lastPoint = hitPoint;
            //this.transform.position = new Vector3(Mathf.Clamp(this.transform.position.x, minPoint.x, maxPoint.x), Mathf.Clamp(this.transform.position.y, minPoint.y, maxPoint.y),0);
        }

        /// <summary>
        /// Checkea el valor actual del slider usando la distancia total y la distancia relativa al final del slider
        /// </summary>
        void CheckValue()
        {
            float distance = 0;
            float currentDistance = 0;
            if (slider.isHorizontal)
            {
                distance = maxPoint.x - minPoint.x;
                currentDistance = maxPoint.x - this.transform.position.x;
            }
            else
            {
                distance = maxPoint.y - minPoint.y;
                currentDistance = maxPoint.y - this.transform.position.y;
            }
            

            slider.currentValue = 1 - currentDistance / distance;
            Mathf.Abs(slider.currentValue);
        }
    }

    
    #endregion

    #region FUNCTIONS
#if UNITY_EDITOR
    /// <summary>
    /// Crea el objeto usando el menu de la ruta de pestañas que pone en los parentesis
    /// </summary>
    [MenuItem("Tesicnor/VR UI/VRSlider")]
    public static void Create()
    {
        //Se crea el GameObject principal
        GameObject self = null;
        self = new GameObject("VRSlider", typeof(VRInteractable_Slider));

        //Se establecem los valores de su Transform
        if (Selection.gameObjects.Length > 0) self.transform.parent = Selection.gameObjects[0].transform;
        self.transform.localPosition = Vector3.zero;
        self.transform.localScale = Vector3.one;

        //Se crea y setea el GameObject del punto
        GameObject dot = new GameObject("Dot", typeof(dot));
        dot.transform.parent = self.transform;
        dot.transform.localPosition = Vector3.zero;
        dot.transform.localScale = Vector3.one;

        //Accedemos a la imagen y le asignamos tamaño y color
        Image selfIMG = self.GetComponent<Image>();
        selfIMG.rectTransform.sizeDelta = new Vector2(300, 50);
        selfIMG.color *= 0.75f;

        //Accedemos a la imagen del punto y le asignamos un tamaño
        Image dotIMG = dot.GetComponent<Image>();
        dotIMG.rectTransform.sizeDelta = new Vector2(60, 60);

        //Accedemos al compontente dot y le seteamos los valores necesarios para su funcionamiento
        dot inter = dot.GetComponent<dot>();
        inter.is3DObject = false;
        inter.slider = self.GetComponent<VRInteractable_Slider>();
        inter.SetupUICollider();

        //Accedemos al componente VRInteractable_Slider del GameObject principal y le seteamos los valores necesarios
        VRInteractable_Slider slider = self.GetComponent<VRInteractable_Slider>();
        slider.dot_Interactable = inter;
        slider.selfIMG = selfIMG;
    }

    public static GameObject Create_GO(GameObject parent)
    {
        //Se crea el GameObject principal
        GameObject self = null;
        self = new GameObject("VRSlider", typeof(VRInteractable_Slider));

        //Se establecem los valores de su Transform
        self.transform.parent = parent.transform;
        self.transform.localPosition = Vector3.zero;
        self.transform.localScale = Vector3.one;

        //Se crea y setea el GameObject del punto
        GameObject dot = new GameObject("Dot", typeof(dot));
        dot.transform.parent = self.transform;
        dot.transform.localPosition = Vector3.zero;
        dot.transform.localScale = Vector3.one;

        //Accedemos a la imagen y le asignamos tamaño y color
        Image selfIMG = self.GetComponent<Image>();
        selfIMG.rectTransform.sizeDelta = new Vector2(300, 50);
        selfIMG.color *= 0.75f;

        //Accedemos a la imagen del punto y le asignamos un tamaño
        Image dotIMG = dot.GetComponent<Image>();
        dotIMG.rectTransform.sizeDelta = new Vector2(60, 60);

        //Accedemos al compontente dot y le seteamos los valores necesarios para su funcionamiento
        dot inter = dot.GetComponent<dot>();
        inter.is3DObject = false;
        inter.slider = self.GetComponent<VRInteractable_Slider>();
        inter.SetupUICollider();

        //Accedemos al componente VRInteractable_Slider del GameObject principal y le seteamos los valores necesarios
        VRInteractable_Slider slider = self.GetComponent<VRInteractable_Slider>();
        slider.dot_Interactable = inter;
        slider.selfIMG = selfIMG;

        return self;
    }

    /// <summary>
    /// Crea el componente siguiendo al ruta de ventanas que hay entre parentesis
    /// </summary>
    [MenuItem("Component/UI/VRSlider")]
    static void Create_Component()
    {
        //Accedemos al GameObject seleccionado y le añadimos el componente
        GameObject self = Selection.gameObjects[0];
        Selection.gameObjects[0].AddComponent<VRInteractable_Slider>();

        //Creamos y seteamos el GameObject del punto
        GameObject dot = new GameObject("Dot", typeof(dot));
        dot.transform.parent = self.transform;
        dot.transform.localPosition = Vector3.zero;
        dot.transform.localScale = Vector3.one;

        //Accedemos a la imagen y le damos tamaño y color
        Image selfIMG = self.GetComponent<Image>();
        selfIMG.rectTransform.sizeDelta = new Vector2(300, 50);
        selfIMG.color *= 0.75f;

        //Accedemos a la imagen del punto y le asignamos un tamaño
        Image dotIMG = dot.GetComponent<Image>();
        dotIMG.rectTransform.sizeDelta = new Vector2(60, 60);

        //Accedemos al componente dot del punto y le asignamos los valores para su funcionamiento
        dot inter = dot.GetComponent<dot>();
        inter.is3DObject = false;
        inter.slider = self.GetComponent<VRInteractable_Slider>();
        inter.SetupUICollider();

        //Accedemos al componente VRInteractable_Slider del GameObject principal y le asignamos los valores necesarios para que funcione
        VRInteractable_Slider slider = self.GetComponent<VRInteractable_Slider>();
        slider.dot_Interactable = inter;
        slider.selfIMG = selfIMG;
    }
#endif

    private void Start()
    {
        StartCoroutine("update");
    }

    /// <summary>
    /// Update del slider
    /// </summary>
    WaitForEndOfFrame frame = new WaitForEndOfFrame();
    private IEnumerator update()
    {
        while (true)
        {
            ValueChanges();
            yield return frame;
        }
    }

    float lastValue = 0;
    bool firstFrame = true;
    /// <summary>
    /// Checkea si cambia el valor del slider y si es asi invoca el evento correspondiente
    /// </summary>
    /// <returns></returns>
    private bool ValueChanges()
    {
        if(!firstFrame && lastValue != currentValue) { onValueChanged.Invoke(currentValue); firstFrame = false; return true; }
        firstFrame = false;
        return false;
    }
#endregion
}
