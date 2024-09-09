using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuizManager : MonoBehaviour
{
    #region PARAMETERS
    [Header("El objeto de pregunta que se usara")]
    [SerializeField] private QuizQuestion QuestionObj;

    [Header("La lista de preguntas y respuestas que se van a hacer")]
    [SerializeField] private QuizQuestionData[] QuestionsDatas;

    [Header("Evento que se lanza cuando se responde una pregunta")]
    public UnityEngine.Events.UnityEvent<bool, int> OnQuestionAnswered;

    [Header("Evento que se lanza cuando se activa una pregunta")]
    public UnityEngine.Events.UnityEvent<int> OnQuestionDisplayed;

    /// <summary>
    /// La lista de resultados
    /// </summary>
    public List<bool> AnswersResults { get { return answersResults; } }

    private List<bool> answersResults = new List<bool>();

    [Header("Se deberia relentizar el tiempo?")]
    [SerializeField] private bool bShouldSlowTime = true;

    [Header("La transicion de la escala del tiempo es suave?")]
    [SerializeField] private bool bUseSoftTimeScaleTransition = true;

    [Header("El valor del tiempo en slowmo")]
    [SerializeField] private float TimeScaleForSlowmo = 0.2f;
    #endregion

    #region METHODS
    private void Awake()
    {
        CheckSingleton();

        foreach(var question in QuestionsDatas) answersResults.Add(false);

        if(QuestionObj)
        QuestionObj.OnAnswerSelected.AddListener((bool correct, int index) =>
        {
            this.OnQuestionAnswered.Invoke(correct, index);

            answersResults[index] = correct;

            if (bUseSoftTimeScaleTransition) StartCoroutine(nameof(SoftTransitionFromSlowmo));
            else Time.timeScale = 1;
        });
    }

    public void DisplayQuestion(int index)
    {
        if(index >= QuestionsDatas.Length) { Debug.LogError("La pregunta a la que intentas acceder esta fuera del array, usa un indice dentro del rango por favor"); return; }

        if (bShouldSlowTime && !bUseSoftTimeScaleTransition) Time.timeScale = TimeScaleForSlowmo;
        else if (bShouldSlowTime && bUseSoftTimeScaleTransition) StartCoroutine(nameof(SoftTransitionToSlowmo));

        QuizQuestionData currentData = QuestionsDatas[index];

        if(QuestionObj) QuestionObj.ToggleQuestion(true, currentData);

        OnQuestionDisplayed.Invoke(index);
    }

    IEnumerator SoftTransitionToSlowmo()
    {
        while(Time.timeScale > TimeScaleForSlowmo * 1.1f)
        {
            Time.timeScale = Mathf.Lerp(Time.timeScale, TimeScaleForSlowmo, Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }

        StopCoroutine(nameof(SoftTransitionToSlowmo));
    }

    IEnumerator SoftTransitionFromSlowmo()
    {
        while(Time.timeScale < 1)
        {
            Time.timeScale = Mathf.Lerp(Time.timeScale, 1.1f, Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }

        Time.timeScale = 1;
        StopCoroutine(nameof(SoftTransitionFromSlowmo));
    }
    #endregion

    #region SINGLETON
    private static QuizManager instance;
    public static QuizManager Instance { get { return instance; } }

    private void CheckSingleton()
    {
        if (!instance) instance = this;
        else Destroy(this);
    }
    #endregion
}
