using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_STANDALONE_WIN
/// https://github.com/Elringus/UnityRawInput
using UnityRawInput;
#endif


[CreateAssetMenu(fileName = "DefaultKeyBind", menuName = "ScriptableObjects/CreateDefaultKeyBind")]
public class DefaultKeyBind : ScriptableObject
{
    // 今は固定で JIS 配列のデータが入っているが、いつか US や UK などを作ってアセットにしていく
    public ushort[] RawKeyMap = new ushort [] {32, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 49, 50, 51, 52, 53, 54, 55, 56, 57, 48, 189, 222, 220, 192, 219, 187, 186, 221, 188, 190, 191, 226, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 16, 0, 13, 27, 0, 0, 0, 0, 0, 0, 0};
    public ushort[] NullKeyMap = new ushort [] {39, 84};
    // string CharCode などで持つべきか？ また、文字コードやフォントのデータも持つべきか？
    public char[] CharMap = " abcdefghijklmnopqrstuvwxyz1234567890-^@[;:],./\\ABCDEFGHIJKLMNOPQRSTUVWXYZ!\"#$%&'()=~|`{+*}<>?_]".ToCharArray();
}