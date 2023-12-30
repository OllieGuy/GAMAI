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
        npc.globalGameTimer = GameObject.Find("Global Timer").GetComponent<GameTimer>();
        npc.currentDesire = Desire.Enter;
        npc.desireToStateExecution();
        //Debug.Log("Suuuueyyyy " + Room.locateRoom(new Vector2Int(0,3)));
        //npc.state = new MoveState(npc);
        //bool a = npc.state.enterState(new Vector3(2f, 0.5f, 2f));
        //if(!a)
        //{
        //    Debug.Log("tgejgiej4in");
        //}
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
