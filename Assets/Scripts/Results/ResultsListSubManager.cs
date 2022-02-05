using UnityEngine;

/// <summary>
/// Results 配下の SubManager
/// Results 一覧画面の表示管理やソート処理などを行う
/// </summary>
public class ResultsListSubManager : MonoBehaviour
{
    // 表示対象の設定
    int gameMode;
    bool isTerminated = false; // 未実装
    bool isGuestResult = false; // 未実装

    // 上司
    ResultsManager resultsManager;

    // 部下となるシーン内のコントローラー達
    ResultsListObjectController resultsListObjectController;

    ResultSummaries resultSummaries;

    void Awake()
    {
        // 上司を見つける
        resultsManager = GetComponent<ResultsManager>();

        // 部下を見つける
        resultsListObjectController = GetComponent<ResultsListObjectController>();
    }
    // Start is called before the first frame update
    void Start()
    {
        gameMode = resultsManager.GetGameMode();
        resultSummaries = new ResultSummaries(gameMode, isTerminated, isGuestResult);

        resultsListObjectController.DisplayResultSummaries(resultSummaries);
        Debug.Log("Completed!");
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnDestroy()
    {
    }
    public void SortByRankAscendingValueChangedHandler(bool isOn)
    {
        Debug.Log($"RankAscending Changed To {isOn}");
        if (isOn)
        {
            resultSummaries.SortByRankAscending();
            resultsListObjectController.DisplayResultSummaries(resultSummaries);
        }
    }
    public void SortByRankDescendingValueChangedHandler(bool isOn)
    {
        Debug.Log($"RankDescending Changed To {isOn}");
        if (isOn)
        {
            resultSummaries.SortByRankDescending();
            resultsListObjectController.DisplayResultSummaries(resultSummaries);
        }
    }
    public void SortByTimeAscendingValueChangedHandler(bool isOn)
    {
        Debug.Log($"TimeAscending Changed To {isOn}");
        if (isOn)
        {
            resultSummaries.SortByTimeAscending();
            resultsListObjectController.DisplayResultSummaries(resultSummaries);
        }
    }
    public void SortByTimeDescendingValueChangedHandler(bool isOn)
    {
        Debug.Log($"TimeDescending Changed To {isOn}");
        if (isOn)
        {
            resultSummaries.SortByTimeDescending();
            resultsListObjectController.DisplayResultSummaries(resultSummaries);
        }
    }
    public void SortByMissAscendingValueChangedHandler(bool isOn)
    {
        Debug.Log($"MissAscending Changed To {isOn}");
        if (isOn)
        {
            resultSummaries.SortByMissAscending();
            resultsListObjectController.DisplayResultSummaries(resultSummaries);
        }
    }
    public void SortByMissDescendingValueChangedHandler(bool isOn)
    {
        Debug.Log($"MissDescending Changed To {isOn}");
        if (isOn)
        {
            resultSummaries.SortByMissDescending();
            resultsListObjectController.DisplayResultSummaries(resultSummaries);
        }
    }
    public void SortByDateTimeAscendingValueChangedHandler(bool isOn)
    {
        if (isOn)
        {
            resultSummaries.SortByDateTimeAscending();
            resultsListObjectController.DisplayResultSummaries(resultSummaries);
        }
    }
    public void SortByDateTimeDescendingValueChangedHandler(bool isOn)
    {
        if (isOn)
        {
            resultSummaries.SortByDateTimeDescending();
            resultsListObjectController.DisplayResultSummaries(resultSummaries);
        }
    }
}
