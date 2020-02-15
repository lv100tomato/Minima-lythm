using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ノート毎の情報を管理する
/// </summary>
public class NoteInfo
{
    /// <summary>
    /// 小節
    /// </summary>
    public int measure { get; }
    /// <summary>
    /// 小節の分割数
    /// </summary>
    public int denominator { get; }
    /// <summary>
    /// 小節内の分割数に対する位置
    /// </summary>
    public int numerator { get; }
    /// <summary>
    /// ノーツの種類
    /// </summary>
    public int channel { get; }
    /// <summary>
    /// ノーツの持つパラメータ
    /// </summary>
    public int parameter { get; }
    /// <summary>
    /// ノーツの出現位置(ms)
    /// </summary>
    private int prog;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mes">小節</param>
    /// <param name="den">分割数</param>
    /// <param name="num">位置</param>
    /// <param name="ch">チャンネル</param>
    /// <param name="para">パラメータ</param>
    public NoteInfo(int mes, int den, int num, int ch, int para)
    {
        measure = mes;
        denominator = den;
        numerator = num;
        channel = ch;
        parameter = para;
    }

    /// <summary>
    /// 小節頭を基準にしたノーツの出現位置(ms)
    /// </summary>
    /// <param name="bpm">テンポ</param>
    /// <param name="percent">小節の長さ</param>
    /// <returns></returns>
    public int MsInMes(float bpm, int percent)
    {
        return (int)(((double)60 * 1000 * 4 / (double)bpm) * ((double)percent / FumenData.split) * ((double)numerator / (double)denominator));
    }

    /// <summary>
    /// ノーツの出現位置を設定
    /// </summary>
    /// <param name="ms"></param>
    public void SetTiming(int ms)
    {
        prog = ms;
    }

    /// <summary>
    /// ノーツの出現位置を取得
    /// </summary>
    /// <returns></returns>
    public int GetTiming()
    {
        return prog;
    }

    public void Debugging()
    {
        Debug.Log("(" + measure + ", " + denominator + ", " + numerator + ", " + channel + ", " + parameter + ") is on " + prog + " (ms).");
    }

    public static bool operator >(NoteInfo l, NoteInfo r)
    {
        if(l.measure == r.measure)
        {
            if (l.numerator * r.denominator == r.numerator * l.denominator)
            {
                if(l.channel == r.channel)
                {
                    return false;
                }
                else
                {
                    return l.channel > r.channel;
                }
            }
            else
            {
                return l.numerator * r.denominator > r.numerator * l.denominator;
            }
        }
        else
        {
            return l.measure > r.measure;
        }
    }

    public static bool operator <(NoteInfo l, NoteInfo r)
    {
        return r > l;
    }

    public static bool operator >=(NoteInfo l, NoteInfo r)
    {
        return !(r > l);
    }

    public static bool operator <=(NoteInfo l, NoteInfo r)
    {
        return !(l > r);
    }
}
