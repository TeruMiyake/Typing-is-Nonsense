using UnityEngine;

public class ResultsManager : MonoBehaviour
{
    // 未実装
    int gameMode = 0;

    // 状態変数
    bool isDisplayingResult = false;

    // ScriptableObject
    [SerializeField]
    KeyBind keyBind;
    KeyBindDicts keyBindDicts;

    // 部下となるシーン内のサブマネジャー達
    ResultsListSubManager resultsListSubManager;

    void Awake()
    {
        keyBind = new KeyBind();
        keyBind.LoadFromJson(0);
        keyBindDicts = new KeyBindDicts(keyBind);

        // 部下を見つける
        resultsListSubManager = GetComponent<ResultsListSubManager>();

        // 制御すべきオブジェクトなどの取得と初期化


        // EventBus にキー押下イベント発生時のメソッド実行を依頼
        EventBus.Instance.SubscribeEscKeyDown(BackButtonClickHandler);
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
        EventBus.Instance.UnsubscribeEscKeyDown(BackButtonClickHandler);
    }

    // 部下が gameMode をチェックするメソッド
    // そのうち、イベントを使った実装に置き換えたい
    public int GetGameMode()
    {
        return gameMode;
    }

    // イベントハンドラ
    // EventBus もしくは UI 内のイベントに渡して実行してもらう
    public void BackButtonClickHandler()
    {
        if (isDisplayingResult) return;
        else MySceneManager.ChangeSceneRequest("TitleScene");
    }


}
