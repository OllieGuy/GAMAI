using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC
{
    private float happiness;
    private float turnLength;
    private int turnsSinceSpawn;
    private Interest interest;
    private Vector3 targetPos;
    private List<Artefact> visitedArtefacts = new List<Artefact>();
    private List<ArtefactMemory> memorisedArtefacts = new List<ArtefactMemory>();
    public NPC()
    {
        System.Random rand = new System.Random();
        happiness = 0.5f;
        turnLength = 5f;
        turnsSinceSpawn = 0;
        interest = (Interest)rand.Next(0, Enum.GetValues(typeof(Interest)).Length);
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
