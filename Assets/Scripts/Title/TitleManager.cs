using UnityEngine;
using UnityEngine.EventSystems;

public class TitleManager : MonoBehaviour
{
    // ScriptableObject 
    [SerializeField]
    KeyBind keyBind = null;
    KeyBindDicts dicts;
    
    private void Awake()
    {
        dicts = new KeyBindDicts(keyBind);

        // EventBus にキー押下イベント発生時のメソッド実行を依頼 *Unsubscribe 忘れず！
        EventBus.Instance.SubscribeNormalKeyDown(OnNormalKeyDown);
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnDestroy()
    {
        EventBus.Instance.UnsubscribeNormalKeyDown(OnNormalKeyDown);
    }

    void JumpToWebsite()
    {
        Application.OpenURL("https://terum.jp/tin/");
    }
    void ExitGame()
    {
        Application.Quit();
    }

    // イベントハンドラ
    public void OnKeybindingStartButtonClick()
    {
        MySceneManager.ChangeSceneRequest("KeybindingScene");
    }
    public void OnGameMainStartButtonClick()
    {
        MySceneManager.ChangeSceneRequest("GameMainScene");
    }
    public void OnJumpToWebsiteButtonClick()
    {
        JumpToWebsite();
    }
    public void OnExitButtonClick()
    {
        Debug.Log("Bye...");
        ExitGame();
    }
    [System.Obsolete("バインド機能つけたら、文字の判別方法を変更要")]
    public void OnNormalKeyDown(ushort charID)
    {
        if (charID == 14) OnGameMainStartButtonClick(); // T
        if (charID == 5) OnKeybindingStartButtonClick(); // E
        if (charID == 10) OnJumpToWebsiteButtonClick(); // J
    }
}
