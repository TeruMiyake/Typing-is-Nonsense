using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class SimpleConfigurerManager : MonoBehaviour
{
    // ゲームモード変数
    public int GameMode;
    
    // 自分が管理するオブジェクト
    public GameObject missLimitInput;

    int missLimit;
    int defaultMissLimit;
    int maxMissLimit;

    void Awake()
    {
        // 定数の読み込み
        defaultMissLimit = GlobalConsts.DefaultMissLimit[GameMode];
        maxMissLimit = GlobalConsts.MaxMissLimit[GameMode];
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void SetMissLimitTMP(int misslim)
    {
        missLimitInput.GetComponent<TMP_InputField>().text = misslim.ToString();
    }
    string GetMissLimitTMP()
    {
        return missLimitInput.GetComponent<TMP_InputField>().text;
    }
    public void OnMissLimitEndEdit()
    {
        if (GetMissLimitTMP() == null)
            missLimit = defaultMissLimit;
        else
        {
            missLimit = int.Parse(GetMissLimitTMP());
        }
        if (missLimit > maxMissLimit) missLimit = maxMissLimit;
        else if (missLimit < 0) missLimit = 0;

        SetMissLimitTMP(missLimit);
        PlayerPrefs.SetInt("MissLimit", missLimit);
    }

}
