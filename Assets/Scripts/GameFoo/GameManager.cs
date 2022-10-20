using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using TMPro; // スクリプトから TextMeshPro の変更

public class GameManager : MonoBehaviour
{
    GameState gameState = GameState.Waiting;

    // ScriptableObject
    [SerializeField]
    KeyBind keyBind;
    KeyBindDicts keyBindDicts;

    // 部下となるシーン内のサブマネジャー達
    TweeterManager tweeter;
    TrialSubManager trialSubManager;
    SimpleConfigurerManager simpleConfigurerManager;

    // 簡易設定オブジェクト
    public GameObject MissLimitInput;

    // ここから基本設定

    // Config（ミス制限など）
    Config config = new Config();

    void Awake()
    {
        keyBind = new KeyBind();
        keyBind.LoadFromJson(0);
        keyBindDicts = new KeyBindDicts(keyBind);

        // 部下を見つける
        tweeter = GameObject.Find("Tweeter").GetComponent<TweeterManager>();
        trialSubManager = GetComponent<TrialSubManager>();

        // 制御すべきオブジェクトなどの取得と初期化

        // 表示制御
        tweeter.SetVisible(false);

        // Config の読み込みと表示への反映
        config.Load();
        GameObject.Find("MissLimiterInput").GetComponent<TMP_InputField>().text = config.MissLimit.ToString();

        // EventBus にキー押下イベント発生時のメソッド実行を依頼
        EventBus.Instance.SubscribeNormalKeyDown(OnNormalKeyDown);
        EventBus.Instance.SubscribeReturnKeyDown(OnGameStartButtonClick);
        EventBus.Instance.SubscribeEscKeyDown(OnBackButtonClick);
    }
    // Start is called before the first frame update
    void Start()
    {
        // 部下の初期設定
        trialSubManager.SetKeyBind(keyBind);
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

    // 部下とのやり取り
    public GameState GetGameState()
    {
        return gameState;
    }
    public void TryToSetGameState(GameState state)
    {
        bool canEditMissLimit = (state != GameState.Countdown && state != GameState.TrialOn);
        if (canEditMissLimit)
        {
            // 簡易設定を再開
            MissLimitInput.GetComponent<TMP_InputField>().readOnly = false;
        }
        else
        {
            // 簡易設定を停止
            MissLimitInput.GetComponent<TMP_InputField>().readOnly = true;
        }
        // 後々、ここから GameStateManager を呼ぶようにする？
        // 別のもっと良い呼び方があるかもだが
        gameState = state;
    }


    // 状態制御メソッド
    void StartCountdown()
    {
        // 簡易設定中は動かせない ※シフト絡みのバグがここに眠ってるかも
        if (MissLimitInput.GetComponent<TMP_InputField>().isFocused) return;

        TryToSetGameState(GameState.Countdown);

        // 画面表示の初期化
        tweeter.SetVisible(false);

        // 設定の再読み込み（簡易設定で更新している場合があるので、ここで再度読み込む）
        config.Load();
        trialSubManager.SetMissLimit(config.MissLimit);

        trialSubManager.StartCountdown();
    }
    void CancelTrial()
    {
        trialSubManager.CancelTrial();
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
            trialSubManager.NotifyNormalKeyDown(charID);
        }
        // if (Countdown || Waiting), do nothing
    }

}
