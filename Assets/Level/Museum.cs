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
    public static Cell[,] grid = new Cell[15, 15]; //THE FIRST IS X THE SECOND IS Y KEEP THIS THE SAME
    public static List<Room> roomsInMuseum = new List<Room>();
    public static int[,] roomGrid = new int[grid.GetLength(0), grid.GetLength(1)];
    [SerializeField] Controls controls;
    void Start()
    {
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                grid[i, j] = new Cell(i,j);
                roomGrid[i, j] = -1;
            }
        }
        //newWall(new Vector2Int(2, 1), new Vector2Int(4, 1), true);
        //newWall(new Vector2Int(4, 1), new Vector2Int(9, 1), false);
        //newWall(new Vector2Int(9, 1), new Vector2Int(9, 9), false);
        //newWall(new Vector2Int(9, 9), new Vector2Int(6, 9), false);
        //newWall(new Vector2Int(6, 9), new Vector2Int(6, 7), false);
        //newWall(new Vector2Int(6, 7), new Vector2Int(2, 7), false);
        //newWall(new Vector2Int(2, 7), new Vector2Int(2, 1), false);

        //newWall(new Vector2Int(6, 1), new Vector2Int(6, 4), false);
        //newWall(new Vector2Int(6, 4), new Vector2Int(8, 4), false);
        //newWall(new Vector2Int(8, 4), new Vector2Int(9, 4), true);

        newWall(new Vector2Int(2, 1), new Vector2Int(4, 1), true);
        newWall(new Vector2Int(4, 1), new Vector2Int(9, 1), false);
        newWall(new Vector2Int(9, 1), new Vector2Int(9, 6), false);
        newWall(new Vector2Int(9, 6), new Vector2Int(9, 9), true);
        newWall(new Vector2Int(9, 9), new Vector2Int(6, 9), false);
        newWall(new Vector2Int(6, 9), new Vector2Int(6, 7), false);
        newWall(new Vector2Int(6, 7), new Vector2Int(4, 7), false);
        newWall(new Vector2Int(4, 7), new Vector2Int(2, 7), true);
        newWall(new Vector2Int(2, 7), new Vector2Int(2, 1), false);

        newWall(new Vector2Int(6, 1), new Vector2Int(6, 4), false);
        newWall(new Vector2Int(6, 4), new Vector2Int(7, 4), false);
        newWall(new Vector2Int(7, 4), new Vector2Int(9, 4), true);

        newWall(new Vector2Int(9, 1), new Vector2Int(13, 1), false);
        newWall(new Vector2Int(13, 1), new Vector2Int(13, 14), false);
        newWall(new Vector2Int(13, 5), new Vector2Int(12, 5), false);
        newWall(new Vector2Int(12, 5), new Vector2Int(10, 5), true);
        newWall(new Vector2Int(10, 5), new Vector2Int(9, 5), true);

        newWall(new Vector2Int(13, 14), new Vector2Int(2, 14), false);
        newWall(new Vector2Int(8, 14), new Vector2Int(8, 13), true);
        newWall(new Vector2Int(8, 13), new Vector2Int(8, 11), false);
        newWall(new Vector2Int(8, 11), new Vector2Int(8, 9), true);

        newWall(new Vector2Int(2, 14), new Vector2Int(2, 7), false);
        newWall(new Vector2Int(2, 11), new Vector2Int(4, 11), false);
        newWall(new Vector2Int(4, 11), new Vector2Int(6, 11), true);
        newWall(new Vector2Int(6, 11), new Vector2Int(8, 11), false);
        //newWall(new Vector2Int(, ), new Vector2Int(, ),false);
        //Room.displayRooms();
        controls.meshUpdate = true;
    }
    void newWall(Vector2Int startPos, Vector2Int endPos, bool isDoorway)
    {
        Wall wall = new Wall(startPos, endPos, isDoorway);
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
                if(placedWall.isDoorway)
                {
                    grid[placedWall.startPos.x, startPlacingPos + i].lWall = Occupation.Soft;
                }
                else
                {
                    grid[placedWall.startPos.x, startPlacingPos + i].lWall = Occupation.Hard; //put a wall on the left side of the cell
                }            
            }
        }
        else //if the wall is horizontal, do the same but swapped by 90 degrees
        {
            int startPlacingPos = Math.Min(placedWall.startPos.x, placedWall.endPos.x);
            for (int i = 0; i < Math.Abs(placedWall.startPos.x - placedWall.endPos.x); i++)
            {
                if (placedWall.isDoorway)
                {
                    grid[startPlacingPos + i, placedWall.startPos.y].bWall = Occupation.Soft; //put a wall on the bottom side of the cell
                }
                else
                {
                    grid[startPlacingPos + i, placedWall.startPos.y].bWall = Occupation.Hard; //put a wall on the bottom side of the cell
                }
            }
        }
        
    }
    bool detectRoom(Vector2Int checkPos)
    {
        if (grid[checkPos.x, checkPos.y].placedInRoomThisCheck)
        {
            return false;
        }
        Queue<Cell> cellQueue = new Queue<Cell>();
        bool[,] visitedThisDetect = new bool[grid.GetLength(0), grid.GetLength(1)];
        int depthCount = 0;
        cellQueue.Enqueue(grid[checkPos.x, checkPos.y]);
        while (cellQueue.Count > 0 && depthCount <= 70) //MAX ROOM SIZE
        {
            Cell curCell = cellQueue.Dequeue();
            try
            {
                roomsInMuseum.RemoveAt(Room.locateRoom(curCell));
            }
            catch (ArgumentOutOfRangeException e) { }
            visitedThisDetect[curCell.x, curCell.y] = true;
            depthCount += checkAndEnqueue(cellQueue, curCell.x, curCell.y - 1, grid[curCell.x, curCell.y].bWall);
            depthCount += checkAndEnqueue(cellQueue, curCell.x - 1, curCell.y, grid[curCell.x, curCell.y].lWall);
            try
            {
                depthCount += checkAndEnqueue(cellQueue, curCell.x, curCell.y + 1, grid[curCell.x, curCell.y + 1].bWall);
            }
            catch (IndexOutOfRangeException e) { }
            try
            {
                depthCount += checkAndEnqueue(cellQueue, curCell.x + 1, curCell.y, grid[curCell.x + 1, curCell.y].lWall);
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
            room.updateRoomGridWithNewRoom();
            return true;
        }
        return false;
        int checkAndEnqueue(Queue<Cell> cellQueue, int x, int y, Occupation wallCondition)
        {
            if (x >= 0 && x < grid.GetLength(0) && y >= 0 && y < grid.GetLength(1) && wallCondition == Occupation.None && !grid[x, y].placedInRoomThisCheck)
            {
                grid[x, y].placedInRoomThisCheck = true;
                cellQueue.Enqueue(grid[x, y]);
                return 1;
            }
            return 0;
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