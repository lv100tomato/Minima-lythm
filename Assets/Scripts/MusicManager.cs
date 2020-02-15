using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 譜面・曲データのファイル名や選曲のボタンの管理など
/// </summary>
public class MusicManager : MonoBehaviour
{
    public GameObject ButtonPref;
    public GameObject ScrollView;
    public GameObject KakuninBase;

    private GameObject ScrollPoint;

    static private string[][] fumensAndMusics;
    static private string[] ButtonTexts;
    static private string[] ButtonInfoTexts;
    static private string[] FumenHashes;
    static private string[] FumenScoreTexts;
    private GameObject[] Buttons;
    private int nowLoading;
    private string selectedMusic;
    private string selectedFumen;

    // Start is called before the first frame update
    void Start()
    {
        //Screen.SetResolution(1280, 720, false, 60);

        selectedMusic = "";
        selectedFumen = "";

        //譜面・曲のファイル名が読み込まれていないなら読み込む
        if (fumensAndMusics == null) fumensAndMusics = FumenAndMusicNames();

        //スクロールする画面の大きさを調整
        ScrollPoint = ScrollView.transform.Find("Viewport").transform.Find("Content").gameObject;
        ScrollPoint.GetComponent<RectTransform>().sizeDelta = new Vector3(0, 120 * fumensAndMusics[0].Length);

        nowLoading = 0; //読み込みが完了した譜面の数
        bool isFirst = ButtonTexts == null; //初回読み込みかどうか
        if (isFirst)
        {
            ButtonTexts = new string[fumensAndMusics[0].Length];
            ButtonInfoTexts = new string[fumensAndMusics[0].Length];
            FumenHashes = new string[fumensAndMusics[0].Length];
            FumenScoreTexts = new string[fumensAndMusics[0].Length];
        }
        Buttons = new GameObject[fumensAndMusics[0].Length];

        //各ボタンの配置とテキスト調整
        for(int i = 0; i < fumensAndMusics[0].Length; ++i)
        {
            GameObject button = Instantiate(ButtonPref, new Vector3(0, 0, 0), this.transform.rotation);
            button.transform.SetParent(ScrollPoint.transform);
            button.transform.localPosition = new Vector3(0, -60 - 120 * i, 0);
            Buttons[i] = button;

            if (isFirst)
            {
                ButtonTexts[i] = " now loading...";
                ButtonInfoTexts[i] = "";
                FumenScoreTexts[i] = "";
                FumenHashes[i] = "0";
            }
            else
            {
                nowLoading = fumensAndMusics[0].Length;
                FumenScoreTexts[i] = "MaxScore : " + UserData.LoadFumenScore(FumenHashes[i]);
            }

            button.transform.Find("Text").gameObject.GetComponent<Text>().text = ButtonTexts[i];
            button.transform.Find("InfoText").gameObject.GetComponent<Text>().text = FumenScoreTexts[i] + "\n" + ButtonInfoTexts[i];
            button.GetComponent<Button>().setMusicManager(this);
            button.GetComponent<Button>().setButtonId(i);

        }

        if (nowLoading == 0)
        {
            //読み込みの準備
            FumenData.SetLoadedFalse();
            StartCoroutine(FumenData.LoadFumen(fumensAndMusics[0][0], true));
            Debug.Log("Load Start");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(nowLoading < fumensAndMusics[0].Length)
        {
            if (FumenData.IsDataLoaded)
            {
                //読み込みが完了したら各種データを格納して次の読み込みに移る
                ButtonTexts[nowLoading] = " " + FumenData.title + "\n LEVEL : " + FumenData.playlevel;
                ButtonInfoTexts[nowLoading] = "composer : " + FumenData.artist;
                FumenHashes[nowLoading] = FumenData.GetFumenHash();
                Buttons[nowLoading].transform.Find("Text").gameObject.GetComponent<Text>().text = ButtonTexts[nowLoading];
                FumenScoreTexts[nowLoading] = "MaxScore : " + UserData.LoadFumenScore(FumenHashes[nowLoading]);
                Buttons[nowLoading].transform.Find("InfoText").gameObject.GetComponent<Text>().text = FumenScoreTexts[nowLoading] + "\n" + ButtonInfoTexts[nowLoading];
                
                //次の読み込みの準備
                FumenData.SetLoadedFalse();
                ++nowLoading;
                if (nowLoading < fumensAndMusics[0].Length) StartCoroutine(FumenData.LoadFumen(fumensAndMusics[0][nowLoading], true));
            }
        }
    }

    /// <summary>
    /// 譜面・曲データのファイル名を取得する
    /// </summary>
    /// <returns>ファイル名を格納した配列([0]に譜面、[1]に曲)</returns>
    private string[][] FumenAndMusicNames()
    {
        //拡張子で検索
        string[] bmsFiles = System.IO.Directory.GetFiles(Application.streamingAssetsPath, "*.bms", System.IO.SearchOption.AllDirectories);
        string[] bmeFiles = System.IO.Directory.GetFiles(Application.streamingAssetsPath, "*.bme", System.IO.SearchOption.AllDirectories);
        List<string> fumenFiles = bmsFiles.Concat(bmeFiles).ToList();

        List<string> musicFiles = new List<string>();
        for (int i = 0; i < fumenFiles.Count; ++i)
        {
            string place = System.IO.Path.GetDirectoryName (fumenFiles[i]);

            //譜面に対応する曲があるかを調べる
            string[] kouho = System.IO.Directory.GetFiles(place, "*.wav");
            if (kouho.Length < 1) kouho = System.IO.Directory.GetFiles(place, "*.ogg");
            if (kouho.Length < 1) kouho = System.IO.Directory.GetFiles(place, "*.mp3");

            if (kouho.Length < 1)
            {
                Debug.LogWarning("Music File Not Found");
                fumenFiles.Remove(fumenFiles[i]);
                --i;
            }
            else
            {
                musicFiles.Add(kouho[0]);
            }
        }

        string[][] output = new string[2][];
        output[0] = fumenFiles.ToArray();
        output[1] = musicFiles.ToArray();

        return output;
    }

    /// <summary>
    /// ボタンに対応する選曲確認画面に移動する
    /// </summary>
    /// <param name="id">ボタンのid</param>
    public void StartGame(int id)
    {
        selectedFumen = fumensAndMusics[0][id];
        selectedMusic = fumensAndMusics[1][id];

        ScrollView.GetComponent<UIMover>().Move(new Vector2(-1280, 0));
        KakuninBase.GetComponent<UIMover>().MoveTo(new Vector2(0, 0));

        KakuninBase.transform.Find("TitleAndLevel").gameObject.GetComponentInChildren<Text>().text = ButtonTexts[id];
    }

    /// <summary>
    /// 選曲画面に戻る
    /// </summary>
    public void BackToSelect()
    {
        ScrollView.GetComponent<UIMover>().MoveReset();
        KakuninBase.GetComponent<UIMover>().MoveReset();
    }

    /// <summary>
    /// 選曲を確定してシーン遷移を行う
    /// </summary>
    public void MoveToGameScene()
    {
        FumenInfo.SetFumenName(selectedFumen);
        FumenInfo.SetMusicName(selectedMusic);
        SceneManager.LoadScene("Otoge");
    }

    /// <summary>
    /// 譜面の数を取得する
    /// </summary>
    /// <returns>譜面数</returns>
    public static int FumenNums()
    {
        if (fumensAndMusics == null)
        {
            return -1;
        }
        else
        {
            return fumensAndMusics[0].Length;
        }
    }
}
