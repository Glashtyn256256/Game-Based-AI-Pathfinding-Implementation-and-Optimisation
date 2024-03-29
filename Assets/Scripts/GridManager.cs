﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    //A 2d array of nodes.
    public Node[,] gridNodes;

    //int[,] mapData;
    private int gridWidth;
    private int gridHeight;
    public int GetGridWidth { get { return gridWidth; } }
    public int GetGridHeight { get { return gridHeight; } }
    public int MaxGridSize { get { return gridWidth * gridHeight; } }
    //Saves us time having to use SQRT which is expensive to use.
    const float squareRouteOf2 = 1.41421356237f;

    float customDistanceStraight = 1;
    float customDistanceDiagnol  = 1;

    //Create two one for all directions and one for four directions, then have a option to toggle it on or off.
    public static readonly Vector2[] nodeNeighbourDirections =
    {   
        //x, y
        new Vector2(0f,1f), //top
        new Vector2(1f,0f), //right
        new Vector2(0f,-1f), //bottom
        new Vector2(-1f,0f), // left
        new Vector2(1f,1f), //top right
        new Vector2(1f,-1f), //bottom right    
        new Vector2(-1f,-1f), //bottom left 
        new Vector2(-1f,1f), // top left
    };

    public void CreateGrid(int[,] mapdata)
    {
        gridWidth = mapdata.GetLength(0); //first array so x
        gridHeight = mapdata.GetLength(1); //first array so y;

        gridNodes = new Node[gridWidth, gridHeight];

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                //change nodetype depending on what number we gave it in the mapdata
                NodeType type = (NodeType)mapdata[x, y];
                //Node world position
                Vector3 worldPosition = new Vector3(x, 0, y);
                //create a new node
                Node newNode = new Node(x, y, type, worldPosition);
                //place it in the grid of nodes
                gridNodes[x, y] = newNode;
            }
        }

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                //pre-calc neighbours.
                gridNodes[x, y].neighbours = GetNeighbours(x, y);
            }
        }
    }

    public bool IsWithinBounds(int x, int y)
    {
        return (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight);
    }
    List<Node> GetNeighbours(int x, int y)
    {
        return GetNeighbours(x, y, gridNodes, nodeNeighbourDirections);
    }
    List<Node> GetNeighbours(int x, int y, Node[,] nodeArray, Vector2[] directions)
    {
        List<Node> neighbourNodes = new List<Node>();    
        foreach (Vector2 dir in directions)
        {
            int newX = x + (int)dir.x;
            int newY = y + (int)dir.y;

            if (IsWithinBounds(newX, newY) && nodeArray[newX, newY] != null)
            {
                neighbourNodes.Add(nodeArray[newX, newY]);
            }
        }
        return neighbourNodes;
    }


    //Nodes world position matches the nodes grid position in the array so 3,0,3 (x,y,z) in world space is 3,3 in grid.
    //When the playernode is passed through it may be between two nodes so we round it to the closest int meaning
    //the node it is closest to. This might result in it going back to a node if it's closer.
    public Node GetNodeFromWorldPoint(Vector3 worldposition)
    {
        int x = Mathf.RoundToInt(worldposition.x);
        int y = Mathf.RoundToInt(worldposition.z);

        return gridNodes[x, y];
    }
    public bool CheckIfUnitOnNode(Vector3 worldposition, Node currentnode)
    {
        int x = Mathf.RoundToInt(worldposition.x);
        int y = Mathf.RoundToInt(worldposition.z);

        if (gridNodes[x, y] != currentnode)
        {
            return false;
        }

        currentnode.UnitAbove = false;
        currentnode.nodeParent.UnitAbove = true;
       // RecalculateNeighbours(currentnode);
        return true;
    }

    public void RecalculateNeighbours(Node currentnode)
    {
        currentnode.nodeParent = currentnode;
        foreach (Node neighbour in currentnode.neighbours)
        {
            if (neighbour.nodeType == NodeType.Blocked || neighbour.UnitAbove == true)
            {
                
                continue;
            }
            
            if (neighbour.gCost < currentnode.gCost)
            {
                currentnode.nodeParent = neighbour;
            }

            //foreach (Node nodeinneighbour in neighbour.neighbours)
            //{
            //    if (neighbour.nodeType == NodeType.Blocked || nodeinneighbour.UnitAbove == true)
            //    {

            //        continue;
            //    }

            //    if (nodeinneighbour.gCost < neighbour.gCost)
            //    {
            //        neighbour.nodeParent = nodeinneighbour;
            //    }

            //}
        }
    }

    //Talk about this more later, from tutorial. explain in write up what it does.
    //Issue with previous herustic, noticed it wasnt getting the most optimal path
    //May be wrong but I feel it finds it faster for a less optimal path
    //changed it and now getting optimal path. 
    public float GetNodeDistance(Node source, Node target, int heuristicindex)
    {
        switch (heuristicindex)
        {
            case 0:
                return PatricksDiagnolDistance(source, target);
            case 1:
                return ManhattenDistance(source, target);
            case 2:
                return EuclideanDistance(source, target);
            case 3:
                return ChebyshevDistance(source, target);
            case 4:
                return OctileDistance(source, target);
            case 5:
                return CustomDiagnolDistance(source, target);
       }
        //This will never be hit but we have to return a value since it will complain
        return Mathf.Infinity;
    }

    float PatricksDiagnolDistance(Node source, Node target)
    {
        int xDistance = Mathf.Abs(source.xIndexPosition - target.xIndexPosition);
        int yDistance = Mathf.Abs(source.yIndexPosition - target.yIndexPosition);

        if (xDistance > yDistance)
        {
            return 1.4f * yDistance + 1.0f * (xDistance - yDistance);
        }
        return 1.4f * xDistance + 1.0f * (yDistance - xDistance);
    }

    //Only use this if you are using for four directions #up,down,left,right
    float ManhattenDistance(Node source, Node target)
    {
        int xDistance = Mathf.Abs(source.xIndexPosition - target.xIndexPosition);
        int yDistance = Mathf.Abs(source.yIndexPosition - target.yIndexPosition);

        return 1.0f * (xDistance + yDistance);
    }

    //This is a straight line distance, if your unit can move any direction then you would 
    //want to use this.
    float EuclideanDistance(Node source, Node target)
    {
        int xDistance = Mathf.Abs(source.xIndexPosition - target.xIndexPosition);
        int yDistance = Mathf.Abs(source.yIndexPosition - target.yIndexPosition);

        return 1.0f * Mathf.Sqrt((xDistance * xDistance) + (yDistance * yDistance));
    }

    //All distances cost the same, diagnol and cardinal movements cost 10
    float ChebyshevDistance(Node source, Node target)
    {
        int xDistance = Mathf.Abs(source.xIndexPosition - target.xIndexPosition);
        int yDistance = Mathf.Abs(source.yIndexPosition - target.yIndexPosition);
        ////D * max(dx, dy) + (D2-D) * min(dx, dy)
        return 1.0f * Mathf.Max(xDistance, yDistance) + (1.0f - 1.0f) * Mathf.Min(xDistance, yDistance);
    }

    float OctileDistance(Node source, Node target)
    {
        int xDistance = Mathf.Abs(source.xIndexPosition - target.xIndexPosition);
        int yDistance = Mathf.Abs(source.yIndexPosition - target.yIndexPosition);
        //1.41421356237 square route of 2 
        //D * (dx + dy) + (D2 - 2 * D) * min(dx, dy)
        return 1.0f * (xDistance + yDistance) + (1.4f - 2 * 1.0f) * Mathf.Min(xDistance, yDistance);
    }

    float CustomDiagnolDistance(Node source, Node target)
    {
        int xDistance = Mathf.Abs(source.xIndexPosition - target.xIndexPosition);
        int yDistance = Mathf.Abs(source.yIndexPosition - target.yIndexPosition);

        if (xDistance > yDistance)
        {
            return customDistanceDiagnol * yDistance + customDistanceStraight * (xDistance - yDistance);
        }
        return customDistanceDiagnol * xDistance + customDistanceStraight * (yDistance - xDistance);
    }

    public void SetCustomDistanceValues(float diagnol, float straight)
    {
        customDistanceDiagnol = diagnol;
        customDistanceStraight = straight;
    }

    public void IntegrationField()
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if(gridNodes[x,y].nodeType == NodeType.Blocked)
                {
                    continue;
                }

                gridNodes[x, y].nodeParent = gridNodes[x, y];

                foreach (Node neighbour in gridNodes[x, y].neighbours)
                {
                    if (neighbour.nodeType == NodeType.Blocked || neighbour.UnitAbove == true)
                    {
                        continue;
                    }

                    if (neighbour.gCost < gridNodes[x, y].nodeParent.gCost)
                    {
                        gridNodes[x, y].nodeParent = neighbour;
                    }
                }
            }
        }
    }
}
