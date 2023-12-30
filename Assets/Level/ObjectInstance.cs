using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ObjectInstance : MonoBehaviour
{
    public Object theObject;
    public GameObject warning;
    [SerializeField] GameObject interactionPos;
    [SerializeField] public List<Vector2Int> worldFootprint = new List<Vector2Int>(); //The first entry of this list is the root, and is used to find things
    [SerializeField] public List<TranslatedPosition> worldInteractionPositions = new List<TranslatedPosition>();
    private int donationBoxMoneyContained;

    void Start()
    {
        foreach (LocalPosition v in theObject.localFootprint)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = new Vector3(v.xOffset, 0f, v.yOffset);
            cube.transform.parent = transform;
            cube.tag = "Artefact Representation";
        }

        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>().Where(child => child.CompareTag("Artefact Representation")).ToArray();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            Destroy(meshFilters[i].gameObject);
        }
        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combine);
        transform.GetComponent<MeshFilter>().sharedMesh = mesh;
        transform.gameObject.SetActive(true);

        gameObject.GetComponent<MeshRenderer>().material = theObject.material;
        gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
        //NavMeshModifier a = gameObject.AddComponent<NavMeshModifier>();
    }
    public void updateMuseumGridWithHard(bool isUpdate)
    {
        int roomIndex = Room.locateRoom(this);
        if (roomIndex != -1)
        {
            if (!isUpdate)
            {
                Museum.roomsInMuseum[roomIndex].addObjectToRoom(this);
            }
        }
        foreach (Vector2Int v in worldFootprint)
        {
            Museum.grid[v.x, v.y].occupation = Occupation.Hard;
        }
    }
    public void updateMuseumGridWithSoft()
    {
        foreach (TranslatedPosition tp in worldInteractionPositions)
        {
            Museum.grid[tp.position.x, tp.position.y].occupation = Occupation.Soft;
        }
    }
    public void createObject(Vector2Int placePos)
    {
        int roomIndex = Room.locateRoom(Museum.grid[placePos.x,placePos.y]);
        worldFootprint = theObject.calculateWorldFootprint(placePos);
        if (roomIndex != -1)
        {
            Museum.roomsInMuseum[roomIndex].addObjectToRoom(this);
        }
        updateMuseumGridWithSoft();
    }
    public void removeObject()
    {
        foreach (Vector2Int v in worldFootprint)
        {
            Museum.grid[v.x, v.y].occupation = Occupation.None;
        }
        foreach (TranslatedPosition tp in worldInteractionPositions)
        {
            Museum.grid[tp.position.x, tp.position.y].occupation = Occupation.None;
        }
        int roomIndex = Room.locateRoom(this);
        if (roomIndex != -1)
        {
            Museum.roomsInMuseum[roomIndex].objectsInRoom.RemoveAt(Museum.roomsInMuseum[roomIndex].objectsInRoom.IndexOf(this));
            Museum.roomsInMuseum[roomIndex].recalculateObjectsInRoom();
        }
        Destroy(gameObject);
    }
    public void displayInteractionPoints()
    {
        List<GameObject> prevInteractionPoints = new List<GameObject>();
        foreach (Transform child in transform)
        {
            if (child.gameObject.tag == "Interaction Point")
            {
                prevInteractionPoints.Add(child.gameObject);
            }
        }
        foreach (GameObject g in prevInteractionPoints)
        {
            Destroy(g);
        }
        foreach (TranslatedPosition tvp in worldInteractionPositions)
        {
            GameObject intPos = Instantiate(interactionPos, new Vector3(tvp.position.x, 0.55f, tvp.position.y), Quaternion.identity);
            intPos.transform.parent = transform;
            intPos.tag = "Interaction Point";
        }
        if(worldInteractionPositions.Count == 0)
        {
            GameObject instWarn = Instantiate(warning, new Vector3(worldFootprint.First().x, 2f, worldFootprint.First().y), Quaternion.identity);
            instWarn.transform.parent = transform;
            instWarn.tag = "Interaction Point";
        }
    }
    public Vector3? returnOpenInteractionPosition()
    {
        List<TranslatedPosition> shuffledInteractionPositions = worldInteractionPositions;
        shuffledInteractionPositions.Sort((x, y) => MathsFunctions.randomValue(-1, 2));
        foreach (TranslatedPosition tp in worldInteractionPositions)
        {
            if(!tp.beingUsed)
            {
                return new Vector3(tp.position.x, 0.5f, tp.position.y); //returns the first open one from the list, could make it take a pos and get closest
            }
        }
        return null;
    }
    public float calculateHappinessChange(NPC npc)
    {
        float happinessChange = theObject.baseHappinessValue;
        happinessChange += 0; //distance walked
        if(theObject is Artefact)
        {
            Artefact recastObject = (Artefact)theObject;
            if (npc.interest == recastObject.type)
            {
                happinessChange *= 2; //could replace later
            }
        }
        happinessChange *= 1 - (npc.turnsInCurrentState * 0.1f);
        happinessChange *= 0.1f; //Make the value closer to the desired
        return Mathf.Clamp01(happinessChange);
    }
    public float calculateHappinessChange(NPC npc, DonationBox donationBox)
    {
        float happinessChange = -(npc.happiness - 0.5f);
        donationBoxMoneyContained += (MathsFunctions.donateAmount(npc.happiness));
        Debug.Log("money in da box: " + donationBoxMoneyContained);
        Debug.Log("happiness change: " + happinessChange );
        return Mathf.Clamp01(happinessChange);
    }
    public bool authCheck(NPC npc)
    {
        if (theObject is Artefact)
        {
            Artefact recastObject = (Artefact)theObject;
            if (npc.interest == recastObject.type)
            {
                if(!recastObject.authenticity && UnityEngine.Random.value < recastObject.baseHappinessValue)
                {
                    return true;
                }
            }
        }
        return false;
    }
}

