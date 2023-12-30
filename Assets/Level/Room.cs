using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Room
{
    public List<Cell> cellsInside;
    public List<ObjectInstance> objectsInRoom;
    public Room(List<Cell> _cellsInside)
    {
        cellsInside = _cellsInside;
        objectsInRoom = new List<ObjectInstance>();
    }
    public void updateRoomGridWithNewRoom()
    {
        for (int i = 0; i < Museum.roomGrid.GetLength(0); i++)
        {
            for (int j = 0; j < Museum.roomGrid.GetLength(1); j++)
            {
                Museum.roomGrid[i, j] = -1;
            }
        }
        int roomIndex = 0;
        foreach (Room room in Museum.roomsInMuseum)
        {
            foreach (Cell cell in room.cellsInside)
            {
                Museum.roomGrid[cell.x, cell.y] = roomIndex;
                //Debug.Log(roomIndex);
            }
            roomIndex++;
        }
    }
    public void recalculateObjectsInRoom()
    {
        int countTo = objectsInRoom.Count;
        foreach (Cell cell in cellsInside)
        {
            cell.occupation = Occupation.None;
        }
        for (int i = 0; i < countTo; i++) //Do not try to optimise this by having it check the count - it will crash unity
        {
            Vector2Int pos = objectsInRoom[i].worldFootprint.First();
            objectsInRoom[i].updateMuseumGridWithHard(true);
        }
        for (int i = 0; i < countTo; i++) //Do not try to optimise this by having it check the count - it will crash unity
        {
            Vector2Int pos = objectsInRoom[i].worldFootprint.First();
            objectsInRoom[i].worldInteractionPositions = objectsInRoom[i].theObject.calculateValidSoftPlacement(pos);
            objectsInRoom[i].updateMuseumGridWithSoft();
            objectsInRoom[i].displayInteractionPoints();
        }
    }
    public void addObjectToRoom(ObjectInstance _object)
    {
        objectsInRoom.Add(_object);
        recalculateObjectsInRoom();//THIS MIGHT BE TEMP
    }
    public static int locateRoom(Cell cellInRoom)
    {
        //int roomIndex = 0;
        //foreach (Room room in Museum.roomsInMuseum)
        //{
        //    foreach (Cell cell in room.cellsInside)
        //    {
        //        if (cell == cellInRoom)
        //        {
        //            return roomIndex;
        //        }
        //    }
        //    roomIndex++;
        //}
        //Debug.Log(Museum.roomGrid[cellInRoom.x, cellInRoom.y]);
        return Museum.roomGrid[cellInRoom.x, cellInRoom.y];
        //return -1;
    }
    public static int locateRoom(Vector2Int positionToFind)
    {
        return Museum.roomGrid[positionToFind.x, positionToFind.y];
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
    public static void displayRooms()
    {
        foreach (Room room in Museum.roomsInMuseum)
        {
            foreach (Cell cell in room.cellsInside)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.position = new Vector3(cell.x, 1f, cell.y);
                sphere.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            }
        }
    }
}
