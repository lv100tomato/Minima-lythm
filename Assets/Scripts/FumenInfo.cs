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

public class FumenInfo : MonoBehaviour{

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

    private AudioSource audioS;
    private bool first;

    //以下は譜面を読み込む前に操作する
    private static string fumenName = "";   //譜面ファイルのありか
    private static string musicName = "";   //楽曲ファイルのありか
    private static bool auto = false;   //自動演奏するかどうか
    private static bool play = false;   
    private static int playerShift = 0; //プレイヤー側で決めるノーツタイミングのずれ(+で遅れる、-で早まる)
    private static char speed = (char)10;   //ノーツスピードの10倍の値(10なら等倍、5～99)
    private static int interval = 2000;    //ノーツが発射されてから到達するまでの時間
    private static int intervalBase = 4000;//上の計算のもとになる数
    private static decimal intervalMul = 2.0M;//速度倍率
    private static int count;   //譜面再生開始位置(ms)

    //以下は譜面を読み込むときに書き込む
    private static char level = (char)0;    //譜面のレベル
    private static int shift = 0;   //音源再生開始位置(ms)のずれ(+で遅れる、-で早まる)
    private static List<int[]> fumen = new List<int[]>();   //譜面データ
    private static AudioClip music; //Unityで曲を流す準備
    private static bool ready;  //譜面を再生する準備が出来ているかどうか
    private static FumenData data;  //譜面の基本情報
        //以下は読み込むときに使う
    private static bool ok1 = false;
    private static bool ok2 = false;
    private static bool ng = false;

    //最終的に音源を再生するのは、譜面が再生され始めてから
    //shift - playerShift + interval (ms)後になる

    //以下は譜面再生中に使う
    private int progress = 0;   //譜面再生位置(ms)
    private int[] notesRank;    //判定ごとの総数
    private int maxCombo, combo;

    private int outro;
    //private long mark = 0;   //譜面再生位置(ms)の基準点(BPMが変化するときとかに変更される)
    public List<Note> canHitQueue;//判定ライン内にあるノーツ達


    public FumenInfo() {

    }

    public static void SetFumenName(string newName)
    {
        fumenName = newName;
    }

    public static void SetMusicName(string newName)
    {
        musicName = newName;
    }

    public static void SetAuto(bool flag)
    {
        auto = flag;
    }

    public static void setTiming(int timing)
    {
        playerShift = timing;
    }

    public static void setSpeed(char spd)
    {
        speed = spd;
        interval = 20000 / (int)spd;
    }

    public static void setLevel(char lv)
    {
        level = lv;
    }

    public static void ResetCount(int pos = 0)
    {
        count = pos;
    }

    public static void PlayFlag(bool flag)
    {
        play = flag;
    }

    public void LoadData()
    {
        //譜面を読み込む処理が入る
        /*
         *  譜面の仕様について(大体BMSと同じなので違う部分を羅列していく)
         *  [ヘッダ]
         *  WAV??はWAV01のみを流す音源として使用。
         *  STAGEFILEはジャケット画像に使用。
         *  MIDIFILEやBMPは不使用。
         *  [メインデータ]
         *  小節とかの仕様はそのままで
         *  11～17、21～27のパラメータで指定する値は鳴らす音ではなくノーツの種類
         *  
         */

        //曲をDLするのは時間がかかるので並列処理させたい→Coroutineでなんとかできそう？
        ok1 = false;
        ok2 = false;
        ng = false;


        /*
        Parallel.For(0, 3, i =>
          {
              switch (i)
              {
                  case 0:
                      //曲を読み込む
                      //UnityWebRequest dataM = UnityWebRequest.Get(musicName);
                      //while (!dataM.isDone) ;
                      //music = dataM.GetAudioClip();

                      StartCoroutine("LoadMusic");

                      ok1 = true;
                      break;
                  case 1:
                      //譜面を読み込む
                      WWW dataF = new WWW(fumenName);
                      while (!dataF.isDone) ;
                      fumen = new List<int[]>();

                      //以下、譜面を読み込んでいく


                      ok2 = true;
                      break;
                  case 2:
                      //DL中の演出とか

                      break;
              }
          });
        /**/

        //StartCoroutine(LoadMusic());
        //StartCoroutine(LoadFumen());

    }

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
        /*
        string karifumen =
            "*---------------------- HEADER FIELD\n" +
            "#PLAYER 1\n" +
            "#GENRE Sample\n" +
            "#TITLE Sample\n" +
            "#ARTIST Sample\n" +
            "#BPM 120\n" +
            "#PLAYLEVEL 5\n" +
            "#TOTAL 100\n" +
            "#RANK 2\n" +
            "#STAGEFILE aaa.bmp\n" +
            "#bmp00 miss.bmp\n" +
            "#bmp01 1.bmp\n" +
            "#wav01 1.wav\n" +
            "\n" +
            "*----------------------MAIN DATA FIELD\n" +
            "\n" +
            "#00101:4F\n" +
            "#00104:0102030401020304\n" +
            "#00111:01010101\n" +
            "#07115:D2";
        */
        //FumenData.readData(karifumen);
        //FumenData.readNotesData(karifumen);

        //fumenName = Application.streamingAssetsPath + "/end_time_death.bms";
        //fumenName = Application.streamingAssetsPath + "/dive_K03redezigh.bms";
        //fumenName = Application.streamingAssetsPath + "/gengaozobt1afo.bme";
        //fumenName = Application.streamingAssetsPath + "/9-finalbeatmaniamix-.bms";
        //fumenName = Application.streamingAssetsPath + "/sq_07-01.bms";
        //fumenName = Application.streamingAssetsPath + "/_sasoribi_7light.bme";
        //fumenName = Application.streamingAssetsPath + "/_sasoribi_7izigen.bme";

        //StartCoroutine(LoadMusic(Application.streamingAssetsPath + "/End Time.wav"));
        //StartCoroutine(LoadMusic(Application.streamingAssetsPath + "/FREEDOM DiVE↓.wav"));
        //StartCoroutine(LoadMusic(Application.streamingAssetsPath + "/Ｇｅｎｇａｏｚｏ.wav"));
        //StartCoroutine(LoadMusic(Application.streamingAssetsPath + "/L9-finalbeatmaniamix-.wav"));
        //StartCoroutine(LoadMusic(Application.streamingAssetsPath + "/千年女王.wav"));
        //StartCoroutine(LoadMusic(Application.streamingAssetsPath + "/ピアノ協奏曲第１番蠍火 (なんでも吸い込むピンク色のための).wav"));
        //StartCoroutine(LoadMusic(Application.streamingAssetsPath + "/ピアノ協奏曲第１番蠍火 (なんでも詰め込む神域譜面のための).wav"));

        /*
        Debug.Log("load start");
        //LoadFumen();
        //string ftxt = "";
        FileInfo fi = new FileInfo(fumenName);
        try
        {
            // 一行毎読み込み
            using (StreamReader sr = new StreamReader(fi.OpenRead(), Encoding.UTF8))
            {
                ftxt = sr.ReadToEnd();
            }
        }
        catch (Exception e)
        {
            // 改行コード
            ftxt += "\n";
            Debug.Log("読み込みに失敗しました");
        }
        */

        //譜面を自動取得->ランダムに選ぶ
        /*
        string[] bmsFiles = System.IO.Directory.GetFiles(Application.streamingAssetsPath, "*.bms", System.IO.SearchOption.AllDirectories);
        string[] bmeFiles = System.IO.Directory.GetFiles(Application.streamingAssetsPath, "*.bme", System.IO.SearchOption.AllDirectories);
        string[] fumenFiles = bmsFiles.Concat(bmeFiles).ToArray();

        //Debug.Log(fumenFiles[0]);

        string[] musicFiles = new string[fumenFiles.Length];
        for(int i = 0; i < musicFiles.Length; ++i)
        {
            //Debug.Log(fumenFiles[i]);
            string place = fumenFiles[i].Substring(0, fumenFiles[i].LastIndexOf("\\"));
            //Debug.Log(place);

            string[] kouho = System.IO.Directory.GetFiles(place, "*.wav");
            if (kouho.Length < 1) kouho = System.IO.Directory.GetFiles(place, "*.ogg");
            if (kouho.Length < 1) kouho = System.IO.Directory.GetFiles(place, "*.mp3");

            if (kouho.Length < 1)
            {
                musicFiles[i] = "";
                Debug.LogWarning("Music File Not Found");
            }
            else musicFiles[i] = kouho[0];
        }

        int selection = UnityEngine.Random.Range(0, fumenFiles.Length - 1);
        fumenName = fumenFiles[selection];
        StartCoroutine(LoadMusic(musicFiles[selection]));
        */
        //

        FumenData.setLoadedFalse();
        StartCoroutine(FumenData.LoadFumen(fumenName));
        StartCoroutine(LoadMusic(musicName));

        //while (ftxt == null) ;

        count = 0;
        progress = 0 - interval;
        ready = false;
        first = false;
        canHitQueue = new List<Note>();
        notesRank = new int[4];
        for(int i = 0; i < 4; ++i)
        {
            notesRank[i] = 0;
        }

        progress = 0;
        maxCombo = 0;
        combo = 0;
        outro = 0;

        ComboScore.text = "0";
        MaxComboScore.text = "0";
        MissScore.text = "0";
        GoodScore.text = "0";
        GreatScore.text = "0";
        PerfectScore.text = "0";
        StatusText.text = "Click or Enter to Start";
        StatusText.fontSize = 70;
        //StatusText.text = Application.streamingAssetsPath;
        //StatusText.fontSize = 30;
        MultiIntervalText.text = "Speed\n   ↑\nx" + intervalMul + "\n   ↓";
    }

    // Update is called once per frame
    void Update()
    {
        if (ready)
        {
            //譜面を再生しているとき

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
                    Debug.Log("Music Start");
                }

                progress = (int)(audioS.time * 1000) + interval;
                count = (int)((float)progress * 60f / 1000f);
            }

            if (!audioS.isPlaying && FumenData.fumenIsFinished())
            {
                //譜面の再生が終わったとき
                if (outro <= 1200) StatusText.fontSize = outro / 15;

                if (outro < 1)
                {
                    if(notesRank[0] > 0)
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
                else SceneManager.LoadScene("SelectMusic");
                }

            List<NoteInfo> hoge = FumenData.GetNotes(progress);

            foreach (NoteInfo i in hoge)
            {
                if (i.channel >= 10 || i.channel < 0)
                {
                    //i.debugging();
                    GameObject newNote = (i.channel < 0)? Instantiate(Line, new Vector3((i.channel - 16), 15.0f, 0.0f), this.transform.rotation)
                                                        : Instantiate(Note, new Vector3((i.channel - 16), 15.0f, 0.0f), this.transform.rotation);
                    Note newNoteNote = newNote.GetComponent<Note>();
                    newNoteNote.setFumenInfo(this);
                    newNoteNote.setNoteInfo(i);
                    newNoteNote.setBaseMs(i.getTiming());
                    newNoteNote.setReachTime(interval);
                }
            }

            int hantei = getNumsOfKeysDown();

            for(int i = 0; i < hantei; ++i)
            {
                if (canHitQueue.Count == 0) break;

                canHitQueue[0].judging();
            }

            //Destroy(gameObject);
        }
        else
        {
            if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return)) && FumenData.isFumenLoaded)
            {
                ready = true;
                StatusText.text = "";
                StatusText.fontSize = 0;
                interval = (int)(intervalBase / intervalMul);
                MultiIntervalText.text = "";
                //MaxComboScore.text = FumenData.getMaxCombo().ToString();
                //audioS.clip = Music;
                //audioS.Play();
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (intervalMul < 10) intervalMul += 0.1M;
                MultiIntervalText.text = "Speed\n   ↑\nx" + intervalMul + "\n   ↓";
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (intervalMul > 0.5M) intervalMul -= 0.1M;
                MultiIntervalText.text = "Speed\n   ↑\nx" + intervalMul + "\n   ↓";
            }




            //読み込むとき
            if (ng)
            {
                //読み込みに失敗したときの処理
            }
            else
            {
                if(ok1 && ok2)
                {
                    //譜面と曲の両方が読み込めた時の処理
                    ready = true;
                }
            }
        }
    }

    public int getProgress()
    {
        return progress;
    }

    public void addNotesRank(int rank)
    {
        if(rank>=0 && rank < notesRank.Length)
        {
            ++notesRank[rank];

            switch (rank)
            {
                case 0:
                    MissScore.text = notesRank[rank].ToString();
                    combo = 0;
                    break;
                case 1:
                    GoodScore.text = notesRank[rank].ToString();
                    combo = 0;
                    break;
                case 2:
                    GreatScore.text = notesRank[rank].ToString();
                    ++combo;
                    break;
                case 3:
                    PerfectScore.text = notesRank[rank].ToString();
                    ++combo;
                    break;
            }

            maxCombo = (maxCombo < combo) ? combo : maxCombo;
            ComboScore.text = combo.ToString();
            MaxComboScore.text = maxCombo.ToString();
        }
    }

    private int getNumsOfKeysDown()
    {
        int output = 0;

        foreach(KeyCode i in Enum.GetValues(typeof(KeyCode)))
        {
            if (Convert.ToInt32(i) == 0) continue;

            if (Input.GetKeyDown(i)) ++output;
        }

        //Debug.Log("number of pushed : " + output);
        return output;
    }

}
