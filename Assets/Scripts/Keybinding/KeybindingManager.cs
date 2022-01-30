using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

#if UNITY_STANDALONE_WIN
/// https://github.com/Elringus/UnityRawInput
using UnityRawInput;
#endif

public class KeybindingManager : MonoBehaviour
{
    // プレハブ設定、及びプレハブ格納配列
    public GameObject KeyBinderPrefab; 
    GameObject[] keyBinder = new GameObject[104];

    public GameObject CharBinderPrefab;
    GameObject[] charBinder = new GameObject[97];

    // 実際にこのシーンで入力を得るためのキーバインドデータ
    [SerializeField]
    KeyBind keyBind;
    KeyBindDicts dicts;

    // 設定中のキーバインドデータ
    [SerializeField]
    KeyBind nowBindingKeyBind;

    // 状態変数
    // キー入力の受付待ち状態
    // -1 : notListening, 0 ~ 50 : Listening KeyID 0~50 
    int nowListening = -1;

    private void Awake()
    {
        // 操作用キーバインドの読み込み
        keyBind = new KeyBind();
        keyBind.LoadFromJson(0);
        dicts = new KeyBindDicts(keyBind);

        // 設定用キーバインドの生成
        nowBindingKeyBind = keyBind;

        // エラーメッセージの初期化
        GameObject.Find("ErrorTMP").GetComponent<TextMeshProUGUI>().text = "";

        // イベントハンドラの登録（引数つきはインスペクタから登録できないため）
        GameObject.Find("UseThisButton").GetComponent<Button>().onClick.AddListener(
            () =>
            {
                if (ValidateNowBinding())
                {
                    nowBindingKeyBind.SaveToJson(0);
                    keyBind = nowBindingKeyBind;
                    // MyKeyBind0.json の変更を EventBus に通知 -> MyInputManager が検知
                    EventBus.Instance.NotifyKeyBindChanged();
                }
            });
        GameObject.Find("Save1Button").GetComponent<Button>().onClick.AddListener(() => OnSaveButtonClick(1));
        GameObject.Find("Save2Button").GetComponent<Button>().onClick.AddListener(() => OnSaveButtonClick(2));
        GameObject.Find("Save3Button").GetComponent<Button>().onClick.AddListener(() => OnSaveButtonClick(3));
        GameObject.Find("Load1Button").GetComponent<Button>().onClick.AddListener(() => OnLoadButtonClick(1));
        GameObject.Find("Load2Button").GetComponent<Button>().onClick.AddListener(() => OnLoadButtonClick(2));
        GameObject.Find("Load3Button").GetComponent<Button>().onClick.AddListener(() => OnLoadButtonClick(3));

        // KeyID : 000 ~ 050
        for (int i = 0; i < 51; i++) InstantiateKeyBinder(i, 190, -2 - i * 40);

        // CharBinderID : 00 ~ 48 (Unshifted)
        for (int i = 0; i < 49; i++) InstantiateCharBinder(i, 0, -82 - i * 40);

        // CharBinderID : 49 ~ 96 (Shifted)
        for (int i = 49; i < 97; i++)
        {
            InstantiateCharBinder(i, 380, -122 - (i-49) * 40);
            // InputField.colors.normalColor に直接 Color を代入できないため、まず ColorBlock をまるごと作って渡す
            ColorBlock cb = charBinder[i].GetComponent<RectTransform>().Find("CharInputField").GetComponent<InputField>().colors;
            cb.normalColor = new Color(0.85f, 0.7f, 0.5f, 1f);
            charBinder[i].GetComponent<RectTransform>().Find("CharInputField").GetComponent<InputField>().colors = cb;
        }

        // EventBus に登録
        EventBus.Instance.SubscribeRawKeyDown(RawKeyDownEventHandler);
    }

    // Start is called before the first frame update
    void Start()
    {
        // 描画の更新
        UpdateDisplay();
    }

    // Update is called once per frame
    void Update()
    {
    }
    void OnDestroy()
    {
        // EventBus から脱退
        EventBus.Instance.UnsubscribeRawKeyDown(RawKeyDownEventHandler);
    }

    bool ValidateNowBinding()
    {
        string errMsg = "";
        if (nowBindingKeyBind.ValidationCheck(ref errMsg))
        {
            GameObject.Find("ErrorTMP").GetComponent<TextMeshProUGUI>().text = "";
            return true;
        }
        else
        {
            GameObject.Find("ErrorTMP").GetComponent<TextMeshProUGUI>().text = errMsg;
            return false;
        }

    }

    // プレハブ生成
    void InstantiateKeyBinder(int id, int x, int y)
    {
        keyBinder[id] = Instantiate(KeyBinderPrefab, GameObject.Find("Content").GetComponent<RectTransform>());
        keyBinder[id].GetComponent<RectTransform>().localPosition = new Vector3(x, y, 0);
        keyBinder[id].GetComponent<RectTransform>().Find("KeyIDText").GetComponent<Text>().text = $"{id:#000}";

        // イベントハンドラの設定
        // ここで id をそのまま引数に渡すと、スコープの関係でバグる
        ushort buttonNum = (ushort)id;
        keyBinder[id].GetComponent<RectTransform>().Find("KeyBindButton").GetComponent<Button>().onClick.AddListener(() => OnKeyBindButtonClick(buttonNum));
    }
    void InstantiateCharBinder(int id, int x, int y)
    {
        charBinder[id] = Instantiate(CharBinderPrefab, GameObject.Find("Content").GetComponent<RectTransform>());
        charBinder[id].GetComponent<RectTransform>().localPosition = new Vector3(x, y, 0);
        charBinder[id].GetComponent<RectTransform>().Find("CharIDText").GetComponent<Text>().text = $"{id:#00}";

        // ここで id をそのまま引数に渡すと、スコープの関係でバグる
        ushort buttonNum = (ushort)id;
        charBinder[id].GetComponent<RectTransform>().Find("CharInputField").GetComponent<InputField>().onEndEdit.AddListener((str) => OnCharEdited(buttonNum, str));
    }

    // イベントハンドラ
    public void OnKeyBindButtonClick(ushort keyID)
    {
        Debug.Log("keyBinder clicked. keyID: " + keyID);
        nowListening = keyID;
        //keyBinder[keyID].Find("KeyBindButton").GetComponent<Button>().colors = 
    }
    public void OnCharEdited(ushort charBinderID, string str)
    {
        char c;
        Debug.Log("charBinder edited. charBinderID: " + charBinderID + "\nstr : " + str);
        if (str == "" || str == "\0") c = '\0';
        else c = str[0];
        nowBindingKeyBind.CharMap[charBinderID] = c;
        UpdateDisplay();
        ValidateNowBinding();
    }
    public void OnSaveButtonClick(ushort slot)
    {
        if (ValidateNowBinding()) nowBindingKeyBind.SaveToJson(slot);
    }
    public void OnLoadButtonClick(ushort slot)
    {
        nowBindingKeyBind.LoadFromJson(slot);
        UpdateDisplay();
        ValidateNowBinding();
    }
    public void OnLoadJISButtonClick()
    {
        nowBindingKeyBind.SetToDefault("JIS");
        UpdateDisplay();
        ValidateNowBinding();
    }
    public void OnLoadUSButtonClick()
    {
        nowBindingKeyBind.SetToDefault("US");
        UpdateDisplay();
        ValidateNowBinding();
    }

    public void OnBackToTitleButtonClick()
    {
        MySceneManager.ChangeSceneRequest("TitleScene");
    }
    public void RawKeyDownEventHandler(RawKey key)
    {
        if (nowListening == -1) return;
        nowBindingKeyBind.RawKeyMap[nowListening] = (ushort)key;
        nowListening = -1;
        UpdateDisplay();
        ValidateNowBinding();
    }


    // 描画制御
    void UpdateDisplay()
    {
        // KeyID : 000 ~ 050
        for (int i = 0; i < 51; i++)
        {
            keyBinder[i].GetComponent<RectTransform>().Find("KeyBindButton").Find("RawKeyText").GetComponent<Text>().text = ((RawKey)(nowBindingKeyBind.RawKeyMap[i])).ToString();
        }

        // CharBinderID : 00 ~ 48 (Unshifted)
        for (int i = 0; i < 49; i++)
        {
            string s = nowBindingKeyBind.CharMap[i].ToString();
            if (s == "\0" || s == "") s = "(NULL)";
            if (s == " ") s = "(Space)";
            charBinder[i].GetComponent<RectTransform>().Find("CharInputField").GetComponent<InputField>().text = s;
        }

        // CharBinderID : 49 ~ 96 (Shifted)
        for (int i = 49; i < 97; i++)
        {
            string s = nowBindingKeyBind.CharMap[i].ToString();
            if (s == "\0" || s == "") s = "(NULL)";
            if (s == " ") s = "(Space)";
            charBinder[i].GetComponent<RectTransform>().Find("CharInputField").GetComponent<InputField>().text = s;
        }
    }

}
