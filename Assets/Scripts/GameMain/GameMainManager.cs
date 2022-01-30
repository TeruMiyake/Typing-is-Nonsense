using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography; // 乱数生成
using UnityEngine;
using UnityEngine.InputSystem;

using TMPro; // スクリプトから TextMeshPro の変更

public class GameMainManager : MonoBehaviour
{
    GameState gameState = GameState.Waiting;

    // ScriptableObject
    [SerializeField]
    KeyBind keyBind;
    KeyBindDicts keyBindDicts;

    // 部下となるシーン内のマネジャー達
    TweeterManager tweeter;

    // 簡易設定オブジェクト
    public GameObject MissLimitInput;

    // 課題文字表示用プレハブ
    public GameObject AssignedCharTMPPrefab;
    GameObject[] assignedCharTMPs;

    // 情報表示オブジェクト
    public GameObject TotalTimeTMP;
    public GameObject TotalMissTMP;
    public GameObject[] LapTimeTMP;
    public GameObject[] LapMissTMP;
    public GameObject TotalCPSTMP;
    TextMeshProUGUI countDownTMPUGUI;


    // トライアル毎のデータ保管
    TrialData nowTrialData;

    // nowTrialData.AllInputIDs 用の定数
    const ushort shiftdownInputID = 97;
    const ushort shiftupInputID = 98;

    // ここから基本設定

    // アサインされた打鍵パターンの種類数 26*2 + 22*2 + Space
    public const ushort NumOfKeyPatterns = 97;
    // 文字種数（文字にバインドされない打鍵パターンが 2 つあるので - 2 ）
    public const ushort NumOfChars = NumOfKeyPatterns - 2;

    // Config（ミス制限など）
    Config config = new Config();

    // 課題文字の左上座標
    // アンカーは (0, 1) つまり 親オブジェクト Assignment の左上からの相対距離で指定
    const int displayInitX = 12;
    const int displayInitY = -10;

    // 課題文字の Text Mesh Pro 表示をいくつずつズラすか
    const int displayCharXdiff = 18;
    const int displayCharYdiff = -41;

    // TrialData に渡すゲームモード変数と、それが示す意味
    const int gameMode = 0;
    const int assignmentLength = 360;
    const int lapLength = 36;
    const int numOfLaps = 10;

    // 色設定
    Color typedCharColor = new Color(0.25f, 0.15f, 0.15f, 0.1f);

    // ストップウォッチ
    System.Diagnostics.Stopwatch myStopwatch;

    void Awake()
    {
        keyBind = new KeyBind();
        keyBind.LoadFromJson(0);
        keyBindDicts = new KeyBindDicts(keyBind);

        // 部下を見つける
        tweeter = GameObject.Find("Tweeter").GetComponent<TweeterManager>();

        // 制御すべきオブジェクトなどの取得と初期化
        countDownTMPUGUI = GameObject.Find("CountDownTMP").GetComponent<TextMeshProUGUI>();
        countDownTMPUGUI.text = "";

        // 表示制御
        tweeter.SetVisible(false);

        // Config の読み込みと表示への反映
        config.Load();
        GameObject.Find("MissLimiterInput").GetComponent<TMP_InputField>().text = config.MissLimit.ToString();

        myStopwatch = new System.Diagnostics.Stopwatch();

        // EventBus にキー押下イベント発生時のメソッド実行を依頼
        EventBus.Instance.SubscribeNormalKeyDown(OnNormalKeyDown);
        EventBus.Instance.SubscribeReturnKeyDown(OnGameStartButtonClick);
        EventBus.Instance.SubscribeEscKeyDown(OnBackButtonClick);
        EventBus.Instance.SubscribeShiftKeyDown(ShiftKeyDownHandler);
        EventBus.Instance.SubscribeShiftKeyUp(ShiftKeyUpHandler);
    }
    // Start is called before the first frame update
    void Start()
    {
    }
    // Update is called once per frame
    void Update()
    {
        // Completed, Canceled でも TrialInfo は必要だが、(キャンセル|完了) に描画しているからここでは不要
        if (gameState == GameState.Countdown)
        {
            long ms = myStopwatch.ElapsedMilliseconds;
            if (ms >= 3000) {
                countDownTMPUGUI.text = "";
                StartTrial();
            }
            else if (ms >= 2000)
            {
                countDownTMPUGUI.text = "1";
            }
            else if (ms >= 1000)
            {
                countDownTMPUGUI.text = "2";
            }
            else countDownTMPUGUI.text = "3";
        }
        else if (gameState == GameState.TrialOn) UpdateTrialInfo();
    }
    void OnDestroy()
    {
        EventBus.Instance.UnsubscribeNormalKeyDown(OnNormalKeyDown);
        EventBus.Instance.UnsubscribeReturnKeyDown(OnGameStartButtonClick);
        EventBus.Instance.UnsubscribeEscKeyDown(OnBackButtonClick);
        EventBus.Instance.UnsubscribeShiftKeyDown(ShiftKeyDownHandler);
        EventBus.Instance.UnsubscribeShiftKeyUp(ShiftKeyUpHandler);
    }

    // 部下とのやり取り
    /// <summary>
    /// TweeterManager が表示すべき TrialData を取得するためのメソッド
    /// </summary>
    /// <returns></returns>
    public System.Tuple<string, TrialData> GetTrialData()
    {
        if (gameState == GameState.TrialOn || gameState == GameState.Waiting)
            return new System.Tuple<string, TrialData>(gameState.ToString(), null);
        else
            return new System.Tuple<string, TrialData>(gameState.ToString(), nowTrialData);
    }

    // ユーティリティメソッド
    /// <summary>
    /// long で保存してあるミリ秒を、表示用の X.XXX s に変える
    /// </summary>
    /// <param name="ms"></param>
    /// <returns></returns>
    string ToFormattedTime(long ms)
    {
        return (ms / 1000).ToString() + "." + (ms % 1000).ToString("000");
    }

    // 状態制御メソッド
    void StartCountdown()
    {
        gameState = GameState.Countdown;

        // カウントダウンスタート
        myStopwatch.Restart();

        // 簡易設定中は動かせない ※シフト絡みのバグがここに眠ってるかも
        if (MissLimitInput.GetComponent<TMP_InputField>().isFocused) return;

        // 簡易設定を停止
        MissLimitInput.GetComponent<TMP_InputField>().readOnly = true;

        // 画面表示の初期化
        TotalTimeTMP.GetComponent<TextMeshProUGUI>().text = $"0.000";
        TotalMissTMP.GetComponent<TextMeshProUGUI>().text = $"0";
        TotalCPSTMP.GetComponent<TextMeshProUGUI>().text = $"0.000";
        if (assignedCharTMPs != null)
            foreach (GameObject obj in assignedCharTMPs) Destroy(obj);
        tweeter.SetVisible(false);


        // ボタンの表示変更
        GameObject.Find("StartButton").GetComponent<UnityEngine.UI.Button>().interactable = false;
        GameObject.Find("BackButtonTMP").GetComponent<TextMeshProUGUI>().text = "Cancel Trial [Esc]";

        // 設定の再読み込み（簡易設定で更新している場合があるので、ここで再度読み込む）
        config.Load();

        // nowTrialData の初期化
        nowTrialData = new TrialData(gameMode, config.MissLimit, keyBind);

        // 乱数を生成して nowTrialData にセット
        RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        byte[] randomBytes = new byte[4 * assignmentLength];
        rng.GetBytes(randomBytes);
        for (int i = 0; i < assignmentLength; i++)
        {
            ushort rnd_charID = (ushort)(Math.Abs(BitConverter.ToInt32(randomBytes, i * 4)) % NumOfChars); // 0 ~ 94

            // Null 文字をまたぐ毎に CharID を ++ する必要がある
            if (rnd_charID >= keyBind.NullKeyMap[0]) rnd_charID++;
            if (rnd_charID >= keyBind.NullKeyMap[1]) rnd_charID++;

            nowTrialData.TaskCharIDs[i] = rnd_charID;
        }

        // RNGCryptoServiceProvider は IDisposable インターフェイスを実装していて、使用が完了したら型を破棄しなきゃいけないらしい（公式ドキュメントより）
        rng.Dispose();
    }
    /// <summary>
    /// トライアル開始メソッド
    /// カウントダウン終了時に Update() から呼び出される
    /// </summary>
    void StartTrial()
    {
        gameState = GameState.TrialOn;

        // TrialData.AllInputIDs[0] に初期シフト状態を格納
        nowTrialData.AllInputIDs.Add(MyInputManager.GetShiftState());
        nowTrialData.AllInputTime.Add(0);

        // 課題文字の描画
        assignedCharTMPs = new GameObject[assignmentLength];

        // Assignment オブジェクトの子として描画するため
        GameObject asg = GameObject.Find("Assignment");
        assignedCharTMPs = new GameObject[assignmentLength];

        for (int i = 0; i < assignmentLength; i++)
        {
            assignedCharTMPs[i] = Instantiate(AssignedCharTMPPrefab, asg.transform);
            TextMeshProUGUI assignedCharTMPUGUI = assignedCharTMPs[i].GetComponent<TextMeshProUGUI>();
            assignedCharTMPUGUI.text = keyBindDicts.ToChar_FromCharID(nowTrialData.TaskCharIDs[i]).ToString();

            // 表示場所の指定
            assignedCharTMPs[i].GetComponent<RectTransform>().localPosition = new Vector3(displayInitX + displayCharXdiff * (i % lapLength), displayInitY + displayCharYdiff * (i / lapLength), 0);
        }

        // 処理が終わってからゲーム開始＆ストップウォッチを開始
        myStopwatch.Restart();

        UpdateTrialInfo();
    }
    void CompleteLap()
    {
        int lap = nowTrialData.TypedKeys / lapLength;
        // nowTrialData へのラップタイムの入力
        // 並列処理の関係でもしかしたら KeyTime がズレるのかもしれないので、ここでは .TotalTime を使っていない
        nowTrialData.SetLapTime(lap, nowTrialData.CorrectKeyTime[nowTrialData.TypedKeys]);
        // nowTrialData へのラップミスの入力
        nowTrialData.LapMiss[lap] = nowTrialData.TotalMiss;

        // 最終ラップでない時のみ、ラップ番号を更新（現在ラップが境界外に出てしまうことを防ぐ）
        // また、次のラップも前のミス数と同じにする（ 0 のままだと表示が崩れる）
        if (lap < numOfLaps)
            nowTrialData.LapMiss[lap+1] = nowTrialData.LapMiss[lap];
    }
    /// <summary>
    /// トライアル完了時に固有の処理を行う。トライアル終了時の共通処理は OnEndTrial()
    /// </summary>
    void CompleteTrial()
    {
        // ストップウォッチを止めるが、ここの時間はもはや関係無い（キー打鍵時に計測しているため）
        myStopwatch.Stop();

        // nowTrialData の更新
        nowTrialData.IsTerminated = false;

        Debug.Assert(gameState == GameState.TrialOn);
        gameState = GameState.Completed;

        // 終了時共通処理の呼び出し
        OnEndTrial();
    }
    /// <summary>
    /// トライアル中断時に固有の処理を行う。トライアル終了時の共通処理は OnEndTrial()
    /// ミス制限を超えると -TotalTime Failed, Esc -> Canceled
    /// </summary>
    void CancelTrial()
    {
        // ストップウォッチを止めるが、ここの時間はもはや関係無い（OnNormalKeyDwn() で計測しているため）
        myStopwatch.Stop();

        // nowTrialData の更新
        nowTrialData.IsTerminated = true;

        // TrialData に最終ラップタイムをセット
        int lap = nowTrialData.TypedKeys / lapLength + 1;
        nowTrialData.SetLapTime(lap, nowTrialData.TotalTime);

        Debug.Assert(gameState == GameState.TrialOn || gameState == GameState.Countdown);
        if (nowTrialData.TotalMiss <= nowTrialData.MissLimit) gameState = GameState.Canceled;
        else gameState = GameState.Failed;

        // 終了時共通処理の呼び出し
        OnEndTrial();
    }
    /// <summary>
    /// CompleteTrial() と CancelTrial() から呼び出す、トライアル終了時の共通処理
    /// </summary>
    void OnEndTrial()
    {
        // nowTrialData の設定
        nowTrialData.DateTimeWhenFinished = System.DateTime.Now;

        // データの保存
        nowTrialData.SaveLog();

        // トライアル情報の描画（トライアル中と計測の仕方が違うため、終了処理後に呼び出し）
        UpdateTrialInfo();

        // ボタンの表示変更
        GameObject.Find("StartButton").GetComponent<UnityEngine.UI.Button>().interactable = true;
        GameObject.Find("BackButtonTMP").GetComponent<TextMeshProUGUI>().text = "Back To Title [Esc]";

        // 簡易設定を再起動
        MissLimitInput.GetComponent<TMP_InputField>().readOnly = false;
    }

    // 表示制御

    // トライアル Information 描画メソッド
    // ここでは nowTrialData に記録済みのデータを描画するだけなので、更新は各イベント時に済ませる必要あり
    void UpdateTrialInfo()
    {
        Debug.Assert(gameState != GameState.Waiting);

        int lap = nowTrialData.TypedKeys / lapLength + 1;
        // トライアル完了時のみ lap > numOfLaps となってしまうので、それを避ける
        if (lap > numOfLaps) lap = numOfLaps;

        long totalTime;
        // ゲーム中であった場合、リアルな経過時間を使って計算
        if (gameState == GameState.TrialOn) totalTime = myStopwatch.ElapsedMilliseconds;
        // ゲーム(完了|キャンセル)後であった場合、最後にキーを打鍵した時間を使って計算
        else totalTime = nowTrialData.TotalTime;
        int keys = nowTrialData.TypedKeys;
        double cps = (double)(keys * 1000) / totalTime;

        // テキスト更新用の使い捨て関数
        void _UpdateText(GameObject obj, string str) => obj.GetComponent<TextMeshProUGUI>().text = str;

        // トータルタイムの表示
        _UpdateText(TotalTimeTMP, ToFormattedTime(totalTime));
        // トータルミスの表示
        _UpdateText(TotalMissTMP, $"{nowTrialData.TotalMiss}");
        // トータル CPS の表示
        _UpdateText(TotalCPSTMP, $"{cps:F3}");
        // ラップタイムの表示
        // 1 ~ lap-1 までは nowTrialData に書き込み済み（1-indexed 注意）
        long[] singleLapTime = new long[lap + 1];
        singleLapTime[0] = 0;
        for (int i = 1; i <= lap-1; i++)
        {
            singleLapTime[i] = nowTrialData.GetSingleLapTime(i);
        }
        // 現在ラップは time を使う
        // time : ゲーム中ならリアルな打鍵時間、terminated なら最終キー打鍵時間が入っている
        singleLapTime[lap] = totalTime - nowTrialData.GetLapTime(lap-1);
        // LapTimeTMP [] は 0-indexed であることにも注意 -> [(i|lap)-1] でアクセス
        for (int i = 1; i <= lap; i++)
        {
            _UpdateText(LapTimeTMP[i - 1], ToFormattedTime(singleLapTime[i]));
        }
        // ラップミスの表示
        for (int i = 1; i <= lap; i++)
        {
            int iLapMiss = nowTrialData.LapMiss[i] - nowTrialData.LapMiss[i - 1];
            _UpdateText(LapMissTMP[i - 1], iLapMiss == 0 ? "" : iLapMiss.ToString());
        }
        // 終了していないラップは 空白 で埋める
        for (int i = lap + 1; i <= numOfLaps; i++)
        {
            _UpdateText(LapTimeTMP[i - 1], "");
            _UpdateText(LapMissTMP[i - 1], "");
        }

    }

    // トライアル制御
    void OnCorrectKeyDown()
    {
        int idx = nowTrialData.TypedKeys;
        TextMeshProUGUI typedCharTMPUGUI
            = assignedCharTMPs[idx].GetComponent<TextMeshProUGUI>();
        typedCharTMPUGUI.color = typedCharColor;

        // nowTrialData の更新処理
        // ここでトータルタイムは入力しない（OnNormalButtonClick() で入力してあるため）
        nowTrialData.TypedKeys++;
        nowTrialData.CorrectKeyTime[nowTrialData.TypedKeys] = nowTrialData.TotalTime;

        // ラップ完了
        if (nowTrialData.TypedKeys % lapLength == 0) CompleteLap();
        // トライアル完了
        if (nowTrialData.TypedKeys == assignmentLength) CompleteTrial();
    }
    void OnIncorrectKeyDown()
    {
        // ここでトータルタイムは入力しない（OnNormalButtonClick() で入力してあるため）

        // nowTrialData にトータルミスの入力
        nowTrialData.TotalMiss++;
        // nowTrialData にラップミスの入力
        int lap = nowTrialData.TypedKeys / lapLength + 1;
        nowTrialData.LapMiss[lap] = nowTrialData.TotalMiss;

        if (nowTrialData.TotalMiss > nowTrialData.MissLimit)
        {
            Debug.Log($"ミスが多すぎます。最初からやり直してください。");
            CancelTrial();
        }
    }

    // イベントハンドラ
    // EventBus に渡して実行してもらう
    // OnBackButtonClick() では時間を測らない
    // キャンセル（Esc）時の TotalTime は キャンセル時基準ではなく、最後の正解打鍵orミス打鍵時をとるため。
    public void OnBackButtonClick()
    {
        if (gameState == GameState.TrialOn || gameState == GameState.Countdown) CancelTrial();
        //else if (isCompleted) ;
        else MySceneManager.ChangeSceneRequest("TitleScene");
    }
    public void OnGameStartButtonClick()
    {
        if (gameState == GameState.TrialOn || gameState == GameState.Countdown) return;
        else StartCountdown();
    }
    public void OnTweeterButtonClick()
    {
        // トライアル中にわざわざマウス触るぐらいだから、 Space じゃなくて直でボタンをクリックした場合は、トライアル中でも表示させてもいいかも
        // if (gameState == GameState.Completed ||  gameState == GameState.Canceled ||  gameState == GameState.Failed)
        tweeter.ToggleVisible();
    }
    // OnNormalKeyDown() で時間を計測
    // トライアル中の情報表示は Update() 内でタイマーを止めて雑に測ればいいが、ラップ・トライアル完了時の時間計測（キー押下にかかった時間）は正確にとる必要があるため。
    void OnNormalKeyDown(ushort charID)
    {
        // ゲーム外
        if (gameState == GameState.Completed || gameState == GameState.Canceled || gameState == GameState.Failed) {
            if (charID == 0) // default: Space
                OnTweeterButtonClick();
            else return;
        }
        else if (gameState == GameState.TrialOn)
        {
            nowTrialData.TotalTime = myStopwatch.ElapsedMilliseconds;
            // nowTrialData の更新
            nowTrialData.AllInputIDs.Add(charID);
            nowTrialData.AllInputTime.Add(nowTrialData.TotalTime);
            // 正打鍵
            if (nowTrialData.TaskCharIDs[nowTrialData.TypedKeys] == charID)
            {
                OnCorrectKeyDown();
            }
            // ゲーム中で誤打鍵
            else
            {
                OnIncorrectKeyDown();
            }
        }
        // if (Countdown || Waiting), do nothing
    }
    /// <summary>
    /// シフト状態を元に得られる文字を判定する処理は MyInputManager が行っているが、GameMainManager ではシフトの上下動を TrialData に書き込むため、このハンドラを要する。
    /// </summary>
    void ShiftKeyDownHandler()
    {
        if (gameState == GameState.TrialOn)
        {
            nowTrialData.AllInputIDs.Add(shiftdownInputID);
            nowTrialData.AllInputTime.Add(myStopwatch.ElapsedMilliseconds);
        }
    }
    /// <summary>
    /// シフト状態を元に得られる文字を判定する処理は MyInputManager が行っているが、GameMainManager ではシフトの上下動を TrialData に書き込むため、このハンドラを要する。
    /// </summary>
    void ShiftKeyUpHandler()
    {
        if (gameState == GameState.TrialOn)
        {
            nowTrialData.AllInputIDs.Add(shiftupInputID);
            nowTrialData.AllInputTime.Add(myStopwatch.ElapsedMilliseconds);
        }
    }

}
