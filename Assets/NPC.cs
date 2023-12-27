using System;
using System.Collections;
using System.Collections.Generic;
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
    public Perception perception;
    public List<Artefact> visitedArtefacts = new List<Artefact>();
    public List<ArtefactMemory> memorisedArtefacts = new List<ArtefactMemory>();
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
    void calculateDesire()
    {
        foreach (ArtefactMemory a in memorisedArtefacts)
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
public class ArtefactMemory
{
    public float desirability;
    public DateTime seenTime;
    public Artefact artefact;
    public ArtefactMemory(DateTime _seenTime, Artefact _artefact)
    {
        seenTime = _seenTime;
        artefact = _artefact;
    }
}
