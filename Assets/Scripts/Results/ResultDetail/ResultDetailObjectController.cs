using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResultDetailObjectController : MonoBehaviour
{
    TrialData trialData;
    KeyBindDicts dicts;

    // 課題文字表示用プレハブ
    public GameObject ResultDetailAssignedCharTMPPrefab;
    GameObject[] assignedCharTMPs;

    // 情報表示オブジェクト
    public GameObject TotalTimeTMP;
    public GameObject TotalMissTMP;
    public GameObject[] LapTimeTMP;
    public GameObject[] LapMissTMP;
    public GameObject TotalCPSTMP;


    // 課題文字の左上座標
    // アンカーは (0, 1) つまり 親オブジェクト Assignment の左上からの相対距離で指定
    // Trial とは異なるため GlobalConsts で設定しない
    int displayInitX = 2;
    int displayInitY = 0;

    // 課題文字の Text Mesh Pro 表示をいくつずつズラすか
    // Trial とは異なるため GlobalConsts で設定しない
    int displayCharXDiff = 15;
    int displayCharYDiff = -27;

    // TrialData に渡すゲームモード変数と、それが示す意味
    int assignmentLength;
    int lapLength;
    int numOfLaps;

    private void Awake()
    {
        assignmentLength = GlobalConsts.AssignmentLength[0];
        lapLength = GlobalConsts.LapLength[0];
        numOfLaps = GlobalConsts.NumOfLaps[0];
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// ResultDetail を表示する大本のメソッド
    /// 実際の描画処理はここでは行わず、もっと小さなメソッドたちを呼ぶ
    /// </summary>
    /// <param name="_trialData"></param>
    public void ShowResultDetail(TrialData _trialData)
    {
        trialData = _trialData;
        dicts = new KeyBindDicts(trialData.GetKeyBind());

        InitializeAllDisplay();

        SetAssignedChars(trialData.TaskCharIDs);
        SetTotalTime(trialData.TotalTime);
        SetTotalMiss(trialData.TotalMiss);
        double cps = trialData.TotalTime.ToCPS(assignmentLength);
        SetCPS(cps);

        // laptime : 1-indexed
        for (int i = 1; i < numOfLaps + 1; i++)
        {
            SetLapTime(i, trialData.GetSingleLapTime(i));
        }
        Debug.Log("ShowResultDetail completed.");
    }
    public void InitializeAllDisplay()
    {
        TotalTimeTMP.GetComponent<TextMeshProUGUI>().text = $"0.000";
        TotalMissTMP.GetComponent<TextMeshProUGUI>().text = $"0";
        TotalCPSTMP.GetComponent<TextMeshProUGUI>().text = $"0.000";
        if (assignedCharTMPs != null)
            foreach (GameObject obj in assignedCharTMPs) Destroy(obj);
    }
    public void SetAssignedChars(ushort[] taskcharIDs)
    {
        PrepareAssignedCharTMPPrefabs();

        for (int i = 0; i < assignmentLength; i++)
        {
            SetAssignedChar(i, dicts.ToChar_FromCharID(taskcharIDs[i]));
        }
    }
    public void SetAssignedChar(int i, char c)
    {
        TextMeshProUGUI assignedCharTMPUGUI = assignedCharTMPs[i].GetComponent<TextMeshProUGUI>();
        assignedCharTMPUGUI.text = c.ToString();
    }
    /// <summary>
    /// TMP プレハブを準備
    /// TrialObjectController と実質的には同じ内容
    /// </summary>
    public void PrepareAssignedCharTMPPrefabs()
    {
        // 課題文字の描画
        assignedCharTMPs = new GameObject[assignmentLength];

        // Assignment オブジェクトの子として描画するため
        GameObject asg = GameObject.Find("Assignment");
        assignedCharTMPs = new GameObject[assignmentLength];

        // 課題文字を描画するためのオブジェクトを用意
        for (int i = 0; i < assignmentLength; i++)
        {
            assignedCharTMPs[i] = Instantiate(ResultDetailAssignedCharTMPPrefab, asg.transform);

            // 表示場所の指定
            assignedCharTMPs[i].GetComponent<RectTransform>().localPosition = new Vector3(displayInitX + displayCharXDiff * (i % lapLength), displayInitY + displayCharYDiff * (i / lapLength), 0);
        }
    }

    // ここから先、TrialObjectController と内容一緒
    // だから使わないものも入ってるかも。そのうち整理する？それか共通させといた方がいい？

    /// <summary>
    /// 課題文字の色を薄くする
    /// </summary>
    /// <param name="idx"></param>
    public void SetCharColorToTyped(int idx)
    {
        TextMeshProUGUI typedCharTMPUGUI
            = assignedCharTMPs[idx].GetComponent<TextMeshProUGUI>();
        typedCharTMPUGUI.color = GlobalConsts.TypedCharColor;
    }
    public void SetTotalTime(MilliSecond totalTime)
    {
        TotalTimeTMP.GetComponent<TextMeshProUGUI>().text = totalTime.ToFormattedTime();
    }
    public void SetTotalMiss(int totalMiss)
    {
        TotalMissTMP.GetComponent<TextMeshProUGUI>().text = $"{totalMiss}";
    }
    public void SetCPS(double cps)
    {
        TotalCPSTMP.GetComponent<TextMeshProUGUI>().text = $"{cps:F3}";
    }
    /// <summary>
    /// 終了していないラップの場合は 0 を渡す（空白を表示）
    /// </summary>
    /// <param name="lap">1-indexed</param>
    /// <param name="lapTime"></param>
    public void SetLapTime(int lap, MilliSecond lapTime)
    {
        Debug.Log("started setlaptime()");
        string str = (lapTime == 0) ? "" : lapTime.ToFormattedTime();
        LapTimeTMP[lap - 1].GetComponent<TextMeshProUGUI>().text = str;
        Debug.Log("completed setlaptime()");
    }
    /// <summary>
    /// 終了していないラップの場合は 0 を渡す（空白を表示）
    /// </summary>
    /// <param name="lap"></param>
    /// <param name="lapMiss"></param>
    public void SetLapMiss(int lap, int lapMiss)
    {
        string str = (lapMiss == 0) ? "" : lapMiss.ToString();
        LapMissTMP[lap - 1].GetComponent<TextMeshProUGUI>().text = str;
    }

}