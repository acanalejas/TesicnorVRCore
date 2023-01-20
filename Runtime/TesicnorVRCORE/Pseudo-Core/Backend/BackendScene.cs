using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Http;
using System.IO;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BackendScene : MonoBehaviour
{
    #region PARAMETERS
    public static string BackendSceneName { get { return "BackendEmailScene"; } }
    static int nextBuildIndex;

    private HttpClient httpClient;

    public Animator PopUp_Animator;

    public GameObject WarningPopUp_go;

    List<Image> imgs = new List<Image>();
    List<TextMeshProUGUI> texts = new List<TextMeshProUGUI>();
    List<Text> texts_legacy = new List<Text>();
    #endregion

    #region METHODS
    public void Start()
    {
        GetAllRenderedComponentsInWarningPopUp();
        SetWarningPopUpToTransparent();
        httpClient = new HttpClient();
    }

    private void GetAllRenderedComponentsInWarningPopUp()
    {
        imgs.Clear();
        texts.Clear();
        texts_legacy.Clear();

        imgs.AddRange(WarningPopUp_go.GetComponentsInChildren<Image>());
        texts.AddRange(WarningPopUp_go.GetComponentsInChildren<TextMeshProUGUI>());
        texts_legacy.AddRange(WarningPopUp_go.GetComponentsInChildren<Text>());
    }
    private void SetWarningPopUpToTransparent()
    {
        foreach(var img in imgs)
        {
            img.color = new Color(img.color.r, img.color.g, img.color.b, 0);
        }
        foreach(var text in texts)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
        }
        foreach(var text in texts_legacy)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
        }
    }
    public async void EnterButton()
    {
        string user = "";
        if (VRInteractable_Keyboard.inputfieldText) user = VRInteractable_Keyboard.inputfieldText.text;
        string appCode = BackendGetter.appCode.ToString();

        var cts = new System.Threading.CancellationTokenSource();
        using (var request = new HttpRequestMessage(HttpMethod.Get, BackendGetter.urlNoParams + "?applicationId=" + appCode + "&" + "userName=" + user))
        {
            using(var response = await httpClient.SendAsync(request, cts.Token))
            {
                if(response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    Debug.Log("Backend connected with correct user");
                    string content = response.Content.ReadAsStringAsync().Result;
                    BackendData data = JsonUtility.FromJson<BackendData>(content);
                    BackendGetter.backendData = data;
                    PlayerPrefs.SetString(BackendGetter.BackendDataKey, content);
                    PlayerPrefs.SetString("Username", user);
                    Debug.Log(data.name);
                }
                else
                {
                    Debug.Log(response.StatusCode);
                    StartCoroutine(nameof(ShowPopUp));
                }
            }
        }
        SceneManager.LoadScene(nextBuildIndex);
    }

    WaitForEndOfFrame frame = new WaitForEndOfFrame();
    private IEnumerator ShowPopUp()
    {
        if (imgs[0].color.a > 0) yield break;
        float alpha = 0;
        while(alpha < 1)
        {
            alpha += 0.01f;
            foreach (var img in imgs) img.color += new Color(0, 0, 0, 0.01f);
            foreach (var text in texts) text.color += new Color(0, 0, 0, 0.01f);
            foreach (var text in texts_legacy) text.color += new Color(0, 0, 0, 0.01f);
            yield return frame;
        }

        yield return new WaitForSeconds(2f);

        while(alpha > 0)
        {
            alpha -= 0.01f;
            foreach (var img in imgs) img.color -= new Color(0, 0, 0, 0.01f);
            foreach (var text in texts) text.color -= new Color(0, 0, 0, 0.01f);
            foreach (var text in texts_legacy) text.color -= new Color(0, 0, 0, 0.01f);
            yield return frame;
        }
        StopCoroutine(nameof(ShowPopUp));
    }

    public static void GoToBackendScene(int nextSceneBuildIndex)
    {
        SceneManager.LoadScene(BackendSceneName);
        nextBuildIndex = nextSceneBuildIndex;
    }
    #endregion
}
