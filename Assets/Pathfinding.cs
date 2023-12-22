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
    private List<Node> returnOpenCells(Node currentNode, bool[,] closedListCheck, Vector2Int end)
    {
        List<Node> returnList = new List<Node>();
        checkAndEnqueue(currentNode.x, currentNode.y - 1, Museum.grid[currentNode.x, currentNode.y].bWall);
        checkAndEnqueue(currentNode.x - 1, currentNode.y, Museum.grid[currentNode.x, currentNode.y].lWall);
        try
        {
            checkAndEnqueue(currentNode.x, currentNode.y + 1, Museum.grid[currentNode.x, currentNode.y + 1].bWall);
        }
        catch (IndexOutOfRangeException e) { }
        try
        {
            checkAndEnqueue(currentNode.x + 1, currentNode.y, Museum.grid[currentNode.x + 1, currentNode.y].lWall);
        }
        catch (IndexOutOfRangeException e) { }
        void checkAndEnqueue(int x, int y, Occupation wallCondition)
        {
            if (x >= 0 && x < museumSizeX && y >= 0 && y < museumSizeY && wallCondition != Occupation.Hard && Museum.grid[x, y].occupation != Occupation.Hard && !closedListCheck[x, y])
            {
                returnList.Add(new Node(Museum.grid[x, y], currentNode, end));
            }
        }
        return returnList;
    }
    public List<Node> AStarSolve(Vector2Int start, Vector2Int end)
    {
        List<Node> openList = new List<Node>();
        List<Node> closedList = new List<Node>();
        bool[,] closedListCheck = new bool[museumSizeX, museumSizeY];
        bool[,] openListCheck = new bool[museumSizeX, museumSizeY];
        List<Node> orderedOpenList = new List<Node>();
        openList.Add(new Node(start, end));
        while (openList.Count() != 0)
        {
            orderedOpenList = openList.OrderBy(o => o.f).ToList();
            Node current = orderedOpenList[0];
            if (current.x == end.x && current.y == end.y)
            {
                closedList.Add(current);
                break;
            }
            openList.Remove(current);
            List<Node> successorList = returnOpenCells(current, closedListCheck, end);
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
        List<Node> finalList = new List<Node>();
        finalList = FinalList(closedList[closedList.Count() - 1], finalList);
        return (finalList);
        
        List<Node> FinalList(Node current, List<Node> finalList)
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

public class Node : Pathfinding
{
    public int x;
    public int y;
    public int g; // g cost - cost to move to the node
    public double h; // h cost - distance from the end
    public double f; // f cost - combination of g and h
    public Node parent;
    public Node(Cell cell, Node cur, Vector2Int end)
    {
        x = cell.x;
        y = cell.y;
        parent = cur;
        g = 1000000;
        h = hCost(new Vector2Int(cell.x, cell.y), end);
        f = fCost(new Vector2Int(cell.x, cell.y), end);
    }
    public Node(Vector2Int start, Vector2Int end)
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
    public static List<Vector2Int> convertToVector2Int(List<Node> inList)
    {
        List<Vector2Int> convList = new List<Vector2Int>();
        foreach (Node n in inList)
        {
            convList.Add(new Vector2Int(n.x, n.y));
        }
        return convList;
    }
}
