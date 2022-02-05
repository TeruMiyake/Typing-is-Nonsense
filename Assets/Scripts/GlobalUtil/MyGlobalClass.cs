/// ゲーム全体で使用するクラスや構造体を定義
/// using System; は UnityEngine と重複する部分があるので行わず、毎回明示的に使う
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq; // 配列初期化用
using System.IO; // ファイル操作

#if UNITY_STANDALONE_WIN
/// https://github.com/Elringus/UnityRawInput
using UnityRawInput;
#endif


/// <summary>
/// ゲーム状態を表す型
/// </summary>
public enum GameState
{
    Waiting,
    Countdown,
    TrialOn,
    Completed,
    Canceled,
    Failed
}

/// <summary>
/// ミリ秒型（long <-> MilliSecond は暗黙的に型変換できる）
/// </summary>
public class MilliSecond : System.IEquatable<MilliSecond>, System.IComparable<MilliSecond>
{
    public long ms;
    public MilliSecond(long _ms) { ms = _ms; }
    public static implicit operator long(MilliSecond milliSecond)
    {
        return milliSecond.ms;
    }
    public static implicit operator MilliSecond(long _ms)
    {
        return new MilliSecond(_ms);
    }
    public static implicit operator string(MilliSecond milliSecond)
    {
        return milliSecond.ms.ToString();
    }
    /// <summary>
    /// long で保存してあるミリ秒を、表示用の "X.XXX" s に変える
    /// </summary>
    /// <param name="ms"></param>
    /// <returns></returns>
    public string ToFormattedTime()
    {
        return (ms / 1000).ToString() + "." + (ms % 1000).ToString("000");
    }

    // インタフェース
    public bool Equals(MilliSecond other) => ms == other.ms;
    public int CompareTo(MilliSecond other)
    {
        if (ms > other.ms) return 1;
        else if (ms == other.ms) return 0;
        else return -1;
    }
}

/// <summary>
/// トライアルデータ
/// </summary>
[System.Serializable]
public class TrialData
{
    // not saved in .log
    public int MissLimit = 50;
    public int[] LapMiss; // 合計ミス数方式, [0] は番兵
    // line 1
    public int LogVersion = 0;
    // line 2
    public int GameMode; // 0:Mode Nonsense
    public int CharMode = 0; // 0:normal
    public int Platform = 0; // 0:Win
    // line 3
    // 終了時刻は厳密でなくて良い。普段は MinValue にしておき、SaveLog() 時のみ計測
    public System.DateTime DateTimeWhenFinished = System.DateTime.MinValue;
    // line 4
    public bool IsTerminated = true; // 0:completed, 1:terminated
    public int TypedKeys = 0; // GameMain.OnCorrectKeyDown() で更新
    // line 5
    // それぞれのタイム・ミスは全て「合計タイム」で入れるので、打鍵時間を出すには引き算が必要
    // key の配列の [0] は番兵
    public MilliSecond TotalTime = 0; // 微妙（typedKeysとCorrectKeyTimeから算出はできる）
    public int TotalMiss = 0;
    // line 6
    public MilliSecond[] LapTime; // 合計タイム方式, [0] は番兵
    // line 7
    public bool IsProtected = false;
    // line 8
    public ushort[] TaskCharIDs;
    // line 9
    // 合計タイム方式, [0] は番兵
    // GameMain.OnCorrectKeyDown() で更新
    public MilliSecond[] CorrectKeyTime;
    // line 10, 11
    // ミスやシフト上下も含めた、全てのキー入力を保管する
    // 基本は charID で入れるが、ShiftDown:97, ShiftUp:98
    // また、IDs[0] には 初期シフト状態（0 ~ 2）を格納する。> 0 で shifted
    // GameMain.OnNormalKeyDown(), Shift(Down|Up)Handler() で更新
    // [0] 初期シフト状態は GameMain.StartTrial() で得る
    public List<ushort> AllInputIDs = new List<ushort>();
    // 合計タイム方式, [0] は番兵（初期シフト状態 = 0)
    public List<long> AllInputTime = new List<long>();
    // line 12 ~ 14
    // ゲーム開始時のコンストラクタで得る
    KeyBind TrialKeyBind = new KeyBind();

    // 未実装
    //public string RegistrationCode;

    // コンストラクタ
    /// <summary>
    /// GameMode を明示しない場合、MODE:NONSENSE と解釈する
    /// </summary>
    public TrialData(): this(0) { }
    /// <summary>
    /// MissLimit を明示しない場合、MissLimit:50 とする
    /// とりあえず生成したいだけの場合は、これで良い
    /// </summary>
    public TrialData(int gameMode) : this(gameMode, 50) { }
    /// <summary>
    /// キーバインドを指定せずに MissLimit を明示して生成することは今のところ想定していない
    /// よって直接使うことは無さそうなので、一旦 private にしてある
    /// </summary>
    /// <param name="missLim"></param>
    /// <param name="gameMode"></param>
    private TrialData(int gameMode, int missLim)
    {
        int assignmentLength;
        int numOfLaps;
        switch (gameMode)
        {
            // MODE NONSENSE
            case 0:
                assignmentLength = 360;
                numOfLaps = 10;
                break;
            default:
                assignmentLength = 360;
                numOfLaps = 10;
                break;
        }
        // not saved in .log
        MissLimit = missLim;
        LapMiss = Enumerable.Repeat<int>(0, numOfLaps + 1).ToArray();
        // saved in .log
        GameMode = gameMode;
        LapTime = Enumerable.Repeat<MilliSecond>(0, numOfLaps + 1).ToArray();
        TaskCharIDs = new ushort[assignmentLength];
        CorrectKeyTime = Enumerable.Repeat<MilliSecond>(0, assignmentLength + 1).ToArray();
    }
    /// <summary>
    /// ゲーム開始時はこれを使う。キーバインドが既に判明しているため
    /// </summary>
    /// <param name="gameMode"></param>
    /// <param name="missLim"></param>
    /// <param name="keyBind"></param>
    public TrialData(int gameMode, int missLim, KeyBind keyBind) : this(gameMode, missLim)
    {
        TrialKeyBind = keyBind;
    }

    public void SaveLog()
    {
        // ディレクトリパスの生成
        string saveDataDirPath = Application.dataPath + "/SaveData";
        string logDirPath = LogFileUtil.GetLogDirPath(GameMode, IsTerminated, false);

        // ファイルパス接頭辞の生成
        string filePath = logDirPath;
        switch (GameMode)
        {
            // MODE:NONSENSE
            case 0:
                filePath += "/N";
                break;
            default:
                logDirPath = saveDataDirPath + "/N";
                break;
        }
        switch (IsTerminated)
        {
            case false:
                filePath += "C";
                break;
            case true:
                filePath += "T";
                break;
        }

        // ファイルパス本体の生成
        filePath += DateTimeWhenFinished.ToString("yyyyMMddHHmmssfff") + ".log";
        if (!File.Exists(filePath))
        {
            using (StreamWriter sw = File.CreateText(filePath))
            {
                // MilliSecond[] をそのまま Join できないため、一旦 long[] へと変換
                long[] _lapTime = new long[LapTime.Length];
                for (int i = 0; i < LapTime.Length; i++)
                    _lapTime[i] = LapTime[i];
                long[] _correctKeyTime = new long[CorrectKeyTime.Length];
                for (int i = 0; i < CorrectKeyTime.Length; i++)
                    _correctKeyTime[i] = CorrectKeyTime[i];

                sw.WriteLine(LogVersion.ToString());
                sw.WriteLine(string.Join(",", new int[] { GameMode, CharMode, Platform }));
                sw.WriteLine(DateTimeWhenFinished.ToString("yyyyMMddHHmmssfff"));
                sw.WriteLine((IsTerminated ? "1," : "0,") + TypedKeys.ToString());
                sw.WriteLine(string.Join(",", new long[] { TotalTime, TotalMiss }));
                sw.WriteLine(string.Join(",", _lapTime));
                sw.WriteLine(IsProtected ? "1" : "0");
                sw.WriteLine(string.Join(",", TaskCharIDs));
                sw.WriteLine(string.Join(",", _correctKeyTime));
                sw.WriteLine(string.Join(",", AllInputIDs));
                sw.WriteLine(string.Join(",", AllInputTime));
                sw.WriteLine(string.Join(",", TrialKeyBind.RawKeyMap));
                sw.WriteLine(string.Join(",", TrialKeyBind.NullKeyMap));
                sw.WriteLine(string.Join("", TrialKeyBind.CharMap));
            }
        }
        else Debug.Log("A log file of the same name already exists.");
    }

    /// <summary>
    /// 1-indexed の lap 値を受け取り、long のラップタイム（合計値）を返す
    /// Assert(0 <= lap); (0) は番兵
    /// </summary>
    /// <param name="lap"></param>
    /// <returns></returns>
    public long GetLapTime(int lap)
    {
        Debug.Assert(0 <= lap);
        return LapTime[lap];
    }
    /// <summary>
    /// 1-indexed の lap 値を受け取り、long のラップタイム（合計値を引き算したもの）を返す
    /// </summary>
    /// <param name="lap"></param>
    /// <returns></returns>
    public long GetSingleLapTime(int lap)
    {
        Debug.Assert(0 <= lap);
        // ラップ 0 は番兵
        if (lap == 0) return 0;
        else return LapTime[lap] - LapTime[lap - 1];
    }
    public void SetLapTime(int lap, long lapTime)
    {
        Debug.Assert(1 <= lap);
        // ラップ 0 は番兵
        LapTime[lap] = lapTime;
    }
}


/// <summary>
/// 注）NullKeyMap は Keybinding 中には更新されず、KeyBind.SaveToJson() 時に更新される
/// </summary>
[System.Serializable]
public class KeyBind
{
    public ushort[] RawKeyMap; // 0 ~ 50
    public ushort[] NullKeyMap; // 0 ~ 2
    public char[] CharMap; // 0 ~ 96. 97:SDown, 98:SUp は固定のため

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
        dictToKeyID_FromRawKey[RawKey.Return] = GlobalConsts.KeyID_Return; // 13
        dictToKeyID_FromRawKey[RawKey.Escape] = GlobalConsts.KeyID_Esc; // 27
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