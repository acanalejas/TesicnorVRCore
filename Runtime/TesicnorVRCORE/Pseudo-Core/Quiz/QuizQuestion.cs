using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

[System.Serializable]
public struct QuizQuestionData
{
    [Header("El indice de la pregunta dentro de una lista, en caso de que esté")]
    public int index;
    [Header("El indice de la respuesta correcta dentro de la lista de respuestas")]
    public int correctAnswer;
    [Header("El texto de la pregunta")]
    public string questionText;
    [Header("La lista de respuestas posibles")]
    public List<string> answerList;
}
[RequireComponent(typeof(Animator))]
public class QuizQuestion : MonoBehaviour
{
    #region PARAMETERS
    [Header("La estructura de datos que contiene la informacion sobre la pregunta")]
    [SerializeField] protected QuizQuestionData data;

    [Header("El prefab de botón de respuesta")]
    [SerializeField] protected GameObject answerPrefab;

    [Header("El GO del padre de las respuestas posibles")]
    [SerializeField] private Transform answersParent;

    [Header("El texto de la pregunta")]
    [SerializeField] private TextMeshProUGUI questionText;

    [Header("Usa animacion al activarse o desactivarse?")]
    [SerializeField] private bool bUsesAnim = false;

    [Header("El nombre del estado de animacion al aparecer")]
    [SerializeField] private string appearAnimName = "Appear";

    [Header("El nombre del estado de animacion al desaparecer")]
    [SerializeField] private string desappearAnimName = "Desappear";

    [Header("El tiempo que tarda la animacion de desaparecer")]
    [SerializeField] private float desappearAnimTime = 1;

    [Header("El evento que se lanza al seleccionar una respuesta")]
    public UnityEvent<bool, int> OnAnswerSelected;

    /// <summary>
    /// Los componentes de TextMeshProUGUI en orden de creacion
    /// </summary>
    private List<TextMeshProUGUI> answersTexts = new List<TextMeshProUGUI>();

    /// <summary>
    /// Los gameobject instanciados de los botones de respuesta en orden de creacion
    /// </summary>
    protected List<GameObject> instancedAnswers = new List<GameObject>();

    private Animator animator;
    #endregion

    #region METHODS
    private void Awake()
    {
        
    }

    private void Start()
    {
        animator = this.GetComponent<Animator>();
        this.OnAnswerSelected.AddListener((bool correct, int index) =>
        {
            this.ToggleQuestion(false);
        });
    }

    public void SetupQuestionAndAnswers(QuizQuestionData _data)
    {
        data = _data;

        foreach(var answer in data.answerList)
        {
            GameObject newAnswer = GameObject.Instantiate(answerPrefab, answersParent);

            VRInteractable_Button _button = null;
            if(!newAnswer.GetComponent<VRInteractable_Button>()) _button = newAnswer.AddComponent<VRInteractable_Button>();
            else _button = newAnswer.GetComponent<VRInteractable_Button>();

            TextMeshProUGUI _answerText = newAnswer.GetComponentInChildren<TextMeshProUGUI>();
            if (_answerText) _answerText.text = answer;

            answersTexts.Add(_answerText);
            instancedAnswers.Add(newAnswer);

            _button.onRelease.AddListener(() =>
            {
                //TODO
                //Meter aqui el codigo de seleccionar respuesta
                int _answerSelected = instancedAnswers.IndexOf(newAnswer);
                SelectAnswer(_answerSelected);
            });
        }

        questionText.text = data.questionText;
    }

    bool SelectAnswer(int answerSelected)
    {
        bool correctAnswer = answerSelected == data.correctAnswer;

        OnAnswerSelected.Invoke(correctAnswer, data.index);
        return correctAnswer;
    }

    public void ToggleQuestion(bool _value, QuizQuestionData _data = new QuizQuestionData())
    {
        //Vaciar listas y demás para que este listo para reusarse
        if(_value == false)
        {
            foreach (var answer in instancedAnswers) Destroy(answer);
            instancedAnswers.Clear();
            answersTexts.Clear();
        }
        else
        {
            if(_data.answerList.Count > 0)
            {
                SetupQuestionAndAnswers(_data);
            }
        }
        //En caso de que se use animacion
        if(bUsesAnim)
        {
            //En caso de que se active
            if (_value) 
            {
                this.gameObject.SetActive(true);
                this.animator.Play(appearAnimName);
            }
            else
            {
                this.animator.Play(desappearAnimName);
                Invoke(nameof(DisableGameobject), desappearAnimTime);
            }
        }
        else this.gameObject.SetActive(_value);
    }

    private void DisableGameobject()
    {
        this.gameObject.SetActive(false);
    }
    #endregion
}
