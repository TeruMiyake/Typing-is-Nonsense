using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro; // スクリプトから TextMeshPro の変更

public class TrialObjectController : MonoBehaviour
{
    // ゲームモード変数
    public int gameMode;

    // 課題文字表示用プレハブ
    // このプレハブには、ユーザー追加フォントをアセット化して読み込む必要がある
    public GameObject AssignedCharTMPPrefab;
    GameObject[] assignedCharTMPs;

    // 情報表示オブジェクト
    public GameObject TotalTimeTMP;
    public GameObject TotalMissTMP;
    public GameObject[] LapTimeTMP;
    public GameObject[] LapMissTMP;
    public GameObject TotalCPSTMP;
    TextMeshProUGUI countDownTMPUGUI;

    // 細かいゲーム変数（定数から設定する）

    // 課題文字の左上座標
    // アンカーは (0, 1) つまり 親オブジェクト Assignment の左上からの相対距離で指定
    int displayInitX;
    int displayInitY;

    // 課題文字の Text Mesh Pro 表示をいくつずつズラすか
    int displayCharXDiff;
    int displayCharYDiff;

    // TrialData に渡すゲームモード変数と、それが示す意味
    int assignmentLength;
    int lapLength;
    int numOfLaps;

    private void Awake()
    {
        // 定数の読み込み
        displayInitX = GlobalConsts.DisplayInitX[0];
        displayInitY = GlobalConsts.DisplayInitY[0];
        displayCharXDiff = GlobalConsts.DisplayCharXDiff[0];
        displayCharYDiff = GlobalConsts.DisplayCharYDiff[0];
        assignmentLength = GlobalConsts.AssignmentLength[0];
        lapLength = GlobalConsts.LapLength[0];
        numOfLaps = GlobalConsts.NumOfLaps[0];

        // 制御すべきオブジェクトなどの取得と初期化
        countDownTMPUGUI = GameObject.Find("CountDownTMP").GetComponent<TextMeshProUGUI>();
        SetCountDownText("");

        // フォントフォールバック追加
        TextMeshProUGUI prefabUGUI = AssignedCharTMPPrefab.GetComponent<TextMeshProUGUI>();
        TMP_FontAsset prefabFontAsset = prefabUGUI.font;
        RuntimeFontController.Instance.AddUserFontsToFallback(prefabFontAsset);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetCountDownText(string str)
    {
        countDownTMPUGUI.text = str;
    }

    /// <summary>
    /// TrialSubManager から GameState の変動を伝える
    /// </summary>
    /// <param name="state"></param>
    public void NotifyGameStateChanged(GameState state)
    {
        SetDisplay(state);
    }
    /// <summary>
    /// 画面表示を状態毎のデフォルト値にセットする
    /// </summary>
    /// <param name="state"></param>
    void SetDisplay(GameState state)
    {
        switch (state)
        {
            case GameState.Waiting:
                TotalTimeTMP.GetComponent<TextMeshProUGUI>().text = $"0.000";
                TotalMissTMP.GetComponent<TextMeshProUGUI>().text = $"0";
                TotalCPSTMP.GetComponent<TextMeshProUGUI>().text = $"0.000";
                for (int i = 0; i < numOfLaps; i++)
                {
                    SetLapTime(i + 1, 0);
                    SetLapMiss(i + 1, 0);
                }
                if (assignedCharTMPs != null)
                    foreach (GameObject obj in assignedCharTMPs) Destroy(obj);
                break;
            case GameState.Countdown:
                TotalTimeTMP.GetComponent<TextMeshProUGUI>().text = $"0.000";
                TotalMissTMP.GetComponent<TextMeshProUGUI>().text = $"0";
                TotalCPSTMP.GetComponent<TextMeshProUGUI>().text = $"0.000";
                for (int i = 0; i < numOfLaps; i++)
                {
                    SetLapTime(i + 1, 0);
                    SetLapMiss(i + 1, 0);
                }
                if (assignedCharTMPs != null)
                    foreach (GameObject obj in assignedCharTMPs) Destroy(obj);
                break;
            case GameState.Completed:
                // ボタンの表示変更
                GameObject.Find("StartButton").GetComponent<UnityEngine.UI.Button>().interactable = true;
                GameObject.Find("BackButtonTMP").GetComponent<TextMeshProUGUI>().text = "Back To Title [Esc]";
                break;
            case GameState.Canceled:
                // ボタンの表示変更
                GameObject.Find("StartButton").GetComponent<UnityEngine.UI.Button>().interactable = true;
                GameObject.Find("BackButtonTMP").GetComponent<TextMeshProUGUI>().text = "Back To Title [Esc]";
                break;
            case GameState.Failed:
                // ボタンの表示変更
                GameObject.Find("StartButton").GetComponent<UnityEngine.UI.Button>().interactable = true;
                GameObject.Find("BackButtonTMP").GetComponent<TextMeshProUGUI>().text = "Back To Title [Esc]";
                break;
        }
    }

    public void StartCountDown()
    {
        // ボタンの表示変更
        GameObject.Find("StartButton").GetComponent<UnityEngine.UI.Button>().interactable = false;
        GameObject.Find("BackButtonTMP").GetComponent<TextMeshProUGUI>().text = "Cancel Trial [Esc]";
    }
    /// <summary>
    /// SubManager が StartTrial に入ったとき、開始前にする下準備
    /// </summary>
    public void PrepareToStartTrial()
    {
        // 課題文字の描画
        assignedCharTMPs = new GameObject[assignmentLength];

        // Assignment オブジェクトの子として描画するため
        GameObject asg = GameObject.Find("Assignment");
        assignedCharTMPs = new GameObject[assignmentLength];

        // 課題文字を描画するためのオブジェクトを用意
        for (int i = 0; i < assignmentLength; i++)
        {
            assignedCharTMPs[i] = Instantiate(AssignedCharTMPPrefab, asg.transform);

            // 表示場所の指定
            assignedCharTMPs[i].GetComponent<RectTransform>().localPosition = new Vector3(displayInitX + displayCharXDiff * (i % lapLength), displayInitY + displayCharYDiff * (i / lapLength), 0);
        }
    }

    // ここから先、ResultDetailObjectController と内容一緒

    /// <summary>
    /// SubManager が nowTrialData を生成したあと、課題文字を表示するためのもの
    /// </summary>
    public void SetAssignedChar(int i, char c)
    {
        TextMeshProUGUI assignedCharTMPUGUI = assignedCharTMPs[i].GetComponent<TextMeshProUGUI>();
        assignedCharTMPUGUI.text = c.ToString();
    }
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
        string str = (lapTime == 0) ? "" : lapTime.ToFormattedTime();
        LapTimeTMP[lap-1].GetComponent<TextMeshProUGUI>().text = str;
    }
    /// <summary>
    /// 終了していないラップの場合は 0 を渡す（空白を表示）
    /// </summary>
    /// <param name="lap"></param>
    /// <param name="lapMiss"></param>
    public void SetLapMiss(int lap, int lapMiss)
    {
        string str = (lapMiss == 0) ? "" : lapMiss.ToString();
        LapMissTMP[lap-1].GetComponent<TextMeshProUGUI>().text = str;
    }
}
