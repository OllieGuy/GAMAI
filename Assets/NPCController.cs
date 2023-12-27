using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    NPC npc;
    void Start()
    {
        NavMeshAgent navMeshAgent = GetComponent<NavMeshAgent>();
        npc = new NPC(navMeshAgent);
        npc.gameTimer = GetComponent<GameTimer>();
        npc.state = new MoveState(npc);
        npc.state.enterState(new Vector3(6f, 0.5f, 1.5f));
    }

    void Update()
    {
        npc.state.frameUpdate();
    }
}
