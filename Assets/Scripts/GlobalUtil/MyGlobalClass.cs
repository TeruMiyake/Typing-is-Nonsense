/// ゲーム全体で使用するクラスや構造体を定義
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq; // 配列初期化用
using System.IO; // ファイル操作

#if UNITY_STANDALONE_WIN
/// https://github.com/Elringus/UnityRawInput
using UnityRawInput;
#endif


[System.Serializable]
public class TrialData
{
    // ミスリミット
    public ushort missLimit = 50;
    // 正しく打ったキー数 = 次打つキーID。0 から始まり assignmentLength で打切
    public int typedKeys = 0; // 本質
    // 現在何ラップ目か 1-indexed
    public int nowLap = 1; // 非本質（typedKeysから算出可）
    // トータルタイム・ラップタイム・各正解打鍵タイム
    // それぞれのタイム・ミスは全て「合計タイム」で入れるので、打鍵時間を出すには引き算が必要
    // lap, key の配列の [0] は番兵
    public long totalTime = 0; // 微妙（typedKeysとkeyTimeから算出はできる）
    public long[] lapTime; // 非本質（keyTimeから算出可）
    public long[] keyTime; // 本質
    // トータルミス・ラップミス・各キー辺りミス（要るか？）
    // それぞれのタイム・ミスは全て「合計タイム」で入れるので、打鍵時間を出すには引き算が必要
    // lap, key の配列の [0] は番兵
    public int totalMiss = 0; // 半本質　keyMiss を入れるなら本質ではないが、合計を出すのに時間がかかるからあった方がいい
    public int[] lapMiss; // 非本質　上記と同様時間がかからんでもないが……
    public int[] keyMiss; // 本質？　必要かは置いておいて、この機能をつけるならこれが本質

    public ushort[] trialAssignment_CharID; // 本質
    //public char[] trialAssignment_Char; // 非本質

    public TrialData(ushort lim, int assignmentLength, int numOfLaps)
    {
        Debug.Log("New trialData was instantiated.");

        missLimit = lim;

        lapTime = Enumerable.Repeat<long>(0, numOfLaps + 1).ToArray();
        keyTime = Enumerable.Repeat<long>(0, assignmentLength + 1).ToArray();

        lapMiss = Enumerable.Repeat<int>(0, numOfLaps + 1).ToArray();
        keyMiss = Enumerable.Repeat<int>(0, assignmentLength + 1).ToArray();

        trialAssignment_CharID = new ushort[assignmentLength];
    }
}

/// <summary>
/// 注）NullKeyMap はキーバインド中に更新されず、KeyBind.SaveToJson() 時に更新される
/// </summary>
[System.Serializable]
public class KeyBind
{
    public ushort[] RawKeyMap; // 0 ~ 50
    public ushort[] NullKeyMap; // 0 ~ 2
    public char[] CharMap; // 0 ~ 96

    public KeyBind()
    {
        // デフォルトをセット
        SetToDefault("JIS");
    }
    public KeyBind(string def)
    {
        SetToDefault(def);
    }

    public void SetToDefault(string def)
    {
        switch (def)
        {
            case "JIS":
                // 今は固定で JIS 配列のデータが入っているが、いつか US や UK などを作ってアセットにしていく
                RawKeyMap = new ushort[] { 16, 16, 32, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 49, 50, 51, 52, 53, 54, 55, 56, 57, 48, 189, 222, 220, 192, 219, 187, 186, 221, 188, 190, 191, 226 };
                NullKeyMap = new ushort[] { 39, 84 };
                CharMap = " abcdefghijklmnopqrstuvwxyz1234567890-^\0@[;:],./\\ABCDEFGHIJKLMNOPQRSTUVWXYZ!\"#$%&'()\0=~|`{+*}<>?_".ToCharArray();
                break;
            case "US":
                KeyBind loaded = JsonUtility.FromJson<KeyBind>("{ \"RawKeyMap\":[16,16,32,65,66,67,68,69,70,71,72,73,74,75,76,77,78,79,80,81,82,83,84,85,86,87,88,89,90,49,50,51,52,53,54,55,56,57,48,189,187,192,219,221,220,186,222,188,190,191,35],\"NullKeyMap\":[48,96],\"CharMap\":[32,97,98,99,100,101,102,103,104,105,106,107,108,109,110,111,112,113,114,115,116,117,118,119,120,121,122,49,50,51,52,53,54,55,56,57,48,45,61,96,91,93,92,59,39,44,46,47,0,65,66,67,68,69,70,71,72,73,74,75,76,77,78,79,80,81,82,83,84,85,86,87,88,89,90,33,64,35,36,37,94,38,42,40,41,95,43,126,123,125,124,58,34,60,62,63,0]}");

                // 読み込んだ KeyBind がバリデーションを通れば採用
                // MyKeyBind0.json への保存はユーザが SAVE するまでは行わない
                string errMsg = "";
                if (loaded.ValidationCheck(ref errMsg))
                {
                    RawKeyMap = loaded.RawKeyMap;
                    NullKeyMap = loaded.NullKeyMap;
                    CharMap = loaded.CharMap;
                }
                else
                {
                    Debug.Log($"Validation failed. KeyBind not changed.\n{errMsg}");
                }
                break;
            default:
                Debug.Log($"{def} is Invalid Default Keyboard Layout name.");
                break;
        }
    }

    // validation
    public bool ValidationCheck(ref string ErrorMsg)
    {
        bool ret = true;
        // RawKeyMap をチェック
        Debug.Assert(RawKeyMap.Length == 51);
        HashSet<ushort> rawKeys = new HashSet<ushort>();
        for (int i = 0; i < RawKeyMap.Length; i++)
        {
            // RawKey の重複は許さないが、KeyID:1 で重複が出るのは OK（シフトキーの重複だから）
            // KeyID: 0 -> 1 の順で見ているから、1 で重複が出る＝シフトキーの重複と断定可能
            if (rawKeys.Contains(RawKeyMap[i]) && i != 1)
            {
                ErrorMsg += $"Duplicated Raw Key: {RawKeyMap[i]} (KeyID: {i})\n";
                ret = false;
            }
            if (!System.Enum.IsDefined(typeof(RawKey), RawKeyMap[i]))
            {
                ErrorMsg += $"Invalid Raw Key: {RawKeyMap[i]} (KeyID: {i})\n";
                ret = false;
            }
            rawKeys.Add(RawKeyMap[i]);
        }

        // NullKeyMap はチェックしない（CharMap が正しければ Save 時に正しく生成される）

        // CharMap をチェック
        Debug.Assert(CharMap.Length == 97);
        // 重複チェック
        HashSet<ushort> chars = new HashSet<ushort>();
        for (int i = 0; i < 97; i++)
        {
            if (chars.Contains(CharMap[i]) && CharMap[i] != '\0')
            {
                ErrorMsg += $"Duplicated Char: {CharMap[i]} (CharID: {i})\n";
                ret = false;
            }
            chars.Add(CharMap[i]);
        }
        // Unshifted: 0 ~ 48 に null が 1 つ必要
        int l_null = 0;
        for (int i = 0; i < 49; i++) if (CharMap[i] == '\0') l_null++;
        if (l_null != 1)
        {
            ErrorMsg += $"You need 1 (NULL) in Char (Unshifted). Now: {l_null}\n";
            ret = false;
        }
        // Unshifted: 49 ~ 96 に null が 1 つ必要
        int r_null = 0;
        for (int i = 49; i < 97; i++) if (CharMap[i] == '\0') r_null++;
        if (r_null != 1)
        {
            ErrorMsg += $"You need 1 (NULL) in Char (Shifted). Now: {r_null}\n";
            ret = false;
        }

        return ret;
    }

    /// <summary>
    /// セーブ・ロード slot 0 は「今採用しているキーバインド」を示す
    /// </summary>
    /// <param name="slot"></param>
    public void SaveToJson(ushort slot)
    {
        Debug.Log($"save to slot {slot}");
        string ErrMsg = "";
        if (ValidationCheck(ref ErrMsg))
        {
            int nullCnt = 0;
            for (int i = 0; i < CharMap.Length; i++)
            {
                if (CharMap[i] == '\0')
                {
                    NullKeyMap[nullCnt] = (ushort)i;
                    nullCnt++;
                }
            }
            string jsonstr = JsonUtility.ToJson(this);
            if (!Directory.Exists(Application.dataPath + "/SaveData"))
                Directory.CreateDirectory(Application.dataPath + "/SaveData");
            string datapath = Application.dataPath + "/SaveData/MyKeyBind" + slot.ToString() + ".json";
            StreamWriter writer = new StreamWriter(datapath, false);
            writer.WriteLine(jsonstr);
            writer.Flush();
            writer.Close();
        }
        else
        {
            Debug.Log($"Validation failed. KeyBind not saved to slot {slot}.\n{ErrMsg}");

        }
    }
    /// <summary>
    /// MyKeyBind{slot}.json から KeyBind クラスにデータをロードする
    /// この時点では、実際に使用しているキーバインド（MyKeyBind0.json）には反映しない
    /// </summary>
    /// <param name="slot"></param>
    public void LoadFromJson(ushort slot)
    {
        Debug.Log($"load from slot {slot}");
        if (!Directory.Exists(Application.dataPath + "/SaveData"))
            Directory.CreateDirectory(Application.dataPath + "/SaveData");
        string loaddatapath = Application.dataPath + "/SaveData/MyKeyBind" + slot.ToString() + ".json";
        if (File.Exists(loaddatapath))
        {
            StreamReader reader = new StreamReader(loaddatapath);
            string loadedjson = reader.ReadToEnd();
            reader.Close();
            KeyBind loaded = JsonUtility.FromJson<KeyBind>(loadedjson);

            // 読み込んだ KeyBind がバリデーションを通れば採用して slot 0 に保管
            string errMsg = "";
            if (loaded.ValidationCheck(ref errMsg))
            {
                RawKeyMap = loaded.RawKeyMap;
                NullKeyMap = loaded.NullKeyMap;
                CharMap = loaded.CharMap;
            }
            else {
                Debug.Log($"Validation failed. KeyBind not changed.\n{errMsg}");
            }
        }
        else
        {
            Debug.Log($"MyKeyBind{slot}.json Not Found. KeyBind not changed.");
        }
    }
}

/// <summary>
/// PlayerPrefs に入れる設定
/// </summary>
[System.Serializable]
public class Config
{
    public ushort MissLimit = 50;

    public void Save()
    {
        PlayerPrefs.SetInt("MissLimit", MissLimit);
    }
    public void Load()
    {
        int misslim = PlayerPrefs.GetInt("MissLimit", 50);
        Debug.Assert(0 <= misslim && misslim <= 360);
        MissLimit = (ushort)misslim;
    }
}

[System.Obsolete("When implementing UtilKeyBinding, changes needed.")]
public class KeyBindDicts
{
    // KeyID と CharID（Key にアサインされた文字）間の変換
    public Dictionary<ushort, ushort> dictToKeyID_FromCharID;
    public Dictionary<ushort, ushort> dictToCharID_FromKeyID;

    // Char と CharID 間の変換
    public Dictionary<ushort, char> dictToChar_FromCharID;
    public Dictionary<char, ushort> dictToCharID_FromChar;

#if UNITY_STANDALONE_WIN
    public Dictionary<RawKey, ushort> dictToKeyID_FromRawKey = new Dictionary<RawKey, ushort>();
#endif

    // キーバインド機能をつけたから引数なしのコンストラクタは不要。呼べないようにする
    private KeyBindDicts() { }
    public KeyBindDicts(KeyBind keyBind)
    {
        dictToKeyID_FromCharID = new Dictionary<ushort, ushort>();
        dictToCharID_FromKeyID = new Dictionary<ushort, ushort>();
        dictToChar_FromCharID = new Dictionary<ushort, char>();
        dictToCharID_FromChar = new Dictionary<char, ushort>();
        dictToKeyID_FromRawKey = new Dictionary<RawKey, ushort>();

        // KeyID <- RawKey
        for (int i = 0; i < 51; i++)
        {
            // 0 == null
            dictToKeyID_FromRawKey[(RawKey)(keyBind.RawKeyMap[i])] = (ushort)i;
        }
        // Char <-> CharID
        for (int i = 0; i < 97; i++)
        {
            if (keyBind.CharMap[i] == '\0') continue;
            dictToChar_FromCharID[(ushort)i] = keyBind.CharMap[i];
            dictToCharID_FromChar[keyBind.CharMap[i]] = (ushort)i;
        }
        // KeyID <-> CharID
        for (int i = 0; i < 97; i++)
        {
            if (keyBind.NullKeyMap[0] == i || keyBind.NullKeyMap[1] == i) continue;
            else
            {
                dictToKeyID_FromCharID[(ushort)i] = (ushort)(i + 2);
                dictToCharID_FromKeyID[(ushort)(i + 2)] = (ushort)i;
            }
        }
        // 特殊キーの処理（そのうちバインドを Config などから流し込む機能をつける）
        dictToKeyID_FromRawKey[RawKey.Return] = 100; // 13
        dictToKeyID_FromRawKey[RawKey.Escape] = 101; // 27
    }

    // メソッド
#if UNITY_STANDALONE_WIN
    public ushort ToKeyID_FromRawKey(RawKey key)
    {
        return dictToKeyID_FromRawKey[key];
    }
#endif

    // ここから、プラットフォームに依存しない処理
    // この時点までで、入力は全て KeyID に変換されている

    // KeyID と CharID（Key にアサインされた文字）間の変換
    public ushort ToKeyID_FromCharID(ushort charID)
    {
        return dictToKeyID_FromCharID[charID];
    }
    public ushort ToCharID_FromKeyID(ushort keyID)
    {
        return dictToCharID_FromKeyID[keyID];
    }

    // Char と CharID 間の変換
    public char ToChar_FromCharID(ushort charID)
    {
        return dictToChar_FromCharID[charID];
    }
    public ushort ToCharID_FromChar(char c)
    {
        return dictToCharID_FromChar[c];
    }
}