using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteInfo
{
    public int measure { get; }
    public int denominator { get; }
    public int numerator { get; }
    public int channel { get; }
    public int parameter { get; }

    private int prog;

    public NoteInfo(int mes, int den, int num, int ch, int para)
    {
        measure = mes;
        denominator = den;
        numerator = num;
        channel = ch;
        parameter = para;
        //Debug.Log("(" + mes + ", " + den + ", " + num + ", " + ch + ", " + para + ") is generated.");
    }

    public int msInMes(float bpm, int percent)
    {
        //Debug.Log("num / den : " + ((double)numerator / (double)denominator));
        //Debug.Log("mes : " + ((double)60 * 1000 * 4 / (double)bpm) * ((double)percent / FumenData.split));
        return (int)(((double)60 * 1000 * 4 / (double)bpm) * ((double)percent / FumenData.split) * ((double)numerator / (double)denominator));

    }

    public void setTiming(int ms)
    {
        prog = ms;
        //debugging();
    }

    public int getTiming()
    {
        return prog;
    }

    public void debugging()
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
