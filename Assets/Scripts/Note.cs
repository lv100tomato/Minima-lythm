using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour {
    private int start;
    private int reach;
    private FumenInfo fInfo;
    private NoteInfo nInfo;
    private float percentage;
    private SpriteRenderer sprite;
    private int[] judge;
    private bool added;
    private bool ini;

    public GameObject particle;

    private static float upper = 4.0f;
    private static float lower = -4.0f;
    private static float brake = 5.0f;

	// Use this for initialization
	void Start () {
        //GetComponent<Rigidbody2D>().velocity = new Vector2(0, -5);
        transform.position = new Vector3(10,-10);
        percentage = 0;
        sprite = GetComponent<SpriteRenderer>();
        judge = getJjudgeTiming();
        added = false;
        ini = false;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if(!ini)
        {
            if(nInfo != null && fInfo != null)
            {
                if (nInfo.channel < -1)
                {
                    sprite.color *= 0.25f;
                }

                ini = true;
            }
        }
        else
        {
            percentage = (float)(fInfo.getProgress() - start) / reach;
            if (percentage > 1) percentage = (percentage + brake - 1) / brake;

            transform.position = route(percentage);

            if (nInfo.channel < 0)
            {
                transform.localScale = new Vector3(percentage * 10, percentage * 0.05f + 0.01f);
            }
            else
            {
                transform.localScale = new Vector3(percentage, percentage * 0.3f);
            }

            //判定ライン内にいるときだけキューに入れる
            if (fInfo.getProgress() - start - reach > -1 * judge[judge.Length - 1] && nInfo.channel >= 0 && !added)
            {
                fInfo.canHitQueue.Add(this);
                added = true;
            }
            else if (fInfo.getProgress() - start - reach > judge[judge.Length - 1] && nInfo.channel >= 0)
            {
                if (fInfo.canHitQueue.Contains(this))
                {
                    fInfo.canHitQueue.Remove(this);
                    fInfo.addNotesRank(0);
                }
                sprite.color = new Color(0.5f, 0.5f, 0.5f);
            }

            if (fInfo.getProgress() - start >= reach + 500 && transform.position.y < -6.0f)
            {
                if (fInfo.canHitQueue.Contains(this))
                {
                    fInfo.canHitQueue.Remove(this);
                }
                Destroy(this.gameObject);
            }
        }

        
	}

    public void setFumenInfo(FumenInfo fi)
    {
        fInfo = fi;
    }

    public void setNoteInfo(NoteInfo ni)
    {
        nInfo = ni;
    }

    public void setBaseMs(int ms)
    {
        start = ms;
    }

    public void setReachTime(int ms)
    {
        reach = ms;
    }

    private Vector3 route(float percent)
    {
        //return Vector3.Lerp(new Vector3(transform.position.x, upper), new Vector3(transform.position.x, lower), (float)(fInfo.getProgress() - start) / reach);
        Vector3 launch = new Vector3(0, upper);
        Vector3 goal = new Vector3((channelToFloat(nInfo.channel)), lower);

        float x, y;
        float pxp = percent * percent * percent;
        
        x = launch.x * (1 - percent) + goal.x * percent;
        y = launch.y * (1 - pxp) + goal.y * pxp;

        return new Vector3(x, y);
    }

    private float channelToFloat(int ch)
    {
        if(ch < 0)
        {
            return 0;
        }
        else if(ch >= 11 && ch <= 15)
        {
            return ch - 13.5f;
        }
        else if(ch >= 16 && ch <= 17)
        {
            return -3.5f;
        }
        else if(ch >= 18 && ch <= 19)
        {
            return ch - 15.5f;
        }
        else
        {
            return channelToFloat(ch - 10);
        }

        return -100;
    }

    private int[] getJjudgeTiming()
    {
        int[] output = new int[3];

        output[0] = (FumenData.rank + 3) * 8;
        output[1] = (FumenData.rank + 3) * 16;
        output[2] = (FumenData.rank + 3) * 25;

        return output;
    }

    public void judging()
    {
        fInfo.canHitQueue.Remove(this);

        Color pCol = new Color();

        if (Mathf.Abs(fInfo.getProgress() - start - reach) < judge[0])
        {
            pCol = new Color(0.9f, 1, 1);
            fInfo.addNotesRank(3);
        }
        else if (Mathf.Abs(fInfo.getProgress() - start - reach) < judge[1])
        {
            pCol = new Color(1, 1, 0.2f);
            fInfo.addNotesRank(2);
        }
        else
        {
            pCol = new Color(1, 0.3f, 0);
            fInfo.addNotesRank(1);
        }

        launchParticle(pCol);

        Destroy(this.gameObject);
    }

    private void launchParticle(Color col)
    {
        GameObject p;

        for(int i = 0; i < 50; ++i)
        {
            p = Instantiate(particle, new Vector3(channelToFloat(nInfo.channel) + Random.Range(-0.25f, 0.25f) + Random.Range(-0.25f, 0.25f), lower), transform.rotation);
            p.GetComponent<Particle>().setcolor(col);
        }
    }
}
