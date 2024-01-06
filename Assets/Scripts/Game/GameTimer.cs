using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    public float gameTime;
    public int tickCount;
    private static float timeScale = 1f;
    private static float ticksPerSecond = 2f;
    private static int ticksPerTurn = 5;

    public static float GameTimeScale { get { return timeScale; } }
    public static float TickTime { get { return (1/ticksPerSecond)/timeScale; } }
    public static float TicksPerTurn { get { return ticksPerTurn; } }
    void Start()
    {
        gameTime = 0f;
        tickCount = 0;
    }
    void Update()
    {
        gameTime += Time.deltaTime * timeScale;
    }
    public void resetCounter()
    {
        gameTime = 0;
        tickCount = 0;
    }
}
