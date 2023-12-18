using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager;
using UnityEngine;

public class Museum : MonoBehaviour
{
    /*
     * The grid must ALWAYS have one inaccessable cell as a border that cannot be built on, otherwise the algorithm treats it as a wall and breaks
     * The maximum room size is 10, but that can be edited by changing the depthCount < 10 in the while loop that checks for rooms
    */
    public static Cell[,] grid = new Cell[11,11]; //THE FIRST IS X THE SECOND IS Y KEEP THIS THE SAME
    public static List<Room> roomsInMuseum = new List<Room>();
    void Start()
    {
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                grid[i, j] = new Cell(i,j);
            }
        }
        newWall(new Vector2Int(2, 1), new Vector2Int(9, 1));
        newWall(new Vector2Int(9, 1), new Vector2Int(9, 9));
        newWall(new Vector2Int(9, 9), new Vector2Int(6, 9));
        newWall(new Vector2Int(6, 9), new Vector2Int(6, 7));
        newWall(new Vector2Int(6, 7), new Vector2Int(2, 7));
        newWall(new Vector2Int(2, 7), new Vector2Int(2, 1));

        newWall(new Vector2Int(6, 1), new Vector2Int(6, 4));
        newWall(new Vector2Int(6, 4), new Vector2Int(9, 4));
        //newWall(new Vector2Int(, ), new Vector2Int(, ));
        //Room.displayRooms();

    }
    void newWall(Vector2Int startPos, Vector2Int endPos)
    {
        Wall wall = new Wall(startPos, endPos);
        updateGridWithWall(wall);
        wall.draw();
        if (wall.determineIfVertical())
        {
            int startPlacingPos = Math.Min(wall.startPos.y, wall.endPos.y); //find the lowest Y pos of the wall
            for (int i = 0; i < Math.Abs(wall.startPos.y - wall.endPos.y); i++) //for each cell in the wall
            {
                detectRoom(new Vector2Int(wall.startPos.x, startPlacingPos + i));
                detectRoom(new Vector2Int(wall.startPos.x - 1, startPlacingPos + i));
            }
        }
        else //if the wall is horizontal, do the same but swapped by 90 degrees
        {
            int startPlacingPos = Math.Min(wall.startPos.x, wall.endPos.x);
            for (int i = 0; i < Math.Abs(wall.startPos.x - wall.endPos.x); i++)
            {
                detectRoom(new Vector2Int(startPlacingPos + i, wall.startPos.y));
                detectRoom(new Vector2Int(startPlacingPos + i, wall.startPos.y - 1));
            }
        }
        resetPlacedInRoomThisCheck();
    }
    void resetPlacedInRoomThisCheck()
    {
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                grid[i, j].placedInRoomThisCheck = false;
            }
        }
    }
    void updateGridWithWall(Wall placedWall) //updates the grid of cells so that the flood fill can be performed
    {
        if (placedWall.determineIfVertical())
        {
            int startPlacingPos = Math.Min(placedWall.startPos.y, placedWall.endPos.y); //find the lowest Y pos of the wall
            for (int i = 0; i < Math.Abs(placedWall.startPos.y-placedWall.endPos.y); i++) //for each cell in the wall
            {
                grid[placedWall.startPos.x, startPlacingPos + i].lWall = true; //put a wall on the left side of the cell
            }
        }
        else //if the wall is horizontal, do the same but swapped by 90 degrees
        {
            int startPlacingPos = Math.Min(placedWall.startPos.x, placedWall.endPos.x);
            for (int i = 0; i < Math.Abs(placedWall.startPos.x - placedWall.endPos.x); i++)
            {
                grid[startPlacingPos + i,placedWall.startPos.y].bWall = true; //put a wall on the bottom side of the cell
            }
        }
        
    }
    bool detectRoom(Vector2Int checkPos)
    {
        if (grid[checkPos.x,checkPos.y].placedInRoomThisCheck)
        {
            return false;
        }
        Queue<Cell> cellQueue = new Queue<Cell>();
        bool[,] visitedThisDetect = new bool[grid.GetLength(0), grid.GetLength(1)];
        int depthCount = 0;
        cellQueue.Enqueue(grid[checkPos.x, checkPos.y]);
        while (cellQueue.Count > 0 && depthCount <= 48) //MAX ROOM SIZE
        {
            Cell curCell = cellQueue.Dequeue();
            try
            {
                roomsInMuseum.RemoveAt(Room.locateRoom(curCell));
            }
            catch (ArgumentOutOfRangeException e) { }
            visitedThisDetect[curCell.x, curCell.y] = true;
            depthCount += checkAndEnqueue(cellQueue, curCell.x, curCell.y - 1, !grid[curCell.x, curCell.y].bWall);
            depthCount += checkAndEnqueue(cellQueue, curCell.x - 1, curCell.y, !grid[curCell.x, curCell.y].lWall);
            try
            {
                depthCount += checkAndEnqueue(cellQueue, curCell.x, curCell.y + 1, !grid[curCell.x, curCell.y + 1].bWall);
            }
            catch (IndexOutOfRangeException e) { }
            try
            {
                depthCount += checkAndEnqueue(cellQueue, curCell.x + 1, curCell.y, !grid[curCell.x + 1, curCell.y].lWall);
            }
            catch (IndexOutOfRangeException e) { }
        }
        if (cellQueue.Count == 0)
        {
            List<Cell> cellsInRoom = new List<Cell>();
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    if (visitedThisDetect[i, j])
                    {
                        cellsInRoom.Add(grid[i, j]);
                    }
                }
            }
            Room room = new Room(cellsInRoom);
            roomsInMuseum.Add(room);
            return true;
        }
        return false;
        int checkAndEnqueue(Queue<Cell> cellQueue, int x, int y, bool wallCondition)
        {
            if (x >= 0 && x < grid.GetLength(0) && y >= 0 && y < grid.GetLength(1) && wallCondition && !grid[x, y].placedInRoomThisCheck)
            {
                grid[x, y].placedInRoomThisCheck = true;
                cellQueue.Enqueue(grid[x, y]);
                return 1;
            }
            return 0;
        }
    }
    public void updateGridWithPlacedObject(ObjectInstance objectToPlace)
    {
        bool recalculationNecessary = false;
        foreach (Vector2Int v in objectToPlace.worldFootprint)
        {
            if (grid[v.x, v.y].occupation == Occupation.Soft)
            {
                recalculationNecessary = true;
            }
            grid[v.x, v.y].occupation = Occupation.Hard;
            //IF THE GRID IS SOFT WHERE TRYING TO PLACE REMOVE THAT POSITION FROM THE ARTEFACTS VIEWPOINTS
            //MAKE SURE THAT THE VIEW POSITIONS ARE RECALCED WHEN PLACING THINGS IN THE SAME ROOM
            //OPERATE THIS FROM WITHIN ROOM SO THAT IT DOESNT ITERATE THROUGH THE WHOLE MUSEUM
            //THINK ABOUT WHAT HAPPENS WHEN A ROOM IS DESTROYED, NEED TO REASSIGN ARTEFACTS TO THE NEW ROOM AND RECALC
        }
        foreach (TranslatedPosition t in objectToPlace.worldInteractionPositions)
        {
            grid[t.position.x, t.position.y].occupation = Occupation.Soft;
        }
        if(recalculationNecessary)
        {
            //do a thing
        }
    }
}

public class Wall
{
    public Vector2Int startPos;
    public Vector2Int endPos;
    public Wall(Vector2Int _startPos, Vector2Int _endPos)
    {
        startPos = _startPos;
        endPos = _endPos;
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
        cube.transform.position = new Vector3((float)(startPos.x + endPos.x)/2 - 0.5f, 1f, (float)(startPos.y + endPos.y) / 2 - 0.5f);
        cube.transform.localScale = new Vector3(Math.Abs(startPos.x - endPos.x) + 0.01f, 1, Math.Abs(startPos.y - endPos.y) + 0.01f);
    }
}

public class Cell
{
    public bool lWall;
    public bool bWall;
    public bool placedInRoomThisCheck;
    public Occupation occupation;
    public int x;
    public int y;
    public Cell(int _x, int _y)
    {
        lWall = false;
        bWall = false;
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