using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour
{
    private float speed;
    private float size = 0.15f;
    private int leftTime;
    private float progress;
    private Vector3 startPosition;

    // Start is called before the first frame update
    void Start()
    {
        speed = UnityEngine.Random.Range(-0.1f, 0.4f);
        leftTime = UnityEngine.Random.Range(300, 700);
        //startTime = DateTime.Now;
        progress = 0;
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        progress += Time.deltaTime * 1000;

        if(progress > leftTime)
        {
            Destroy(this.gameObject);
        }
        else
        {
            transform.position = new Vector3(startPosition.x, startPosition.y + speed * 16 * (progress - progress * progress / (leftTime * 2)) / leftTime);
            transform.localScale = new Vector3(size * (leftTime - progress) / leftTime, size * (leftTime - progress) / leftTime);
            //speed *= dim;
            //size *= dim;
        }



    }

    public void setcolor(Color col)
    {
        GetComponent<SpriteRenderer>().color = new Color(col.r / 4 + 0.25f, col.g / 4 + 0.25f, col.b / 4 + 0.25f);
    }
}
