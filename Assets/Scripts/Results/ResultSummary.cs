using System.IO;
using UnityEngine;

public class ResultSummary
{
    public int Rank;
    public MilliSecond TotalTime;
    public int TotalMiss;
    public MilliSecond[] LapTime;
    public System.DateTimeOffset DateTimeWhenFinishedUtc;
    public string FilePath;

    public bool IsProtected = false;
    public bool HasLog = false;
    public bool HasRegistrationCode = false;

    public ResultSummary() { }

    /// <summary>
    /// *.log のファイルパスを指定して ResultSummary を生成する
    /// </summary>
    /// <param name="gameMode"></param>
    /// <param name="filePath"></param>
    public ResultSummary(int gameMode, string filePath)
    {
        int numOfLaps = GlobalConsts.NumOfLaps[gameMode];
        if (File.Exists(filePath))
        {
            FilePath = filePath;
            // ファイルが存在する = ログがあるので HasLog = true とする
            HasLog = true;

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line1 = reader.ReadLine(); // 無視
                string line2 = reader.ReadLine(); // 無視
                string line3 = reader.ReadLine();
                DateTimeWhenFinishedUtc = System.DateTimeOffset.ParseExact(line3 + "Z", "yyyyMMddHHmmssfffZ", System.Globalization.CultureInfo.InvariantCulture);
                string line4 = reader.ReadLine(); // 無視

                string line5 = reader.ReadLine();
                string[] line5arr = line5.Split(',');
                TotalTime = new MilliSecond(long.Parse(line5arr[0]));
                TotalMiss = int.Parse(line5arr[1]);

                string line6 = reader.ReadLine();
                string[] line6arr = line6.Split(',');
                Debug.Assert(line6arr.Length == numOfLaps + 1);
                LapTime = new MilliSecond[numOfLaps];
                for (int i = 0; i < numOfLaps; i++)
                {
                    LapTime[i] = long.Parse(line6arr[i + 1]) - long.Parse(line6arr[i]);
                }

                string line7 = reader.ReadLine();
                IsProtected = line7 == "1" ? true : false;
            }
        }
        else
        {
            Debug.Log("There's no such log file.");
        }
    }

    /// <summary>
    /// ranking?.txt 用の行を返す
    /// </summary>
    /// <returns></returns>
    public string ToStringForRankingTxt()
    {
        int numOfLaps = LapTime.Length;

        string[] strarr = new string[8 + LapTime.Length];

        strarr[0] = Rank.ToString();
        strarr[1] = TotalTime;
        strarr[2] = TotalMiss.ToString();
        for (int i = 0; i < LapTime.Length; i++)
        {
            strarr[3 + i] = LapTime[i];
        }
        strarr[LapTime.Length + 3] = DateTimeWhenFinishedUtc.ToString("yyyyMMddHHmmssfff");
        strarr[LapTime.Length + 4] = Path.GetFileName(FilePath);
        strarr[LapTime.Length + 5] = IsProtected ? "1" : "0";
        strarr[LapTime.Length + 6] = HasLog ? "1" : "0";
        strarr[LapTime.Length + 7] = HasRegistrationCode ? "1" : "0";

        string res = string.Join(",", strarr);
        return res;
    }
}