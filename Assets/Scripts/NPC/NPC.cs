using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.AI;

public class NPC
{
    public GameObject gameObj;
    public float happiness;
    public float turnLength;
    public int turnsSinceSpawn;
    public int turnsInCurrentState;
    public ArtefactType interest;
    public Desire currentDesire;
    public Desire previousDesire;
    public Vector3 currentGoalPosition;
    public NPCPathfinding pathfinding;
    public NPCState state;
    private MoveState moveState;
    private VisitState visitState;
    private PanicState panicState;
    private bool endOfDesire;
    public SelectedObjectInfo objectCurrentlyVisiting;
    public int stageOfCurrentDesire;
    public GameTimer gameTimer;
    public GameTimer globalGameTimer;
    public Perception perception;
    public List<ObjectInstance> visitedObjects = new List<ObjectInstance>();
    public List<ObjectMemory> memorisedObjects = new List<ObjectMemory>();
    public List<NPCMemory> memorisedNPCs = new List<NPCMemory>();
    public List<Room> relevantRooms = new List<Room>();
    public NPC(NavMeshAgent _navMeshAgent)
    {
        moveState = new MoveState(this);
        visitState = new VisitState(this);
        panicState = new PanicState(this);
        gameObj = _navMeshAgent.gameObject;
        System.Random rand = new System.Random();
        happiness = 0.5f;
        turnLength = 5f;
        turnsSinceSpawn = 0;
        turnsInCurrentState = 0;
        endOfDesire = false;
        interest = (ArtefactType)rand.Next(0, Enum.GetValues(typeof(ArtefactType)).Length);
        pathfinding = new NPCPathfinding(_navMeshAgent);
        perception = new Perception();
    }
    public void updateMemory()
    {
        int memorisedObjectCount = memorisedObjects.Count;
        List<ObjectMemory> objectsToRemove = new List<ObjectMemory>();
        for (int i = 0; i < memorisedObjectCount; i++)
        {
            try
            {
                if (memorisedObjects[i].turnsInMemory > memorisedObjects[i].seenObject.theObject.turnsInMemory)
                {
                    objectsToRemove.Add(memorisedObjects[i]);
                }
                else
                {
                    memorisedObjects[i].turnsInMemory++;
                }
            }
            catch (MissingReferenceException e)
            {
                objectsToRemove.Add(memorisedObjects[i]);
            }
            
        }
        foreach (ObjectMemory om in objectsToRemove)
        {
            //Debug.Log("removed " + om.seenObject.name);
            memorisedObjects.Remove(om);
        }
    }
    public void calculateDesire()
    {
        previousDesire = currentDesire;
        if (previousDesire == Desire.Leave || previousDesire == Desire.Panic)
        {
            currentDesire = Desire.Leave;
            return;
        }
        foreach (NPCMemory nm in memorisedNPCs)
        {
            if (nm.seenNPC.currentDesire == Desire.Panic)
            {
                currentDesire = Desire.Panic;
                return;
            }
        }
        if (previousDesire != Desire.Wander)
        {
            //Debug.Log("Last desire was NOT wander, adding wander buffer" + previousDesire);
            currentDesire = Desire.Wander;
            return;
        }
        Dictionary<Desire, float> desireDictionary = new Dictionary<Desire, float>();
        Dictionary<ObjectInstance, float> memorisedObjectDictionary = new Dictionary<ObjectInstance, float>();
        List<ObjectInstance> donationBoxes = new List<ObjectInstance>();
        List <ObjectMemory> badMemories = new List<ObjectMemory>();
        float totalArtefactDesirability = 0;
        foreach (ObjectMemory o in memorisedObjects)
        {
            try
            {
                if (o.seenObject.tag != "Donation Box")
                {
                    if(!visitedObjects.Contains(o.seenObject))
                    {
                        float desirability = calculateVisitObjectDesirability(o.seenObject);
                        memorisedObjectDictionary.Add(o.seenObject, desirability);
                        totalArtefactDesirability += desirability;
                    }
                    else
                    {
                        badMemories.Add(o);
                    }
                }
                else
                {
                    donationBoxes.Add(o.seenObject);
                }
            }
            catch (MissingReferenceException e) 
            {
                badMemories.Add(o);
            }
        }
        foreach (ObjectMemory o in badMemories)
        {
            //Debug.Log("removed thing");
            memorisedObjects.Remove(o);
        }
        float visitDesirability = totalArtefactDesirability; //the more artefacts an NPC has memorised, the less likely they are to wander
        float wanderDesirability = calculateWanderDesirability();
        float donateDesirability = calculateDonationDesirability(donationBoxes);
        float leaveDesirability = calculateLeaveDesirability();
        float totalDesire = visitDesirability + wanderDesirability + donateDesirability;
        float multiplier = (1 - leaveDesirability)/totalDesire;
        desireDictionary.Add(Desire.Visit, visitDesirability * multiplier);
        desireDictionary.Add(Desire.Wander, wanderDesirability * multiplier);
        desireDictionary.Add(Desire.Donate, donateDesirability * multiplier);
        desireDictionary.Add(Desire.Leave, leaveDesirability);
        //Debug.Log("Visit: " + (visitDesirability * multiplier) + " Wander: " + (wanderDesirability * multiplier) + " Donate: " + (donateDesirability * multiplier) + " Leave: " + leaveDesirability); 
        Desire chosen = generateDesireFromDictionary();
        switch(chosen)
        {
            case (Desire.Visit):
                SelectedObjectInfo soi = generateObjectToVisitFromDictionary();
                
                //Debug.Log(soi.objectInstance.theObject.objectName + " " + soi.openInteractionLocation);
                if (previousDesire != Desire.Visit)
                {
                    try
                    {
                        if (reachable(soi.openInteractionLocation, Desire.Visit, Desire.Wander))
                        {
                            objectCurrentlyVisiting = soi;
                            objectCurrentlyVisiting.objectInstance.updateOpenInteractionPoint(objectCurrentlyVisiting.convOpenInteractionLocationToInt(), true);
                        }
                    }
                    catch (NullReferenceException e) { }
                }
                else
                {
                    currentDesire = Desire.Wander;
                }
                break;
            case (Desire.Wander):
                //Debug.Log("Wander");
                currentDesire = Desire.Wander;
                break;
            case (Desire.Donate):
                SelectedObjectInfo donoBox = getClosestDonationBox();
                if(donoBox != null)
                {
                    if (reachable(donoBox.openInteractionLocation, Desire.Donate, Desire.Wander))
                    {
                        objectCurrentlyVisiting = donoBox;
                    }
                }
                else
                {
                    currentDesire = Desire.Wander;
                }
                break;
            case (Desire.Leave):
                //Debug.Log("Leave");
                reachable(new Vector3(0, 0.5f, 0), Desire.Leave, Desire.Panic); //replace with exit location
                break;
            default:
                currentDesire = Desire.Wander;
                //Debug.Log("uh oh");
                break;
        }
        if (previousDesire != currentDesire)
        {
            turnsInCurrentState = 0;
            stageOfCurrentDesire = 0;
        }
        turnsSinceSpawn++;
        bool reachable(Vector3 pos, Desire target, Desire targetFailed)
        {
            Vector2Int npcCurrentPos = new Vector2Int((int)Mathf.Round(gameObj.transform.position.x), (int)Mathf.Round(gameObj.transform.position.z));
            Pathfinding p = new Pathfinding();
            Vector2Int reachPos = new Vector2Int((int)Mathf.Round(pos.x), (int)Mathf.Round(pos.z));
            if (p.AStarSolveReturnBool(npcCurrentPos, reachPos)) //separate if so slightly less expensive
            {
                currentDesire = target;
                return true;
            }
            Debug.Log("Wanted to go to a thing but couldn't reach at " + pos);
            Debug.Log("Current desire when failed: " + currentDesire);
            currentDesire = targetFailed;
            return false;
        }
        Desire generateDesireFromDictionary()
        {
            float randomValue = UnityEngine.Random.value;
            float cumulativeProbability = 0;
            foreach (var kvp in desireDictionary)
            {
                cumulativeProbability += kvp.Value;
                if (randomValue <= cumulativeProbability)
                {
                    return kvp.Key;
                }
            }
            return Desire.Wander; //contingency plan
        }
        SelectedObjectInfo generateObjectToVisitFromDictionary()
        {
            Dictionary<ObjectInstance, float> normalisedMemorisedObjectDictionary = new Dictionary<ObjectInstance, float>();
            Dictionary<ObjectInstance, Vector3> memorisedObjectInteractionPositionDictionary = new Dictionary<ObjectInstance, Vector3>();
            float totalObjectDesire = 0;
            List<ObjectInstance> markedForDeath = new List<ObjectInstance>();
            foreach (var kvp in memorisedObjectDictionary) //need to add contingency if object is removed
            {
                Vector3? openInteractionLocation = kvp.Key.returnOpenInteractionPosition();
                if (openInteractionLocation != null)
                {
                    Vector3 realOpenInteractionLocation = openInteractionLocation.GetValueOrDefault(Vector3.zero);
                    memorisedObjectInteractionPositionDictionary.Add(kvp.Key, realOpenInteractionLocation);
                    totalObjectDesire += kvp.Value;
                }
                else
                {
                    markedForDeath.Add(kvp.Key);
                }
            }
            foreach (ObjectInstance oi in markedForDeath)
            {
                memorisedObjectDictionary.Remove(oi);
            }
            float objectMultiplier = 1 / totalObjectDesire;
            foreach (var kvp in memorisedObjectDictionary)
            {
                normalisedMemorisedObjectDictionary.Add(kvp.Key,kvp.Value * objectMultiplier);
            }
            float randomValue = UnityEngine.Random.value;
            float cumulativeProbability = 0;
            foreach (var kvp in normalisedMemorisedObjectDictionary)
            {
                cumulativeProbability += kvp.Value;
                if (randomValue <= cumulativeProbability)
                {
                    SelectedObjectInfo soi = new SelectedObjectInfo(kvp.Key, memorisedObjectInteractionPositionDictionary[kvp.Key]);
                    return soi;
                }
            }
            return null; //not much of a contingency plan this time - just pray it'll work
        }
        SelectedObjectInfo getClosestDonationBox()
        {
            if (donationBoxes.Count > 0)
            {
                List<ObjectInstance> orderedList = donationBoxes.OrderBy(obj => Vector3.Distance(obj.transform.position, gameObj.transform.position)).ToList();
                foreach (ObjectInstance oi in orderedList) //need to add contingency if object is removed
                {
                    Vector3? openInteractionLocation = oi.returnOpenInteractionPosition();
                    if (openInteractionLocation != null)
                    {
                        Vector3 realOpenInteractionLocation = openInteractionLocation.GetValueOrDefault(Vector3.zero);
                        return new SelectedObjectInfo(orderedList[0], realOpenInteractionLocation);
                    }
                }
            }
            return null; //will return null if there are no open positions on any seen donation boxes 
        }
    }
    public void doUpdate()
    {
        //Debug.Log("Current desire: " + currentDesire+ "Previous desire: " + previousDesire);
        //Debug.Log(state);
        if (state.endState)
        {
            if (endOfDesire)
            {
                //Debug.Log("this is the end");
                calculateDesire();
                stageOfCurrentDesire = 0;
                endOfDesire = false;
            }
            else
            {
                if(currentDesire != Desire.Wander)
                {
                    //stageOfCurrentDesire++;
                }
            }
            desireToStateExecution();
            state.endState = false;
        }
        state.frameUpdate();
    }
    public void desireToStateExecution()
    {
        switch(currentDesire)
        {
            case (Desire.Enter):
                //Debug.Log("Switch says enter" + stageOfCurrentDesire);
                enterStateExecution();
                break;
            case (Desire.Visit):
                //Debug.Log("Switch says visit" + stageOfCurrentDesire);
                visitStateExecution();
                break;
            case (Desire.Wander):
                //Debug.Log("Switch says wander" + stageOfCurrentDesire);
                wanderStateExecution();
                break;
            case (Desire.Donate):
                //Debug.Log("Switch says donate" + stageOfCurrentDesire);
                donateStateExecution();
                break;
            case (Desire.Panic):
                //Debug.Log("Switch says panic" + stageOfCurrentDesire);
                panicStateExecution();
                break;
            case (Desire.Leave):
                //Debug.Log("Switch says leave" + stageOfCurrentDesire);
                leaveStateExecution();
                break;
            default:
                
                break;
        }
    }
    private void enterStateExecution()
    {
        switch (stageOfCurrentDesire)
        {
            case 0:
                //Debug.Log("Entered Enter");
                currentGoalPosition = new Vector3(2, 1.5f, 2); //replace with entrance reference
                state = moveState;
                state.enterState();
                stageOfCurrentDesire++;
                break;
            default:
                currentDesire = Desire.Wander;
                state.exitState();
                endOfDesire = true;
                break;
        }
    }
    private void visitStateExecution()
    {
        switch (stageOfCurrentDesire)
        {
            case 0:
                //Debug.Log("Entered Visit");
                
                currentGoalPosition = objectCurrentlyVisiting.openInteractionLocation;
                state = moveState;
                state.enterState();
                stageOfCurrentDesire++;
                break;
            default:
                visitedObjects.Add(objectCurrentlyVisiting.objectInstance);
                state = visitState;
                state.enterState();
                stageOfCurrentDesire++;
                endOfDesire = true;
                break;
        }
    }
    private void wanderStateExecution()
    {
        switch (stageOfCurrentDesire)
        {
            case 0:
                //Debug.Log("Entered Wander");
                currentGoalPosition = randomOpenPosition();
                state = moveState;
                state.enterState();
                stageOfCurrentDesire++;
                break;
            case 1:
                //Debug.Log("Continuing to Wander in state " + state);
                currentGoalPosition = randomOpenPosition();
                state.enterState();
                break;
            default:
                endOfDesire = true;
                break;
        }
    }
    private void donateStateExecution()
    {
        //Debug.Log("Donate called at " + stageOfCurrentDesire);
        switch (stageOfCurrentDesire)
        {
            case 0:
                //Debug.Log("Entered Donate");
                currentGoalPosition = objectCurrentlyVisiting.openInteractionLocation;
                state = moveState;
                state.enterState();
                stageOfCurrentDesire++;
                break;
            default:
                //Debug.Log("Got 2 da box");
                state = visitState;
                visitState.enterState();
                stageOfCurrentDesire++;
                endOfDesire = true;
                break;
        }
    }
    private void panicStateExecution()
    {
        switch (stageOfCurrentDesire)
        {
            case 0:
                //Debug.Log("Entered Panic");
                currentGoalPosition = randomOpenPosition();
                state = panicState;
                state.enterState();
                stageOfCurrentDesire++;
                break;
            default:
                if (exitPossible())
                {
                    endOfDesire=true;
                }
                else
                {
                    currentGoalPosition = randomOpenPosition();
                    state = panicState;
                    state.enterState();
                }
                break;
        }
    }
    private void leaveStateExecution()
    {
        switch (stageOfCurrentDesire)
        {
            case 0:
                //Debug.Log("Entered Leave");
                if(exitPossible())
                {
                    state = moveState;
                    state.enterState();
                    stageOfCurrentDesire++;
                }
                else
                {
                    currentDesire = Desire.Panic;
                }
                break;
            case 1:
                Debug.Log("Boom");
                NPCController.destroyNPC(gameObj); //Destroys the game object - be careful
                break;
            default:
                endOfDesire = true;
                break;
        }
    }
    private Vector3 randomOpenPosition()
    {
        int randX;
        int randY;
        System.Random random = new System.Random();
        Vector2Int npcCurrentPos = new Vector2Int((int)Mathf.Round(gameObj.transform.position.x),(int)Mathf.Round(gameObj.transform.position.z));
        Pathfinding p = new Pathfinding();
        int safety = 0;
        while (safety < 40) //arbitary value. Very low chance something will not be able to be generated
        {
            randX = MathsFunctions.randomValue(0, Museum.grid.GetLength(0) - 1); //just gotta do this -1 unfortunately
            randY = MathsFunctions.randomValue(0, Museum.grid.GetLength(1) - 1);
            //if (Museum.grid[randX, randY].occupation == Occupation.None && moveState.reachablePosition(new Vector3(randX, 1.5f, randY)))
            Vector2Int newRandPos = new Vector2Int(randX, randY);
            if ((npcCurrentPos - newRandPos).sqrMagnitude>1 && Room.locateRoom(newRandPos) != -1)
            {
                if(p.AStarSolveReturnBool(npcCurrentPos, newRandPos)) //separate if so slightly less expensive
                {
                    return new Vector3(randX, 1.5f, randY);
                }
            }
            safety++;
        }
        return new Vector3(0, 1.5f, 0);
    }
    private bool exitPossible()
    {
        Pathfinding p = new Pathfinding();
        Vector2Int npcCurrentPos = new Vector2Int((int)Mathf.Round(gameObj.transform.position.x), (int)Mathf.Round(gameObj.transform.position.z));
        Vector2Int exitPos = new Vector2Int(0, 0); //change if the exit position changes
        if (p.AStarSolveReturnBool(npcCurrentPos, exitPos))
        {
            currentGoalPosition = new Vector3(exitPos.x, 1.5f, exitPos.y);
            return true;
        }
        return false;
    }
    private float calculateVisitObjectDesirability(ObjectInstance objInst)
    {
        float desirability = 0;
        desirability += objInst.theObject.baseHappinessValue;
        desirability = desirability * distanceMultiplier(objInst);
        return desirability;
    }
    private float calculateWanderDesirability()
    {
        if (currentDesire == Desire.Wander)
        {
            return 0.5f;
        }
        else if (currentDesire == Desire.Visit)
        {
            return 0.2f;
        }
        else if (currentDesire == Desire.Donate)
        {
            return 0.5f;
        }
        return 0;
    }
    private float calculateDonationDesirability(List<ObjectInstance> donationBoxes)
    {
        if(donationBoxes.Count > 0)
        {
            List<ObjectInstance> orderedList = donationBoxes.OrderBy(obj => Vector3.Distance(obj.transform.position, gameObj.transform.position)).ToList();
            return MathsFunctions.donateValue(happiness) * distanceMultiplier(orderedList[0]);
        }
        return 0;
    }
    private float calculateLeaveDesirability()
    {
        float desirability = turnsSinceSpawn * 0.1f; // 1/(second number) is how many turns itll take for NPCs to be guaranteed to leave
        return Mathf.Clamp01(desirability);
    }
    private float distanceMultiplier(ObjectInstance objInst)
    {
        float dist = (objInst.transform.position - gameObj.transform.position).magnitude;
        float value = MathsFunctions.distanceMultiplier(dist);
        return Mathf.Clamp01(value);
    }
    public void percieve()
    {
        List<GameObject>[] allSeenThings = perception.fireRaycasts(gameObj.transform);
        List<ObjectInstance> percievedObjects = Perception.convertToObjectInstanceList(allSeenThings[0]);
        List<NPC> percievedNPCs = Perception.convertToNPCList(allSeenThings[1]);
        foreach (ObjectInstance oi in percievedObjects)
        {
            findAndUpdateObjectWithinMemory(oi);
        }
        foreach (NPC theNPC in percievedNPCs)
        {
            findAndUpdateNPCWithinMemory(theNPC);
        }
        //Debug.Log(memorisedObjects.Count);
    }
    public void findAndUpdateObjectWithinMemory(ObjectInstance _seenObject)
    {
        int curIndex = memoryIndexCheck();
        if (curIndex != -1)
        {
            memorisedObjects[curIndex].turnsInMemory = 0;
        }
        else
        {
            memorisedObjects.Add(new ObjectMemory(_seenObject));
        }
        int memoryIndexCheck()
        {
            int i = 0;
            foreach (ObjectMemory om in memorisedObjects)
            {
                if (om.seenObject == _seenObject)
                { return i; }
                i++;
            }
            return -1;
        }
    }
    public void findAndUpdateNPCWithinMemory(NPC _seenNPC)
    {
        int curIndex = memoryIndexCheck();
        if (curIndex != -1)
        {
            memorisedNPCs[curIndex].turnsInMemory = 0;
        }
        else
        {
            memorisedNPCs.Add(new NPCMemory(_seenNPC));
        }
        int memoryIndexCheck()
        {
            int i = 0;
            foreach (NPCMemory nm in memorisedNPCs)
            {
                if (nm.seenNPC == _seenNPC)
                { return i; }
                i++;
            }
            return -1;
        }
    }
}
public class SelectedObjectInfo
{
    public ObjectInstance objectInstance;
    public Vector3 openInteractionLocation;
    public SelectedObjectInfo(ObjectInstance _objectInstance, Vector3 _openInteractionLocation)
    {
        objectInstance = _objectInstance;
        openInteractionLocation = _openInteractionLocation;
    }
    public Vector2Int convOpenInteractionLocationToInt()
    {
        Vector2Int pos = new Vector2Int();
        pos.x = (int)Mathf.Round(openInteractionLocation.x);
        pos.y = (int)Mathf.Round(openInteractionLocation.z);
        return pos;
    }


}
public enum Desire
{
    Enter,
    Leave,
    Visit,
    Wander,
    Donate,
    Panic
}
public class ObjectMemory
{
    public float desirability;
    public int turnsInMemory;
    public ObjectInstance seenObject;
    public ObjectMemory(ObjectInstance _seenObject)
    {
        turnsInMemory = 0;
        seenObject = _seenObject;
    }
}
public class NPCMemory
{
    public int turnsInMemory;
    public NPC seenNPC;
    public NPCMemory(NPC _seenNPC)
    {
        turnsInMemory = 0;
        seenNPC = _seenNPC;
    }
}
