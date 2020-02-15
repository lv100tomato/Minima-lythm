using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdjustInOption : MonoBehaviour
{
    public GameObject MoveMaster;
    public Text SpeedText;
    public Text TimingText;

    private bool canAdjust;

    // Start is called before the first frame update
    void Start()
    {
        UserData.LoadData();
        updateText();
        canAdjust = false;
    }

    // Update is called once per frame
    void Update()
    {
        if((MoveMaster.transform.localPosition - new Vector3(0,1440,0)).magnitude < 10)
        {
            canAdjust = true;

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                UserData.IntervalMul += 0.1M;
                updateText();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                UserData.IntervalMul -= 0.1M;
                updateText();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                UserData.PlayerShift += 1;
                updateText();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                UserData.PlayerShift -= 1;
                updateText();
            }
        }
        else
        {
            if (canAdjust)
            {
                UserData.SaveData();
            }

            canAdjust = false;
        }
    }

    private void updateText()
    {
        SpeedText.text = "Speed\n↑\nx" + UserData.IntervalMul + "\n↓";
        TimingText.text = "Timing\n\n←    →\n" + ((UserData.PlayerShift == 0) ? "±" : ((UserData.PlayerShift > 0) ? "+" : "")) + UserData.PlayerShift + "ms\n";
    }
}
