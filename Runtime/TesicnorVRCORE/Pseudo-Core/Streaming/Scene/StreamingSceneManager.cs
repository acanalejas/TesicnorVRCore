using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using TesicnorVR;
using System.Text.RegularExpressions;

public class StreamingSceneManager : MonoBehaviour
{
    #region PARAMETERS
    public static int nextSceneIndex = 0;
    public static string nextSceneName = "";

    static string InputIP;
    static string Enter;

    public GameObject questionCanvas, keyboardCanvas;
    public GameObject SampleText;
    public GameObject EnterButton;
    public TextMeshProUGUI IPText;
    public TextMeshProUGUI InputIPText;
    public TextMeshProUGUI EnterText;

    public bool ShouldChangeSceneOnEnter = true;

    string ip = "";
    #endregion

    #region FUNCTIONS

    private void Start()
    {
        //questionCanvas.SetActive(true);
        //keyboardCanvas.SetActive(false);
        StreamingCSharp.HttpClient_Custom.IntializeClient();

        if(StreamingCSharp.HttpClient_Custom.isStreaming)
        {
            if(questionCanvas) questionCanvas.SetActive(true);
            if(keyboardCanvas) keyboardCanvas.SetActive(false);
        }

        if(EnterText)
            EnterText.text = Enter;
        //if(InputIPText)
        //    InputIPText.text = InputIP;
    }

    public void Yes()
    {
        StreamingCSharp.HttpClient_Custom.url = "";

        SceneManager.LoadScene(nextSceneIndex);
    }
    public void No()
    {
        SceneManager.LoadScene(nextSceneIndex);
    }

    private int dotAmount(string _input)
    {
        int dots = 0;

        foreach(char c in _input)
        {
            if (c == '.') dots++;
        }

        return dots;
    }

    public static void GoToStreamingScene(int _nextSceneIndex,  string inputIP, string enter, string _nextSceneName = "")
    {
        nextSceneIndex = _nextSceneIndex;
        nextSceneName = _nextSceneName;
        SceneManager.LoadScene("StreamingScene");

        InputIP = inputIP;
        Enter = enter;
    }

    public void GoToNextScene()
    {
        if(nextSceneName != "")
        {
            SceneManager.LoadScene(nextSceneName);
            return;
        }
        SceneManager.LoadScene(nextSceneIndex);
    }

    public void GoToKeyboard()
    {
        if (!questionCanvas || !keyboardCanvas) return;

        if(questionCanvas)
            questionCanvas.SetActive(false);
        if(keyboardCanvas)
            keyboardCanvas.SetActive(true);
    }

    public void Keyboard(string input)
    {
        ip += input;
        IPText.text = ip;

        if(SampleText)
        SampleText.SetActive(string.IsNullOrEmpty(ip));
        if(EnterButton)
        EnterButton.SetActive(dotAmount(ip) >= 3);
    }
    public void KeyboardDelete()
    {
        char[] chars = ip.ToCharArray();
        if (chars.Length <= 0) return;
        string newString = "";
        for(int i = 0; i < chars.Length - 1; i++)
        {
            newString += chars[i];
        }
        ip = newString;
        if(IPText)
        IPText.text = ip;

        if(SampleText)
        SampleText.SetActive(string.IsNullOrEmpty(ip));
        if(EnterButton)
        EnterButton.SetActive(dotAmount(ip) >= 3);
    }
    public void KeyboardEnter()
    {
        ip = IPText.text;
        Debug.Log(ip);
        string url = "http://" + ip + ":8080/";
        Regex sWhitespace = new Regex(@"\s+");
        url = sWhitespace.Replace(url, "");
        Debug.Log(url);
        StreamingCSharp.HttpClient_Custom.url = url;
        StreamingCSharp.HttpClient_Custom.IntializeClient();

        if(ShouldChangeSceneOnEnter)
            GoToNextScene();
    }

    #endregion
}
