using System; // Math
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography; // 乱数生成
using UnityEngine;

public class TrialSubManager : MonoBehaviour
{
    // ゲームモード変数
    public int gameMode;

    // 上司
    GameManager gameManager;

    // 部下となるシーン内のコントローラー達
    TrialObjectController trialObjectController;

    // 上司から得る変数
    int missLimit;
    KeyBind keyBind;
    KeyBindDicts keyBindDicts;

    // トライアル毎のデータ保管
    TrialData nowTrialData;

    // ストップウォッチ
    System.Diagnostics.Stopwatch myStopwatch;


    // 細かいゲーム変数（定数から設定する）

    // TrialData に渡すゲームモード変数と、それが示す意味
    int assignmentLength;
    int lapLength;
    int numOfLaps;

    void Awake()
    {
        // 上司を見つける
        gameManager = GetComponent<GameManager>();

        // 部下を見つける
        trialObjectController = GetComponent<TrialObjectController>();

        myStopwatch = new System.Diagnostics.Stopwatch();

        // 定数の読み込み
        assignmentLength = GlobalConsts.AssignmentLength[0];
        lapLength = GlobalConsts.LapLength[0];
        numOfLaps = GlobalConsts.NumOfLaps[0];

        // 画面の初期化
        trialObjectController.NotifyGameStateChanged(GameState.Waiting);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Completed, Canceled でも TrialInfo は必要だが、(キャンセル|完了) に描画しているからここでは不要
        GameState gameState = gameManager.GetGameState();
        if (gameState == GameState.Countdown)
        {
            long ms = myStopwatch.ElapsedMilliseconds;
            if (ms >= 3000)
            {
                trialObjectController.SetCountDownText("");
                StartTrial();
            }
            else if (ms >= 2000)
            {
                trialObjectController.SetCountDownText("1");
            }
            else if (ms >= 1000)
            {
                trialObjectController.SetCountDownText("2");
            }
            else
                trialObjectController.SetCountDownText("3");
        }
        else if (gameState == GameState.TrialOn) UpdateTrialInfo();
    }

    // 上司とのやり取り
    public void SetMissLimit(int misslim)
    {
        missLimit = misslim;
    }
    public void SetKeyBind(KeyBind kb)
    {
        keyBind = kb;
        keyBindDicts = new KeyBindDicts(kb);
    }

    /// <summary>
    /// TweeterManager が表示すべき TrialData を取得するためのメソッド
    /// </summary>
    /// <returns></returns>
    public System.Tuple<string, TrialData> GetTrialData()
    {
        GameState gameState = gameManager.GetGameState();
        if (gameState == GameState.TrialOn || gameState == GameState.Waiting)
            return new System.Tuple<string, TrialData>(gameState.ToString(), null);
        else
            return new System.Tuple<string, TrialData>(gameState.ToString(), nowTrialData);
    }

    public void StartCountdown()
    {
        // カウントダウンスタート
        myStopwatch.Restart();

        // 上司と部下に状態の変動を伝える
        trialObjectController.StartCountDown();
        trialObjectController.NotifyGameStateChanged(GameState.Countdown);

        // nowTrialData の初期化
        nowTrialData = new TrialData(gameMode, missLimit, keyBind);

        // 乱数を生成して nowTrialData にセット
        RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        byte[] randomBytes = new byte[4 * assignmentLength];
        rng.GetBytes(randomBytes);
        for (int i = 0; i < assignmentLength; i++)
        {
            // 乱数の精度を上げるために int で生成したあと、[0, NumofChars) に変換
            int intrnd = BitConverter.ToInt32(randomBytes, i * 4);
            ushort rnd_charID = (ushort)(Math.Abs(intrnd) % GlobalConsts.NumOfChars);

            // Null 文字をまたぐ毎に CharID を ++ する必要がある
            if (rnd_charID >= keyBind.NullKeyMap[0]) rnd_charID++;
            if (rnd_charID >= keyBind.NullKeyMap[1]) rnd_charID++;

            nowTrialData.TaskCharIDs[i] = rnd_charID;
        }

        // RNGCryptoServiceProvider は IDisposable インターフェイスを実装していて、使用が完了したら型を破棄しなきゃいけないらしい（公式ドキュメントより）
        rng.Dispose();
    }
    public void StartTrial()
    {
        // 上司と部下に状態の変動を伝える
        gameManager.TryToSetGameState(GameState.TrialOn);
        trialObjectController.NotifyGameStateChanged(GameState.TrialOn);

        // ObjectController に課題文字の TMP を準備させる
        trialObjectController.PrepareToStartTrial();

        // TrialData.AllInputIDs[0] に初期シフト状態を格納
        nowTrialData.AllInputIDs.Add(MyInputManager.GetShiftState());
        nowTrialData.AllInputTime.Add(0);

        for (int i = 0; i < assignmentLength; i++)
        {
            char c = keyBindDicts.ToChar_FromCharID(nowTrialData.TaskCharIDs[i]);
            trialObjectController.SetAssignedChar(i, c);
        }

        // 処理が終わってからゲーム開始＆ストップウォッチを開始
        myStopwatch.Restart();

        UpdateTrialInfo();
    }

    public void NotifyNormalKeyDown(ushort charID)
    {
        OnNormalKeyDown(charID);
    }

    // イベントハンドラ
    void OnNormalKeyDown(ushort charID)
    {
        nowTrialData.TotalTime = myStopwatch.ElapsedMilliseconds;
        // nowTrialData の更新
        nowTrialData.AllInputIDs.Add(charID);
        nowTrialData.AllInputTime.Add(nowTrialData.TotalTime);

        // 正打鍵か誤打鍵の判定をする（ただし、シフト上下動はどちらでもない）
        ushort shiftdown = GlobalConsts.CharID_ShiftDown;
        ushort shiftup = GlobalConsts.CharID_ShiftUp;
        bool isShiftToggling = (charID == shiftdown || charID == shiftup);
        if (isShiftToggling) return;
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
    void OnCorrectKeyDown()
    {
        int idx = nowTrialData.TypedKeys;
        trialObjectController.SetCharColorToTyped(idx);

        // nowTrialData の更新処理
        // ここでトータルタイムは入力しない（OnNormalKeyDown() で入力してあるため）
        nowTrialData.TypedKeys++;
        nowTrialData.CorrectKeyTime[nowTrialData.TypedKeys] = nowTrialData.TotalTime;

        // ラップ完了
        if (nowTrialData.TypedKeys % lapLength == 0) OnCompleteLap();
        // トライアル完了
        if (nowTrialData.TypedKeys == assignmentLength) OnCompleteTrial();
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
    void OnCompleteLap()
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
            nowTrialData.LapMiss[lap + 1] = nowTrialData.LapMiss[lap];
    }
    /// <summary>
    /// トライアル完了時に固有の処理を行う。トライアル終了時の共通処理は OnEndTrial()
    /// </summary>
    void OnCompleteTrial()
    {
        // ストップウォッチを止めるが、ここの時間はもはや関係無い（キー打鍵時に計測しているため）
        myStopwatch.Stop();

        // nowTrialData の更新
        nowTrialData.IsTerminated = false;

        gameManager.TryToSetGameState(GameState.Completed);

        // 終了時共通処理の呼び出し
        OnEndTrial();
    }
    /// <summary>
    /// トライアル中断時に固有の処理を行う。トライアル終了時の共通処理は OnEndTrial()
    /// ミス制限を超えると -TotalTime Failed, Esc -> Canceled
    /// </summary>
    public void CancelTrial()
    {
        // ストップウォッチを止めるが、ここの時間はもはや関係無い（OnNormalKeyDwn() で計測しているため）
        myStopwatch.Stop();

        // nowTrialData の更新
        nowTrialData.IsTerminated = true;

        // TrialData に最終ラップタイムをセット
        int lap = nowTrialData.TypedKeys / lapLength + 1;
        nowTrialData.SetLapTime(lap, nowTrialData.TotalTime);

        if (nowTrialData.TotalMiss <= nowTrialData.MissLimit)
            gameManager.TryToSetGameState(GameState.Canceled);
        else
            gameManager.TryToSetGameState(GameState.Failed);

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

        // UI の表示更新
        trialObjectController.NotifyGameStateChanged(gameManager.GetGameState());
    }

    // 表示制御

    // トライアル Information 描画メソッド
    // ここでは nowTrialData に記録済みのデータを描画するだけなので、更新は各イベント時に済ませる必要あり
    void UpdateTrialInfo()
    {
        GameState gameState = gameManager.GetGameState();
        Debug.Assert(gameState != GameState.Waiting);

        int lap = nowTrialData.TypedKeys / lapLength + 1;
        // トライアル完了時のみ lap > numOfLaps となってしまうので、それを避ける
        if (lap > numOfLaps) lap = numOfLaps;

        MilliSecond totalTime;
        // ゲーム中であった場合、リアルな経過時間を使って計算
        if (gameState == GameState.TrialOn) totalTime = myStopwatch.ElapsedMilliseconds;
        // ゲーム(完了|キャンセル)後であった場合、最後にキーを打鍵した時間を使って計算
        else totalTime = nowTrialData.TotalTime;
        int keys = nowTrialData.TypedKeys;
        double cps = (double)(keys * 1000) / totalTime;

        // トータルタイムの表示
        trialObjectController.SetTotalTime(totalTime);
        // トータルミスの表示
        trialObjectController.SetTotalMiss(nowTrialData.TotalMiss);
        // トータル CPS の表示
        trialObjectController.SetCPS(cps);
        // ラップタイムの表示
        // 1 ~ lap-1 までは nowTrialData に書き込み済み（1-indexed 注意）
        MilliSecond[] singleLapTime = new MilliSecond[lap + 1];
        singleLapTime[0] = 0;
        for (int i = 1; i <= lap - 1; i++)
        {
            singleLapTime[i] = nowTrialData.GetSingleLapTime(i);
        }
        // 現在ラップは time を使う
        // time : ゲーム中ならリアルな打鍵時間、terminated なら最終キー打鍵時間が入っている
        singleLapTime[lap] = totalTime - nowTrialData.GetLapTime(lap - 1);
        // LapTimeTMP [] は 0-indexed であることにも注意 -> [(i|lap)-1] でアクセス
        for (int i = 1; i <= lap; i++)
        {
            trialObjectController.SetLapTime(i, singleLapTime[i]);
        }
        // ラップミスの表示
        for (int i = 1; i <= lap; i++)
        {
            int iLapMiss = nowTrialData.LapMiss[i] - nowTrialData.LapMiss[i - 1];
            trialObjectController.SetLapMiss(i, iLapMiss);
        }
        // 終了していないラップは 0 を渡す（controller が空白で埋める）
        for (int i = lap + 1; i <= numOfLaps; i++)
        {
            trialObjectController.SetLapTime(i, 0);
            trialObjectController.SetLapMiss(i, 0);
        }

    }
}
