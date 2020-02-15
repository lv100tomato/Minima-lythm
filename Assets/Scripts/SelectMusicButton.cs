using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ボタンに対応する挙動を行う
/// </summary>
public class SelectMusicButton : MonoBehaviour
{
    public enum Act
    {
        start,
        option,
        title,
        back
    }

    public Act action;
    public GameObject Manager;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClickButton()
    {
        switch (action)
        {
            case Act.start:
                UserData.LoadData();
                Manager.GetComponent<MusicManager>().MoveToGameScene();
                break;
            case Act.option:
                break;
            case Act.title:
                SceneManager.LoadScene("TitleScene");
                break;
            case Act.back:
                Manager.GetComponent<MusicManager>().BackToSelect();
                break;
        }
    }
}
