using System;
using System.Collections;
using System.Collections.Generic;
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
    public Interest interest;
    public NPCPathfinding pathfinding;
    public NPCState state;
    public GameTimer gameTimer;
    public GameTimer globalGameTimer;
    public Perception perception;
    public List<Artefact> visitedArtefacts = new List<Artefact>();
    public List<ObjectMemory> memorisedObjects = new List<ObjectMemory>();
    public List<Room> relevantRooms = new List<Room>();
    public NPC(NavMeshAgent _navMeshAgent)
    {
        gameObj = _navMeshAgent.gameObject;
        System.Random rand = new System.Random();
        happiness = 0.5f;
        turnLength = 5f;
        turnsSinceSpawn = 0;
        interest = (Interest)rand.Next(0, Enum.GetValues(typeof(Interest)).Length);
        pathfinding = new NPCPathfinding(_navMeshAgent);
        perception = new Perception();
    }
    public void findAndUpdateObjectWithinMemory(ObjectInstance _seenObject)
    {
        int curIndex = memoryIndexCheck();
        if (curIndex != -1)
        {
            memorisedObjects[curIndex].seenTime = globalGameTimer.gameTime;
        }
        else
        {
            memorisedObjects.Add(new ObjectMemory(globalGameTimer.gameTime, _seenObject));
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
    void calculateDesire()
    {
        foreach (ObjectMemory o in memorisedObjects)
        {

        }
    }
    void percieve()
    {
        
    }
    bool checkForFake(Artefact artefact)
    {
        return false;
    }
    void updateHappiness(float change)
    {

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
public class ObjectMemory
{
    public float desirability;
    public float seenTime;
    public ObjectInstance seenObject;
    public ObjectMemory(float _seenTime, ObjectInstance _seenObject)
    {
        seenTime = _seenTime;
        seenObject = _seenObject;
    }
}
