using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;


public class NetworkManager_PlayFab : MonoBehaviour {

    public const string titleId = "3797";
    public string playFabId;
    private LoginResult m_LoginResult;
    private List<QuizQuestion> m_ReturnedResults = new List<QuizQuestion>();

    public LoginResult LoginResult { get { return m_LoginResult; } }
    public List<QuizQuestion> ReturnedResults { get { return m_ReturnedResults; } }

    public void Login(System.Action onComplete = null)
    {
        LoginWithCustomIDRequest request = new LoginWithCustomIDRequest()
        {
            TitleId = titleId,
            CreateAccount = true,
            CustomId = SystemInfo.deviceUniqueIdentifier
        };

        PlayFabClientAPI.LoginWithCustomID(request, (result) => {
            playFabId = result.PlayFabId;
            Debug.Log("Got PlayFabID: " + playFabId);

            if (result.NewlyCreated)
            {
                Debug.Log("(new account)");
            }
            else
            {
                Debug.Log("(existing account)");
            }
            m_LoginResult = result;
            onComplete();
        },
        (error) => {
            Debug.Log("Error logging in player with custom ID:");
            Debug.Log(error.ErrorMessage);
        });
    }

    public void GetQuestions(System.Action onComplete = null)
    {
        GetCatalogItemsRequest request = new GetCatalogItemsRequest();

        PlayFabClientAPI.GetCatalogItems(request, (result) => {
            foreach(CatalogItem c in result.Catalog)
            {
                QuizQuestion question = new QuizQuestion();
                question = JsonUtility.FromJson<QuizQuestion>(c.CustomData);
                question.Question = c.DisplayName;
                switch (c.ItemClass)
                {
                    default:
                    case "EASY":
                        question.difficulty = Difficulty.EASY;
                        break;
                    case "MODERATE":
                        question.difficulty = Difficulty.MODERATE;
                        break;
                    case "HARD":
                        question.difficulty = Difficulty.HARD;
                        break;
                }
                m_ReturnedResults.Add(question);
            }
            onComplete();
        },
        (error) => {
            Debug.Log("Error logging in player with custom ID:");
            Debug.Log(error.ErrorMessage);
        });
    }
}
