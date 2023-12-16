using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectInstance : MonoBehaviour
{
    public Artefact artefact;
    [SerializeField] GameObject interactionPos;
    [NonSerialized] public List<Vector2Int> worldFootprint = new List<Vector2Int>(); //The first entry of this list is the root, and is used to find things
    [NonSerialized] public List<TranslatedPosition> worldInteractionPositions = new List<TranslatedPosition>();

    void Start()
    {
        //gameObject.GetComponent<MeshFilter>().mesh = artefact.mesh;
        foreach (LocalPosition v in artefact.localFootprint)
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
            meshFilters[i].gameObject.SetActive(false);
        }
        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combine);
        transform.GetComponent<MeshFilter>().sharedMesh = mesh;
        transform.gameObject.SetActive(true);

        gameObject.GetComponent<MeshRenderer>().material = artefact.material;
        gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    public void updateMuseumGrid(bool isUpdate)
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
        foreach (TranslatedPosition tp in worldInteractionPositions)
        {
            Debug.Log(tp.position.x + " " + tp.position.y);
            Museum.grid[tp.position.x, tp.position.y].occupation = Occupation.Soft;
        }
    }

    public void removeObject() //local interaction footprint not being cleared properly - might be to do with when at least one point is recalced as thats the only time it seems to happen
    {
        //it isnt clearing any of the soft occupation when the object has already been recalculated
        //ITS BECAUSE THE SOFT ONES ARE ALREADY THERE FROM BEFORE THE RECALC BUT THEN IT SETS THE NEW LIST TO 0 THINGS
        //SO WHEN THE THING IS RECALCED IT DOESNT THINK THERE ARE ANY SOFT OCCUPATIONS
        foreach (Vector2Int v in worldFootprint)
        {
            Museum.grid[v.x, v.y].occupation = Occupation.None;
            Debug.Log("hard Cleared x:" + v.x + " y:" + v.y);
        }
        Debug.Log(worldInteractionPositions.Count);
        foreach (TranslatedPosition tp in worldInteractionPositions)
        {
            Museum.grid[tp.position.x, tp.position.y].occupation = Occupation.None;
            Debug.Log("soft Cleared x:" + tp.position.x + " y:" + tp.position.y);
        }
        int roomIndex = Room.locateRoom(this);
        if (roomIndex != -1)
        {
            Museum.roomsInMuseum[roomIndex].objectsInRoom.RemoveAt(Museum.roomsInMuseum[roomIndex].objectsInRoom.IndexOf(this));
            Museum.roomsInMuseum[roomIndex].recalculateObjectsInRoom();
        }
        Destroy(gameObject);
        Debug.Log("----------");
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
    }
}

