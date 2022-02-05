using UnityEngine;

public static class GlobalConsts
{
    // keyID
    public static readonly ushort KeyID_RShift = 0;
    public static readonly ushort KeyID_LShift = 1;
    public static readonly ushort KeyID_Space = 2;
    public static readonly ushort KeyID_Return = 200;
    public static readonly ushort KeyID_Esc = 201;

    // charID
    public static readonly ushort CharID_ShiftDown = 97;
    public static readonly ushort CharID_ShiftUp = 98;

    // アサインされた打鍵パターンの種類数 26*2 + 22*2 + Space
    public static readonly int NumOfKeyPatterns = 97;
    // 文字種数（文字にバインドされない打鍵パターンが 2 つあるので - 2 ）
    public static readonly int NumOfChars = NumOfKeyPatterns - 2;

    // ゲームモード別の定数
    // 0:Nonsense, 

    // 課題文字の左上座標
    // アンカーは (0, 1) つまり 親オブジェクト Assignment の左上からの相対距離で指定
    public static readonly int[] DisplayInitX = { 12 };
    public static readonly int[] DisplayInitY = { -10 };

    // 課題文字の Text Mesh Pro 表示をいくつずつズラすか
    public static readonly int[] DisplayCharXDiff = { 18 };
    public static readonly int[] DisplayCharYDiff = { -41 };

    // TrialData に渡すゲームモード変数と、それが示す意味
    public static readonly int[] AssignmentLength = { 360 };
    public static readonly int[] LapLength = { 36 };
    public static readonly int[] NumOfLaps = { 10 };

    // その他
    public static readonly int[] DefaultMissLimit = { 50 };
    public static readonly int[] MaxMissLimit = { 360 };


    // デザインがらみ

    // 色設定
    public static readonly Color TypedCharColor = new Color(0.25f, 0.15f, 0.15f, 0.1f);
}