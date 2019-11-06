using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMover : MonoBehaviour
{
    public float speed = 1.0f; 

    private Vector3 defaultPos;
    private Vector3 targetPos;
    private readonly float defaultSpeed = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        defaultPos = this.transform.localPosition;
        targetPos = defaultPos;
        if (speed < 0.1) speed = 0.1f;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.localPosition = (this.transform.localPosition * defaultSpeed + targetPos * speed * Time.deltaTime) / (defaultSpeed + speed * Time.deltaTime);
    }

    public void Move(Vector2 position)
    {
        targetPos += (Vector3)position;
    }

    public void MoveTo(Vector2 position)
    {
        targetPos = position;
    }

    public void MoveReset()
    {
        targetPos = defaultPos;
    }
}
