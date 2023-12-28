using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathsFunctions
{
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
        return Mathf.Clamp01((float)(1.05 - 1.02 / (1 + (Math.Pow(happiness,13) / 0.04))));
    }
}
