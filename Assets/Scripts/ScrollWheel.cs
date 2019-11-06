using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollWheel : MonoBehaviour
{
    public float speed = 1.0f;

    private Scrollbar bar;
    private static int index = 0;
    private int maxIndex = 0;
    private float target;
    private readonly float defaultSpeed = 0.1f;

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

        maxIndex = MusicManager.fumenNums();
        if (maxIndex < 0)
        {
            maxIndex = 0;
        }

        float wheel = Input.GetAxis("Mouse ScrollWheel");
        if (wheel < 0)
        {
            if (index < maxIndex)
            {
                ++index;
            }
        }
        else if (wheel > 0)
        {
            if (index > 0)
            {
                --index;
            }
        }

        if(maxIndex > 0)
        {
            target = 1.0f - ((float)index / maxIndex);
        }

        bar.value = (bar.value * defaultSpeed + target * speed * Time.deltaTime) / (defaultSpeed + speed * Time.deltaTime);
    }
}
