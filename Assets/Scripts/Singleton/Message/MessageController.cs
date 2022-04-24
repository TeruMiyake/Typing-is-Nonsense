using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// いまいちよく分からずに使っているので、いつかちゃんとする
/// </summary>
public class MessageController : MonoBehaviour
{
    private static MessageController instance;
    public static MessageController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (MessageController)FindObjectOfType(typeof(MessageController));
            }
            return instance;
        }
    }

    public GameObject Usability;
    public GameObject MessagePrefab;

    private void Awake()
    {
        if (instance == null)
        {
            Debug.Log("in");
            instance = this;
        }
    }

    /// <summary>
    /// 画面にメッセージを表示する
    /// </summary>
    /// <param name="messageText"></param>
    /// <param name="timer">float。デフォルト： 1.5 秒</param>
    public void ShowMessage(string messageText, float timer=1.5f)
    {
        Debug.Log("clicked test");
        GameObject canvas = GameObject.Find("Canvas");
        GameObject message = Instantiate(MessagePrefab, canvas.transform);
        message.GetComponentInChildren<TextMeshProUGUI>().text = messageText;
        Destroy(message, timer);
    }
}
