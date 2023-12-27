using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall
{
    public Vector2Int startPos;
    public Vector2Int endPos;
    public bool isDoorway;
    public static Material doorwayMaterial;
    public Wall(Vector2Int _startPos, Vector2Int _endPos, bool _isDoorway)
    {
        startPos = _startPos;
        endPos = _endPos;
        isDoorway = _isDoorway;
    }
    public bool determineIfVertical()
    {
        if (startPos.x - endPos.x == 0)
        {
            return true;
        }
        return false;
    }
    public void draw()
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = new Vector3((float)(startPos.x + endPos.x) / 2 - 0.5f, 2f, (float)(startPos.y + endPos.y) / 2 - 0.5f);
        cube.transform.localScale = new Vector3(Math.Abs(startPos.x - endPos.x) + 0.01f, 3, Math.Abs(startPos.y - endPos.y) + 0.01f);
        if (isDoorway)
        {
            cube.GetComponent<MeshRenderer>().material = doorwayMaterial;
            cube.GetComponent<BoxCollider>().enabled = false;
            cube.layer = LayerMask.NameToLayer("Doorway");
        }
        else
        {
            cube.tag = "Wall";
            cube.layer = LayerMask.NameToLayer("Wall");
        }
    }
}

public class Cell
{
    public Occupation lWall;
    public Occupation bWall;
    public bool placedInRoomThisCheck;
    public Occupation occupation;
    public int x;
    public int y;
    public Cell(int _x, int _y)
    {
        lWall = Occupation.None;
        bWall = Occupation.None;
        placedInRoomThisCheck = false;
        occupation = Occupation.None;
        x = _x;
        y = _y;
    }
}
public enum Occupation
{
    None,
    Soft,
    Hard
}
