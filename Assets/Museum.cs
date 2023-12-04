using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public static Cell[,] grid = new Cell[21,21]; //THE FIRST IS X THE SECOND IS Y KEEP THIS THE SAME
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
        foreach(Room r in roomsInMuseum)
        {
            foreach (Cell c in r.cellsInside)
            {
                grid[c.x, c.y].placedInRoomThisCheck = false;
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
                roomsInMuseum.RemoveAt(Room.locateRoom(curCell));
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
                        //GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        //cube.transform.position = new Vector3(i, 1f, j);
                        //cube.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
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
public class Room
{
    public List<Cell> cellsInside;
    public List<ObjectInstance> objectsInRoom;
    public Room(List<Cell> _cellsInside)
    {
        cellsInside = _cellsInside;
        objectsInRoom = new List<ObjectInstance>();
    }
    public void recalculateObjectsInRoom()
    {
        int countTo = objectsInRoom.Count;
        foreach (ObjectInstance oi in objectsInRoom)
        {
            Debug.Log("name: " + oi.artefact.name + " x" + oi.worldFootprint[0].x + " y" + oi.worldFootprint[0].y);
        }
        for (int i = 0; i < countTo; i++) //Do not try to optimise this by having it check the count - it will crash unity
        {
            Vector2Int pos = objectsInRoom[i].worldFootprint.First();
            objectsInRoom[i].worldFootprint = objectsInRoom[i].artefact.calculateWorldFootprint(pos);
            objectsInRoom[i].worldInteractionPositions = objectsInRoom[i].artefact.checkGridForValidSoftPlacement(pos);
            objectsInRoom[i].updateMuseumGrid(true);
            objectsInRoom[i].displayInteractionPoints();
        }
    }
    public void addObjectToRoom(ObjectInstance _object)
    {
        objectsInRoom.Add(_object);
    }
    public static int locateRoom(Cell cellInRoom)
    {
        int roomIndex = 0;
        foreach (Room room in Museum.roomsInMuseum)
        {
            foreach (Cell cell in room.cellsInside)
            {
                if (cell == cellInRoom)
                {
                    return roomIndex;
                }
            }
            roomIndex++;
        }
        return -1;
    }
    public static int locateRoom(ObjectInstance objectToFind)
    {
        int roomIndex = 0;
        int xSearch = objectToFind.worldFootprint.First().x;
        int ySearch = objectToFind.worldFootprint.First().y;
        foreach (Room room in Museum.roomsInMuseum)
        {
            foreach (Cell cell in room.cellsInside)
            {
                if (cell.x == xSearch && cell.y == ySearch)
                {
                    return roomIndex;
                }
            }
            roomIndex++;
        }
        return -1;
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