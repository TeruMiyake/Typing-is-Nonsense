/// ゲーム全体で使用するクラスや構造体を定義

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq; // 配列初期化用

public class TrialData
{
    // ミスリミット
    public ushort missLimit = 50;
    // 正しく打ったキー数 = 次打つキーID。0 から始まり assignmentLength で打切
    public int typedKeys = 0;
    // 現在何ラップ目か 1-indexed
    public int nowLap = 1;
    // トータルタイム・ラップタイム・各正解打鍵タイム
    // それぞれのタイム・ミスは全て「合計タイム」で入れるので、打鍵時間を出すには引き算が必要
    // lap, key の配列の [0] は番兵
    public long totalTime = 0;
    public long[] lapTime;
    public long[] keyTime;
    // トータルミス・ラップミス・各キー辺りミス（要るか？）
    // それぞれのタイム・ミスは全て「合計タイム」で入れるので、打鍵時間を出すには引き算が必要
    // lap, key の配列の [0] は番兵
    public int totalMiss = 0;
    public int[] lapMiss;
    public int[] keyMiss;

    public ushort[] trialAssignment_CharID;
    public char[] trialAssignment_Char;

    public TrialData(ushort lim, int assignmentLength, int numOfLaps)
    {
        Debug.Log("New trialData was instantiated.");

        missLimit = lim;

        lapTime = Enumerable.Repeat<long>(0, numOfLaps + 1).ToArray();
        keyTime = Enumerable.Repeat<long>(0, assignmentLength + 1).ToArray();

        lapMiss = Enumerable.Repeat<int>(0, numOfLaps + 1).ToArray();
        keyMiss = Enumerable.Repeat<int>(0, assignmentLength + 1).ToArray();

        trialAssignment_CharID = new ushort[assignmentLength];
        trialAssignment_Char = new char[assignmentLength];
    }
}