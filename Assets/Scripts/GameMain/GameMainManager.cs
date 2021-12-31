using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography; // 乱数生成
using UnityEngine;
using UnityEngine.InputSystem;

using TMPro; // スクリプトから TextMeshPro の変更


public class GameMainManager : MonoBehaviour
{
    // ゲーム状態変数
    enum GameState
    {
        Waiting,
        TrialOn,
        ResultOn
    }
    GameState gameState = GameState.Waiting;

    // 課題文字表示用プレハブ
    public GameObject AssignedCharTMPPrefab;
    GameObject[] assignedCharTMPs;

    // トライアル毎のデータ保管
    trialData nowTrialData;

    // ここから基本設定

    // アサインされた打鍵パターンの種類数 26*2 + 22*2 + Space
    public const ushort NumOfKeyPatterns = 97;
    // アサインされたキーの数（スペースとシフト込み文字を引いたもの）
    public const ushort NumOfUniqueChars = 48;
    // 文字種数（文字にバインドされない打鍵パターンが 2 つあるので - 2 ）
    public const ushort NumOfChars = NumOfKeyPatterns - 2;

    // ミス制限
    public ushort MissLimit = 50;

    // 課題文字の左上座標
    const int displayInitX = -375;
    const int displayInitY = 210;

    // 課題文字の Text Mesh Pro 表示をいくつずつズラすか
    const int displayCharXdiff = 19;
    const int displayCharYdiff = -39;

    // 課題文字の文字数、表示の行数など
    const int assignmentLength = 360;
    const int displayRowLength = 36;


    class trialData
    {
        public long totalTime;
        public long [] eachTime;
        // 正しく打ったキー数 = 次打つキーID。0 から始まり assignmentLength で打切
        public int typedKeys;
        // ミスキー数。MissLimit を超えると Esc
        public int missedKeys;

        public ushort[] trialAssignment_CharID;
        public char[] trialAssignment_Char;

        public trialData(int assignmentLength)
        {
            Debug.Log("New trialData was instantiated.");
            totalTime = 0;
            eachTime = new long[assignmentLength];
            typedKeys = 0;

            trialAssignment_CharID = new ushort[assignmentLength];
            trialAssignment_Char = new char[assignmentLength];
        }
    }

    // ストップウォッチ
    System.Diagnostics.Stopwatch myStopwatch;

    void Awake()
    {
        myStopwatch = new System.Diagnostics.Stopwatch();

        // EventBus にキー押下イベント発生時のメソッド実行を依頼
        EventBus.Instance.SubscribeNormalKeyDown(OnNormalKeyDown);
        EventBus.Instance.SubscribeReturnKeyDown(OnGameStartButtonClick);
        EventBus.Instance.SubscribeEscKeyDown(OnBackButtonClick);
    }
    // Start is called before the first frame update
    void Start()
    {
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    void OnDestroy()
    {
        EventBus.Instance.UnsubscribeNormalKeyDown(OnNormalKeyDown);
        EventBus.Instance.UnsubscribeReturnKeyDown(OnGameStartButtonClick);
        EventBus.Instance.UnsubscribeEscKeyDown(OnBackButtonClick);
    }

    // トライアル制御
    void OnCorrectKeyDown()
    {
        assignedCharTMPs[nowTrialData.typedKeys].SetActive(false);
        nowTrialData.typedKeys++;
    }
    void OnIncorrectKeyDown()
    {
        nowTrialData.missedKeys++;
        if (nowTrialData.missedKeys >= MissLimit)
        {
            Debug.Log($"ミスが多すぎます。最初からやり直してください。");
            CancelTrial();
        }
    }

    // 状態制御メソッド
    void StartTrial()
    {
        nowTrialData = new trialData(assignmentLength);

        // 乱数を生成して nowTrialData にセット
        RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        byte[] Seeds = new byte[assignmentLength];
        rng.GetBytes(Seeds);

        for (int i = 0; i < assignmentLength; i++)
        {
            System.Random rnd = new System.Random(Seeds[i]);
            ushort rnd_charID = (ushort)(rnd.Next(0, NumOfChars - 1));
            nowTrialData.trialAssignment_CharID[i] = rnd_charID;
            nowTrialData.trialAssignment_Char[i] = MyInputManager.ToChar_FromCharID(rnd_charID);
        }

        Debug.Log(string.Join(" ", nowTrialData.trialAssignment_CharID));
        Debug.Log(string.Join(" ", nowTrialData.trialAssignment_Char));


        // 課題文字の描画
        assignedCharTMPs = new GameObject[assignmentLength];

        // Assignment オブジェクトの個として描画するため
        GameObject asg = GameObject.Find("Assignment");
        assignedCharTMPs = new GameObject[assignmentLength];

        for (int i = 0; i < assignmentLength; i++)
        {
            assignedCharTMPs[i] = Instantiate(AssignedCharTMPPrefab, asg.transform);
            TextMeshProUGUI assignedCharTMPUGUI = assignedCharTMPs[i].GetComponent<TextMeshProUGUI>();
            assignedCharTMPUGUI.text = nowTrialData.trialAssignment_Char[i].ToString();

            // 表示場所の指定
            assignedCharTMPs[i].GetComponent<RectTransform>().localPosition = new Vector3(displayInitX + displayCharXdiff * (i % displayRowLength), displayInitY + displayCharYdiff * (i / displayRowLength), 0);
            //assignedCharTMPs[i].GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        }

        // RNGCryptoServiceProvider は IDisposable インターフェイスを実装していて、使用が完了したら型を破棄しなきゃいけないらしい（公式ドキュメントより）
        // IDisposable インターフェイスには Dispose() というメソッドだけが実装されている
        // これを継承するクラスは、「リソースを抱え込んでるから使い終わったら（GC を待たず）Dispose() で破棄した方がいい」と考えればいいだろう
        rng.Dispose();

        // 処理が終わってからゲーム開始＆ストップウォッチを開始
        gameState = GameState.TrialOn;
        myStopwatch.Restart();
    }
    void CancelTrial()
    {
        Debug.Assert(gameState == GameState.TrialOn);

        Debug.Log("Quitting Trial...");
        gameState = GameState.Waiting;
        myStopwatch.Stop();
        nowTrialData = null;

        foreach (GameObject obj in assignedCharTMPs)
        {
            Destroy(obj);
        }        
    }

    // イベントハンドラ
    // EventBus に渡して実行してもらう
    void OnBackButtonClick()
    {
        if (gameState == GameState.TrialOn) CancelTrial();
        //else if (isResultOn) ;
        else MySceneManager.ChangeSceneRequest("TitleScene");
    }
    void OnGameStartButtonClick()
    {
        if (gameState == GameState.TrialOn) return;
        else StartTrial();
    }
    void OnNormalKeyDown(ushort charID)
    {
        if (gameState != GameState.TrialOn) return;
        if (nowTrialData.trialAssignment_CharID[nowTrialData.typedKeys] == charID)
        {
            Debug.Log($"Correct Key {charID}:{MyInputManager.ToChar_FromCharID(charID)} was Down @ {myStopwatch.ElapsedMilliseconds} ms");
            OnCorrectKeyDown();
        }
        else
        {
            Debug.Log($"Incorrect Key {charID}:{MyInputManager.ToChar_FromCharID(charID)} was Down @ {myStopwatch.ElapsedMilliseconds} ms");
            OnIncorrectKeyDown();
        }
    }

}
