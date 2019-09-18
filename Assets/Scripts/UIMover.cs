using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMover : MonoBehaviour
{
    private Vector3 defaultPos;

    // Start is called before the first frame update
    void Start()
    {
        defaultPos = this.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Move(Vector2 position)
    {
        this.transform.localPosition += (Vector3)position;
    }

    public void MoveTo(Vector2 position)
    {
        this.transform.localPosition = position;
    }

    public void MoveReset()
    {
        this.transform.localPosition = defaultPos;
    }
}
