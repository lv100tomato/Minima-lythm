using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleButton : MonoBehaviour
{
    public enum Act
    {
        start,
        option,
        exit,
        back,
        timing
    }

    public Act action;

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
                SceneManager.LoadScene("SelectMusic");
                break;
            case Act.option:
                transform.parent.gameObject.GetComponent<UIMover>().MoveTo(new Vector2(0, 720));
                break;
            case Act.exit:
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #elif UNITY_STANDALONE
                UnityEngine.Application.Quit();
                #endif
                break;
            case Act.back:
                transform.parent.gameObject.GetComponent<UIMover>().MoveReset();
                break;
            case Act.timing:
                transform.parent.gameObject.GetComponent<UIMover>().MoveTo(new Vector2(-1280, 720));
                break;
        }
    }
}
