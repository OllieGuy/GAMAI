using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    public NPC npc;
    void Start()
    {
        NavMeshAgent navMeshAgent = GetComponent<NavMeshAgent>();
        npc = new NPC(navMeshAgent);
        npc.gameTimer = GetComponent<GameTimer>();
        npc.globalGameTimer = GameObject.Find("Global Timer").GetComponent<GameTimer>();
        npc.currentDesire = Desire.Enter;
        npc.desireToStateExecution();
    }

    void Update()
    {
        npc.doUpdate();
    }

    public static void destroyNPC(GameObject theVictim)
    {
        Destroy(theVictim);
    }
}
