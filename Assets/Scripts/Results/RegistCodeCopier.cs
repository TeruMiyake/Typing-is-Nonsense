using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

/// <summary>
/// RegistrationCode をコピーするためのスクリプト
/// static RegistCodeButtonClickedHandler(string) を呼び出して使う
/// </summary>
public class RegistCodeCopier : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// 登録コードボタンがクリックされた場合のハンドラ
    /// ファイル内容をクリップボードにコピーできれば true、失敗すれば false を返す。
    /// </summary>
    /// <param name="registCodePath"></param>
    /// <returns></returns>
    public static bool RegistCodeButtonClickedHandler(string registCodePath)
    {
        if (File.Exists(registCodePath))
        {
            string registCode = "";
            using (StreamReader reader = new StreamReader(registCodePath))
            {
                registCode = reader.ReadToEnd();
            }
            GUIUtility.systemCopyBuffer = registCode;
            return true;
        }
        else
        {
            return false;
        }
    }
}
