using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[CreateAssetMenu(fileName = "New Object", menuName = "Object")]
public class Object : ScriptableObject
{
    [SerializeField] public string objectName;
    [SerializeField] public string description;
    [SerializeField] public int value;
    [SerializeField] public Rarity rarity;
    [SerializeField] public List<LocalPosition> localFootprint = new List<LocalPosition>() { new LocalPosition(0,0)}; //Initialises the root as 0,0, as at least one cell will always be filled
    [SerializeField] protected List<LocalPosition> localInteractionPositions;

    public List<Vector2Int> calculateWorldFootprint(Vector2Int posInWorld)
    {
        List<Vector2Int> worldFootprint = new List<Vector2Int>();
        foreach (LocalPosition laf in localFootprint)
        {
            worldFootprint.Add(TranslatedPosition.translatePos(laf, posInWorld));
        }
        return worldFootprint;
    }
    public static bool checkGridForValidHardPlacement(List<LocalPosition> cellsTakenUp, Vector2Int posInWorld)
    {
        List<Vector2Int> potentialWorldPos = TranslatedPosition.translatePos(cellsTakenUp, posInWorld);
        int rootRoom = Room.locateRoom(Museum.grid[posInWorld.x, posInWorld.y]);
        foreach (Vector2Int v in potentialWorldPos)
        {
            if (Museum.grid[v.x, v.y].occupation == Occupation.Hard || Room.locateRoom(Museum.grid[v.x, v.y]) != rootRoom) //switch to wall checks bc of walls in same room
            {
                return false;
            }
        }
        return true;
    }
    public List<TranslatedPosition> checkGridForValidSoftPlacement(Vector2Int worldPos) //worldPos is the "root" of the artefact
    {
        List<TranslatedPosition> validPositions = new List<TranslatedPosition>();
        int worldPosRoomIndex = Room.locateRoom(Museum.grid[worldPos.x, worldPos.y]);
        foreach (LocalPosition l in localInteractionPositions)
        {
            TranslatedPosition tp = new TranslatedPosition(TranslatedPosition.translatePos(l, worldPos));
            //could add an extra check for overwriting old soft occupation
            //Debug.Log("x: "+ tp.position.x + " y: " + tp.position.y + " occ:" + Museum.grid[tp.position.x, tp.position.y].occupation);
            if (Museum.grid[tp.position.x, tp.position.y].occupation == Occupation.None && worldPosRoomIndex == Room.locateRoom(Museum.grid[tp.position.x, tp.position.y]))
            {
                validPositions.Add(tp);
            }
        }
        return validPositions;
    }
}

public enum Rarity
{
    Common,
    Rare,
    Epic,
    Legendary
}
[Serializable]
public class LocalPosition
{
    public int xOffset;
    public int yOffset;
    public LocalPosition(int _xOffset, int _yOffset)
    {
        xOffset = _xOffset;
        yOffset = _yOffset;
    }
}
public class TranslatedPosition
{
    public Vector2Int position;
    public bool beingUsed;
    public TranslatedPosition(LocalPosition localPosition, Vector2Int worldPos)
    {
        position = translatePos(localPosition, worldPos);
        beingUsed = false;
    }
    public TranslatedPosition(Vector2Int _position)
    {
        position = _position;
    }

    public static Vector2Int translatePos(LocalPosition localPosition, Vector2Int worldPos)
    {
        Vector2Int rPos = new Vector2Int(worldPos.x + localPosition.xOffset, worldPos.y + localPosition.yOffset);
        return rPos;
    }
    public static List<Vector2Int> translatePos(List<LocalPosition> localPositions, Vector2Int worldPos)
    {
        List<Vector2Int> rList = new List<Vector2Int>();
        foreach (LocalPosition lp in localPositions)
        {
            rList.Add(new Vector2Int(worldPos.x + lp.xOffset, worldPos.y + lp.yOffset));
        }
        return rList;
    }
}
