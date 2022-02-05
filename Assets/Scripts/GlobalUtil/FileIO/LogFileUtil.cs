using System.Collections;
using System.Collections.Generic;
using System.Linq;

using System.IO;

using UnityEngine;

public static class LogFileUtil
{
    public static string SaveDataDirPath = Application.dataPath + "/SaveData";

    /// <summary>
    /// 保存されているログファイルのパスを返す
    /// </summary>
    /// <param name="gameMode"></param>
    /// <param name="isTerminated"></param>
    /// <param name="isGuestResult">未実装</param>
    /// <returns></returns>
    public static string[] GetLogFileNameList(int gameMode, bool isTerminated, bool isGuestResult)
    {
        string logDirPath = GetLogDirPath(gameMode, isTerminated, isGuestResult);
        string[] res = Directory.GetFiles(logDirPath, "*.log").Select(x => Path.GetFileName(x)).ToArray();
        return res;
    }

    /// <summary>
    /// ログファイルのディレクトリを返す
    /// </summary>
    /// <param name="gameMode"></param>
    /// <param name="isTerminated"></param>
    /// <param name="isGuestResult">未実装</param>
    /// <returns></returns>
    public static string GetLogDirPath(int gameMode, bool isTerminated, bool isGuestResult)
    {
        string logDirPath = SaveDataDirPath;
        if (!isGuestResult)
        {
            switch (gameMode)
            {
                // MODE:NONSENSE
                case 0:
                    logDirPath += "/Nonsense";
                    break;
                default:
                    logDirPath += "/Nonsense";
                    break;
            }
            switch (isTerminated)
            {
                case false:
                    logDirPath += "/Completed";
                    break;
                case true:
                    logDirPath += "/Terminated";
                    break;
            }
            if (!Directory.Exists(logDirPath))
                Directory.CreateDirectory(logDirPath);
            return logDirPath;
        }
        else
        {
            Debug.Log("ゲスト記録機能は未実装です。");
            return "";
        }

    }
}
