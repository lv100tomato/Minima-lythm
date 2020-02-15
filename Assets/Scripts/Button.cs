using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 選曲に使うボタン
/// </summary>
public class Button : MonoBehaviour
{
    private MusicManager manager;
    private int id = -1; 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setMusicManager(MusicManager m)
    {
        manager = m;
    }

    public void setButtonId(int id)
    {
        this.id = id;
    }

    public void OnClickButton()
    {
        if(manager != null && id >= 0)
        {
            manager.StartGame(id);
        }
    }
}
