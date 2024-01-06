using Palmmedia.ReportGenerator.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;
using static UnityEngine.GraphicsBuffer;

public class Pathfinding
{
    int museumSizeX;
    int museumSizeY;
    public Pathfinding()
    {
        museumSizeX = Museum.grid.GetLength(0);
        museumSizeY = Museum.grid.GetLength(1);
    }
    private List<AStarNode> returnOpenCells(AStarNode currentAStarNode, bool[,] closedListCheck, Vector2Int end)
    {
        List<AStarNode> returnList = new List<AStarNode>();
        checkAndEnqueue(currentAStarNode.x, currentAStarNode.y - 1, Museum.grid[currentAStarNode.x, currentAStarNode.y].bWall);
        checkAndEnqueue(currentAStarNode.x - 1, currentAStarNode.y, Museum.grid[currentAStarNode.x, currentAStarNode.y].lWall);
        try
        {
            checkAndEnqueue(currentAStarNode.x, currentAStarNode.y + 1, Museum.grid[currentAStarNode.x, currentAStarNode.y + 1].bWall);
        }
        catch (IndexOutOfRangeException e) { }
        try
        {
            checkAndEnqueue(currentAStarNode.x + 1, currentAStarNode.y, Museum.grid[currentAStarNode.x + 1, currentAStarNode.y].lWall);
        }
        catch (IndexOutOfRangeException e) { }
        void checkAndEnqueue(int x, int y, Occupation wallCondition)
        {
            if (x >= 0 && x < museumSizeX && y >= 0 && y < museumSizeY && wallCondition != Occupation.Hard && Museum.grid[x, y].occupation != Occupation.Hard && !closedListCheck[x, y])
            {
                returnList.Add(new AStarNode(Museum.grid[x, y], currentAStarNode, end));
            }
        }
        return returnList;
    }
    public List<AStarNode> AStarSolve(Vector2Int start, Vector2Int end)
    {
        List<AStarNode> openList = new List<AStarNode>();
        List<AStarNode> closedList = new List<AStarNode>();
        bool[,] closedListCheck = new bool[museumSizeX, museumSizeY];
        bool[,] openListCheck = new bool[museumSizeX, museumSizeY];
        List<AStarNode> orderedOpenList = new List<AStarNode>();
        openList.Add(new AStarNode(start, end));
        while (openList.Count() != 0)
        {
            orderedOpenList = openList.OrderBy(o => o.f).ToList();
            AStarNode current = orderedOpenList[0];
            if (current.x == end.x && current.y == end.y)
            {
                closedList.Add(current);
                break;
            }
            openList.Remove(current);
            List<AStarNode> successorList = returnOpenCells(current, closedListCheck, end);
            for (int i = 0; i < successorList.Count(); i++)
            {
                int successorCost = current.g + 1;
                if (openListCheck[successorList[i].x, successorList[i].y])
                {
                    if (successorList[i].g <= successorCost)
                    {
                        break;
                    }
                }
                else if (closedListCheck[successorList[i].x, successorList[i].y])
                {
                    if (successorList[i].g <= successorCost)
                    {
                        break;
                    }
                    closedList.Remove(successorList[i]);
                    closedListCheck[successorList[i].x, successorList[i].y] = false;
                }
                else
                {
                    openList.Add(successorList[i]);
                    openListCheck[successorList[i].x, successorList[i].y] = true;
                }
                successorList[i].g = successorCost;
                successorList[i].parent = current;
            }
            closedList.Add(current);
            closedListCheck[current.x, current.y] = true;
        }
        List<AStarNode> finalList = new List<AStarNode>();
        finalList = FinalList(closedList[closedList.Count() - 1], finalList);
        return (finalList);
        
        List<AStarNode> FinalList(AStarNode current, List<AStarNode> finalList)
        {
            finalList.Add(current);
            if (current.parent != null)
            {
                FinalList(current.parent, finalList);
            }
            return finalList;
        }
    }
    public bool AStarSolveReturnBool(Vector2Int start, Vector2Int end)
    {
        List<AStarNode> openList = new List<AStarNode>();
        List<AStarNode> closedList = new List<AStarNode>();
        bool[,] closedListCheck = new bool[museumSizeX, museumSizeY];
        bool[,] openListCheck = new bool[museumSizeX, museumSizeY];
        List<AStarNode> orderedOpenList = new List<AStarNode>();
        openList.Add(new AStarNode(start, end));
        while (openList.Count() != 0)
        {
            orderedOpenList = openList.OrderBy(o => o.f).ToList();
            AStarNode current = orderedOpenList[0];
            if (current.x == end.x && current.y == end.y)
            {
                closedList.Add(current);
                break;
            }
            openList.Remove(current);
            List<AStarNode> successorList = returnOpenCells(current, closedListCheck, end);
            for (int i = 0; i < successorList.Count(); i++)
            {
                int successorCost = current.g + 1;
                if (openListCheck[successorList[i].x, successorList[i].y])
                {
                    if (successorList[i].g <= successorCost)
                    {
                        break;
                    }
                }
                else if (closedListCheck[successorList[i].x, successorList[i].y])
                {
                    if (successorList[i].g <= successorCost)
                    {
                        break;
                    }
                    closedList.Remove(successorList[i]);
                    closedListCheck[successorList[i].x, successorList[i].y] = false;
                }
                else
                {
                    openList.Add(successorList[i]);
                    openListCheck[successorList[i].x, successorList[i].y] = true;
                }
                successorList[i].g = successorCost;
                successorList[i].parent = current;
            }
            closedList.Add(current);
            closedListCheck[current.x, current.y] = true;
        }
        List<AStarNode> finalList = new List<AStarNode>();
        finalList = FinalList(closedList[closedList.Count() - 1], finalList);
        List<Vector2Int> vect2FinalList = AStarNode.convertToVector2Int(finalList);
        if (vect2FinalList[0] == end)
        {
            return true;
        }
        return false;

        List<AStarNode> FinalList(AStarNode current, List<AStarNode> finalList)
        {
            finalList.Add(current);
            if (current.parent != null)
            {
                FinalList(current.parent, finalList);
            }
            return finalList;
        }
    }
}

public class AStarNode : Pathfinding
{
    public int x;
    public int y;
    public int g; // g cost - cost to move to the AStarNode
    public double h; // h cost - distance from the end
    public double f; // f cost - combination of g and h
    public AStarNode parent;
    public AStarNode(Cell cell, AStarNode cur, Vector2Int end)
    {
        x = cell.x;
        y = cell.y;
        parent = cur;
        g = 1000000;
        h = hCost(new Vector2Int(cell.x, cell.y), end);
        f = fCost(new Vector2Int(cell.x, cell.y), end);
    }
    public AStarNode(Vector2Int start, Vector2Int end)
    {
        x = start.x;
        y = start.y;
        g = 0;
        h = hCost(start, end);
        f = hCost(start, end);
        parent = null;
    }
    private double hCost(Vector2Int curPos, Vector2Int endPos)
    {
        double result = Math.Sqrt(Math.Pow(endPos.x - curPos.x, 2) + (Math.Pow(endPos.y - curPos.y, 2)));
        return result;
    }
    private double fCost(Vector2Int startPos, Vector2Int endPos)
    {
        double fCost = g + hCost(startPos, endPos);
        return fCost;
    }
    public static List<Vector2Int> convertToVector2Int(List<AStarNode> inList)
    {
        List<Vector2Int> convList = new List<Vector2Int>();
        foreach (AStarNode n in inList)
        {
            convList.Add(new Vector2Int(n.x, n.y));
        }
        return convList;
    }
}
