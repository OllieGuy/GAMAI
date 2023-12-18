using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC
{
    public float happiness;
    public float turnLength;
    public int turnsSinceSpawn;
    public string interest;
    public Vector3 targetPos;
    public List<Artefact> visitedArtefacts = new List<Artefact>();
    public List<ArtefactMemory> memorisedArtefacts = new List<ArtefactMemory>();
    public NPC()
    {
        System.Random rand = new System.Random();
        happiness = 0.5f;
        turnLength = 5f;
        turnsSinceSpawn = 0;
        interest = ((Interest)rand.Next(0, Enum.GetValues(typeof(Interest)).Length)).ToString();
    }
    void calculateDesire()
    {
        foreach (ArtefactMemory a in memorisedArtefacts)
        {

        }
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
