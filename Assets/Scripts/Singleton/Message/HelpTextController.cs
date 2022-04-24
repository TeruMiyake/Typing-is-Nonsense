using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpTextController : MonoBehaviour
{
    [TextArea]
    public string HelpText;
    public float Timer = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// （暫定版。そのうちホバーにしたい）
    /// ShowMessage() を利用して、ホバー時にヘルプメッセージを表示する
    /// </summary>
    /// <param name="text"></param>
    /// <param name="timer"></param>
    public void ShowHelpText()
    {
        MessageController.Instance.ShowMessage(HelpText, Timer);
    }
}
