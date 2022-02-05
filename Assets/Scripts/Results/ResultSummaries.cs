﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

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

    // ranking?.txt と ログファイルの有無は一致しないので管理する
    HashSet<string> fileNameSetInRankingTxt = new HashSet<string>();
    HashSet<string> fileNameSetInLogDir = new HashSet<string>();

    public ResultSummaries(int _gameMode, bool isTerminated, bool isGuestResult)
    {
        // ゲームモード定数の設定
        gameMode = _gameMode;
        numOfLaps = GlobalConsts.NumOfLaps[gameMode];
        logDirPath = LogFileUtil.GetLogDirPath(gameMode, isTerminated, isGuestResult);

        // ResultSummary の読み込みと、ファイル名の集合の管理
        // isTerminated の場合、ranking?.txt は無い
        if (!isTerminated)
        {
            // 1. ranking?.txt 
            LoadFromRankingTxt();

            // 2. log directory
            // ここで、ranking?.txt 内になかった記録なら、summaryList に追加
            string[] fileNames = LogFileUtil.GetLogFileNameList(gameMode, isTerminated, isGuestResult);
            foreach(string fileName in fileNames)
            {
                fileNameSetInLogDir.Add(fileName);
                if (!fileNameSetInRankingTxt.Contains(fileName))
                {
                    summaryList.Add(new ResultSummary(gameMode, logDirPath + "/" + fileName));
                }
            }

            // これで summaryList が揃った
            NumOfResults = summaryList.Count;

            // 並べ替えて ResultSummary.Rank を設定
            SortByDateTimeDescending();
            SortByTimeAscending();
            for (int i = 1; i < NumOfResults + 1; i++)
            {
                summaryList[i - 1].Rank = i;
            }
        }

    }

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
        summaryList = new List<ResultSummary>(summaryList.OrderBy(x => x.DateTimeWhenFinished));
    }
    public void SortByDateTimeDescending()
    {
        summaryList = new List<ResultSummary>(summaryList.OrderByDescending(x => x.DateTimeWhenFinished));
    }

    /// <summary>
    /// ranking?txt 内にあるデータを summaryList へ読み込む
    /// 行ごとの形式は rank, time, miss, lap[num], datetime, filename, bool, bool, bool
    /// また、fileNameSetInRankingTxt に ranking?.txt 内にある記録を格納する
    /// </summary>
    void LoadFromRankingTxt()
    {
        string rankingTxtPath = logDirPath + "/ranking" + gameMode.ToString() + ".txt";
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
                    summary.DateTimeWhenFinished = System.DateTime.ParseExact(data[2 + numOfLaps + 1], "yyyyMMddHHmmssfff", System.Globalization.CultureInfo.InvariantCulture);

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
}


public class ResultSummary
{
    public int Rank;
    public MilliSecond TotalTime;
    public int TotalMiss;
    public MilliSecond[] LapTime;
    public System.DateTime DateTimeWhenFinished;
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
            // ファイルが存在する = ログがある
            HasLog = true;

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line1 = reader.ReadLine(); // 無視
                string line2 = reader.ReadLine(); // 無視
                string line3 = reader.ReadLine();
                DateTimeWhenFinished = System.DateTime.ParseExact(line3, "yyyyMMddHHmmssfff", System.Globalization.CultureInfo.InvariantCulture);
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
    }
}