using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC
{
<<<<<<< Updated upstream
    public float investigatorExplorer;
    public float happiness;
    public float turnLength;
    public int turnsSinceSpawn;
    public string interest;
    public Vector3 targetPos;
    public List<Artefact> visitedArtefacts = new List<Artefact>();
    public List<ArtefactMemory> memorisedArtefacts = new List<ArtefactMemory>();
=======
    private float happiness;
    private float turnLength;
    private int turnsSinceSpawn;
    private Interest interest;
    private Vector3 targetPos;
    private List<Artefact> visitedArtefacts = new List<Artefact>();
    private List<ArtefactMemory> memorisedArtefacts = new List<ArtefactMemory>();
>>>>>>> Stashed changes
    public NPC()
    {
        System.Random rand = new System.Random();
        investigatorExplorer = (float)rand.NextDouble();
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
    void calculateHappinessChange(float distanceWalkedToArtefact, Artefact artefact)
    {
        float happinessChange = 0f;
        happinessChange += (distanceWalkedToArtefact * (investigatorExplorer - 0.5f)); //* memorisedArtefacts.Count;
        happinessChange += (float)Math.Log((0.01f*artefact.value) + 1);
        happinessChange += 0; //change to weight according to rarity
        happinessChange += 0; //auth check
        happinessChange += 0;
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
