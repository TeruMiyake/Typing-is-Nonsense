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
    KeyBindDicts dicts;


    void Awake()
    {
        keyBind = new KeyBind();
        keyBind.LoadFromJson(0);
        dicts = new KeyBindDicts(keyBind);

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
        if (!dicts.dictToKeyID_FromRawKey.ContainsKey(key)) {
            Debug.Log($"No KeyID for RawKey : {key.ToString()}");
            return;
        }
        else OnKeyDown(dicts.ToKeyID_FromRawKey(key));
    }
    private void KeyUpHandlerForWin(RawKey key)
    {
        if (dicts.dictToKeyID_FromRawKey.ContainsKey(key))
        {
            ushort keyID = dicts.ToKeyID_FromRawKey(key);
            if (keyID == 0 || keyID == 1) OnShiftKeyUp();
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
        ushort retkey = keyID;
        if (retkey == 0 || retkey == 1) OnShiftKeyDown(); // RawKey:16
        else if (retkey == 100) OnReturnKeyDown(); // RawKey:13
        else if (retkey == 101) OnEscKeyDown(); // RawKey:27
        // KeyID:2 (Preset:Space) にはシフト関係無しなので、シフト処理に移行させない
        else if (retkey == 2) OnNormalKeyDown(0); // CharID of Space: 0
        else
        {
            bool shifted = false;
            if (_shifted > 0) shifted = true;
            if (shifted) retkey += 48;

            if (!dicts.dictToCharID_FromKeyID.ContainsKey(retkey))
                Debug.Log($"KeyID {retkey} に CharID がアサインされていません。");
            else OnNormalKeyDown(dicts.ToCharID_FromKeyID(retkey));
        }

    }
    /// CharID のアサインされていないキー押下は無視すればいいので、
    private void OnNormalKeyDown(ushort charID)
    {
        EventBus.Instance.NotifyNormalKeyDown(charID);
    }
    private void OnShiftKeyDown()
    {
        _shifted++;
        if (_shifted > 2) _shifted = 2;
        EventBus.Instance.NotifyShiftKeyDown();
    }
    private void OnShiftKeyUp()
    {
        _shifted--;
        if (_shifted < 0) _shifted = 0;
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
        dicts = new KeyBindDicts(keyBind);
    }


}
