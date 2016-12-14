using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    // Number of available answers to choose from
    private const int answerCount = 3;

    // Amount to fill the difficulty bar image
    private const float diffFillEasy = 0.297f;
    private const float diffFillMod = 0.632f;
    private const float diffFillHard = 1.0f;

    // Rate at which the fill UI adjusts
    private const float barFillRate = 0.8f;
    private const float wheelFillRate = 0.8f;

    public delegate void ButtonEvent(string answer);
    public event ButtonEvent ButtonClick;

    [SerializeField]
    private Text questionText;
    [SerializeField]
    private Text answer01Text;
    [SerializeField]
    private Text answer02Text;
    [SerializeField]
    private Text answer03Text;
    [SerializeField]
    private Image difficultyBarImage;
    [SerializeField]
    private Image scoreWheelGreenImage;
    [SerializeField]
    private Image scoreWheelRedImage;
    [SerializeField]
    private Text scoreTitle;
    [SerializeField]
    private Text currentScore;
    [SerializeField]
    private Text scoreMax;
    [SerializeField]
    private Button AnswerBtn01;
    [SerializeField]
    private Button AnswerBtn02;
    [SerializeField]
    private Button AnswerBtn03;
    [SerializeField]
    private GameObject ResultsObj;
    [SerializeField]
    private Text resultsCorrectText;
    [SerializeField]
    private Text resultsLoseText;
    [SerializeField]
    private Image timer;

    private Difficulty m_CurrentDifficulty;
    private bool buttonsEnabled = false;

    public void Init(int _scoreMax, Difficulty diff = Difficulty.EASY)
    {
        SetDifficulty(diff);
        ResetScore(_scoreMax);
    }

    public void SetNextQuestionUI(QuizQuestion quizQuestion)
    {
        questionText.text = quizQuestion.Question;

        //Randomly place answers
        int rand = Random.Range(0, answerCount);
        switch (rand)
        {
            case 0:
                answer01Text.text = quizQuestion.Correct;
                answer02Text.text = quizQuestion.Incorrect_01;
                answer03Text.text = quizQuestion.Incorrect_02;
                break;
            case 1:
                answer01Text.text = quizQuestion.Incorrect_01;
                answer02Text.text = quizQuestion.Correct;
                answer03Text.text = quizQuestion.Incorrect_02;
                break;
            case 2:
                answer01Text.text = quizQuestion.Incorrect_01;
                answer02Text.text = quizQuestion.Incorrect_02;
                answer03Text.text = quizQuestion.Correct;
                break;
        }
    }

    public void UpdateScoreboardUI(Difficulty diff, int newScore, int maxScore)
    {
        SetDifficulty(diff);
        SetScore(newScore, maxScore);
    }

    public void UpdateTimer(float _fillAmount)
    {
        timer.fillAmount = _fillAmount;
    }

    private void ResetScore(int _scoreMax)
    {
        scoreWheelGreenImage.fillAmount = 0;
        scoreWheelRedImage.fillAmount = 0;
        currentScore.text = "0";
        scoreMax.text = _scoreMax.ToString();
    }

    private void SetDifficulty(Difficulty diff)
    {
        if (diff == m_CurrentDifficulty)
            return;

        m_CurrentDifficulty = diff;

        switch (m_CurrentDifficulty)
        {
            case Difficulty.EASY:
                StartCoroutine(LerpFillAmount(difficultyBarImage, diffFillEasy, barFillRate));
                break;
            case Difficulty.MODERATE:
                StartCoroutine(LerpFillAmount(difficultyBarImage, diffFillMod, barFillRate));
                break;
            case Difficulty.HARD:
                StartCoroutine(LerpFillAmount(difficultyBarImage, diffFillHard, barFillRate));
                break;
        }
    }

    private void SetScore(int _newScore, int _maxScore)
    {
        float fillTo = (float) Mathf.Abs(_newScore / (float) _maxScore);
        scoreTitle.text = _newScore >= 0 ? "Correct" : "Wrong";
        currentScore.text = Mathf.Abs(_newScore).ToString();
        scoreMax.text = _maxScore.ToString();

        if(_newScore > 0)
        {
            StartCoroutine(LerpFillAmount(scoreWheelRedImage, 0, wheelFillRate));
            StartCoroutine(LerpFillAmount(scoreWheelGreenImage, fillTo, wheelFillRate, ()=>
            {
                if (_newScore < _maxScore)
                    ButtonsEnabled(true);
            }));
        }
        else
        {
            StartCoroutine(LerpFillAmount(scoreWheelGreenImage, 0, wheelFillRate));
            StartCoroutine(LerpFillAmount(scoreWheelRedImage, fillTo, wheelFillRate, ()=>{
                if(_newScore < _maxScore)
                   ButtonsEnabled(true);
            }));
        }
    }

    private IEnumerator LerpFillAmount(Image image, float fillTo, float rate = 1, System.Action onComplete = null)
    {
        if (image.fillAmount == fillTo)
            yield break;

        float lerpBuffer = 0.1f;
        if (image.fillAmount < fillTo)
        {
            while(image.fillAmount < fillTo)
            {
                image.fillAmount = Mathf.Lerp(image.fillAmount, fillTo+lerpBuffer, (1/ rate)*Time.deltaTime);
                yield return null;
            }
            if(onComplete != null)
                onComplete();
        }
        else
        {
            while (image.fillAmount > fillTo)
            {
                image.fillAmount = Mathf.Lerp(image.fillAmount, fillTo-lerpBuffer, (1/rate)*Time.deltaTime);
                yield return null;
            }
            if (onComplete != null)
                onComplete();
        }
    }

    public void ButtonClicked(Text selection)
    {
        string selectedAnswer = selection.text;
        ButtonsEnabled(false);
        ButtonClick(selectedAnswer);
    }


    public void ButtonsEnabled(bool isEnabled)
    {
        AnswerBtn01.interactable = isEnabled;
        AnswerBtn02.interactable = isEnabled;
        AnswerBtn03.interactable = isEnabled;
    }

    public void ToggleResultsShow(int _correct = -1, int _incorrect = -1)
    {
        ResultsObj.SetActive(!ResultsObj.activeInHierarchy);

        if (_correct < 0 || _incorrect < 0)
            return;

        resultsCorrectText.text = _correct.ToString();
        resultsLoseText.text = _incorrect.ToString();
    }
}
