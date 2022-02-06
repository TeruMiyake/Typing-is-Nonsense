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
    /// <summary>
    /// GameMode など全てファイルから読み込み
    /// MissLimit はデフォルトに設定される
    /// LapMiss は計算すれば出せるが、時間がかかるので未実装
    /// </summary>
    /// <param name="filePath">ログファイル *.log のフルパス</param>
    public TrialData(string filePath) : this()
    {
        LoadFromLogFile(filePath);
    }

    public void SaveToLogFile()
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
    /// GameMode など全てファイルから読み込み
    /// MissLimit はいじらない（デフォルトのまま）
    /// LapMiss は計算すれば出せるが、時間がかかるので未実装
    /// </summary>
    /// <param name="filePath">ログファイル *.log のフルパス</param>
    public void LoadFromLogFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            int numOfLines = 14;
            string[] lines = new string[numOfLines];
            using (StreamReader sr = new StreamReader(filePath))
            {
                for (int i = 0; i < numOfLines; i++)
                {
                    lines[i] = sr.ReadLine();
                }
            }
            LogVersion = int.Parse(lines[0]);
            // line 2
            int[] line2intarr = lines[1].Split(',').Select(x => int.Parse(x)).ToArray();
            GameMode = line2intarr[0]; // 0:Mode Nonsense
            CharMode = line2intarr[1]; // 0:normal
            Platform = line2intarr[2]; // 0:Win
            // line 3
            // 終了時刻は厳密でなくて良い。普段は MinValue にしておき、SaveLog() 時のみ計測
            DateTimeWhenFinished = System.DateTime.ParseExact(lines[2], "yyyyMMddHHmmssfff", System.Globalization.CultureInfo.InvariantCulture);
            // line 4
            int[] line4intarr = lines[3].Split(',').Select(x => int.Parse(x)).ToArray();
            IsTerminated = line4intarr[0] == 1 ? true : false; // 0:completed, 1:terminated
            TypedKeys = line4intarr[1]; // GameMain.OnCorrectKeyDown() で更新
            // line 5
            // それぞれのタイム・ミスは全て「合計タイム」で入れるので、打鍵時間を出すには引き算が必要
            // key の配列の [0] は番兵
            string[] line5arr = lines[4].Split(',');
            TotalTime = long.Parse(line5arr[0]);
            TotalMiss = int.Parse(line5arr[1]);
            // line 6
            // 合計タイム方式, [0] は番兵
            LapTime = lines[5].Split(',').Select(x => new MilliSecond(long.Parse(x))).ToArray();
            // line 7
            IsProtected = lines[6] == "1" ? true : false;
            // line 8
            TaskCharIDs = lines[7].Split(',').Select(x => ushort.Parse(x)).ToArray();
            // line 9
            // 合計タイム方式, [0] は番兵
            // GameMain.OnCorrectKeyDown() で更新
            CorrectKeyTime = lines[8].Split(',').Select(x => new MilliSecond(long.Parse(x))).ToArray();
            // line 10, 11
            // ミスやシフト上下も含めた、全てのキー入力を保管する
            // 基本は charID で入れるが、ShiftDown:97, ShiftUp:98
            // また、IDs[0] には 初期シフト状態（0 ~ 2）を格納する。> 0 で shifted
            // GameMain.OnNormalKeyDown(), Shift(Down|Up)Handler() で更新
            // [0] 初期シフト状態は GameMain.StartTrial() で得る
            AllInputIDs = lines[9].Split(',').Select(x => ushort.Parse(x)).ToList();
            // 合計タイム方式, [0] は番兵（初期シフト状態 = 0)
            AllInputTime = lines[10].Split(',').Select(x => long.Parse(x)).ToList();
            // line 12 ~ 14 : KeyBind
            TrialKeyBind = new KeyBind(lines[11], lines[12], lines[13]);
        }
        else
        {
            Debug.Log($"File not found. {filePath}");
        }
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
    public KeyBind GetKeyBind()
    {
        return TrialKeyBind;
    }
}

