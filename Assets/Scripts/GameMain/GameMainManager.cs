using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography; // 乱数生成
using UnityEngine;
using UnityEngine.InputSystem;

using System.Linq; // 配列初期化用
using TMPro; // スクリプトから TextMeshPro の変更


public class GameMainManager : MonoBehaviour
{
    // ゲーム状態変数
    enum GameState
    {
        Waiting,
        TrialOn,
        Completed,
        Canceled
    }
    GameState gameState = GameState.Waiting;

    // 課題文字表示用プレハブ
    public GameObject AssignedCharTMPPrefab;
    GameObject[] assignedCharTMPs;

    // 情報表示オブジェクト
    public GameObject TotalTimeTMP;
    public GameObject TotalMissTMP;
    public GameObject[] LapTimeTMP;
    public GameObject[] LapMissTMP;
    public GameObject TotalCPSTMP;

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
    // アンカーは (0, 1) つまり 親オブジェクト Assignment の左上からの相対距離で指定
    const int displayInitX = 12;
    const int displayInitY = -10;

    // 課題文字の Text Mesh Pro 表示をいくつずつズラすか
    const int displayCharXdiff = 18;
    const int displayCharYdiff = -41;

    // 課題文字の文字数、表示の行数など
    const int assignmentLength = 360;
    const int lapLength = 36;
    const int numOfLaps = 10;


    class trialData
    {
        // 正しく打ったキー数 = 次打つキーID。0 から始まり assignmentLength で打切
        public int typedKeys = 0;
        // 現在何ラップ目か 1-indexed
        public int nowLap = 1;
        // トータルタイム・ラップタイム・各正解打鍵タイム
        // それぞれのタイム・ミスは全て「合計タイム」で入れるので、打鍵時間を出すには引き算が必要
        // lap, key の配列の [0] は番兵
        public long totalTime = 0;
        public long[] lapTime = Enumerable.Repeat<long>(0, numOfLaps + 1).ToArray();
        public long [] keyTime = Enumerable.Repeat<long>(0, assignmentLength + 1).ToArray();
        // トータルミス・ラップミス・各キー辺りミス（要るか？）
        // それぞれのタイム・ミスは全て「合計タイム」で入れるので、打鍵時間を出すには引き算が必要
        // lap, key の配列の [0] は番兵
        public int totalMiss = 0;
        public int[] lapMiss = Enumerable.Repeat<int>(0, numOfLaps + 1).ToArray();
        public int[] keyMiss = Enumerable.Repeat<int>(0, assignmentLength + 1).ToArray();

        public ushort[] trialAssignment_CharID;
        public char[] trialAssignment_Char;

        public trialData()
        {
            Debug.Log("New trialData was instantiated.");

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
        // Completed, Canceled でも TrialInfo は必要だが、(キャンセル|完了) に描画しているからここでは不要
        if (gameState == GameState.TrialOn) UpdateTrialInfo();
    }
    void OnDestroy()
    {
        EventBus.Instance.UnsubscribeNormalKeyDown(OnNormalKeyDown);
        EventBus.Instance.UnsubscribeReturnKeyDown(OnGameStartButtonClick);
        EventBus.Instance.UnsubscribeEscKeyDown(OnBackButtonClick);
    }

    // 状態制御メソッド
    void StartTrial()
    {
        // 画面表示の初期化
        TotalTimeTMP.GetComponent<TextMeshProUGUI>().text = $"0.000";
        TotalMissTMP.GetComponent<TextMeshProUGUI>().text = $"0";
        TotalCPSTMP.GetComponent<TextMeshProUGUI>().text = $"0.000";
        if (gameState != GameState.Waiting)
            foreach (GameObject obj in assignedCharTMPs) Destroy(obj);

        // nowTrialData の初期化
        nowTrialData = new trialData();

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
            assignedCharTMPs[i].GetComponent<RectTransform>().localPosition = new Vector3(displayInitX + displayCharXdiff * (i % lapLength), displayInitY + displayCharYdiff * (i / lapLength), 0);
            //assignedCharTMPs[i].GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        }

        // RNGCryptoServiceProvider は IDisposable インターフェイスを実装していて、使用が完了したら型を破棄しなきゃいけないらしい（公式ドキュメントより）
        // IDisposable インターフェイスには Dispose() というメソッドだけが実装されている
        // これを継承するクラスは、「リソースを抱え込んでるから使い終わったら（GC を待たず）Dispose() で破棄した方がいい」と考えればいいだろう
        rng.Dispose();

        // 処理が終わってからゲーム開始＆ストップウォッチを開始
        gameState = GameState.TrialOn;
        myStopwatch.Restart();

        UpdateTrialInfo();
    }
    void CompleteLap()
    {
        int lap = nowTrialData.nowLap;
        // nowTrialData へのラップタイムの入力
        nowTrialData.lapTime[lap] = nowTrialData.totalTime;
        // nowTrialData へのラップミスの入力
        nowTrialData.lapMiss[lap] = nowTrialData.totalMiss;

        // 最終ラップでない時のみ、ラップ番号を更新（現在ラップが境界外に出てしまうことを防ぐ）
        // また、次のラップも前のミス数と同じにする（ 0 のままだと表示が崩れる）
        if (lap < numOfLaps)
        {
            nowTrialData.lapMiss[lap+1] = nowTrialData.lapMiss[lap];
            nowTrialData.nowLap++;
        }
    }
    void CompleteTrial()
    {
        // ストップウォッチを止めるが、ここの時間はもはや関係無い（キー打鍵時に計測しているため）
        myStopwatch.Stop();

        Debug.Assert(gameState == GameState.TrialOn);
        gameState = GameState.Completed;

        // 完了してからトライアル情報の描画（トライアル中と計測の仕方が違うため）
        UpdateTrialInfo();
    }
    void CancelTrial()
    {
        // ストップウォッチを止めるが、ここの時間はもはや関係無い（キー打鍵時に計測しているため）
        myStopwatch.Stop();

        long time = nowTrialData.totalTime;
        int keys = nowTrialData.typedKeys;
        // nowTrialData にラップタイムを入力
        int lap = nowTrialData.nowLap;
        nowTrialData.lapTime[lap] = time;
        // nowTrialData にトータルタイム

        Debug.Assert(gameState == GameState.TrialOn);
        gameState = GameState.Canceled;

        // キャンセルしてからトライアル情報の描画（トライアル中と計測の仕方が違うため）
        UpdateTrialInfo();
    }

    // トライアル制御
    void OnCorrectKeyDown()
    {
        assignedCharTMPs[nowTrialData.typedKeys].GetComponent<TextMeshProUGUI>().color = new UnityEngine.Color(0.25f, 0.15f, 0.15f, 0.1f);
        // nowTrialData にトータルタイムの入力
        nowTrialData.typedKeys++;
        // ラップ完了
        if (nowTrialData.typedKeys % lapLength == 0) CompleteLap();
        // トライアル完了
        if (nowTrialData.typedKeys == assignmentLength) CompleteTrial();
    }
    void OnIncorrectKeyDown()
    {
        // nowTrialData にトータルミスの入力
        nowTrialData.totalMiss++;
        // nowTrialData にラップミスの入力
        int lap = nowTrialData.nowLap;
        nowTrialData.lapMiss[lap] = nowTrialData.totalMiss;

        if (nowTrialData.totalMiss >= MissLimit)
        {
            Debug.Log($"ミスが多すぎます。最初からやり直してください。");
            CancelTrial();
        }

    }

    // トライアル Information 描画メソッド
    // ここでは nowTrialData に記録済みのデータを描画するだけなので、更新は各イベント時に済ませる必要あり
    void UpdateTrialInfo()
    {
        Debug.Assert(gameState != GameState.Waiting);

        int lap = nowTrialData.nowLap;
        long time;
        // ゲーム中であった場合、リアルな経過時間を使って計算
        if (gameState == GameState.TrialOn) time = myStopwatch.ElapsedMilliseconds;
        // ゲーム(完了|キャンセル)後であった場合、最後にキーを打鍵した時間を使って計算
        else time = nowTrialData.totalTime;
        int keys = nowTrialData.typedKeys;
        double cps = (double)(keys * 1000) / time;

        // トータルタイムの表示
        TotalTimeTMP.GetComponent<TextMeshProUGUI>().text = $"{((double)time / 1000):F3}";
        // トータルミスの表示
        TotalMissTMP.GetComponent<TextMeshProUGUI>().text = $"{nowTrialData.totalMiss}";
        // トータル CPS の表示
        TotalCPSTMP.GetComponent<TextMeshProUGUI>().text = $"{cps:F3}";
        // ラップタイムの表示
        // 1 ~ lap のループなので注意（lap が 1-indexed のため）
        // 更に、LapTimeTMP [] は 0-indexed であることにも注意 -> [(i|lap)-1] でアクセス
        for (int i = 1; i <= lap; i++)
        {
            LapTimeTMP[i-1].GetComponent<TextMeshProUGUI>().text = $"{(double)(nowTrialData.lapTime[i] - nowTrialData.lapTime[i-1]) / 1000:F3}";
        }
        // ゲーム中であった場合、現在ラップはリアルな経過時間を使って計算
        if (gameState == GameState.TrialOn) LapTimeTMP[lap-1].GetComponent<TextMeshProUGUI>().text = $"{(double)(time - nowTrialData.lapTime[lap-1]) / 1000:F3}";
        // ラップミスの表示
        for (int i = 1; i <= lap; i++)
        {
            int iLapMiss = nowTrialData.lapMiss[i] - nowTrialData.lapMiss[i - 1];
            LapMissTMP[i-1].GetComponent<TextMeshProUGUI>().text = (iLapMiss == 0 ? "" : iLapMiss.ToString());
        }
        // 終了していないラップは 空白 で埋める
        for (int i = lap+1; i <= numOfLaps; i++)
        {
            LapTimeTMP[i - 1].GetComponent<TextMeshProUGUI>().text = "";
            LapMissTMP[i - 1].GetComponent<TextMeshProUGUI>().text = "";
        }

    }

    // イベントハンドラ
    // EventBus に渡して実行してもらう
    // OnBackButtonClick() では時間を測らない
    // キャンセル（Esc）時の totalTime は キャンセル時基準ではなく、最後の正解打鍵orミス打鍵時をとるため。
    void OnBackButtonClick()
    {
        if (gameState == GameState.TrialOn) CancelTrial();
        //else if (isCompleted) ;
        else MySceneManager.ChangeSceneRequest("TitleScene");
    }
    void OnGameStartButtonClick()
    {
        if (gameState == GameState.TrialOn) return;
        else StartTrial();
    }
    // OnNormalKeyDown() で時間を計測
    // トライアル中の情報表示は Update() 内でタイマーを止めて雑に測ればいいが、ラップ・トライアル完了時の時間計測（キー押下にかかった時間）は正確にとる必要があるため。
    void OnNormalKeyDown(ushort charID)
    {
        if (gameState != GameState.TrialOn) return;
        if (nowTrialData.trialAssignment_CharID[nowTrialData.typedKeys] == charID)
        {
            nowTrialData.totalTime = myStopwatch.ElapsedMilliseconds;
            Debug.Log($"Correct Key {charID}:{MyInputManager.ToChar_FromCharID(charID)} was Down @ {nowTrialData.totalTime} ms");
            OnCorrectKeyDown();
        }
        else
        {
            nowTrialData.totalTime = myStopwatch.ElapsedMilliseconds;
            Debug.Log($"Incorrect Key {charID}:{MyInputManager.ToChar_FromCharID(charID)} was Down @ {nowTrialData.totalTime} ms");
            OnIncorrectKeyDown();
        }
    }

}
