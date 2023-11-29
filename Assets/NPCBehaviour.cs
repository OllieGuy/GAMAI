using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCBehaviour : MonoBehaviour
{
    public NPC npc;
    void Start()
    {
        npc = new NPC();
    }
    void Update()
    {
        
    }
}

public abstract class NPCAbstract
{
    public abstract void EnterState(NPC npc);
    public abstract void UpdateState(NPC npc);
}
public class WalkToPosition : NPCAbstract
{
    public override void EnterState(NPC npc)
    {
        //npc.
    }
    public override void UpdateState(NPC npc)
    {

    }
}

public class ExamineArtefact
{

}

public class Wander
{

}