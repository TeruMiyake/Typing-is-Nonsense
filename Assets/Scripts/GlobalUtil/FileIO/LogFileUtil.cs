using System.Collections;
using System.Collections.Generic;
using System.Linq;

using System.IO;

using UnityEngine;

/// <summary>
/// *.log 及び *.rcode に関するユーティリティメソッドを集めるクラス
/// （"Log"FileUtil と命名してしまったが、正直ミスった。そのうち名称を変更する）
/// </summary>
public static class LogFileUtil
{
    public static string SaveDataDirPath = Application.dataPath + "/SaveData";

    /// <summary>
    /// 保存されているログファイルの名前（パスではない）を返す
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
    /// ログファイルのディレクトリを返す。存在しなければ生成する
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
    /// <summary>
    /// 保存されている登録コードファイルの名前（パスではない）を返す
    /// </summary>
    /// <param name="gameMode"></param>
    /// <returns></returns>
    public static string[] GetRegistCodeFileNameList(int gameMode)
    {
        string registCodeDirPath = GetRegistCodeDirPath(gameMode);
        string[] res = Directory.GetFiles(registCodeDirPath, "*.rcode").Select(x => Path.GetFileName(x)).ToArray();
        return res;
    }
    /// <summary>
    /// RegistrationCode ファイルのディレクトリを返す。存在しなければ生成する
    /// </summary>
    /// <param name="gameMode"></param>
    /// <returns></returns>
    public static string GetRegistCodeDirPath(int gameMode)
    {
        string registCodeDirPath = SaveDataDirPath;
        switch (gameMode)
        {
            // MODE:NONSENSE
            case 0:
                registCodeDirPath += "/Nonsense/RegistrationCode";
                break;
            default:
                registCodeDirPath += "/Nonsense/RegistrationCode";
                break;
        }
        if (!Directory.Exists(registCodeDirPath))
            Directory.CreateDirectory(registCodeDirPath);
        return registCodeDirPath;
    }
}
