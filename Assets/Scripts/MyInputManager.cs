using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_STANDALONE_WIN
/// https://github.com/Elringus/UnityRawInput
using UnityRawInput;
#endif

/// <summary>
///  プラットフォーム依存のキーボード入力受け取りを行い、EventBus に通知を発行する
///  その後、他のクラスから、通知がされた際に実行してほしいメソッドを EventBus にデリゲートとして登録する
/// </summary>
public class MyInputManager : MonoBehaviour
{
    // シフト管理。シフトキーは 2 つあるので、bool でなく int
    // メソッド内変数（入力受け取り後の確定した shifted）と区別するため、フィールドとしての shifted には _ 付与
    private static int _shifted = 0;

    // ScriptableObject
    [SerializeField]
    KeyBind keyBind;
    KeyBindDicts keyBindDicts;


    void Awake()
    {
        keyBind = new KeyBind();
        keyBind.LoadFromJson(0);
        keyBindDicts = new KeyBindDicts(keyBind);

#if UNITY_STANDALONE_WIN
        Debug.Log("standalone win");
        var workInBackground = false;
        RawKeyInput.Start(workInBackground);

        RawKeyInput.OnKeyDown += RawKeyDownHandlerForWin;
        RawKeyInput.OnKeyDown += KeyDownHandlerForWin;
        RawKeyInput.OnKeyUp += KeyUpHandlerForWin;
#endif

        EventBus.Instance.SubscribeKeyBindChanged(KeyBindChangedHandler);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDisable()
    {
#if UNITY_STANDALONE_WIN
        RawKeyInput.OnKeyDown -= RawKeyDownHandlerForWin;
        RawKeyInput.OnKeyDown -= KeyDownHandlerForWin;
        RawKeyInput.OnKeyUp -= KeyUpHandlerForWin;

        RawKeyInput.Stop(); // 入れないと Unity Editor が落ちる
#endif

        EventBus.Instance.UnsubscribeKeyBindChanged(KeyBindChangedHandler);
    }

    // プラットフォーム依存のキー入力イベントハンドラ
    // プラットフォーム依存のキーコードを、ユーザ固有の KeyID に変換してプラットフォーム非依存処理を呼ぶ
#if UNITY_STANDALONE_WIN
    private void RawKeyDownHandlerForWin(RawKey key)
    {
        EventBus.Instance.NotifyRawKeyDown(key);
    }
    private void KeyDownHandlerForWin(RawKey key)
    {
        if (!keyBindDicts.dictToKeyID_FromRawKey.ContainsKey(key)) {
            Debug.Log($"No KeyID for RawKey : {key.ToString()}");
            return;
        }
        else OnKeyDown(keyBindDicts.ToKeyID_FromRawKey(key));
    }
    private void KeyUpHandlerForWin(RawKey key)
    {
        if (keyBindDicts.dictToKeyID_FromRawKey.ContainsKey(key))
        {
            ushort rshift = GlobalConsts.KeyID_RShift;
            ushort lshift = GlobalConsts.KeyID_LShift;
            ushort keyID = keyBindDicts.ToKeyID_FromRawKey(key);
            if (keyID == rshift || keyID == lshift) OnShiftKeyUp();
        }
    }
#endif

    public static ushort GetShiftState()
    {
        return (ushort)_shifted;
    }

    // EventBus にイベント発生を通知するメソッド群
    /// KeyID が特殊文字 100 ~ であれば、それぞれのメソッドを呼ぶ
    ///         シフト 0 | 1 であれば、OnShiftKeyDown() を実行
    ///         通常文字 2 ~ 50 であれば、シフト判別後 OnNormalKeyDown() を実行
    ///         ※ ただし、CharID が割り振られていない 2 キーの場合は、処理を実行しない
    private void OnKeyDown(ushort keyID)
    {
        ushort rshift = GlobalConsts.KeyID_RShift;
        ushort lshift = GlobalConsts.KeyID_LShift;
        ushort space = GlobalConsts.KeyID_Space;
        ushort ret = GlobalConsts.KeyID_Return;
        ushort esc = GlobalConsts.KeyID_Esc;
        if (keyID == rshift || keyID == lshift) OnShiftKeyDown(); // RawKey:16
        else if (keyID == ret) OnReturnKeyDown(); // RawKey:13
        else if (keyID == esc) OnEscKeyDown(); // RawKey:27
        // KeyID:2 (Preset:Space) にはシフト関係無しなので、シフト処理に移行させない
        else if (keyID == space) OnNormalKeyDown(0); // CharID of Space: 0
        else
        {
            ushort retkey = keyID;
            bool shifted = false;
            if (_shifted > 0) shifted = true;
            if (shifted) retkey += 48;

            if (!keyBindDicts.dictToCharID_FromKeyID.ContainsKey(retkey))
                Debug.Log($"KeyID {retkey} に CharID がアサインされていません。");
            else OnNormalKeyDown(keyBindDicts.ToCharID_FromKeyID(retkey));
        }

    }
    private void OnNormalKeyDown(ushort charID)
    {
        EventBus.Instance.NotifyNormalKeyDown(charID);
    }
    /// <summary>
    /// リファクタリングによって ShiftUp/Down も NormalKeyDown(charID) で渡されるようになったため、NotifyShiftKeyDown() は不必要となったはずだが、とりあえず当面残しておく
    /// </summary>
    private void OnShiftKeyDown()
    {
        _shifted++;
        if (_shifted > 2) _shifted = 2;
        EventBus.Instance.NotifyNormalKeyDown(GlobalConsts.CharID_ShiftDown); // 固定
        EventBus.Instance.NotifyShiftKeyDown();
    }
    /// <summary>
    /// リファクタリングによって ShiftUp/Down も NormalKeyDown(charID) で渡されるようになったため、NotifyShiftKeyUp() は不必要となったはずだが、とりあえず当面残しておく
    /// </summary>
    private void OnShiftKeyUp()
    {
        _shifted--;
        if (_shifted < 0) _shifted = 0;
        EventBus.Instance.NotifyNormalKeyDown(GlobalConsts.CharID_ShiftUp); // 固定
        EventBus.Instance.NotifyShiftKeyUp();
    }
    private void OnReturnKeyDown()
    {
        EventBus.Instance.NotifyReturnKeyDown();
    }
    private void OnEscKeyDown()
    {
        EventBus.Instance.NotifyEscKeyDown();
    }

    // イベントハンドラ
    private void KeyBindChangedHandler()
    {
        keyBind.LoadFromJson(0);
        keyBindDicts = new KeyBindDicts(keyBind);
    }


}
