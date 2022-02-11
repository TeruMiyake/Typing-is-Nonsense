using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultDetailSubManager : MonoBehaviour
{
    // 上司

    // 部下
    ResultDetailObjectController resultDetailObjectController;
    ResultChartsSubManager resultChartsSubManager;

    public GameObject ResultDetail;
    public GameObject ResultsList;

    // 表示すべきトライアルのデータ
    TrialData trialData;

    void Awake()
    {
        resultDetailObjectController = GetComponent<ResultDetailObjectController>();

        // 部下を見つける
        resultChartsSubManager = GetComponent<ResultChartsSubManager>();
    }
    // Start is called before the first frame update
    void Start()
    {
        DeactivateResultDetail();

        resultChartsSubManager.InitializeCharts();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ActivateResultDetail()
    {
        HideResultsListArea();
        ShowResultDetailArea();
    }
    public void DeactivateResultDetail()
    {
        HideResultDetailArea();
        ShowResultsListArea();
    }

    /// <summary>
    /// リザルト詳細画面を表示する
    /// </summary>
    void ShowResultDetailArea()
    {
        foreach (Transform item in ResultDetail.transform)
        {
            item.gameObject.SetActive(true);
        }
    }
   void HideResultDetailArea()
    {
        foreach (Transform item in ResultDetail.transform)
        {
            item.gameObject.SetActive(false);
        }
    }
    void ShowResultsListArea()
    {
        foreach (Transform item in ResultsList.transform)
        {
            item.gameObject.SetActive(true);
        }
    }
    void HideResultsListArea()
    {
        foreach (Transform item in ResultsList.transform)
        {
            item.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// ResultSummary がクリックされた場合の処理
    /// ResultsListObjectController 内から ResultSummary プレハブに AddListener して使う
    /// </summary>
    /// <param name="filePath"></param>
    public void ResultSummaryClickedHandler(string filePath)
    {
        ActivateResultDetail();

        trialData = new TrialData(filePath);
        resultDetailObjectController.ShowResultDetail(trialData);

        resultChartsSubManager.ShowResultChart(trialData);
    }

}
