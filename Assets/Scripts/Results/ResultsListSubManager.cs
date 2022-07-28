using UnityEngine;
using TMPro;

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

    // ResultsListPager
    int pageNumberToShow = 0;
    TMP_Dropdown pagerDropdown;

    void Awake()
    {
        // 上司を見つける
        resultsManager = GetComponent<ResultsManager>();

        // 部下を見つける
        resultsListObjectController = GetComponent<ResultsListObjectController>();

        pagerDropdown = GameObject.Find("PagerDropdown").GetComponent<TMP_Dropdown>();
    }
    // Start is called before the first frame update
    void Start()
    {
        gameMode = resultsManager.GetGameMode();
        resultSummaries = new ResultSummaries(gameMode, isTerminated, isGuestResult);

        resultsListObjectController.FitPagerDropDownToNumOfResults(resultSummaries);
        resultsListObjectController.DisplayResultSummaries(resultSummaries, pageNumberToShow);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnDestroy()
    {
    }
    #region Sort
    public void SortByRankAscendingValueChangedHandler(bool isOn)
    {
        Debug.Log($"RankAscending Changed To {isOn}");
        if (isOn)
        {
            resultSummaries.SortByRankAscending();
            resultsListObjectController.DisplayResultSummaries(resultSummaries, pageNumberToShow);
        }
    }
    public void SortByRankDescendingValueChangedHandler(bool isOn)
    {
        Debug.Log($"RankDescending Changed To {isOn}");
        if (isOn)
        {
            resultSummaries.SortByRankDescending();
            resultsListObjectController.DisplayResultSummaries(resultSummaries, pageNumberToShow);
        }
    }
    public void SortByTimeAscendingValueChangedHandler(bool isOn)
    {
        Debug.Log($"TimeAscending Changed To {isOn}");
        if (isOn)
        {
            resultSummaries.SortByTimeAscending();
            resultsListObjectController.DisplayResultSummaries(resultSummaries, pageNumberToShow);
        }
    }
    public void SortByTimeDescendingValueChangedHandler(bool isOn)
    {
        Debug.Log($"TimeDescending Changed To {isOn}");
        if (isOn)
        {
            resultSummaries.SortByTimeDescending();
            resultsListObjectController.DisplayResultSummaries(resultSummaries, pageNumberToShow);
        }
    }
    public void SortByMissAscendingValueChangedHandler(bool isOn)
    {
        Debug.Log($"MissAscending Changed To {isOn}");
        if (isOn)
        {
            resultSummaries.SortByMissAscending();
            resultsListObjectController.DisplayResultSummaries(resultSummaries, pageNumberToShow);
        }
    }
    public void SortByMissDescendingValueChangedHandler(bool isOn)
    {
        Debug.Log($"MissDescending Changed To {isOn}");
        if (isOn)
        {
            resultSummaries.SortByMissDescending();
            resultsListObjectController.DisplayResultSummaries(resultSummaries, pageNumberToShow);
        }
    }
    public void SortByDateTimeAscendingValueChangedHandler(bool isOn)
    {
        if (isOn)
        {
            resultSummaries.SortByDateTimeAscending();
            resultsListObjectController.DisplayResultSummaries(resultSummaries, pageNumberToShow);
        }
    }
    public void SortByDateTimeDescendingValueChangedHandler(bool isOn)
    {
        if (isOn)
        {
            resultSummaries.SortByDateTimeDescending();
            resultsListObjectController.DisplayResultSummaries(resultSummaries, pageNumberToShow);
        }
    }
    #endregion

    #region ResultsListPager
    public void PagerLeftButtonClickedHandler()
    {
        // 存在するページ番号は [0, ~ max(0, ceil(ResultSummary の数 / 100) - 1)]
        if (pageNumberToShow - 1 >= 0)
        {
            pageNumberToShow--;
            resultsListObjectController.DisplayResultSummaries(resultSummaries, pageNumberToShow);
            pagerDropdown.SetValueWithoutNotify(pageNumberToShow);
        }
    }
    public void PagerRightButtonClickedHandler()
    {
        // 存在するページ番号は [0, ~ max(0, ceil(ResultSummary の数 / 100) - 1)]
        // ceil(a / b) == (a + b - 1) / b
        if (pageNumberToShow + 1 <= (resultSummaries.NumOfResults + 100 - 1) / 100 - 1)
        {
            pageNumberToShow++;
            resultsListObjectController.DisplayResultSummaries(resultSummaries, pageNumberToShow);
            pagerDropdown.SetValueWithoutNotify(pageNumberToShow);
        }
    }
    /// <summary>
    /// </summary>
    /// <param name="newPageNumberToShow">0-indexed</param>
    public void JumpToPage(int newPageNumberToShow)
    {
        // 存在するページ番号は [0, ~ max(0, ceil(ResultSummary の数 / 100) - 1)]
        // ceil(a / b) == (a + b - 1) / b
        // ドロップダウンリストに無いページにはジャンプしようとしないはずだが Assert をかけておく
        Debug.Assert(pageNumberToShow >= 0);
        Debug.Assert(pageNumberToShow <= (resultSummaries.NumOfResults + 100 - 1) / 100 - 1);
        if (newPageNumberToShow != pageNumberToShow)
        {
            pageNumberToShow = newPageNumberToShow;
            resultsListObjectController.DisplayResultSummaries(resultSummaries, pageNumberToShow);
        }
    }
    public void OnPagerDropdownValueChanged(int value)
    {
        JumpToPage(value);
    }

    #endregion
}
