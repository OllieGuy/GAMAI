using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public Interest interest;
    public Desire currentDesire;
    public NPCPathfinding pathfinding;
    public NPCState state;
    private MoveState moveState;
    private VisitState visitState;
    private LeaveState leaveState;
    private PanicState panicState;
    public GameTimer gameTimer;
    public GameTimer globalGameTimer;
    public Perception perception;
    public List<Artefact> visitedArtefacts = new List<Artefact>();
    public List<ObjectMemory> memorisedObjects = new List<ObjectMemory>();
    public List<Room> relevantRooms = new List<Room>();
    public NPC(NavMeshAgent _navMeshAgent)
    {
        moveState = new MoveState(this);
        visitState = new VisitState(this);
        leaveState = new LeaveState(this);
        panicState = new PanicState(this);
        gameObj = _navMeshAgent.gameObject;
        System.Random rand = new System.Random();
        happiness = 0.5f;
        turnLength = 5f;
        turnsSinceSpawn = 0;
        turnsInCurrentState = 0;
        interest = (Interest)rand.Next(0, Enum.GetValues(typeof(Interest)).Length);
        pathfinding = new NPCPathfinding(_navMeshAgent);
        perception = new Perception();
        currentDesire = Desire.Wander;
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
    public void updateMemory()
    {
        int memorisedObjectCount = memorisedObjects.Count;
        List<ObjectMemory> objectsToRemove = new List<ObjectMemory>();
        for (int i = 0; i < memorisedObjectCount; i++)
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
        foreach (ObjectMemory om in objectsToRemove)
        {
            Debug.Log("removed " + om.seenObject.name);
            memorisedObjects.Remove(om);
        }
    }
    public void calculateDesire()
    {
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
                    float desirability = calculateVisitObjectDesirability(o.seenObject);
                    memorisedObjectDictionary.Add(o.seenObject, desirability);
                    totalArtefactDesirability += desirability;
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
            Debug.Log("removed thing");
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
        Debug.Log("Visit: " + (visitDesirability * multiplier) + " Wander: " + (wanderDesirability * multiplier) + " Donate: " + (donateDesirability * multiplier) + " Leave: " + leaveDesirability); 
        Desire chosen = generateDesireFromDictionary();
        switch(chosen)
        {
            case (Desire.Visit):
                SelectedObjectInfo soi = generateObjectToVisitFromDictionary();
                Debug.Log(soi.objectInstance.theObject.objectName + " " + soi.openInteractionLocation);
                if(moveState.enterState(soi.openInteractionLocation))
                {
                    currentDesire = Desire.Visit;
                }
                else
                {
                    Debug.Log("Wanted to go to a thing but couldn't reach");
                    currentDesire = Desire.Wander;
                }
                break;
            case (Desire.Wander):
                Debug.Log("Wander");
                currentDesire = Desire.Wander;
                break;
            case (Desire.Donate):
                Debug.Log("Donate");
                currentDesire = Desire.Donate;
                break;
            case (Desire.Leave):
                Debug.Log("Leave");
                if (moveState.enterState(new Vector3(0, 0.5f, 0))) //replace with exit location
                {
                    currentDesire = Desire.Leave;
                }
                else
                {
                    currentDesire = Desire.Panic;
                }
                break;
            default:
                currentDesire = Desire.Wander;
                Debug.Log("uh oh");
                break;
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
                    memorisedObjectDictionary.Remove(kvp.Key);
                }
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
    }
    public void doUpdate()
    {
        state.frameUpdate();
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
        List<ObjectInstance> percievedObjects = Perception.convertToObjectInstanceList(perception.fireRaycasts(gameObj.transform));
        foreach (ObjectInstance oi in percievedObjects)
        {
            findAndUpdateObjectWithinMemory(oi);
        }
        //Debug.Log(memorisedObjects.Count);
    }
    bool checkForFake(Artefact artefact)
    {
        return false;
    }
    void updateHappiness(float change)
    {

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
    
}
public enum Interest
{
    Red,
    Yellow,
    Blue
}
public enum FrameTickTurn
{
    Frame,
    Tick,
    Turn
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
