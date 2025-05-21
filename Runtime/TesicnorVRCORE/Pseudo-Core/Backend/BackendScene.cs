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

    public GameObject ConnectionPopUp_go;

    public GameObject MailCanvas;

    public GameObject logOutCanvas;

    public GameObject SuccessObject;

    public TextMeshProUGUI emailText;

    public bool ShouldBeTraduced = true;

    public bool ShouldChangeSceneOnEnter = true;

    List<Image> imgs = new List<Image>();
    List<TextMeshProUGUI> texts = new List<TextMeshProUGUI>();
    List<Text> texts_legacy = new List<Text>();

    List<Image> con_imgs = new List<Image>();
    List<TextMeshProUGUI> con_texts = new List<TextMeshProUGUI>();
    List<Text> con_texts_legacy = new List<Text>();

    List<Image> success_imgs = new List<Image>();
    List<TextMeshProUGUI> sucess_texts = new List<TextMeshProUGUI>();

    static string inputMail_str, incorrectEmail_str, badConnection_str, yes_str, no_str, logout_str;
    public TextMeshProUGUI inputMail_text, incorrectMail_text, badConnection_text, yes_text, no_text, logout_text;

    [Header("Para la pantalla del menu principal")]
    [SerializeField] private GameObject lettersGrid, numbersGrid;
    [SerializeField] public TMPro.TextMeshProUGUI EmailInputFieldText;

    [Header("El punto que nos da feedback visual de la conexion")]
    [SerializeField] protected Image ConnectedDot;

    [Header("El texto donde se muestra la conexion wifi")]
    [SerializeField] protected TextMeshProUGUI WifiText;

    [Header("El evento que se lanza al logearse")]
    public UnityEngine.Events.UnityEvent OnLog;
    #endregion

    #region METHODS
    public void Start()
    {
        if(PlayerPrefs.HasKey("Username") && PlayerPrefs.GetString("Username").Length > 0) { if(logOutCanvas)logOutCanvas.SetActive(true); if(MailCanvas)MailCanvas.SetActive(false); }
        else { if(logOutCanvas)logOutCanvas.SetActive(false); if(MailCanvas)MailCanvas.SetActive(true); }
        GetAllRenderedComponentsInWarningPopUp();
        SetWarningPopUpToTransparent();
        httpClient = new HttpClient();
        if (!ShouldBeTraduced) return;
        Traduce();
    }

    public void Traduce()
    {
        if(inputMail_text)
        inputMail_text.text = inputMail_str;
        if(incorrectMail_text)
        incorrectMail_text.text = incorrectEmail_str;
        if(badConnection_text)
        badConnection_text.text = badConnection_str;
        if(yes_text)
        yes_text.text = yes_str;
        if(no_text)
        no_text.text = no_str;
        if(logout_text)
        logout_text.text = logout_str;
    }
    IEnumerator CheckWifi()
    {
        while (true)
        {
            if(Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
            {
                ConnectedDot.color = Color.green;
                
            }
            else
            {
                ConnectedDot.color = Color.red;
            }
            yield return new WaitForSeconds(2);
        }
    }

    public void Yes()
    {
        StartCoroutine(nameof(WaitUntilYes));
    }

    IEnumerator WaitUntilYes()
    {
        yield return new WaitForSeconds(1);
        DemoButton();
        StopCoroutine(nameof(WaitUntilYes));
    }

    public void No()
    {
        StartCoroutine(nameof(WaitUntilNo));
    }

    IEnumerator WaitUntilNo()
    {
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(nextBuildIndex);
        StopCoroutine(nameof(WaitUntilNo));
    }

    private void GetAllRenderedComponentsInWarningPopUp()
    {
        if (!WarningPopUp_go) return;
        imgs.Clear();
        texts.Clear();
        texts_legacy.Clear();

        imgs.AddRange(WarningPopUp_go.GetComponentsInChildren<Image>());
        texts.AddRange(WarningPopUp_go.GetComponentsInChildren<TextMeshProUGUI>());
        texts_legacy.AddRange(WarningPopUp_go.GetComponentsInChildren<Text>());

        con_imgs.Clear();
        con_texts.Clear();
        con_texts_legacy.Clear();

        con_imgs.AddRange(ConnectionPopUp_go.GetComponentsInChildren<Image>());
        con_texts.AddRange(ConnectionPopUp_go.GetComponentsInChildren<TextMeshProUGUI>());
        con_texts_legacy.AddRange(ConnectionPopUp_go.GetComponentsInChildren<Text>());

        success_imgs.AddRange(SuccessObject.GetComponentsInChildren<Image>());
        sucess_texts.AddRange(SuccessObject.GetComponentsInChildren<TextMeshProUGUI>());
    }
    private void SetWarningPopUpToTransparent()
    {
        if (!WarningPopUp_go) return;
        WarningPopUp_go.SetActive(true);

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

        foreach(var img in con_imgs)
        {
            img.color = new Color(img.color.r, img.color.g, img.color.b, 0);
        }
        foreach(var text in con_texts)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
        }
        foreach(var text in con_texts_legacy)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
        }
    }

    private void SetConnectionPopUpToTransparent()
    {
        
    }
    public async void EnterButton()
    {
        string user = "";
        if (VRInteractable_Keyboard.inputfieldText) user = VRInteractable_Keyboard.inputfieldText.text;
        if (EmailInputFieldText) user = EmailInputFieldText.text;
        string appCode = BackendGetter.appCode.ToString();

        var cts = new System.Threading.CancellationTokenSource();
        using (var request = new HttpRequestMessage(HttpMethod.Get, BackendConstants.urlNoParams + "applicationId=" + appCode + "&" + "userName=" + user))
        {
            using(var response = await httpClient.SendAsync(request, cts.Token))
            {
                if(response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string content = response.Content.ReadAsStringAsync().Result;
                    BackendData data = JsonUtility.FromJson<BackendData>(content);
                    BackendGetter.backendData = data;
                    PlayerPrefs.SetString(BackendConstants.BackendDataKey, content);
                    PlayerPrefs.SetString("Username", user);
                    OnLog.Invoke();
                    //SceneManager.LoadScene(nextBuildIndex);
                    if (ShouldChangeSceneOnEnter)SceneManager.LoadScene(nextBuildIndex);
                    if (emailText) emailText.text = user;
#if UNITY_2021
                    if (FindObjectOfType<BackendTimeManager>()) FindObjectOfType<BackendTimeManager>().UpdateTimeInfo();
#endif
#if UNITY_2023_OR_NEWER
                    if (FindFirstObjectByType<BackendTimeManager>()) FindFirstObjectByType<BackendTimeManager>().UpdateTimeInfo();
#endif

                    StopCoroutine(nameof(ShowSuccessPopUp));
                    StartCoroutine(nameof(ShowSuccessPopUp));
                }
                else
                {
                    if(Application.internetReachability != NetworkReachability.NotReachable)
                    {
                        StopCoroutine(nameof(ShowPopUp));
                        StartCoroutine(nameof(ShowPopUp));
                    }
                    else
                    {
                        StopCoroutine(nameof(ShowConPopUp));
                        StartCoroutine(nameof(ShowConPopUp));
                    }
                }
            }
        }
    }

    public void WaitDemoButton()
    {
        StartCoroutine(nameof(waitDemoButton));
    }

    IEnumerator waitDemoButton()
    {
        yield return new WaitForSeconds(1);
        DemoButton();
    }
    public void DemoButton()
    {
        BackendData data = new BackendData();
        data.vrExperiences = new VRExperience[1];
        data.vrExperiences[0] = new VRExperience();
        data.vrExperiences[0].code = "EXT-002";
        data.vrExperiences[0].name = "House-Living Room";
        data.vrExperiences[0].id = 1;

        BackendGetter.backendData = data;

        PlayerPrefs.SetString(BackendConstants.BackendDataKey, JsonUtility.ToJson(data));
        PlayerPrefs.SetString("Username", "");

        SceneManager.LoadScene(nextBuildIndex);
    }

    WaitForEndOfFrame frame = new WaitForEndOfFrame();
    private IEnumerator ShowPopUp()
    {
        if (!WarningPopUp_go) yield break;
        WarningPopUp_go.SetActive(true);
        //if (imgs[0].color.a > 0) yield break;
        float alpha = 0;
        while(alpha < 0.9f)
        {
            alpha += 0.01f;
            foreach (var img in imgs) img.color += new Color(0, 0, 0, 0.01f);
            foreach (var text in texts) text.color += new Color(0, 0, 0, 0.01f);
            foreach (var text in texts_legacy) text.color += new Color(0, 0, 0, 0.01f);
            yield return frame;
        }

        yield return new WaitForSeconds(2f);

        while(alpha > 0.01f)
        {
            alpha -= 0.01f;
            foreach (var img in imgs) img.color -= new Color(0, 0, 0, 0.01f);
            foreach (var text in texts) text.color -= new Color(0, 0, 0, 0.01f);
            foreach (var text in texts_legacy) text.color -= new Color(0, 0, 0, 0.01f);
            yield return frame;
        }
        StopCoroutine(nameof(ShowPopUp));
    }

    private IEnumerator ShowConPopUp()
    {
        if (!ConnectionPopUp_go) yield break;
        ConnectionPopUp_go.SetActive(true);
        //if (con_imgs[0].color.a > 0) yield break;
        float alpha = 0;
        while (alpha < 0.9f)
        {
            alpha += 0.01f;
            foreach (var img in con_imgs) img.color += new Color(0, 0, 0, 0.01f);
            foreach (var text in con_texts) text.color += new Color(0, 0, 0, 0.01f);
            foreach (var text in con_texts_legacy) text.color += new Color(0, 0, 0, 0.01f);
            yield return frame;
        }

        yield return new WaitForSeconds(2f);

        while (alpha > 0.01f)
        {
            alpha -= 0.01f;
            foreach (var img in con_imgs) img.color -= new Color(0, 0, 0, 0.01f);
            foreach (var text in con_texts) text.color -= new Color(0, 0, 0, 0.01f);
            foreach (var text in con_texts_legacy) text.color -= new Color(0, 0, 0, 0.01f);
            yield return frame;
        }
        StopCoroutine(nameof(ShowPopUp));
    }

    private IEnumerator ShowSuccessPopUp()
    {
        if (!SuccessObject) yield break;
        SuccessObject.SetActive(true);
        if (success_imgs[0].color.a > 0)
        {
            foreach(var img in success_imgs) img.color = new Color(img.color.r, img.color.g, img.color.b, 0);
            foreach (var text in sucess_texts) text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
        }
        float alpha = 0;
        while (alpha < 0.9f)
        {
            alpha += 0.01f;
            foreach (var img in success_imgs) img.color += new Color(0, 0, 0, 0.01f);
            foreach (var text in sucess_texts) text.color += new Color(0, 0, 0, 0.01f);
            yield return frame;
        }

        yield return new WaitForSeconds(2f);

        while (alpha > 0.01f)
        {
            alpha -= 0.01f;
            foreach (var img in success_imgs) img.color -= new Color(0, 0, 0, 0.01f);
            foreach (var text in sucess_texts) text.color -= new Color(0, 0, 0, 0.01f);
            yield return frame;
        }
        StopCoroutine(nameof(ShowSuccessPopUp));
    }

    public static void GoToBackendScene(int nextSceneBuildIndex, string InputEmail, string IncorrectEmail, string BadConnection, string logOut, string yes, string no)
    {
        SceneManager.LoadScene(BackendSceneName);
        nextBuildIndex = nextSceneBuildIndex;
        inputMail_str = InputEmail;
        incorrectEmail_str = IncorrectEmail;
        badConnection_str = BadConnection;
        logout_str = logOut;
        yes_str = yes;
        no_str = no;
    }

    public void SwitchKeyboard()
    {
        numbersGrid.SetActive(lettersGrid.activeSelf);
        lettersGrid.SetActive(!lettersGrid.activeSelf);
    }
#endregion
}
