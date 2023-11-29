using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
    Cell[,] grid = new Cell[6,6]; //THE FIRST IS X THE SECOND IS Y KEEP THIS THE SAME
    [SerializeField] Controls controls;
    List<Room> roomsInMuseum = new List<Room>();
    void Start()
    {
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                grid[i, j] = new Cell(i,j);
            }
        }
        newWall(new Vector2Int(1, 2), new Vector2Int(3, 2));
        newWall(new Vector2Int(3, 2), new Vector2Int(3, 5));
        newWall(new Vector2Int(3, 5), new Vector2Int(1, 5));
        newWall(new Vector2Int(1, 5), new Vector2Int(1, 2));
        //second room
        newWall(new Vector2Int(3, 4), new Vector2Int(4, 4));
        newWall(new Vector2Int(4, 4), new Vector2Int(4, 2));
        newWall(new Vector2Int(4, 2), new Vector2Int(5, 2));
        newWall(new Vector2Int(5, 2), new Vector2Int(5, 1));
        newWall(new Vector2Int(5, 1), new Vector2Int(3, 1));
        newWall(new Vector2Int(3, 1), new Vector2Int(3, 2));
        //third room
        newWall(new Vector2Int(1, 1), new Vector2Int(1, 2));
        newWall(new Vector2Int(1, 1), new Vector2Int(3, 1));
        //test room reassignment
        newWall(new Vector2Int(3, 3), new Vector2Int(4, 3));

        int aaa = 0;
        foreach (Room r in roomsInMuseum)
        {
            foreach (Cell c in r.cellsInside)
            {
                Debug.Log("x: " + c.x + " y: " + c.y + " room: " + aaa);
            }
            aaa++;
        }
        controls.meshUpdate = true;
    }
    void newWall(Vector2Int startPos, Vector2Int endPos)
    {
        Wall wall = new Wall(startPos, endPos);
        updateGrid(wall);
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
        foreach(Room r in roomsInMuseum)
        {
            foreach (Cell c in r.cellsInside)
            {
                grid[c.x, c.y].placedInRoomThisCheck = false;
            }
        }
    }
    void updateGrid(Wall placedWall) //updates the grid of cells so that the flood fill can be performed
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
    void detectRoom(Vector2Int checkPos)
    {
        Queue<Cell> cellQueue = new Queue<Cell>();
        cellQueue.Enqueue(grid[checkPos.x, checkPos.y]); //add the end pos of the wall
        bool[,] visitedAlready = new bool[grid.GetLength(0), grid.GetLength(1)];
        int depthCount = 0;
        while (cellQueue.Count > 0 && depthCount < 10 && !cellQueue.Peek().placedInRoomThisCheck)
        {
            Cell curCell = cellQueue.Dequeue();
            try
            {
                roomsInMuseum.RemoveAt(locateRoom(curCell));
            }
            catch (ArgumentOutOfRangeException e) { }
            visitedAlready[curCell.x, curCell.y] = true;
            checkAndEnqueue(grid, cellQueue, visitedAlready, curCell.x, curCell.y - 1, !grid[curCell.x, curCell.y].bWall);
            checkAndEnqueue(grid, cellQueue, visitedAlready, curCell.x - 1, curCell.y, !grid[curCell.x, curCell.y].lWall);
            try
            {
                checkAndEnqueue(grid, cellQueue, visitedAlready, curCell.x, curCell.y + 1, !grid[curCell.x, curCell.y + 1].bWall);
            }
            catch (IndexOutOfRangeException e) { }
            try
            {
                checkAndEnqueue(grid, cellQueue, visitedAlready, curCell.x + 1, curCell.y, !grid[curCell.x + 1, curCell.y].lWall);
            }
            catch (IndexOutOfRangeException e) { }
            depthCount++;
        }
        if (cellQueue.Count == 0)
        {
            List<Cell> cellsInRoom = new List<Cell>();
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    if (visitedAlready[i, j])
                    {
                        cellsInRoom.Add(grid[i, j]);
                        grid[i, j].placedInRoomThisCheck = true;
                        //ALL DEBUG STUFF CAN BE TAKEN OUT LATER
                        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        cube.transform.position = new Vector3(i + 0.5f, 1f, j + 0.5f);
                        cube.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                    }
                }
            }
            Room room = new Room(cellsInRoom);
            roomsInMuseum.Add(room);
        }
        void checkAndEnqueue(Cell[,] grid, Queue<Cell> cellQueue, bool[,] visitedAlready, int x, int y, bool wallCondition)
        {
            if (x >= 0 && x < grid.GetLength(0) && y >= 0 && y < grid.GetLength(1) && wallCondition && !grid[x, y].placedInRoomThisCheck && !visitedAlready[x, y])
            {
                cellQueue.Enqueue(grid[x, y]);
            }
        }
    }
    int locateRoom(Cell cellInRoom)
    {
        int roomToRemoveIndex = 0;
        foreach(Room room in roomsInMuseum)
        {
            foreach(Cell cell in room.cellsInside)
            {
                if(cell == cellInRoom)
                {
                    return roomToRemoveIndex;
                }
            }
            roomToRemoveIndex++;
        }
        return -1;
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
        cube.transform.position = new Vector3((float)(startPos.x + endPos.x)/2, 1f, (float)(startPos.y + endPos.y) / 2);
        cube.transform.localScale = new Vector3(Math.Abs(startPos.x - endPos.x) + 0.01f, 1, Math.Abs(startPos.y - endPos.y) + 0.01f);
    }
}
public class Room
{
    public List<Cell> cellsInside;
    public Room(List<Cell> _cellsInside)
    {
        cellsInside = _cellsInside;
    }
}

public class Cell
{
    public bool lWall;
    public bool bWall;
    public bool placedInRoomThisCheck;
    public int x;
    public int y;
    public Cell(int _x, int _y)
    {
        lWall = false;
        bWall = false;
        placedInRoomThisCheck = false;
        x = _x;
        y = _y;
    }
}