using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;

/// <summary>
/// BMSファイルからデータを抽出し、必要に応じてデータを渡す
/// </summary>
public class FumenData
{
    public static readonly int split = 256;  //ノーツのタイミングの詳細さ
    public static bool IsDataLoaded { get; private set; } = false;
    public static bool IsFumenLoaded { get; private set; } = false;

    public static int player;    //プレイスタイル(仮)(1～4)
    public static string genre;   //曲のジャンル
    public static string title;   //曲のタイトル
    public static string artist;  //曲のアーティスト
    public static float bpm;        //曲のBPM
    public static int playlevel;  //難易度
    public static int rank;      //判定の辛さ(かんたん:0～4:きびしい)
    public static int volwav;     //ボリューム(n%)
    public static int total;      //total値
    public static string wav;     //音源
    public static string stagefile;//ジャケット
    public static int shift;      //音源再生開始位置(ms)のずれ(+で遅れる、-で早まる) ←とりあえず0のままでいいや
    public static NoteInfo[] notes; //ノーツ達
    public static NoteInfo[] realNotes;//実際に流れてくるノーツ達

    private static int maxCombo;  //最大コンボ
    private static string hash;   //譜面のハッシュ

    private static int progress;  //譜面をどこまで読み込んだか(ms)
    private static int noteIndex;     //配列をどこまで読み込んだか
    //private static long measureProg1000;    //小節をどこまで読み込んだか(us)
    //private static int measure;   //現在の小節
    //private static int percent;   //現在の小節の長さ比率(x64)
    //private static int nowBpm;    //現在のBPM
    private static bool finished; //譜面を流し終わったかどうか

    FumenData()
    {
        player = 1;
        genre = "";
        title = "";
        artist = "";
        bpm = 120;
        playlevel = 0;
        rank = 2;
        volwav = 100;
        wav = "";
        stagefile = "";
        shift = 0;
        notes = new NoteInfo[0];
        realNotes = new NoteInfo[0];

        ResetProgress();
    }

    /// <summary>
    /// 文字列で渡された譜面のデータから楽曲情報などを取得する
    /// </summary>
    /// <param name="Text"></param>
    public static void ReadData(string Text)
    {
        
        //UTF-8文字列で取得した譜面データからヘッダー部分の情報を抜き出していく
        Parallel.For(0, 11, i =>
          {
              int start;
              int end;
              string edit;

              switch (i)
              {
                  case 0:
                      start = Text.IndexOf("#PLAYER");
                      if (start >= 0)
                      {
                          start = start + "#PLAYER".Length;
                          end = Text.IndexOf("\n", start);
                          if (end < 0) end = Text.Length;

                          edit = Text.Substring(start, end - start);
                          edit = edit.Trim();

                          player = int.Parse(edit);
                          //Debug.Log("PLAYER : " + player);
                      }
                      break;
                  case 1:
                      start = Text.IndexOf("#GENRE");
                      if (start >= 0)
                      {
                          start = start + "#GENRE".Length;
                          end = Text.IndexOf("\n", start);
                          if (end < 0) end = Text.Length;

                          edit = Text.Substring(start, end - start);
                          edit = edit.Trim();

                          genre = edit;
                          //Debug.Log("GENRE : " + genre);
                      }
                      break;
                  case 2:
                      start = Text.IndexOf("#TITLE");
                      if (start >= 0)
                      {
                          start = start + "#TITLE".Length;
                          end = Text.IndexOf("\n", start);
                          if (end < 0) end = Text.Length;

                          edit = Text.Substring(start, end - start);
                          edit = edit.Trim();

                          title = edit;
                          //Debug.Log("TITLE : " + title);
                      }
                      break;
                  case 3:
                      start = Text.IndexOf("#ARTIST");
                      if (start >= 0)
                      {
                          start = start + "#ARTIST".Length;
                          end = Text.IndexOf("\n", start);
                          if (end < 0) end = Text.Length;

                          edit = Text.Substring(start, end - start);
                          edit = edit.Trim();

                          artist = edit;
                          //Debug.Log("ARTIST : " + artist);
                      }
                      break;
                  case 4:
                      start = Text.IndexOf("#BPM");
                      if (start >= 0)
                      {
                          start = start + "#BPM".Length;
                          end = Text.IndexOf("\n", start);
                          if (end < 0) end = Text.Length;

                          edit = Text.Substring(start, end - start);
                          edit = edit.Trim();

                          bpm = float.Parse(edit);
                          //Debug.Log("BPM : " + bpm);
                      }
                      break;
                  case 5:
                      start = Text.IndexOf("#PLAYLEVEL");
                      if (start >= 0)
                      {
                          start = start + "#PLAYLEVEL".Length;
                          end = Text.IndexOf("\n", start);
                          if (end < 0) end = Text.Length;

                          edit = Text.Substring(start, end - start);
                          edit = edit.Trim();

                          playlevel = int.Parse(edit);
                          //Debug.Log("PLAYLEVEL : " + playlevel);
                      }
                      break;
                  case 6:
                      start = Text.IndexOf("#RANK");
                      if (start >= 0)
                      {
                          start = start + "#RANK".Length;
                          end = Text.IndexOf("\n", start);
                          if (end < 0) end = Text.Length;

                          edit = Text.Substring(start, end - start);
                          edit = edit.Trim();

                          rank = int.Parse(edit);
                          //Debug.Log("RANK : " + rank);
                      }
                      break;
                  case 7:
                      start = Text.IndexOf("#VOLWAV");
                      if (start >= 0)
                      {
                          start = start + "#VOLWAV".Length;
                          end = Text.IndexOf("\n", start);
                          if (end < 0) end = Text.Length;

                          edit = Text.Substring(start, end - start);
                          edit = edit.Trim();

                          volwav = int.Parse(edit);
                          //Debug.Log("VOLWAV : " + volwav);
                      }
                      break;
                  case 8:
                      start = Text.IndexOf("#WAV01", StringComparison.OrdinalIgnoreCase);
                      if (start >= 0)
                      {
                          start = start + "#WAV01".Length;
                          end = Text.IndexOf("\n", start);
                          if (end < 0) end = Text.Length;

                          edit = Text.Substring(start, end - start);
                          edit = edit.Trim();

                          wav = edit;
                          //Debug.Log("WAV01 : " + wav);
                      }
                      break;
                  case 9:
                      start = Text.IndexOf("#STAGEFILE");
                      if (start >= 0)
                      {
                          start = start + "#STAGEFILE".Length;
                          end = Text.IndexOf("\n", start);
                          if (end < 0) end = Text.Length;

                          edit = Text.Substring(start, end - start);
                          edit = edit.Trim();

                          stagefile = edit;
                          //Debug.Log("STAGEFILE : " + stagefile);
                      }
                      break;
                  default:
                      break;

              }
          });

        //ハッシュ値を求める
        hash = "";
        SHA1CryptoServiceProvider encoder = new SHA1CryptoServiceProvider();
        byte[] encoded = encoder.ComputeHash(System.Text.Encoding.UTF8.GetBytes(Text));
        for(int i = 0; i < encoded.Length; ++i)
        {
            hash += encoded[i].ToString();
        }
        encoder.Clear();

        IsDataLoaded = true;

        return;
    }

    /// <summary>
    /// 文字列で渡された譜面のデータからノーツの情報を取得する
    /// </summary>
    /// <param name="Text"></param>
    public static void ReadNotesData(string Text)
    {
        List<NoteInfo> karinotes = new List<NoteInfo>();
        maxCombo = 0;

        int start, end, d;
        string edit;

        start = 0;

        while(true)
        {
            start = Text.IndexOf("#", start);
            if (start < 0) break;

            char decide = Text.Substring(start + 1, 1)[0];

            if (Text.Substring(start - 1,1) == "\n" && decide >= '0' && decide <= '9')
            {
                int i, j;
                int.TryParse(Text.Substring(start + 1, 3), out i);  //何小節目か
                int.TryParse(Text.Substring(start + 4, 2), out j);  //チャンネル

                start = start + 7;  //検索開始位置をずらす
                end = Text.IndexOf("\n", start);    //行の終わりを取得
                if (end < 0) end = Text.Length;     //行の終わりがファイルの最後だった場合

                edit = Text.Substring(start, end - start);  //行の情報を表す文字列を抽出
                edit.Trim();

                d = edit.Length / 2;    //小節の分割数

                if (j != 2)
                {
                    while (edit.Length >= 2)
                    {
                        int hoge; //パラメータになる数値
                        if (j == 3)
                        {
                            hoge = int.Parse(edit.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                        }
                        else
                        {
                            hoge = StringToIntBMS(edit.Substring(0, 2));
                        }
                        if (hoge != 0) karinotes.Add(new NoteInfo(i, d, d - (edit.Length / 2), j, hoge));
                        edit = edit.Substring(2);
                    }
                }
                else
                {
                    karinotes.Add(new NoteInfo(i, 1, 0, j, (int)(float.Parse(edit.Substring(0)) * split)));
                }
            }
            else
            {
                start += 7;
            }
        }

        notes = karinotes.ToArray();
        NotesOddEvenSort();
        //NotesBubbleSort();
        SetNotesTiming();

        ResetProgress();

        IsFumenLoaded = true;
        finished = false;

        return;
    }

    /// <summary>
    /// ノードをバブルソートする
    /// </summary>
    private static void NotesBubbleSort()
    {
        bool sorted;
        NoteInfo mov;

        do
        {
            sorted = true;
            //Debug.Log("Not Sorted");

            for(int i = 0; i < notes.Length - 1; ++i)
            {
                if (notes[i] > notes[i + 1])
                {
                    mov = notes[i];
                    notes[i] = notes[i + 1];
                    notes[i + 1] = mov;
                    sorted = false;
                }
            }

        } while (!sorted);

        Debug.Log("Sorted");
    }

    /// <summary>
    /// ノードを奇偶転置ソートする
    /// </summary>
    private static void NotesOddEvenSort()
    {
        bool[] sorted = new bool[notes.Length - 1];
        bool allSorted = false;

        for(int i=0;i<sorted.Length; ++i)
        {
            sorted[i] = false;
        }

        do
        {
            //Debug.Log("Not Sorted");

            Parallel.For(0, notes.Length / 2, i =>
            {
                if (sorted[i * 2] == false)
                {
                    if (notes[i * 2] > notes[i * 2 + 1])
                    {
                        NoteInfo mov = notes[i * 2];
                        notes[i * 2] = notes[i * 2 + 1];
                        notes[i * 2 + 1] = mov;

                        sorted[i * 2] = false;
                        if (i * 2 + 1 < sorted.Length) sorted[i * 2 + 1] = false;
                        if (i * 2 - 1 >= 0           ) sorted[i * 2 - 1] = false;
                    }
                    else
                    {
                        sorted[i * 2] = true;
                    }
                }
            });

            Parallel.For(0, (notes.Length - 1)/ 2, i =>
            {
                if (sorted[i * 2 + 1] == false)
                {
                    if (notes[i * 2 + 1] > notes[i * 2 + 2])
                    {
                        NoteInfo mov = notes[i * 2 + 1];
                        notes[i * 2 + 1] = notes[i * 2 + 2];
                        notes[i * 2 + 2] = mov;

                        sorted[i * 2 + 1] = false;
                        if (i * 2 + 2 < sorted.Length) sorted[i * 2 + 2] = false;
                        if (i * 2 - 0 >= 0           ) sorted[i * 2 - 0] = false;
                    }
                    else
                    {
                        sorted[i * 2 + 1] = true;
                    }
                }
            });

            allSorted = true;
            for(int i=0; i<sorted.Length; ++i)
            {
                if(sorted[i] == false)
                {
                    allSorted = false;
                    break;
                }
            }
        } while (!allSorted);

        Debug.Log("Sorted");
    }


    public static void ResetProgress()
    {
        progress = 0;
        noteIndex = 0;
    }

    /// <summary>
    /// 前回に指定した時間から新たに指定した時間までのノーツを得る
    /// </summary>
    /// <param name="dest">新たに指定する時間</param>
    /// <returns>ノーツのリスト</returns>
    public static List<NoteInfo> GetNotes(int dest)
    {
        List<NoteInfo> output = new List<NoteInfo>();
        NoteInfo now;

        while (noteIndex < realNotes.Length)
        {
            now = realNotes[noteIndex];

            if (now.GetTiming() > dest) break;

            output.Add(now);
            ++noteIndex;
        }

        if(noteIndex >= realNotes.Length)
        {
            finished = true;    //もうこれ以上ノーツはない
        }

        progress = dest;

        return output;
    }

    /// <summary>
    /// ノーツのデータから譜面として流すものを生成し、ノーツごとの出現位置を計算する
    /// </summary>
    private static void SetNotesTiming()
    {
        //仮想的に譜面を再生しながら時間を計測するイメージ

        long measureProg1000 = 0;   //小節頭の位置(μs)
        int measure = 0;            //再生中の小節
        long reachMeasureProg1000 = 0;  //再生位置(μs)
        int percent = split;            //小節の大きさ
        float nowBpm = bpm;             //テンポ

        List<NoteInfo> output = new List<NoteInfo>();   //ノーツを格納するリスト
        NoteInfo now;   //処理を行うノーツ

        for(int i = 0;i < notes.Length; ++i)
        {
            now = notes[i];

            {
                while (reachMeasureProg1000 < measureProg1000 + now.MsInMes(nowBpm, percent) * 1000)
                {
                    //拍線を追加
                    NoteInfo changemes = new NoteInfo(now.measure, 0, 1, -2, 0);
                    changemes.SetTiming((int)(reachMeasureProg1000 / 1000));
                    output.Add(changemes);

                    reachMeasureProg1000 += (long)((((double)60 * 1000 / (double)nowBpm) * ((double)split / split)) * 1000); //1拍分すすめる
                }

                if (now.measure > measure)  //小節が変わったとき
                {
                    NoteInfo changemes;

                    //再生した小節の再生位置(us)の更新
                    measureProg1000 += (long)(((double)60 * 1000 * 4 / (double)nowBpm) * ((double)percent / split)) * 1000; //1小節ぶん進める
                    measureProg1000 += (long)(((double)60 * 1000 * 4 / (double)nowBpm) * ((double)split / split)) * 1000 * (now.measure - measure - 1); //現在の小節まで進める

                    while (reachMeasureProg1000 < measureProg1000 + now.MsInMes(nowBpm, percent) * 1000)
                    {
                        //拍線を追加
                        changemes = new NoteInfo(now.measure, 0, 1, -2, 0);
                        changemes.SetTiming((int)(reachMeasureProg1000 / 1000));
                        output.Add(changemes);

                        reachMeasureProg1000 += (long)((((double)60 * 1000 / (double)nowBpm) * ((double)split / split)) * 1000); //1拍分すすめる
                    }

                    reachMeasureProg1000 = measureProg1000;

                    //再生している小節の更新
                    measure = now.measure;

                    //小節の長さを元に戻す
                    percent = split;

                    //小節線を追加
                    changemes = new NoteInfo(now.measure, 0, 1, -1, 0);
                    changemes.SetTiming((int)(((double)now.MsInMes(nowBpm, percent) * 1000 + measureProg1000) / 1000));
                    output.Add(changemes);
                }

                if (now.channel == 2)
                {
                    //小節の長さを変える
                    percent = now.parameter;
                }
                else if (now.channel == 3)
                {
                    //BPMの変更
                    measureProg1000 += (long)(now.MsInMes(nowBpm, percent)) * 1000;

                    nowBpm = now.parameter;
                    measureProg1000 -= (long)(now.MsInMes(nowBpm, percent)) * 1000; //小節頭の更新

                }
                else
                {
                    //ノーツのタイミングをセット
                    now.SetTiming((int)(((double)now.MsInMes(nowBpm, percent) * 1000 + measureProg1000) / 1000));

                    if ((now.channel >= 10 && now.channel <= 19) || (now.channel >= 50 && now.channel <= 59))
                    {
                        output.Add(now);
                        if (now.channel >= 10) ++maxCombo;
                    }
                }

            }
        }

        realNotes = output.ToArray();
    }

    /// <summary>
    /// BMS形式の記法から数値に変換する
    /// </summary>
    /// <param name="hoge">文字列</param>
    /// <returns>数値</returns>
    private static int StringToIntBMS(string hoge)
    {
        int output = 0;

        for(int i = 0;i < hoge.Length; ++i)
        {
            output *= 36;
            int add = 0;

            switch(hoge.Substring(i, 1))
            {
                case "0":
                    add = 0;
                    break;
                case "1":
                    add = 1;
                    break;
                case "2":
                    add = 2;
                    break;
                case "3":
                    add = 3;
                    break;
                case "4":
                    add = 4;
                    break;
                case "5":
                    add = 5;
                    break;
                case "6":
                    add = 6;
                    break;
                case "7":
                    add = 7;
                    break;
                case "8":
                    add = 8;
                    break;
                case "9":
                    add = 9;
                    break;
                case "A":
                    add = 10;
                    break;
                case "B":
                    add = 11;
                    break;
                case "C":
                    add = 12;
                    break;
                case "D":
                    add = 13;
                    break;
                case "E":
                    add = 14;
                    break;
                case "F":
                    add = 15;
                    break;
                case "G":
                    add = 16;
                    break;
                case "H":
                    add = 17;
                    break;
                case "I":
                    add = 18;
                    break;
                case "J":
                    add = 19;
                    break;
                case "K":
                    add = 20;
                    break;
                case "L":
                    add = 21;
                    break;
                case "M":
                    add = 22;
                    break;
                case "N":
                    add = 23;
                    break;
                case "O":
                    add = 24;
                    break;
                case "P":
                    add = 25;
                    break;
                case "Q":
                    add = 26;
                    break;
                case "R":
                    add = 27;
                    break;
                case "S":
                    add = 28;
                    break;
                case "T":
                    add = 29;
                    break;
                case "U":
                    add = 30;
                    break;
                case "V":
                    add = 31;
                    break;
                case "W":
                    add = 32;
                    break;
                case "X":
                    add = 33;
                    break;
                case "Y":
                    add = 34;
                    break;
                case "Z":
                    add = 35;
                    break;
                default:
                    break;
            }

            output += add;
        }

        return output;
    }

    /// <summary>
    /// 譜面読み込みの進捗をリセットする
    /// </summary>
    public static void SetLoadedFalse()
    {
        IsDataLoaded = false;
        IsFumenLoaded = false;
    }

    /// <summary>
    /// 最大コンボ数を得る
    /// </summary>
    /// <returns>最大コンボ数</returns>
    public static int GetMaxCombo()
    {
        return maxCombo;
    }

    /// <summary>
    /// GetNotesで得られる全てのノーツを読み込んだかどうか
    /// </summary>
    /// <returns></returns>
    public static bool FumenIsFinished()
    {
        return finished;
    }

    /// <summary>
    /// ファイル名から譜面情報を読み取る
    /// </summary>
    /// <param name="fileName">ファイル名</param>
    /// <param name="isOnlyLoadData">ヘッダのみ読み込むかどうか</param>
    /// <returns></returns>
    public static IEnumerator LoadFumen(string fileName, bool isOnlyLoadData = false)
    {
        if (!fileName.Contains("://") && !fileName.Contains(":///"))
        {
            fileName = "file:///" + fileName;
        }

        WWW www = new WWW(fileName);
        yield return www;

        //string txt = www.text;
        //txt = ToUTF8(txt);
        byte[] bytes = www.bytes;
        string txt = Encoding.GetEncoding("shift_jis").GetString(bytes);

        Debug.Log("Open Fumen file Success!!");

        FumenData.ReadData(txt);
        if (!isOnlyLoadData) FumenData.ReadNotesData(txt);
    }

    /// <summary>
    /// 譜面データから得られるハッシュ文字列を取得する
    /// </summary>
    /// <returns>文字列</returns>
    public static string GetFumenHash()
    {
        return hash;
    }
}
