using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    public NPC npc;
    [SerializeField] Material blue;
    [SerializeField] Material red;
    [SerializeField] Material yellow;
    [SerializeField] GameObject face;
    void Start()
    {
        NavMeshAgent navMeshAgent = GetComponent<NavMeshAgent>();
        npc = new NPC(navMeshAgent);
        MeshRenderer faceColour = face.GetComponent<MeshRenderer>();
        if (npc.interest == ArtefactType.Blue)
        {
            faceColour.material = blue;
        }
        else if (npc.interest == ArtefactType.Red)
        {
            faceColour.material = red;
        }
        else if (npc.interest == ArtefactType.Yellow)
        {
            faceColour.material = yellow;
        }
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
