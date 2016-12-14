using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuizManager : MonoBehaviour {

    private const int MaxScoreWin = 5;
    private const int MaxScoreLose = 3;
    private const float timePerQuestion = 5f;

    private NetworkManager_PlayFab network;
    private UIManager UI;
    private List<QuizQuestion> m_AllQuestions = new List<QuizQuestion>();
    private List<QuizQuestion> m_EasyQuestions = new List<QuizQuestion>();
    private List<QuizQuestion> m_ModerateQuestions = new List<QuizQuestion>();
    private List<QuizQuestion> m_HardQuestions = new List<QuizQuestion>();
    private QuizQuestion m_CurrentQuestion = new QuizQuestion();
    private Difficulty m_Difficulty;
    private GameState m_GameState;

    public List<QuizQuestion> AllQuestions{ get { return m_AllQuestions; } protected set { m_AllQuestions = value; }}
    public List<QuizQuestion> EasyQuestions{ get { return m_AllQuestions; } protected set { m_AllQuestions = value; } }
    public List<QuizQuestion> ModerateQuestions{ get { return m_AllQuestions; } protected set { m_AllQuestions = value; } }
    public List<QuizQuestion> HardQuestions{ get { return m_AllQuestions; } protected set { m_AllQuestions = value; } }
    public Difficulty Difficulty { get { return m_Difficulty; } set { m_Difficulty = value; } }
    
    private int currentScore = 0;
    private int numCorrect = 0;
    private int numIncorrect = 0;
    private float timer;
    private bool isTimeout = false;

    void Awake()
    {
        // Get components
        network = GetComponentInChildren<NetworkManager_PlayFab>();
        UI = GameObject.Find("UI").GetComponent<UIManager>();
        UI.ButtonClick += UpdateScore;
    }

	void Start ()
    {
        Init();
	}

    public void Init()
    {
        timer = timePerQuestion;
        UI.ButtonsEnabled(false);

        // Login to PlayFab and get the questions
        network.Login(() =>
        {
            network.GetQuestions(() => {
                m_AllQuestions = network.ReturnedResults;
                PopulateAllQuestions();
                StartQuiz();
            });
        });
    }

    public void StartQuiz()
    {
        m_Difficulty = GetLastDifficulty();
        ResetScore();
        SetNextQuestion();
        ResetUI();
    }

    private void SetNextQuestion()
    {
        switch (m_Difficulty)
        {
            default:
            case Difficulty.EASY:
                if (m_EasyQuestions.Count > 0)
                    m_CurrentQuestion = GetRandomQuestion(m_EasyQuestions);
                else
                    RepopulateEasyQuestions();
                break;
            case Difficulty.MODERATE:
                if (m_ModerateQuestions.Count > 0)
                    m_CurrentQuestion = GetRandomQuestion(m_ModerateQuestions);
                else
                    RepopulateModerateQuestions();
                break;
            case Difficulty.HARD:
                if (m_HardQuestions.Count > 0)
                    m_CurrentQuestion = GetRandomQuestion(m_HardQuestions);
                else
                    RepopulateHardQuestions();
                break;
        }

        UI.SetNextQuestionUI(m_CurrentQuestion);
        ResetTimer();
    }

    private void UpdateScore(string answer)
    {
        if (answer == m_CurrentQuestion.Correct)
        {
            numCorrect++;
            if (currentScore >= 0)
            {
                currentScore++;
            }
            else
                currentScore = 1;
            CalculateDifficulty();
        }
        else
        {
            numIncorrect++;
            if (currentScore <= 0)
                currentScore--;
            else
                currentScore = -1;
            CalculateDifficulty();
        }

        int max = currentScore >= 0 ? MaxScoreWin : MaxScoreLose;
        UI.UpdateScoreboardUI(m_Difficulty, currentScore, max);

        if (currentScore >= MaxScoreWin)
        {
            StartCoroutine(EndGame(GameState.WIN));
        }
        else if(currentScore <= -MaxScoreLose)
        {
            StartCoroutine(EndGame(GameState.LOSE));
        }
        else
        {
            SetNextQuestion();
        }
    }

    private IEnumerator EndGame(GameState state)
    {
        GameObject winObj;
        string objToLoad;

        isTimeout = true;

        switch (state)
        {
            case GameState.WIN:
                objToLoad = "Win";
                break;
            case GameState.LOSE:
                objToLoad = "Lose";
                break;
            case GameState.TIMEOUT:
                objToLoad = "TimeOut";
                break;
            default:
                yield break;
        }

        PlayerPrefs.SetString("LastDifficulty", m_Difficulty.ToString());

        winObj = (GameObject) Instantiate(Resources.Load(objToLoad));
        UI.ButtonsEnabled(false);
        yield return new WaitForSeconds(winObj.GetComponent<DestroyAfterTime>().timeToDestroy);
        UI.ToggleResultsShow(numCorrect, numIncorrect);
    }

    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            UI.UpdateTimer(timer / timePerQuestion);
        }
        else if (!isTimeout)
        {
            StartCoroutine(EndGame(GameState.TIMEOUT));
        }
    }

    private void ResetScore()
    {
        currentScore = 0;
        numCorrect = 0;
        numIncorrect = 0;
    }

    private void ResetUI()
    {
        UI.ButtonsEnabled(true);
        UI.Init(MaxScoreWin, m_Difficulty);
        UI.ToggleResultsShow();
    }

    private void ResetTimer()
    {
        timer = timePerQuestion;
        isTimeout = false;
    }

    void CalculateDifficulty()
    {
        if (currentScore >= 4 || (m_Difficulty == Difficulty.HARD && currentScore > 0))
            m_Difficulty = Difficulty.HARD;
        else if (currentScore >= 2 || (m_Difficulty == Difficulty.MODERATE && currentScore > 0))
            m_Difficulty = Difficulty.MODERATE;
        else
            m_Difficulty = Difficulty.EASY;
    }

    private QuizQuestion GetRandomQuestion(List<QuizQuestion> qList)
    {
        QuizQuestion rQuestion = new QuizQuestion();
        int rand = Random.Range(0, qList.Count);
        int qIndex = rand;

        for (int i=0; i < qList.Count; i++)
        {
            if (qList[qIndex].Question != null)
            {
                rQuestion = qList[qIndex];
                qList.Remove(rQuestion);
                return rQuestion;
            }

            if (qIndex < qList.Count)
                qIndex++;
            else
                qIndex = 0;            
        }

        return null;
    }

    private Difficulty GetLastDifficulty()
    {
        string lastDifficulty = PlayerPrefs.GetString("LastDifficulty");

        if (lastDifficulty == "")
            return Difficulty.EASY;

        switch (lastDifficulty)
        {
            default:
            case "EASY":
                return Difficulty.EASY;
                break;
            case "MODERATE":
                return Difficulty.MODERATE;
                break;
            case "HARD":
                return Difficulty.HARD;
                break;
        }
    }

    private void PopulateAllQuestions()
    {
        foreach (QuizQuestion q in m_AllQuestions)
        {
            switch (q.difficulty)
            {
                case Difficulty.EASY:
                    m_EasyQuestions.Add(q);
                    break;
                case Difficulty.MODERATE:
                    m_ModerateQuestions.Add(q);
                    break;
                case Difficulty.HARD:
                    m_HardQuestions.Add(q);
                    break;
            }
        }
    }

    public void RepopulateEasyQuestions()
    {
        foreach (QuizQuestion q in m_AllQuestions)
        {
            if(q.difficulty == Difficulty.EASY)
                m_EasyQuestions.Add(q);
        }
    }

    public void RepopulateModerateQuestions()
    {
        foreach (QuizQuestion q in m_AllQuestions)
        {
            if (q.difficulty == Difficulty.MODERATE)
                m_ModerateQuestions.Add(q);
        }
    }

    public void RepopulateHardQuestions()
    {
        foreach (QuizQuestion q in m_AllQuestions)
        {
            if (q.difficulty == Difficulty.HARD)
                m_HardQuestions.Add(q);
        }
    }

    public void Quit()
    {
        Application.Quit();
    }
}

public enum Difficulty
{
    EASY,
    MODERATE,
    HARD
}

public enum GameState
{
    WIN,
    LOSE,
    TIMEOUT
}
