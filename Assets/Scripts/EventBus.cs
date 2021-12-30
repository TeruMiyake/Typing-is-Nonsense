using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_STANDALONE_WIN
/// https://github.com/Elringus/UnityRawInput
using UnityRawInput;
#endif

/// <summary>
/// クラス EventBus
/// 各クラスからリアルタイムにイベント発生通知を受け取り、登録されたデリゲートを実行する
///  https://indie-du.com/entry/2017/05/26/130000
/// </summary>
public class EventBus
{
    // Singleton
    private static EventBus s_instance;
    public static EventBus Instance
    {
        get
        {
            if (s_instance == null)
            {
                s_instance = new EventBus();
            }
            return s_instance;
        }
    }
    private EventBus()
    {

    }

    // 他のクラスが「自分が通知を受け取った時に実行してほしいコールバック関数」の登録をするためのデリゲート
    public delegate void OnNormalKeyDown(ushort keyID);
    public delegate void OnReturnKeyDown();
    public delegate void OnEscKeyDown();
    private event OnNormalKeyDown _onNormalKeyDown;
    private event OnReturnKeyDown _onReturnKeyDown;
    private event OnEscKeyDown _onEscKeyDown;
#if UNITY_STANDALONE_WIN
    public delegate void OnRawKeyDown(RawKey key);
    private event OnRawKeyDown _onRawKeyDown;
#endif

    // 他のクラスが通知受け取り（＝EventBusへのデリゲート登録）をするためのメソッド
    public void SubscribeNormalKeyDown(OnNormalKeyDown onNormalKeyDown)
    {
        _onNormalKeyDown += onNormalKeyDown;
    }
    public void SubscribeReturnKeyDown(OnReturnKeyDown onReturnKeyDown)
    {
        _onReturnKeyDown += onReturnKeyDown;
    }
    public void SubscribeEscKeyDown(OnEscKeyDown onEscKeyDown)
    {
        _onEscKeyDown += onEscKeyDown;
    }
#if UNITY_STANDALONE_WIN
    public void SubscribeRawKeyDown(OnRawKeyDown onRawKeyDown)
    {
        _onRawKeyDown += onRawKeyDown;
    }
#endif

    // 他のクラスが通知受け取り解除をするためのメソッド
    public void UnsubscribeNormalKeyDown(OnNormalKeyDown onNormalKeyDown)
    {
        _onNormalKeyDown -= onNormalKeyDown;
    }
    public void UnsubscribeReturnKeyDown(OnReturnKeyDown onReturnKeyDown)
    {
        _onReturnKeyDown -= onReturnKeyDown;
    }
    public void UnsubscribeEscKeyDown(OnEscKeyDown onEscKeyDown)
    {
        _onEscKeyDown -= onEscKeyDown;
    }
#if UNITY_STANDALONE_WIN
    public void UnsubscribeRawKeyDown(OnRawKeyDown onRawKeyDown)
    {
        _onRawKeyDown -= onRawKeyDown;
    }
#endif

    // 他のクラスから、EventBus に通知を依頼するメソッド
    // char c を渡すのは冗長（MyInputManager.CharMap があるから）だが、便利そうなので残しておく
    public void NotifyNormalKeyDown(ushort keyID)
    {
        if (_onNormalKeyDown != null) _onNormalKeyDown(keyID);
    }
    public void NotifyReturnKeyDown()
    {
        if (_onReturnKeyDown != null) _onReturnKeyDown();
    }
    public void NotifyEscKeyDown()
    {
        if (_onEscKeyDown != null) _onEscKeyDown();
    }

#if UNITY_STANDALONE_WIN
    public void NotifyRawKeyDown(RawKey key)
    {
        if (_onRawKeyDown != null) _onRawKeyDown(key);
    }
#endif
}
