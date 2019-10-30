using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserData
{
    private static string[] keysName = { "playerShift", "intervalMul" };
    private static int playerShift = 0; //プレイヤー側で決めるノーツタイミングのずれ(+で遅れる、-で早まる)
    private static int mulMul = 10;
    private static int intervalMul = 1 * mulMul;//(1/mulMul)速度倍率

    public static int PlayerShift {
        get => playerShift;
        set
        {
            if (value < -1000)
            {
                playerShift = -1000;
            }
            else if (value > 1000)
            {
                playerShift = 1000;
            }
            else
            {
                playerShift = value;
            }
        }
    }

    public static decimal IntervalMul
    {
        get => (decimal)intervalMul / 10M;
        set
        {
            if (value < 0.5M)
            {
                intervalMul = (int)(0.5M * mulMul);
            }else if (value > 10)
            {
                intervalMul = 10 * mulMul;
            }
            else
            {
                intervalMul = (int)(value * mulMul);
            }
        }
    }

    public static void loadData()
    {
        playerShift = PlayerPrefs.GetInt(keysName[0], 0);
        intervalMul = PlayerPrefs.GetInt(keysName[1], 20);
    }

    public static void saveData()
    {
        PlayerPrefs.SetInt(keysName[0], playerShift);
        PlayerPrefs.SetInt(keysName[1], intervalMul);
    }
}
