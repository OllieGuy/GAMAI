using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static UnityEditorInternal.VersionControl.ListControl;

public abstract class NPCState
{
    protected NPC npc;
    public bool endState = false;
    public NPCState(NPC _npc)
    {
        npc = _npc;
    }
    public abstract void enterState();
    public abstract void frameUpdate();
    public abstract void tickUpdate();
    public abstract void turnUpdate();
    public abstract void exitState();
    protected void requiredUpdateCheck()
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
                npc.turnsInCurrentState++;
            }
        }
    }
    protected void stateExited()
    {
        //Debug.Log("exiting state");
        //endState = true;
        //npc.gameTimer.resetCounter();
        //Debug.Log("prev: " + npc.previousDesire + "new: " + npc.currentDesire);
        if (npc.currentDesire != Desire.Wander && npc.currentDesire != Desire.Panic)
        {
            npc.gameTimer.resetCounter();
        }
        endState = true;
    }
}

public class MoveState : NPCState
{
    private float sqrAcceptableReachedRadius = 0.001f; //ARBITARY VALUE ALERT!!! BEEP BEEP BEEP
    private Vector3 posLastTurn = new Vector3(0,1.5f,0);
    private float speed = 1.7f;
    public MoveState(NPC npc) : base(npc){}
    public override void enterState()
    {
        Vector3[] path = npc.pathfinding.findPath(npc.currentGoalPosition);
        npc.pathfinding.currentPath = path;
        npc.pathfinding.currentTargetIndex = 1;
        npc.pathfinding.currentTargetInPath = path[npc.pathfinding.currentTargetIndex];
        npc.state.tickUpdate();
    }
    public override void frameUpdate()
    {
        Vector3 currentPos = npc.pathfinding.moveTowardsCurrentTarget(speed);
        npc.gameObj.transform.position = currentPos;
        inRangeOfCurrentTargetInPathCalculations(currentPos);
        requiredUpdateCheck();
    }
    public override void tickUpdate()
    {
        Vector3 targetPosition = npc.pathfinding.currentPath[npc.pathfinding.currentTargetIndex];
        targetPosition.y = npc.gameObj.transform.position.y;
        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - npc.gameObj.transform.position);
        npc.gameObj.transform.rotation = targetRotation;
        if (npc.currentDesire != Desire.Enter && npc.currentDesire != Desire.Leave && npc.currentDesire != Desire.Panic)
        {
            npc.percieve();
        }
    }
    public override void turnUpdate()
    {
        npc.updateMemory();
        if ((posLastTurn - npc.gameObj.transform.position).magnitude < 0.1f || speed < 0)
        {
            if (UnityEngine.Random.value > 0.5f)
            {
                speed = -speed;
            }
        }
        posLastTurn = npc.gameObj.transform.position;
        if (npc.currentDesire == Desire.Wander)
        {
            npc.calculateDesire();
        }
    }
    public override void exitState()
    {
        stateExited();
    }
    private bool inRangeOfCurrentTargetInPathCalculations(Vector3 currentPos)
    {
        Vector2 dif = new Vector2(currentPos.x,currentPos.z) - new Vector2(npc.pathfinding.currentTargetInPath.x, npc.pathfinding.currentTargetInPath.z);
        if (dif.sqrMagnitude <= sqrAcceptableReachedRadius)
        {
            if (npc.pathfinding.currentTargetIndex != npc.pathfinding.pathLength)
            {
                //Debug.Log("moving to next node");
                npc.pathfinding.currentTargetIndex++;
                npc.pathfinding.currentTargetInPath = npc.pathfinding.currentPath[npc.pathfinding.currentTargetIndex];
            }
            else
            {
                Vector3 correctPos = new Vector3(npc.pathfinding.currentPath[npc.pathfinding.currentTargetIndex].x, 1.5f, npc.pathfinding.currentPath[npc.pathfinding.currentTargetIndex].z);
                npc.gameObj.transform.position = correctPos;
                //Debug.Log("reached target");
                exitState();
            }
            return true;
        }
        return false;
    }
    public bool reachablePosition(Vector3 _targetPos)
    {
        Vector3[] path = npc.pathfinding.findPath(_targetPos);
        if (path == null)
        {
            return false;
        }
        return true;
    }
}

public class VisitState : NPCState
{
    public VisitState(NPC npc) : base(npc) { }
    public override void enterState()
    {
        //Debug.Log("Entered Visit STATE");
        npc.gameObj.transform.LookAt(npc.objectCurrentlyVisiting.objectInstance.transform.position);
    }
    public override void frameUpdate()
    {
        requiredUpdateCheck();
    }
    public override void tickUpdate()
    {
        try
        {
            ObjectInstance testIfExists = npc.objectCurrentlyVisiting.objectInstance; //no worky
        }
        catch (MissingReferenceException e)
        {
            Debug.Log("object destroyed while visiting");
        }
    }
    public override void turnUpdate()
    {
        npc.updateMemory();
        if(npc.currentDesire == Desire.Donate)
        {
            //Debug.Log("Only here to donate");
            npc.happiness += npc.objectCurrentlyVisiting.objectInstance.calculateHappinessChange(npc, (DonationBox)npc.objectCurrentlyVisiting.objectInstance.theObject);
            exitState();
        }
        else if (UnityEngine.Random.value > (1 - (npc.turnsInCurrentState * 0.1f)))
        {
            //Debug.Log("Gross bored");
            npc.happiness += npc.objectCurrentlyVisiting.objectInstance.calculateHappinessChange(npc);
            exitState();
        }
        else
        {
            npc.happiness += npc.objectCurrentlyVisiting.objectInstance.calculateHappinessChange(npc);
        }
        //Debug.Log("really visitin. Current Happiness: " + npc.happiness);
    }
    public override void exitState()
    {
        npc.objectCurrentlyVisiting.objectInstance.updateOpenInteractionPoint(npc.objectCurrentlyVisiting.convOpenInteractionLocationToInt(), false);
        stateExited();
    }
}

public class PanicState : NPCState //Panic! at the museum
{
    private float sqrAcceptableReachedRadius = 0.001f; //ARBITARY VALUE ALERT!!! BEEP BEEP BEEP
    public PanicState(NPC npc) : base(npc) { }
    public override void enterState()
    {
        Vector3[] path = npc.pathfinding.findPath(npc.currentGoalPosition);
        npc.pathfinding.currentPath = path;
        npc.pathfinding.currentTargetIndex = 1;
        npc.pathfinding.currentTargetInPath = path[npc.pathfinding.currentTargetIndex];
        npc.state.tickUpdate();
    }
    public override void frameUpdate()
    {
        Vector3 currentPos = npc.pathfinding.moveTowardsCurrentTarget(3.4f);
        npc.gameObj.transform.position = currentPos;
        inRangeOfCurrentTargetInPathCalculations(currentPos);
        requiredUpdateCheck();
    }
    public override void tickUpdate()
    {
        Vector3 targetPosition = npc.pathfinding.currentPath[npc.pathfinding.currentTargetIndex];
        targetPosition.y = npc.gameObj.transform.position.y;
        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - npc.gameObj.transform.position);
        npc.gameObj.transform.rotation = targetRotation;
    }
    public override void turnUpdate()
    {
        Debug.Log("panicin aggghhh" + npc.happiness);
        npc.happiness -= 0.1f * npc.happiness;
    }
    public override void exitState()
    {
        stateExited();
    }
    private bool inRangeOfCurrentTargetInPathCalculations(Vector3 currentPos)
    {
        Vector2 dif = new Vector2(currentPos.x, currentPos.z) - new Vector2(npc.pathfinding.currentTargetInPath.x, npc.pathfinding.currentTargetInPath.z);
        if (dif.sqrMagnitude <= sqrAcceptableReachedRadius)
        {
            if (npc.pathfinding.currentTargetIndex != npc.pathfinding.pathLength)
            {
                npc.pathfinding.currentTargetIndex++;
                npc.pathfinding.currentTargetInPath = npc.pathfinding.currentPath[npc.pathfinding.currentTargetIndex];
            }
            else
            {
                exitState();
            }
            return true;
        }
        return false;
    }
}