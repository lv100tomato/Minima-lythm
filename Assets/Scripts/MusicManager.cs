using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public GameObject ButtonPref;
    public GameObject ScrollPoint;

    static private string[][] fumensAndMusics;
    static private string[] ButtonTexts;
    private GameObject[] Buttons;
    private int nowLoading;

    // Start is called before the first frame update
    void Start()
    {
        Screen.SetResolution(1280, 720, false, 60);

        if (fumensAndMusics == null) fumensAndMusics = FumenAndMusicNames();

        ScrollPoint.GetComponent<RectTransform>().sizeDelta = new Vector3(0, 120 * fumensAndMusics[0].Length);

        //string[] fumenTitles = new string[fumensAndMusics[0].Length];
        //int[] fumenDifficulties = new int[fumensAndMusics[0].Length];
        //FumenInfo fInfo = new FumenInfo();
        nowLoading = 0;
        bool isFirst = ButtonTexts == null;
        if (isFirst) ButtonTexts = new string[fumensAndMusics[0].Length];
        Buttons = new GameObject[fumensAndMusics[0].Length];

        for(int i = 0; i < fumensAndMusics[0].Length; ++i)
        {
            GameObject button = Instantiate(ButtonPref, new Vector3(0, 0, 0), this.transform.rotation);
            button.transform.SetParent(ScrollPoint.transform);
            button.transform.localPosition = new Vector3(0, -60 - 120 * i, 0);
            Buttons[i] = button;

            if (isFirst)
            {
                //button.transform.GetComponentInChildren<Text>().text = "now loading...";
                ButtonTexts[i] = " now loading...";
                //button.transform.GetComponentInChildren<Text>().text = " " + FumenData.title + "\n LEVEL : " + FumenData.playlevel;
            }
            else
            {
                nowLoading = fumensAndMusics[0].Length;
            }
            button.transform.GetComponentInChildren<Text>().text = ButtonTexts[i];
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
            Debug.Log(fumensAndMusics[0][nowLoading] + " is Loading.");

            if (FumenData.isDataLoaded)
            {
                Debug.Log(fumensAndMusics[0][nowLoading] + " is Loaded.");
                //FumenInfo fInfo = new FumenInfo();
                ButtonTexts[nowLoading] = " " + FumenData.title + "\n LEVEL : " + FumenData.playlevel;
                Buttons[nowLoading].transform.GetComponentInChildren<Text>().text = ButtonTexts[nowLoading];
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
            string place = fumenFiles[i].Substring(0, fumenFiles[i].LastIndexOf("\\"));
            //Debug.Log(place);

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
        FumenInfo.SetFumenName(fumensAndMusics[0][id]);
        FumenInfo.SetMusicName(fumensAndMusics[1][id]);
        SceneManager.LoadScene("Otoge");
    }
}
