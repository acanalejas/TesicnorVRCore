using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class DeadScene : MonoBehaviour
{
    #region PARAMETERS
    public static DeadScene Instance;
    private static int menuIndex;
    private static int lastSceneIndex;

    static string deadScene_name = "DeadScene";
    static string deadScene_results = "";
    static string deadScene_retry = "RETRY";
    static string deadScene_goToMenu = "GO TO MENU";
    static string result0 = "";
    static string result1 = "";
    static string result2 = "";
    static string result3 = "";
    static string result4 = "";
    static string title = "";
    static string seconds = "";
    static string porcentaje = "";

    static bool value0 = false, value1 = false, value2 = false, value3 = false;
    static bool approved = true;

    public TextMeshProUGUI results;
    public Button MenuButton;
    public Button RetryButton;

    [Header("Para el player")]
    public GameObject rightHand;
    public GameObject leftHand;

    [Header("Los textos de los resultados")]
    public TextMeshProUGUI result0Text, result1Text, result2Text, result3Text, result4Text, Title, porcentajeText, secondsText;
    [Header("Los ticks de los resultados")]
    public Image result0Tick, result1Tick, result2Tick, result3Tick, border;
    [Header("Los sprites que se usan")]
    public Sprite greenBorder, redBorder;
    public Sprite tick, cross;
    [Header("La medalla")]
    public GameObject medal;
    [Header("Los colores para el titulo")]
    public Color incorrectColor;

    [HideInInspector] public static List<Component> ForButtons = new List<Component>();

    [HideInInspector] public static List<Component> ForRightHand_Components = new List<Component>();
    [HideInInspector] public static List<GameObject> ForRightHand_GO = new List<GameObject>();

    [HideInInspector] public static List<Component> ForLeftHand_Components = new List<Component>();
    [HideInInspector] public static List<GameObject> ForLeftHand_GO = new List<GameObject>();
    #endregion

    #region FUNCTIONS
    #region Static functions
    public static void GoToDeadScene(int _menuIndex, int _lastSceneIndex, MonoBehaviour _class, string _function , string retryTraduction = "RETRY", string goToMenuTraduction = "GO TO MENU")
    {
        menuIndex = _menuIndex; lastSceneIndex = _lastSceneIndex;  _class.Invoke(_function, 0.0f);
        deadScene_retry = retryTraduction; deadScene_goToMenu = goToMenuTraduction;

        SceneManager.LoadScene(deadScene_name);
    }
    public static void SetDeadText(string input)
    {
        deadScene_results = input;
    }

    public static void SetResults(string _result0, string _result1, string _result2, string _result3, string _result4, bool _value0, bool _value1, bool _value2, bool _value3, string _title, bool _approved, string _porcentaje, string _seconds)
    {
        result0 = _result0; result1 = _result1; result2 = _result2; result3 = _result3; result4 = _result4;
        value0 = _value0; value1 = _value1; value2 = _value2; value3 = _value3;
        title = _title; approved = _approved;
        porcentaje = _porcentaje; seconds = _seconds;
    }

    public static void AddComponentsToButtons(List<Component> toAdd)
    {
        ForButtons.Clear();
        ForButtons.AddRange(toAdd);
    }

    public static void AddComponentsToRightHand(List<Component> toAdd)
    {
        ForRightHand_Components.Clear();
        ForRightHand_Components.AddRange(toAdd);
    }

    public static void AddComponentsToRightHand(List<GameObject> toAdd)
    {
        ForRightHand_GO.Clear();
        ForRightHand_GO.AddRange(toAdd);
    }

    public static void AddComponentsToLeftHand(List<Component> toAdd)
    {
        ForLeftHand_Components.Clear();
        ForLeftHand_Components.AddRange(toAdd);
    }
    public static void AddComponentsToLeftHand(List<GameObject> toAdd)
    {
        ForLeftHand_GO.Clear();
        ForLeftHand_GO.AddRange(toAdd);
    }
    #endregion

    private void SetComponentsForButtons()
    {
        if (ForButtons.Count <= 0) return;
        
        foreach(Component comp in ForButtons)
        {
            Component menuComp = MenuButton.gameObject.AddComponent(comp.GetType());
            menuComp = comp;
            
            Component retryComp = RetryButton.gameObject.AddComponent(comp.GetType());
            retryComp = comp;
        }
    }

    private void SetComponentsHands()
    {
        if(ForRightHand_Components.Count > 0)
        {
            foreach(Component comp in ForRightHand_Components)
            {
                Component _comp = rightHand.gameObject.AddComponent(comp.GetType());
                _comp = comp;
            }
        }
        if(ForLeftHand_Components.Count > 0)
        {
            foreach(Component comp in ForLeftHand_Components)
            {
                Component _comp = leftHand.gameObject.AddComponent(comp.GetType());
                _comp = comp;
            }
        }
    }

    private void SetGameObjectsForHands()
    {
        if(ForRightHand_GO.Count > 0)
        {
            foreach(GameObject go in ForRightHand_GO)
            {
                GameObject newGo = GameObject.Instantiate(go, rightHand.transform);
            }
        }

        if(ForLeftHand_GO.Count > 0)
        {
            foreach(GameObject go in ForLeftHand_GO)
            {
                GameObject newGo = GameObject.Instantiate(go, leftHand.transform);
            }
        }
    }

    private void SetResults()
    {
        result0Text.text = result0; result1Text.text = result1; result2Text.text = result2; result3Text.text = result3; result4Text.text = result4;
        if (!value0) result0Tick.sprite = cross; 
        if (!value1) result1Tick.sprite = cross;
        if (!value2) result2Tick.sprite = cross;
        if (!value3) result3Tick.sprite = cross;

        if (!approved) { border.sprite = redBorder; medal.SetActive(false); }
        Title.text = title;
        if (!approved) Title.color = incorrectColor;
        porcentajeText.text = porcentaje; secondsText.text = seconds;
    }

    private void Start()
    {
        if (Instance == null) Instance = this; else Destroy(this);
        //results.text = deadScene_results;
        SetResults();
        SetComponentsForButtons();
        SetComponentsHands();
        SetGameObjectsForHands();

        MenuButton.onClick.AddListener(GoToMenu);
        RetryButton.onClick.AddListener(Retry);

        MenuButton.GetComponentInChildren<TextMeshProUGUI>().text = deadScene_goToMenu;
        RetryButton.GetComponentInChildren<TextMeshProUGUI>().text = deadScene_retry;
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene(menuIndex);
    }
    public void Retry()
    {
        SceneManager.LoadScene(lastSceneIndex);
    }
    #endregion
}
