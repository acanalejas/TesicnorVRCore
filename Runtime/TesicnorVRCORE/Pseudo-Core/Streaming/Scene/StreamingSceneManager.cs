using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using TesicnorVR;

public class StreamingSceneManager : MonoBehaviour
{
    #region PARAMETERS
    public static int nextSceneIndex = 0;
    public static string nextSceneName = "";

    public GameObject questionCanvas, keyboardCanvas;
    public TextMeshProUGUI IPText;

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
            questionCanvas.SetActive(true);
            keyboardCanvas.SetActive(false);
        }
    }

    public void Yes()
    {
        StreamingCSharp.HttpClient_Custom.url = "";
        StreamingCSharp.HttpClient_Custom.IntializeClient();

        SceneManager.LoadScene(nextSceneIndex);
    }
    public void No()
    {
        SceneManager.LoadScene(nextSceneIndex);
    }

    public static void GoToStreamingScene(int _nextSceneIndex, string _nextSceneName = "")
    {
        nextSceneIndex = _nextSceneIndex;
        nextSceneName = _nextSceneName;
        SceneManager.LoadScene("StreamingScene");
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

        questionCanvas.SetActive(false);
        keyboardCanvas.SetActive(true);
    }

    public void Keyboard(string input)
    {
       
        ip += input;
        IPText.text = ip;
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
        IPText.text = ip;
    }
    public void KeyboardEnter()
    {
        Debug.Log(ip);
        string url = "http://" + ip + ":8080/";
        Debug.Log(url);
        StreamingCSharp.HttpClient_Custom.url = url;

        GoToNextScene();
    }

    #endregion
}
