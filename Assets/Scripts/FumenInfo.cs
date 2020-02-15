using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Text;
using System.Linq;

/// <summary>
/// FumenDataのデータをもとに、再生しながらノーツのオブジェクトを生成する
/// </summary>
public class FumenInfo : MonoBehaviour
{

    public GameObject Note;
    public GameObject Line;
    public AudioClip Music;
    public Text MaxComboScore;
    public Text ComboScore;
    public Text PerfectScore;
    public Text GreatScore;
    public Text GoodScore;
    public Text MissScore;
    public Text StatusText;
    public Text MultiIntervalText;
    public Text AdjustTimingText;
    public Text ScoreText;
    public Image BlackBack;

    private AudioSource audioS;
    private bool first;

    //以下は譜面を読み込む前に操作する
    private static string fumenName = "";   //譜面ファイルのありか
    private static string musicName = "";   //楽曲ファイルのありか
    //private static bool auto = false;   //自動演奏するかどうか
    private static int interval = 2000;    //ノーツが発射されてから到達するまでの時間
    private static readonly int intervalBase = 4000;//上の計算のもとになる数

    private static bool ready;  //譜面を再生する準備が出来ているかどうか

    //最終的に音源を再生するのは、譜面が再生され始めてから
    //shift - playerShift + interval (ms)後になる

    //以下は譜面再生中に使う
    private int progress = 0;   //譜面再生位置(ms)
    private int[] notesRank;    //判定ごとの総数
    private int maxCombo, combo;

    private static readonly int pauseTime = 1000;    //何ms巻き戻すか

    private int pauze = 0;  //ポーズ位置
    private bool isPause = false;   //ポーズ中かどうか

    private int outro;
    public List<Note> canHitQueue;//判定ライン内にあるノーツ達

    private int score;

    private static readonly int[] scoreUnit = { 0, 2, 4, 5 };

    public FumenInfo()
    {

    }

    public static void SetFumenName(string newName)
    {
        fumenName = newName;
    }

    public static void SetMusicName(string newName)
    {
        musicName = newName;
    }

    /// <summary>
    /// ファイル名をもとに曲を読み込む
    /// </summary>
    /// <param name="fileName">ファイル名</param>
    /// <returns></returns>
    public IEnumerator LoadMusic(string fileName)
    {
        if (!fileName.Contains("://") && !fileName.Contains(":///"))
        {
            fileName = "file:///" + fileName;
        }

        WWW www = new WWW(fileName);
        yield return www;

        AudioClip audioClip = www.GetAudioClip(false, true);
        if (audioClip.loadState != AudioDataLoadState.Loaded)
        {
            //ここにロード失敗処理
            Debug.Log("Failed to load AudioClip.");
            yield break;
        }

        Debug.Log("Open Audio file Success!!");

        audioS.clip = audioClip;
    }

    // Use this for initialization
    void Start()
    {
        audioS = GetComponent<AudioSource>();

        FumenData.SetLoadedFalse();
        StartCoroutine(FumenData.LoadFumen(fumenName)); //譜面データを読み込む
        StartCoroutine(LoadMusic(musicName));           //曲を読み込む

        progress = (interval > 0) ? 0 - interval : 0;
        ready = false;
        first = false;
        canHitQueue = new List<Note>();
        notesRank = new int[4];
        for (int i = 0; i < 4; ++i)
        {
            notesRank[i] = 0;
        }

        progress = 0;
        pauze = 0;
        isPause = false;
        maxCombo = 0;
        combo = 0;
        outro = 0;
        score = 0;

        ComboScore.text = "0";
        MaxComboScore.text = "0";
        MissScore.text = "0";
        GoodScore.text = "0";
        GreatScore.text = "0";
        PerfectScore.text = "0";
        StatusText.text = "Click or Enter to Start";
        StatusText.fontSize = 70;
        ScoreText.text = "";
        UpdateMulti();
        UpdateAdjust();
    }

    // Update is called once per frame
    void Update()
    {
        if (ready)
        {
            //譜面を再生しているとき

            if (!isPause)
            {
                if (progress < interval)
                {
                    progress += (int)(Time.deltaTime * 1000);
                }
                else
                {
                    if (!first)
                    {
                        first = true;
                        audioS.Play();
                        audioS.volume = 1.0f;
                        Debug.Log("Music Start");
                    }

                    progress = (int)(audioS.time * 1000) + interval;
                }

                if (!audioS.isPlaying && FumenData.FumenIsFinished())
                {
                    //譜面の再生が終わったとき
                    if (outro <= 1200) StatusText.fontSize = outro / 15 + 5;

                    if (outro < 1)
                    {
                        if (notesRank[0] > 0)
                        {
                            StatusText.text = "Finished";
                        }
                        else if (notesRank[1] > 0)
                        {
                            StatusText.text = "No Miss";
                        }
                        else if (notesRank[2] > 0)
                        {
                            StatusText.text = "Full Combo";
                        }
                        else
                        {
                            StatusText.text = "All Perfect";
                        }
                    }

                    if (outro < 5000) outro += (int)(Time.deltaTime * 1000);
                    else
                    {
                        int maxScore = UserData.LoadFumenScore(FumenData.GetFumenHash());
                        if (CalculateScore() > maxScore)
                        {
                            UserData.SaveFumenScore(FumenData.GetFumenHash(), (int)CalculateScore());
                            UserData.Save();
                            Debug.Log("Score Saved!");
                        }
                        SceneManager.LoadScene("SelectMusic");
                    }
                }
                else
                {
                    //譜面の再生が終わってないとき

                    if (Input.GetKeyDown(KeyCode.Escape) && progress > pauze)
                    {
                        StatusText.text = "PAUSE\n<size=40>press [Q] key to quit</size>";
                        BlackBack.color = new Color(BlackBack.color.r, BlackBack.color.g, BlackBack.color.b, 0.5f);
                        isPause = true;
                        pauze = progress;
                        audioS.volume = 0.0f;
                        audioS.Pause();
                    }

                    if(progress <= pauze)
                    {
                        audioS.volume = (1.0f - 1.1f * (float)(pauze - progress) / (float)pauseTime);
                    }
                }

                List<NoteInfo> hoge = FumenData.GetNotes(GetProgress());

                foreach (NoteInfo i in hoge)
                {
                    if (i.channel >= 10 || i.channel < 0)
                    {
                        //i.debugging();
                        GameObject newNote = (i.channel < 0) ? Instantiate(Line, new Vector3((i.channel - 16), 15.0f, 0.0f), this.transform.rotation)
                                                            : Instantiate(Note, new Vector3((i.channel - 16), 15.0f, 0.0f), this.transform.rotation);
                        Note newNoteNote = newNote.GetComponent<Note>();
                        newNoteNote.SetFumenInfo(this);
                        newNoteNote.SetNoteInfo(i);
                        newNoteNote.SetBaseMs(i.GetTiming());
                        newNoteNote.SetReachTime(interval);
                    }
                }

                int hantei = GetNumsOfKeysDown();

                for (int i = 0; i < hantei; ++i)
                {
                    if (canHitQueue.Count == 0) break;

                    canHitQueue[0].Judging();
                }

                if (hantei > 0)
                {
                    UpdateScore();
                }

            }
            else
            {
                //ポーズ中
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    StatusText.text = "";
                    BlackBack.color = new Color(BlackBack.color.r, BlackBack.color.g, BlackBack.color.b, 0.0f);
                    isPause = false;
                    if(audioS.time >= (float)pauseTime/1000f)
                    {
                        audioS.time -= (float)pauseTime / 1000f;
                        audioS.Play();
                        audioS.volume = 0.0f;
                    }
                    else
                    {
                        audioS.time = 0;
                        first = false;
                    }
                    progress -= pauseTime;
                    canHitQueue.Clear();
                }
                else if (Input.GetKeyDown(KeyCode.Q))
                {
                    SceneManager.LoadScene("SelectMusic");
                }

            }
        }
        else
        {
            //譜面再生前

            if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return)) && FumenData.IsFumenLoaded)
            {
                ready = true;
                StatusText.text = "";
                interval = (int)(intervalBase / UserData.IntervalMul);
                MultiIntervalText.text = "";
                AdjustTimingText.text = "";
                UpdateScore();
                progress = (interval > 0) ? 0 - interval : 0;
                UserData.SaveData();
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                UserData.IntervalMul += 0.1M;
                UpdateMulti();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                UserData.IntervalMul -= 0.1M;
                UpdateMulti();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                UserData.PlayerShift += 1;
                UpdateAdjust();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                UserData.PlayerShift -= 1;
                UpdateAdjust();
            }
        }
    }

    /// <summary>
    /// タイミングをずらした量を考慮した再生位置(ms)
    /// </summary>
    /// <returns></returns>
    public int GetProgress()
    {
        return progress - UserData.PlayerShift;
    }

    /// <summary>
    /// 判定ごとにカウントしていく
    /// </summary>
    /// <param name="rank">判定の種類</param>
    public void AddNotesRank(int rank)
    {
        if (rank >= 0 && rank < notesRank.Length)
        {
            ++notesRank[rank];

            switch (rank)
            {
                case 0:
                    MissScore.text = notesRank[rank].ToString();
                    score += scoreUnit[rank];
                    combo = 0;
                    break;
                case 1:
                    GoodScore.text = notesRank[rank].ToString();
                    score += scoreUnit[rank];
                    combo = 0;
                    break;
                case 2:
                    GreatScore.text = notesRank[rank].ToString();
                    score += scoreUnit[rank];
                    ++combo;
                    break;
                case 3:
                    PerfectScore.text = notesRank[rank].ToString();
                    score += scoreUnit[rank];
                    ++combo;
                    break;
            }

            maxCombo = (maxCombo < combo) ? combo : maxCombo;
            ComboScore.text = combo.ToString();
            MaxComboScore.text = maxCombo.ToString();
        }
    }

    /// <summary>
    /// いくつボタンが押されたかを取得
    /// </summary>
    /// <returns>押されたボタンの数</returns>
    private int GetNumsOfKeysDown()
    {
        int output = 0;

        foreach (KeyCode i in Enum.GetValues(typeof(KeyCode)))
        {
            if (Convert.ToInt32(i) == 0 || i == KeyCode.Escape) continue;

            if (Input.GetKeyDown(i)) ++output;
        }

        return output;
    }

    /// <summary>
    /// 再生倍率の文言を更新
    /// </summary>
    private void UpdateMulti()
    {
        MultiIntervalText.text = "Speed\n   ↑\nx" + UserData.IntervalMul + "\n   ↓";
    }

    /// <summary>
    /// タイミング調整の文言を調整
    /// </summary>
    private void UpdateAdjust()
    {
        AdjustTimingText.text = "Timing\n←    →\n" + ((UserData.PlayerShift == 0) ? "±" : ((UserData.PlayerShift > 0) ? "+" : "")) + UserData.PlayerShift + "ms";
    }

    /// <summary>
    /// スコアの更新
    /// </summary>
    private void UpdateScore()
    {
        ScoreText.text = "Score\n" + CalculateScore().ToString("D9");
    }

    /// <summary>
    /// スコアの計算
    /// </summary>
    /// <returns></returns>
    private long CalculateScore()
    {
        if (FumenData.GetMaxCombo() > 0)
        {
           return ((score * (long)100000000) / (FumenData.GetMaxCombo() * scoreUnit[3]));
        }
        else
        {
            return 0;
        }
    }
}
