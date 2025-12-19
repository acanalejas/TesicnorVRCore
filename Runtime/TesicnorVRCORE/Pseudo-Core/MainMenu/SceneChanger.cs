using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    #region SINGLETON

    private static SceneChanger instance;
    public static SceneChanger Instance { get { return instance; } }

    private void CheckSingleton()
    {
        if (instance == null) instance = this;
        else Destroy(this.gameObject);
    }

    #endregion

    #region PARAMETERS
    [Header("La pantalla que nos sirve para hacer el degradado")]
    [SerializeField] private Material fadeImg;

    [Header("La velocidad a la que se realiza el degradado")]
    [SerializeField] private float alphaPerFrame = 0.05f;

    public bool isGuided = true;

    private SceneData scene;
    #endregion

    #region FUNCTIONS

    private void Awake()
    {
        DontDestroyOnLoad(this);
        CheckSingleton();
    }

    private void Update()
    {
        GoToMenu();
    }
    public void ChangeScene(SceneData data)
    {
        scene = data;

        StartCoroutine("FadeIn");
    }

    public void ChangeScene()
    {
        StartCoroutine("FadeIn");
    }

    public void ChangeScene(int index)
    {
        SceneManager.LoadScene(index);
    }

    public void GoToMenu_Button()
    {
        SceneManager.LoadScene(0);
        //SceneData _data = new SceneData();
        //_data.SceneID = 0;
//
        //ChangeScene(_data);
    }

    public void Retry_Button()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        //SceneData _data = new SceneData();
        //_data.SceneID = SceneManager.GetActiveScene().buildIndex;
        //_data.SceneName = SceneManager.GetActiveScene().name;
//
        //ChangeScene(_data);
    }
    public void GoToMenu()
    {
        HandPoseDetector[] poses = GameObject.FindObjectsOfType<HandPoseDetector>();

        foreach(var pose in poses)
        {
            if(pose.Recognize().GestureName == "GoToMenu_Test")
            {
                SceneManager_Menu.GoToMenu();
            }
        }
    }
    private IEnumerator FadeIn()
    {
        float alpha = 0;
        while(alpha < 1)
        {
            alpha += alphaPerFrame;
            fadeImg.color = new Color(fadeImg.color.r, fadeImg.color.g, fadeImg.color.b, alpha);
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(0.5f);
        SceneManager_Menu.ChangeScene(scene);
        StartCoroutine("FadeOut");
        StopCoroutine("FadeIn");
    }

    private IEnumerator FadeOut()
    {
        float alpha = 1;
        while(alpha > 0)
        {
            alpha -= alphaPerFrame;
            fadeImg.color = new Color(fadeImg.color.r, fadeImg.color.g, fadeImg.color.b, alpha);
            yield return new WaitForEndOfFrame();
        }

        StopCoroutine("FadeOut");
    }

    public void SetScene(SceneData _data)
    {
        scene = _data;
    }

    public SceneData GetScene()
    {
        return scene;
    }

    public void SetIsGuided(bool _value)
    {
        isGuided = _value;
    }
    #endregion
}
