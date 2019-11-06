using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public GameObject ButtonPref;
    public GameObject ScrollView;
    public GameObject KakuninBase;

    private GameObject ScrollPoint;

    static private string[][] fumensAndMusics;
    static private string[] ButtonTexts;
    static private string[] ButtonInfoTexts;
    private GameObject[] Buttons;
    private int nowLoading;
    private string selectedMusic;
    private string selectedFumen;

    // Start is called before the first frame update
    void Start()
    {
        Screen.SetResolution(1280, 720, false, 60);

        selectedMusic = "";
        selectedFumen = "";

        if (fumensAndMusics == null) fumensAndMusics = FumenAndMusicNames();

        ScrollPoint = ScrollView.transform.Find("Viewport").transform.Find("Content").gameObject;
        ScrollPoint.GetComponent<RectTransform>().sizeDelta = new Vector3(0, 120 * fumensAndMusics[0].Length);

        nowLoading = 0;
        bool isFirst = ButtonTexts == null;
        if (isFirst)
        {
            ButtonTexts = new string[fumensAndMusics[0].Length];
            ButtonInfoTexts = new string[fumensAndMusics[0].Length];
        }
        Buttons = new GameObject[fumensAndMusics[0].Length];

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
            }
            else
            {
                nowLoading = fumensAndMusics[0].Length;
            }
            //button.transform.GetComponentInChildren<Text>().text = ButtonTexts[i];
            button.transform.Find("Text").gameObject.GetComponent<Text>().text = ButtonTexts[i];
            button.transform.Find("InfoText").gameObject.GetComponent<Text>().text = ButtonInfoTexts[i];
            button.GetComponent<Button>().setMusicManager(this);
            button.GetComponent<Button>().setButtonId(i);
        }

        if (nowLoading == 0)
        {
            FumenData.setLoadedFalse();
            StartCoroutine(FumenData.LoadFumen(fumensAndMusics[0][0], true));
            Debug.Log("Load Start");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(nowLoading < fumensAndMusics[0].Length)
        {
            if (FumenData.isDataLoaded)
            {
                ButtonTexts[nowLoading] = " " + FumenData.title + "\n LEVEL : " + FumenData.playlevel;
                ButtonInfoTexts[nowLoading] = "composer : " + FumenData.artist;
                //Buttons[nowLoading].transform.GetComponentInChildren<Text>().text = ButtonTexts[nowLoading];
                Buttons[nowLoading].transform.Find("Text").gameObject.GetComponent<Text>().text = ButtonTexts[nowLoading];
                Buttons[nowLoading].transform.Find("InfoText").gameObject.GetComponent<Text>().text = ButtonInfoTexts[nowLoading];
                FumenData.setLoadedFalse();
                ++nowLoading;
                if (nowLoading < fumensAndMusics[0].Length) StartCoroutine(FumenData.LoadFumen(fumensAndMusics[0][nowLoading], true));
            }
        }
    }

    private string[][] FumenAndMusicNames()
    {
        string[] bmsFiles = System.IO.Directory.GetFiles(Application.streamingAssetsPath, "*.bms", System.IO.SearchOption.AllDirectories);
        string[] bmeFiles = System.IO.Directory.GetFiles(Application.streamingAssetsPath, "*.bme", System.IO.SearchOption.AllDirectories);
        List<string> fumenFiles = bmsFiles.Concat(bmeFiles).ToList();

        List<string> musicFiles = new List<string>();
        for (int i = 0; i < fumenFiles.Count; ++i)
        {
            string place = System.IO.Path.GetDirectoryName (fumenFiles[i]);

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
                Debug.Log(fumenFiles[i]);
                musicFiles.Add(kouho[0]);
                Debug.Log(musicFiles[i]);
            }
        }

        string[][] output = new string[2][];
        output[0] = fumenFiles.ToArray();
        output[1] = musicFiles.ToArray();

        Debug.Log(output[0].Length + " = " + output[1].Length);

        return output;
    }

    public void startGame(int id)
    {
        selectedFumen = fumensAndMusics[0][id];
        selectedMusic = fumensAndMusics[1][id];

        ScrollView.GetComponent<UIMover>().Move(new Vector2(-1280, 0));
        KakuninBase.GetComponent<UIMover>().MoveTo(new Vector2(0, 0));

        KakuninBase.transform.Find("TitleAndLevel").gameObject.GetComponentInChildren<Text>().text = ButtonTexts[id];
    }

    public void backToSelect()
    {
        ScrollView.GetComponent<UIMover>().MoveReset();
        KakuninBase.GetComponent<UIMover>().MoveReset();
    }

    public void moveToGameScene()
    {
        FumenInfo.SetFumenName(selectedFumen);
        FumenInfo.SetMusicName(selectedMusic);
        SceneManager.LoadScene("Otoge");
    }

    public static int fumenNums()
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
