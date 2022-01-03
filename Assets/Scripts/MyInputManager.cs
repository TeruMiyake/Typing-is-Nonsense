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
    [System.Obsolete("初期化状態によってバグが発生する可能性あり。要検証")]
    private int _shifted = 0;

    // KeyID と CharID（Key にアサインされた文字）間の変換
    private static Dictionary<ushort, ushort> dictToKeyID_FromCharID = new Dictionary<ushort, ushort>();
    private static Dictionary<ushort, ushort> dictToCharID_FromKeyID = new Dictionary<ushort, ushort>();

    // Char と CharID 間の変換
    private static Dictionary<ushort, char> dictToChar_FromCharID = new Dictionary<ushort, char>();
    private static Dictionary<char, ushort> dictToCharID_FromChar = new Dictionary<char, ushort>();

#if UNITY_STANDALONE_WIN
    private static Dictionary<RawKey, ushort> dictToKeyID_FromRawKey = new Dictionary<RawKey, ushort>();
#endif


    void Awake()
    {
#if UNITY_STANDALONE_WIN
        Debug.Log("standalone win");
        var workInBackground = false;
        RawKeyInput.Start(workInBackground);

        RawKeyInput.OnKeyDown += KeyDownHandlerForWin;
        RawKeyInput.OnKeyUp += KeyUpHandlerForWin;

        // キーバインド機能をつけたら削除する予定の、デフォルト Rawkey:KeyID Dictionary 設定
        dictToKeyID_FromRawKey.Add(RawKey.Space, 0);
        for (int i = 0; i <= 25; i++) dictToKeyID_FromRawKey.Add(RawKey.A + (ushort)i, (ushort)(i + 1));
        dictToKeyID_FromRawKey.Add((RawKey)0x31, 27);
        dictToKeyID_FromRawKey.Add((RawKey)0x32, 28);
        dictToKeyID_FromRawKey.Add((RawKey)0x33, 29);
        dictToKeyID_FromRawKey.Add((RawKey)0x34, 30);
        dictToKeyID_FromRawKey.Add((RawKey)0x35, 31);
        dictToKeyID_FromRawKey.Add((RawKey)0x36, 32);
        dictToKeyID_FromRawKey.Add((RawKey)0x37, 33);
        dictToKeyID_FromRawKey.Add((RawKey)0x38, 34);
        dictToKeyID_FromRawKey.Add((RawKey)0x39, 35);
        dictToKeyID_FromRawKey.Add((RawKey)0x30, 36);
        dictToKeyID_FromRawKey.Add(RawKey.OEMMinus, 37);
        dictToKeyID_FromRawKey.Add(RawKey.OEM7, 38);
        dictToKeyID_FromRawKey.Add(RawKey.OEM5, 39);
        dictToKeyID_FromRawKey.Add(RawKey.OEM3, 40);
        dictToKeyID_FromRawKey.Add(RawKey.OEM4, 41);
        dictToKeyID_FromRawKey.Add(RawKey.OEMPlus, 42);
        dictToKeyID_FromRawKey.Add(RawKey.OEM1, 43);
        dictToKeyID_FromRawKey.Add(RawKey.OEM6, 44);
        dictToKeyID_FromRawKey.Add(RawKey.OEMComma, 45);
        dictToKeyID_FromRawKey.Add(RawKey.OEMPeriod, 46);
        dictToKeyID_FromRawKey.Add(RawKey.OEM2, 47);
        dictToKeyID_FromRawKey.Add(RawKey.OEM102, 48);
        dictToKeyID_FromRawKey.Add(RawKey.Shift, 100);
        dictToKeyID_FromRawKey.Add(RawKey.Return, 101);
        dictToKeyID_FromRawKey.Add(RawKey.Escape, 102);
#endif

        // テスト用、とりあえず KeyID と CharID と Char に適当な対応をつける
        string defaultKeyCharMap = " abcdefghijklmnopqrstuvwxyz1234567890-^\0@[;:],./\\ABCDEFGHIJKLMNOPQRSTUVWXYZ!\"#$%&'()\0=~|`{+*}<>?_";
        ushort idx = 0;
        ushort charidx = 0;
        foreach (char c in defaultKeyCharMap)
        {
            if (c == '\0')
            {
                idx++;
            }
            else
            {
                dictToKeyID_FromCharID[charidx] = idx;
                dictToCharID_FromKeyID[idx] = charidx;
                dictToChar_FromCharID[charidx] = c;
                dictToCharID_FromChar[c] = charidx;
                idx++;
                charidx++;
            }

        }
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
        RawKeyInput.OnKeyDown -= KeyDownHandlerForWin;
        RawKeyInput.OnKeyUp -= KeyUpHandlerForWin;

        RawKeyInput.Stop(); // 入れないと Unity Editor が落ちる
#endif
    }

    // プラットフォーム依存のキー入力イベントハンドラ
    // プラットフォーム依存のキーコードを、ユーザ固有の KeyID に変換してプラットフォーム非依存処理を呼ぶ
#if UNITY_STANDALONE_WIN
    private void KeyDownHandlerForWin(RawKey key)
    {
        if (!dictToKeyID_FromRawKey.ContainsKey(key)) {
            Debug.Log($"No KeyID for RawKey : {key.ToString()}");
            return;
        }
        else OnKeyDown(ToKeyID_FromRawKey(key));
    }
    private void KeyUpHandlerForWin(RawKey key)
    {
        if (key.ToString() == "Shift") OnShiftKeyUp();
    }
    private ushort ToKeyID_FromRawKey(RawKey key)
    {
        return dictToKeyID_FromRawKey[key];
    }
#endif

    // ここから、プラットフォームに依存しない処理
    // この時点までで、入力は全て KeyID に変換されている

    // KeyID と CharID（Key にアサインされた文字）間の変換
    public static ushort ToKeyID_FromCharID(ushort charID)
    {
        return dictToKeyID_FromCharID[charID];
    }
    public static ushort ToCharID_FromKeyID(ushort keyID)
    {
        return dictToCharID_FromKeyID[keyID];
    }

    // Char と CharID 間の変換
    public static char ToChar_FromCharID(ushort charID)
    {
        return dictToChar_FromCharID[charID];
    }
    public static ushort ToCharID_FromChar(char c)
    {
        return dictToCharID_FromChar[c];
    }


    // イベントハンドラ

    /// KeyID が特殊文字 100 ~ であれば、それぞれのイベントを実行
    ///         通常文字 0 ~ 96 であれば、OnNormalKeyDown() を実行
    ///         ※ ただし、CharID が割り振られていない 2 キーの場合は、処理を実行しない
    private void OnKeyDown(ushort keyID)
    {
        ushort retkey = keyID;
        // Space にはシフト関係無しなので、シフト処理に移行させない
        if (retkey == 0) OnNormalKeyDown(0);
        else if (retkey == 100) OnShiftKeyDown();
        else if (retkey == 101) OnReturnKeyDown();
        else if (retkey == 102) OnEscKeyDown();
        else
        {
            bool shifted = false;
            if (_shifted > 0) shifted = true;
            if (shifted) retkey += GameMainManager.NumOfUniqueChars;

            if (!dictToCharID_FromKeyID.ContainsKey(retkey))
                Debug.Log($"KeyID {retkey} に CharID がアサインされていません。");
            else OnNormalKeyDown(ToCharID_FromKeyID(retkey));
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
    }
    private void OnShiftKeyUp()
    {
        _shifted--;
    }
    private void OnReturnKeyDown()
    {
        EventBus.Instance.NotifyReturnKeyDown();
    }
    private void OnEscKeyDown()
    {
        EventBus.Instance.NotifyEscKeyDown();
    }

    /// キーバインド用
#if UNITY_STANDALONE_WIN
    private void OnRawKeyDown(RawKey key) {
        EventBus.Instance.NotifyRawKeyDown(key);
    }
#endif

}
