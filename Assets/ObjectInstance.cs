using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectInstance : MonoBehaviour
{
    public Artefact artefact;
    [SerializeField] GameObject interactionPos;
    [NonSerialized] public List<Vector2Int> worldFootprint = new List<Vector2Int>(); //The first entry of this list is the root, and is used to find things
    [NonSerialized] public List<TranslatedPosition> worldInteractionPositions = new List<TranslatedPosition>();

    void Start()
    {
        gameObject.GetComponent<MeshFilter>().mesh = artefact.mesh;
        gameObject.GetComponent<MeshRenderer>().material = artefact.material;
        gameObject.GetComponent<MeshCollider>().sharedMesh = artefact.mesh;
    }

    public void updateMuseumGrid(bool isUpdate)
    {
        int roomIndex = Room.locateRoom(this);
        if (roomIndex != -1)
        {
            //Debug.Log(Museum.roomsInMuseum[roomIndex]);
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
            //Debug.Log(tp.position.x + " " + tp.position.y);
            Museum.grid[tp.position.x, tp.position.y].occupation = Occupation.Soft;
        }
    }

    public void removeObject() //sometimes it doesnt clear?? no idea why probably something to do with some cases where the grid isnt updated correctly
    {
        foreach (Vector2Int v in worldFootprint)
        {
            Museum.grid[v.x, v.y].occupation = Occupation.None;
            //Debug.Log("hard Cleared x:" + v.x + " y:" + v.y);
        }
        foreach (TranslatedPosition tp in worldInteractionPositions)
        {
            Museum.grid[tp.position.x, tp.position.y].occupation = Occupation.None;
            //Debug.Log("soft Cleared x:" + tp.position.x + " y:" + tp.position.y);
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
        foreach (TranslatedPosition tvp in worldInteractionPositions)
        {
            GameObject intPos = Instantiate(interactionPos, new Vector3(tvp.position.x, 0.55f, tvp.position.y), Quaternion.identity);
            intPos.transform.parent = transform;
        }
    }
}

