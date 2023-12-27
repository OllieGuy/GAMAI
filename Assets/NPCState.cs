using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static UnityEditorInternal.VersionControl.ListControl;

public abstract class NPCState
{
    protected NPC npc;
    public NPCState(NPC _npc)
    {
        npc = _npc;
    }
    public abstract void enterState();
    public abstract bool enterState(Vector3 _targetPos);
    public abstract void frameUpdate();
    public abstract void tickUpdate();
    public abstract void turnUpdate();
    public abstract void exitState();
    public void requiredUpdateCheck()
    {
        float tickTime = GameTimer.TickTime;
        if (npc.gameTimer.gameTime >= tickTime)
        {
            npc.gameTimer.tickCount++;
            npc.gameTimer.gameTime -= tickTime;
            npc.state.tickUpdate();
            if(npc.gameTimer.tickCount >= GameTimer.TicksPerTurn)
            {
                npc.state.turnUpdate();
                npc.gameTimer.tickCount = 0;
            }
        }
    }
}

public class MoveState : NPCState
{
    private float sqrAcceptableReachedRadius = 0.05f; //ARBITARY VALUE ALERT!!! BEEP BEEP BEEP
    public MoveState(NPC npc) : base(npc){}
    public override void enterState()
    {
        Debug.Log("Error: should not have entered Move without target");
    }
    public override bool enterState(Vector3 _targetPos)
    {
        Vector3[] path = npc.pathfinding.findPath(_targetPos);
        if (path == null)
        {
            return false;
        }
        npc.pathfinding.currentPath = path;
        npc.pathfinding.currentTargetIndex = 1;
        npc.pathfinding.currentTargetInPath = path[npc.pathfinding.currentTargetIndex];
        npc.state.tickUpdate();
        return true;
    }
    public override void frameUpdate()
    {
        Vector3 currentPos = npc.pathfinding.moveTowardsCurrentTarget();
        npc.gameObj.transform.position = currentPos;
        requiredUpdateCheck();
    }
    public override void tickUpdate()
    {
        Vector3 currentPos = npc.gameObj.transform.position;
        Vector3 targetPosition = npc.pathfinding.currentPath[npc.pathfinding.currentTargetIndex];
        targetPosition.y = npc.gameObj.transform.position.y;
        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - npc.gameObj.transform.position);
        npc.gameObj.transform.rotation = targetRotation;
        if (inRangeOfCurrentTargetInPath(currentPos))
        {
            if (npc.pathfinding.currentTargetIndex != npc.pathfinding.pathLength)
            {
                //Debug.Log("moving to next node");
                npc.pathfinding.currentTargetIndex++;
                npc.pathfinding.currentTargetInPath = npc.pathfinding.currentPath[npc.pathfinding.currentTargetIndex];
            }
            else
            {
                npc.gameObj.transform.position = npc.pathfinding.currentPath[npc.pathfinding.currentTargetIndex];
                //Debug.Log("reached target");
                npc.state = new VisitState(npc);
            }
        }
        npc.perception.fireRaycasts(npc.gameObj.transform);
        Debug.Log("tick");
    }
    public override void turnUpdate()
    {
        Debug.Log("turn");
    }
    public override void exitState()
    {

    }

    private bool inRangeOfCurrentTargetInPath(Vector3 currentPos)
    {
        Vector2 dif = new Vector2(currentPos.x,currentPos.z) - new Vector2(npc.pathfinding.currentTargetInPath.x, npc.pathfinding.currentTargetInPath.z);
        if (dif.sqrMagnitude <= sqrAcceptableReachedRadius)
        {
            Debug.Log("in range");
            return true;
        }
        return false;
    }
}

public class VisitState : NPCState
{
    public VisitState(NPC npc) : base(npc) { }
    public override void enterState()
    {
        Debug.Log("Visit");
    }
    public override bool enterState(Vector3 _targetPos)
    {
        return true;
    }
    public override void frameUpdate()
    {
        requiredUpdateCheck();
    }
    public override void tickUpdate()
    {
        Debug.Log("Visitin");
    }
    public override void turnUpdate()
    {
        Debug.Log("really visitin");
    }
    public override void exitState()
    {

    }
}