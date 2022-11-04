using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class DeadScene : MonoBehaviour
{
    #region PARAMETERS
    private static int menuIndex;
    private static int lastSceneIndex;

    static string deadScene_name = "DeadScene";
    static string deadScene_results = "";
    static string deadScene_retry = "RETRY";
    static string deadScene_goToMenu = "GO TO MENU";

    public TextMeshProUGUI results;
    public Button MenuButton;
    public Button RetryButton;
    #endregion

    #region FUNCTIONS
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

    private void Start()
    {
        results.text = deadScene_results;

        MenuButton.onClick.AddListener(GoToMenu);
        RetryButton.onClick.AddListener(Retry);

        MenuButton.GetComponentInChildren<TextMeshProUGUI>().text = deadScene_goToMenu;
        RetryButton.GetComponentInChildren<TextMeshProUGUI>().text = deadScene_retry;
    }

    private void GoToMenu()
    {
        SceneManager.LoadScene(menuIndex);
    }
    private void Retry()
    {
        SceneManager.LoadScene(lastSceneIndex);
    }
    #endregion
}
