using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathsFunctions
{
    static float seed = 0; //Random assignment to start
    public static float baseHappinessValue(int value, float rarityMultiplier)
    {
        return Mathf.Clamp01((float)(42.196f + (-25157.86f / (598f + Math.Pow(value, 0.275f)))) * rarityMultiplier);
    }
    public static float distanceMultiplier(float distance)
    {
        return Mathf.Clamp01((float)(2.086f / (1f + Math.Pow(distance, 2.68f) / 10132.4f)) - 1.1f);
    }
    public static float donateValue(float happiness)
    {
        return Mathf.Clamp01((float)(1.05f - 1.02f / (1 + (Math.Pow(happiness,13) / 0.04f))));
    }
    public static int donateAmount(float happiness)
    {
        happiness = Mathf.Clamp(happiness, 0.5f, 1);
        int amount = (int)Mathf.Round((float)(609 - 11715.1f / (19 + (4 * Math.Pow(happiness, 3.94f)))));
        Debug.Log("donation amount: " + amount);
        return amount;
    }
    public static int randomValue(int lowBound, int upBound)
    {
        if(seed == 0)
        {
            System.Random rand = new System.Random();
            seed = (float)rand.NextDouble();
        }
        seed = (float)((Math.Sin((lowBound + seed) * 2916.68857) + 1) / 2);
        int output = (int)Mathf.Round(Mathf.Lerp(lowBound, upBound, seed));
        return output;
    }
}
