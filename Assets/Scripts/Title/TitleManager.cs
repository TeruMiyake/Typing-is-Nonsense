using UnityEngine;
using UnityEngine.EventSystems;

public class TitleManager : MonoBehaviour
{

    private void Awake()
    {
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
        Debug.Log("normalkeydown");
        char c = MyInputManager.ToChar_FromCharID(charID);
        if (c == 'n' || c == 'N') OnGameMainStartButtonClick();
        if (c == 'e' || c == 'E') OnKeybindingStartButtonClick();
        if (c == 'j' || c == 'J') OnJumpToWebsiteButtonClick();
    }
}
