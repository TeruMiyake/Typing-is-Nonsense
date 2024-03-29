﻿using System.Collections.Generic;
using System.IO;
using System.Linq;

public class ResultSummaries
{
    // ゲームモード定数
    int gameMode;
    int numOfLaps;

    // fileNames.Length == numOfResults とは限らない。
    // 「ログを削除してしまったが、ranking?.txt に残っている記録」があり得るため。
    public int NumOfResults = 0;
    public List<ResultSummary> summaryList = new List<ResultSummary>();

    string logDirPath;
    string rankingTxtPath;

    // ranking?.txt と ログファイルの有無は一致しないので管理する
    HashSet<string> fileNameSetInRankingTxt = new HashSet<string>();
    HashSet<string> fileNameSetInLogDir = new HashSet<string>();
    HashSet<string> fileNameSetInRegistCodeDir = new HashSet<string>();

    /// <summary>
    /// ResultSummary は、このコンストラクタ内でランク昇順にソートされて List に格納される
    /// </summary>
    /// <param name="_gameMode"></param>
    /// <param name="isTerminated"></param>
    /// <param name="isGuestResult"></param>
    public ResultSummaries(int _gameMode, bool isTerminated, bool isGuestResult)
    {
        // ゲームモード定数の設定
        gameMode = _gameMode;
        numOfLaps = GlobalConsts.NumOfLaps[gameMode];
        logDirPath = LogFileUtil.GetLogDirPath(gameMode, isTerminated, isGuestResult);
        logDirPath += "/";
        rankingTxtPath = logDirPath + "ranking" + gameMode.ToString() + ".txt";

        // ResultSummary の読み込みと、ファイル名の集合の管理
        // isTerminated の場合、ranking?.txt は無い
        if (!isTerminated)
        {
            // 1. get summaries from ranking?.txt 
            LoadFromRankingTxt();

            // 2. get summaries from log directory
            // ここで、ranking?.txt 内になかった記録なら、summaryList に追加
            // そのうち、ファイル読込部分をメソッドとして切り出したい
            string[] fileNames = LogFileUtil.GetLogFileNameList(gameMode, isTerminated, isGuestResult);
            foreach(string fileName in fileNames)
            {
                fileNameSetInLogDir.Add(fileName);
                if (!fileNameSetInRankingTxt.Contains(fileName))
                {
                    summaryList.Add(new ResultSummary(gameMode, logDirPath + "/" + fileName));
                }
            }

            // 3. check if each summary has *.rcode
            string[] registCodeFileNames = LogFileUtil.GetRegistCodeFileNameList(gameMode);
            foreach (string registCodeFileName in registCodeFileNames)
            {
                fileNameSetInRegistCodeDir.Add(registCodeFileName);
            }
            foreach (ResultSummary summary in summaryList)
            {
                string fileNameIfExisted = summary.DateTimeWhenFinishedUtc.ToString("yyyyMMddHHmmssfff") + ".rcode";
                switch (gameMode)
                {
                    // Nonsense
                    case 0:
                        fileNameIfExisted = "NC" + fileNameIfExisted;
                        break;
                    default:
                        fileNameIfExisted = "NC" + fileNameIfExisted;
                        break;
                }
                if (fileNameSetInRegistCodeDir.Contains(fileNameIfExisted))
                {
                    summary.HasRegistrationCode = true;
                }
            }

            // これで summaryList が揃った
            NumOfResults = summaryList.Count;

            // 並べ替えて ResultSummary.Rank を設定
            // 同じタイムであれば新しい記録の方が良い順位とするルールのため、先に DateTime で降順ソート
            SortByDateTimeDescending();
            SortByTimeAscending();
            for (int i = 1; i < NumOfResults + 1; i++)
            {
                summaryList[i - 1].Rank = i;
            }

            // 3. 保存処理
            SaveToRankingTxt();
        }

    }

    #region SortSummaryList
    public void SortByRankAscending()
    {
        summaryList = new List<ResultSummary>(summaryList.OrderBy(x => x.Rank));
    }
    public void SortByRankDescending()
    {
        summaryList = new List<ResultSummary>(summaryList.OrderByDescending(x => x.Rank));
    }
    public void SortByTimeAscending()
    {
        summaryList = new List<ResultSummary>(summaryList.OrderBy(x => x.TotalTime));
    }
    public void SortByTimeDescending()
    {
        summaryList = new List<ResultSummary>(summaryList.OrderByDescending(x => x.TotalTime));
    }
    public void SortByMissAscending()
    {
        summaryList = new List<ResultSummary>(summaryList.OrderBy(x => x.TotalMiss));
    }
    public void SortByMissDescending()
    {
        summaryList = new List<ResultSummary>(summaryList.OrderByDescending(x => x.TotalMiss));
    }
    public void SortByDateTimeAscending()
    {
        summaryList = new List<ResultSummary>(summaryList.OrderBy(x => x.DateTimeWhenFinishedUtc));
    }
    public void SortByDateTimeDescending()
    {
        summaryList = new List<ResultSummary>(summaryList.OrderByDescending(x => x.DateTimeWhenFinishedUtc));
    }
    #endregion

    /// <summary>
    /// ranking?txt 内にあるデータを summaryList へ読み込む
    /// 行ごとの形式は rank, time, miss, lap[num], datetime, filename, bool, bool, bool
    /// また、fileNameSetInRankingTxt に ranking?.txt 内にある記録を格納する
    /// </summary>
    void LoadFromRankingTxt()
    {
        if (File.Exists(rankingTxtPath))
        {
            using (StreamReader reader = new StreamReader(rankingTxtPath))
            {

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] data = line.Split(',');

                    ResultSummary summary = new ResultSummary();
                    summary.Rank = int.Parse(data[0]);
                    summary.TotalTime = new MilliSecond(long.Parse(data[1]));
                    summary.TotalMiss = int.Parse(data[2]);
                    summary.LapTime = new MilliSecond[numOfLaps];
                    for (int i = 0; i < numOfLaps; i++)
                    {
                        summary.LapTime[i] = new MilliSecond(long.Parse(data[3 + i]));
                    }
                    string datetimeString_Utc = data[2 + numOfLaps + 1] + "Z";
                    summary.DateTimeWhenFinishedUtc = System.DateTimeOffset.ParseExact(datetimeString_Utc, "yyyyMMddHHmmssfffZ", System.Globalization.CultureInfo.InvariantCulture);

                    // ファイル名はセットで管理
                    string fileName = data[2 + numOfLaps + 2];
                    fileNameSetInRankingTxt.Add(fileName);
                    summary.FilePath = logDirPath + fileName;

                    string _isProtected = data[2 + numOfLaps + 3];
                    string _hasLog = data[2 + numOfLaps + 4];
                    string _hasRegistrationCode = data[2 + numOfLaps + 5];
                    summary.IsProtected = _isProtected == "1" ? true : false;
                    summary.HasLog = _hasLog == "1" ? true : false;
                    summary.HasRegistrationCode = _hasRegistrationCode == "1" ? true : false;

                    summaryList.Add(summary);                    
                }

            }
        }
        else
        {
            // 今のところ何もすることを思いつかない
        }
    }
    /// <summary>
    /// summaryList を ranking?txt へと書き込む
    /// 行ごとの形式は rank, time, miss, lap[num], datetime, filename, bool, bool, bool
    /// </summary>
    void SaveToRankingTxt()
    {
        using (StreamWriter sw = new StreamWriter(rankingTxtPath, false))
        foreach (ResultSummary summary in summaryList)
        {
                sw.WriteLine(summary.ToStringForRankingTxt());
        }
    }

    /// <summary>
    /// 1 位の記録を返す
    /// </summary>
    /// <returns></returns>
    public ResultSummary GetBestResultByRank()
    {
        SortByRankAscending();
        return summaryList[0];
    }
}

