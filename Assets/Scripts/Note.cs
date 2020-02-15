using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 各ノートの動きを制御する
/// </summary>
public class Note : MonoBehaviour {
    private int start;
    private int reach;
    private FumenInfo fInfo;
    private NoteInfo nInfo;
    private float percentage;
    private SpriteRenderer sprite;
    private int[] judge;
    private bool missed;
    private bool ini;

    public GameObject particle;

    private static readonly float upper = 4.0f;
    private static readonly float lower = -4.0f;
    private static readonly float brake = 5.0f;

	// Use this for initialization
	void Start ()
    {
        transform.position = new Vector3(10,-10);
        percentage = 0;
        sprite = GetComponent<SpriteRenderer>();
        judge = GetJjudgeTiming();
        missed = false;
        ini = false;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if(!ini)
        {
            //最初だけ行う処理

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
            //基準値の更新
            percentage = (float)(fInfo.GetProgress() - start) / reach;
            if (percentage > 1) percentage = (percentage + brake - 1) / brake;
            else if (percentage < 0) percentage = 0;

            transform.position = Route(percentage);

            if (nInfo.channel < 0)
            {
                transform.localScale = new Vector3(percentage * 10, percentage * 0.05f + 0.01f);
            }
            else
            {
                transform.localScale = new Vector3(percentage, percentage * 0.3f);
            }

            //判定ライン内にいるときだけキューに入れる
            if (fInfo.GetProgress() - start - reach > -1 * judge[judge.Length - 1] && nInfo.channel >= 0 && !fInfo.canHitQueue.Contains(this) && !missed)
            {
                fInfo.canHitQueue.Add(this);
            }
            else if (fInfo.GetProgress() - start - reach > judge[judge.Length - 1] && nInfo.channel >= 0 && !missed)
            {
                if (fInfo.canHitQueue.Contains(this))
                {
                    fInfo.canHitQueue.Remove(this);
                    fInfo.AddNotesRank(0);
                    missed = true;
                }
                sprite.color = new Color(0.5f, 0.5f, 0.5f);
            }

            //ノーツが画面外に行ったら消去する
            if (fInfo.GetProgress() - start >= reach + 500 && transform.position.y < -6.0f)
            {
                if (fInfo.canHitQueue.Contains(this))
                {
                    fInfo.canHitQueue.Remove(this);
                }
                Destroy(this.gameObject);
            }
        }

        
	}

    /// <summary>
    /// 現在再生しているFumenInfoを設定
    /// </summary>
    /// <param name="fi">FumenInfo</param>
    public void SetFumenInfo(FumenInfo fi)
    {
        fInfo = fi;
    }

    /// <summary>
    /// 対応させるNoteInfoを設定
    /// </summary>
    /// <param name="ni">NoteInfo</param>
    public void SetNoteInfo(NoteInfo ni)
    {
        nInfo = ni;
    }

    /// <summary>
    /// ノートが出現するタイミングを再生時間(ms)基準で設定
    /// </summary>
    /// <param name="ms"></param>
    public void SetBaseMs(int ms)
    {
        start = ms;
    }

    /// <summary>
    /// 判定の基準になるタイミングをstart基準で設定
    /// </summary>
    /// <param name="ms"></param>
    public void SetReachTime(int ms)
    {
        reach = ms;
    }

    /// <summary>
    /// ノートの場所を調整
    /// </summary>
    /// <param name="percent"></param>
    /// <returns></returns>
    private Vector3 Route(float percent)
    {
        Vector3 launch = new Vector3(0, upper);
        Vector3 goal = new Vector3((ChannelToFloat(nInfo.channel)), lower);

        float x, y;
        float pxp = percent * percent * percent;
        
        x = launch.x * (1 - percent) + goal.x * percent;
        y = launch.y * (1 - pxp) + goal.y * pxp;

        return new Vector3(x, y);
    }

    /// <summary>
    /// 各チャンネルに対応する位置基準の値を返す
    /// </summary>
    /// <param name="ch"></param>
    /// <returns></returns>
    private float ChannelToFloat(int ch)
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
            return ChannelToFloat(ch - 10);
        }
    }

    /// <summary>
    /// 判定ごとの許容範囲になる時間(ms)を生成
    /// </summary>
    /// <returns>時間(ms)の配列</returns>
    private int[] GetJjudgeTiming()
    {
        int[] output = new int[3];

        output[0] = (FumenData.rank + 3) * 8;
        output[1] = (FumenData.rank + 3) * 16;
        output[2] = (FumenData.rank + 3) * 25;

        return output;
    }

    /// <summary>
    /// 判定を行い、ノートを消去する
    /// </summary>
    public void Judging()
    {
        fInfo.canHitQueue.Remove(this);

        Color pCol;

        if (Mathf.Abs(fInfo.GetProgress() - start - reach) < judge[0])
        {
            pCol = new Color(0.9f, 1, 1);
            fInfo.AddNotesRank(3);
        }
        else if (Mathf.Abs(fInfo.GetProgress() - start - reach) < judge[1])
        {
            pCol = new Color(1, 1, 0.2f);
            fInfo.AddNotesRank(2);
        }
        else
        {
            pCol = new Color(1, 0.3f, 0);
            fInfo.AddNotesRank(1);
        }

        LaunchParticle(pCol);

        Destroy(this.gameObject);
    }

    /// <summary>
    /// 指定した色のエフェクトを再生する
    /// </summary>
    /// <param name="col"></param>
    private void LaunchParticle(Color col)
    {
        GameObject p;

        for(int i = 0; i < 50; ++i)
        {
            p = Instantiate(particle, new Vector3(ChannelToFloat(nInfo.channel) + Random.Range(-0.25f, 0.25f) + Random.Range(-0.25f, 0.25f), lower), transform.rotation);
            p.GetComponent<Particle>().Setcolor(col);
        }
    }
}
