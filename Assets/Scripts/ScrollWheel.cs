using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollWheel : MonoBehaviour
{
    public float speed = 1.0f;

    private Scrollbar bar;
    private static float pos = 1;
    private int maxIndex = 0;
    private float target;
    private readonly float defaultSpeed = 0.1f;
    private bool isFirst = true;

    void Start()
    {
        bar = GetComponent<Scrollbar>();
        maxIndex = MusicManager.fumenNums();
        if (maxIndex < 0)
        {
            maxIndex = 0;
        }
        target = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (speed < 0.1)
        {
            speed = 0.1f;
        }

        float step = 0;

        maxIndex = MusicManager.fumenNums();
        if (maxIndex < 0)
        {
            maxIndex = 0;
        }
        if(maxIndex > 0)
        {
            step = 1.0f / maxIndex;
        }

        float wheel = Input.GetAxis("Mouse ScrollWheel");
        if (wheel < 0)
        {
            pos -= step;

            if (pos < 0)
            {
                pos = 0;
            }
        }
        else if (wheel > 0)
        {
            pos += step;

            if (pos > 1)
            {
                pos = 1;
            }
        }

        //target = pos;

        if (isFirst)
        {
            isFirst = false;
            bar.value = pos;
        }
        else
        {
            if (Mathf.Abs(bar.value - target) < step * 0.01)
            {
                target = (target * defaultSpeed + pos * speed * Time.deltaTime) / (defaultSpeed + speed * Time.deltaTime);
                bar.value = target;
            }
            else
            {
                target = bar.value;
                pos = target;
            }
        }
    }
}
