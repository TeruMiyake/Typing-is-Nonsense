/// ゲーム全体で使用するクラスや構造体を定義
/// using System; は UnityEngine と重複する部分があるので行わず、毎回明示的に使う
using UnityEngine;


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
/// ミリ秒型
/// long <-> MilliSecond は暗黙的に型変換できる
/// MilliSecond -> string は暗黙的に型変換できる（逆はエラー処理が面倒なので対応しない）
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
    /// <summary>
    /// MilliSecond の値を「keys キーの打鍵に要した時間」と捉えたときの double CPS を返す
    /// </summary>
    /// <param name="keys">MilliSecond ms の間に打ったキーの数</param>
    /// <returns></returns>
    public double ToCPS(int keys)
    {
        double cps = (double)(keys * 1000) / ms;
        return cps;
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
