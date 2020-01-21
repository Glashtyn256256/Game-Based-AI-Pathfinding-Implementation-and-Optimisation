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

            //We want the blocked nodes incase they've been changed so we still want to know about them.
            //Make sure we dont add nodes that are outside the boundrys of the grid
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
    public Node GetNodeFromWorldPoint(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt(worldPosition.x);
        int y = Mathf.RoundToInt(worldPosition.z);

        return gridNodes[x, y];
    }
    //void OnDrawGizmos()
    //{
    //    Gizmos.DrawWireCube(nodes[0, 0].position, new Vector3(gridWidth, 1, gridHeight)); //Reason we use Y axis and not z is because the grid

    //    if (nodes != null)
    //    {
    //        foreach (Node n in nodes)
    //        {

    //            Gizmos.DrawCube(n.position, Vector3.one * (nodeDiameter));
    //        }
    //    }
    //}

     //Talk about this more later, from tutorial. explain in write up what it does.
     //Issue with previous herustic, noticed it wasnt getting the most optimal path
     //May be wrong but I feel it finds it faster for a less optimal path
     //changed it and now getting optimal path. 
    public float GetNodeDistance(Node source, Node target)
    {
        int xDistance = Mathf.Abs(source.xIndexPosition - target.xIndexPosition);
        int yDistance = Mathf.Abs(source.yIndexPosition - target.yIndexPosition);

        if (xDistance > yDistance)
        {
            return 14 * yDistance + 10 * (xDistance - yDistance);
        }
        return 14 * xDistance + 10 * (yDistance - xDistance);
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
                    if (neighbour.nodeType == NodeType.Blocked)
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
