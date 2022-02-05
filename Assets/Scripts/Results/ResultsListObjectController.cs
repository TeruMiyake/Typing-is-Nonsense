using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro; // スクリプトから TextMeshPro の変更

public class ResultsListObjectController : MonoBehaviour
{
    // 細かいゲーム変数（定数から設定する）
    int gameMode;
    int numOfLaps;

    // 上司
    ResultsManager resultsManager;

    // ResultsList を表示するための場所
    public GameObject resultsListContent;

    // ResultSummary
    GameObject[] resultSummaryObjects;
    public GameObject ResultSummaryPrefab; // set from inspector

    int numOfResults;

    // 表示位置定数
    // アンカーは (0, 1) つまり 親オブジェクト Content の左上からの相対距離で指定
    int displayInitX = 0;
    int displayInitY = 0;

    // ResultSummary を表示するごとに座標をいくつずつズラすか
    int displayYDiff = -45;


    void Awake()
    {
        // 上司を見つける
        resultsManager = GetComponent<ResultsManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void DisplayResultSummaries(ResultSummaries summaries)
    {
        gameMode = resultsManager.GetGameMode();
        numOfLaps = GlobalConsts.NumOfLaps[gameMode];

        // 古いオブジェクトは消す（遷移がしっかりしていれば、本来、消す必要は無いが……）
        if (resultSummaryObjects != null)
            foreach (GameObject summaryObj in resultSummaryObjects) Destroy(summaryObj);

        // オブジェクト用意
        numOfResults = summaries.NumOfResults;
        resultSummaryObjects = new GameObject[numOfResults];
        for (int i = 0; i < numOfResults; i++)
        {
            // ResultListContent オブジェクトの子として描画
            resultSummaryObjects[i] = Instantiate(ResultSummaryPrefab, resultsListContent.transform);

            // 表示場所の指定
            resultSummaryObjects[i].GetComponent<RectTransform>().localPosition = new Vector3(displayInitX, displayInitY + displayYDiff * i, 0);
        }

        // 表示
        for (int i = 0; i < numOfResults; i++)
        {
            DisplayResultSummary(i, summaries.summaryList[i]);
        }
    }
    public void DisplayResultSummary(int idx, ResultSummary summary)
    {
        gameMode = resultsManager.GetGameMode();
        numOfLaps = GlobalConsts.NumOfLaps[gameMode];

        GameObject summaryObj = resultSummaryObjects[idx];

        // TMPro の描画
        TextMeshProUGUI rankTMP = summaryObj.transform.Find("RankTMP").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI timeTMP = summaryObj.transform.Find("TotalTimeTMP").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI missTMP = summaryObj.transform.Find("TotalMissTMP").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI[] lapTimeTMPs = new TextMeshProUGUI[numOfLaps];
        for (int i = 1; i < numOfLaps + 1; i++)
        {
            lapTimeTMPs[i-1] = summaryObj.transform.Find($"Lap{i}TMP").GetComponent<TextMeshProUGUI>();
        }
        TextMeshProUGUI dateTimeTMP = summaryObj.transform.Find("DateTimeTMP").GetComponent<TextMeshProUGUI>();

        rankTMP.text = summary.Rank.ToString();
        string str = summary.TotalTime.ToFormattedTime();
        str = str.Substring(0, 6);
        timeTMP.text = str;
        missTMP.text = summary.TotalMiss.ToString();
        for (int i = 0; i < numOfLaps; i++)
        {
            lapTimeTMPs[i].text = summary.LapTime[i].ToFormattedTime();
        }
        dateTimeTMP.text = summary.DateTimeWhenFinished.ToString("yyyy/MM/dd") + "\n" + summary.DateTimeWhenFinished.ToString("HH:mm:ss");

        // トグルアイコンの描画
        Toggle protectionToggle = summaryObj.transform.Find("ProtectionToggle").GetComponent<Toggle>();
        Toggle logToggle = summaryObj.transform.Find("LogToggle").GetComponent<Toggle>();
        Toggle keyToggle = summaryObj.transform.Find("KeyToggle").GetComponent<Toggle>();
        protectionToggle.isOn = summary.IsProtected;
        logToggle.isOn = summary.HasLog;
        keyToggle.isOn = summary.HasRegistrationCode;
    }
}
