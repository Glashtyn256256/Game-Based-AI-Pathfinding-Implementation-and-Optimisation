﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GridManager))]
public class GridVisualisation : MonoBehaviour
{
    public GameObject nodeVisualisationPrefab;
    public NodeVisualisation[,] nodesVisualisationData;

    public Color baseColor = Color.white;
    public Color wallColor = Color.black;
    public Color goalColor = Color.red;
    public Color grassColor = Color.green;
    public Color waterColor = Color.blue;


    public void CreateGridVisualisation(GridManager grid)
    {
        if (grid == null)
        {
            Debug.LogWarning("GridManager has returned as null");
            return;
        }

        nodesVisualisationData = new NodeVisualisation[grid.GetGridWidth, grid.GetGridHeight];

        foreach (Node node in grid.gridNodes)
        {
            GameObject instance = Instantiate(nodeVisualisationPrefab, Vector3.zero, Quaternion.identity);
            NodeVisualisation nodeVisualisation = instance.GetComponent<NodeVisualisation>();

            if (nodeVisualisation != null)
            {
                nodeVisualisation.CreateNodeVisualisation(node);
                nodesVisualisationData[node.xIndexPosition, node.yIndexPosition] = nodeVisualisation;
                if (nodeVisualisation.gridNode.nodeType == NodeType.Blocked)
                {
                    nodeVisualisation.ColorNode(wallColor);
                }
                else if (nodeVisualisation.gridNode.nodeType == NodeType.Open)
                {
                    nodeVisualisation.ColorNode(baseColor);
                }
                else if (nodeVisualisation.gridNode.nodeType == NodeType.GoalNode)
                {
                    nodeVisualisation.ColorNode(goalColor);
                }
                else if (nodeVisualisation.gridNode.nodeType == NodeType.Grass)
                {
                    nodeVisualisation.ColorNode(grassColor);
                }
                else if (nodeVisualisation.gridNode.nodeType == NodeType.Water)
                {
                    nodeVisualisation.ColorNode(waterColor);
                }
            }
        }
    }

    public void ColorNodes(List<Node> nodelist, Color color)
    {
        foreach (Node node in nodelist)
        {
            if (node != null)
            {
                NodeVisualisation nodeVisualisation = nodesVisualisationData[node.xIndexPosition, node.yIndexPosition];

                if (nodeVisualisation != null)
                {
                    nodeVisualisation.ColorNode(color);
                }
            }
        }
    }

    public void ShowNodeArrows(bool visualaid)
    {
        foreach(var nodeVisual in nodesVisualisationData)
        {
            if (visualaid == true && nodeVisual.gridNode.nodeType != NodeType.Blocked)
            {
                nodeVisual.ArrowPosition();
            }

            if (nodeVisual.gridNode.nodeType != NodeType.Blocked)
            {
                nodeVisual.EnableObject(nodeVisual.arrow, visualaid);
            }
        }
    }
    public void ResetGridVisualisation()
    {
        foreach (NodeVisualisation nodeVisualisation in nodesVisualisationData)

            if (nodeVisualisation.gridNode.nodeType == NodeType.Blocked)
            {
                nodeVisualisation.ColorNode(wallColor);
            }
            else if (nodeVisualisation.gridNode.nodeType == NodeType.Open)
            {
                nodeVisualisation.ColorNode(baseColor);
            }
            else if (nodeVisualisation.gridNode.nodeType == NodeType.GoalNode)
            {
                nodeVisualisation.ColorNode(goalColor);
            }
            else if (nodeVisualisation.gridNode.nodeType == NodeType.Grass)
            {
                nodeVisualisation.ColorNode(grassColor);
            }
            else if (nodeVisualisation.gridNode.nodeType == NodeType.Water)
            {
                nodeVisualisation.ColorNode(waterColor);
            }
    }

    public void ChangeToFloorNode(NodeVisualisation nodevisualisation)
    {
        nodevisualisation.gridNode.nodeType = NodeType.Open;
        nodevisualisation.ColorNode(baseColor);
    }

    public void ChangeToGrassNode(NodeVisualisation nodevisualisation)
    {
        nodevisualisation.gridNode.nodeType = NodeType.Grass;
        nodevisualisation.ColorNode(grassColor);
    }

    public void ChangeToWaterNode(NodeVisualisation nodevisualisation)
    {
        nodevisualisation.gridNode.nodeType = NodeType.Water;
        nodevisualisation.ColorNode(waterColor);
    }

    public void ChangeToGoalNode(NodeVisualisation nodevisualisation)
    {
        nodevisualisation.gridNode.nodeType = NodeType.GoalNode;
        nodevisualisation.ColorNode(goalColor);
    }

    public void ChangeToGoalNodeColourOnly(NodeVisualisation nodevisualisation)
    { 
        nodevisualisation.ColorNode(goalColor);
    }

    public void ChangeToGoalNodeColourOnly(Node node)
    {
        NodeVisualisation visualNode = nodesVisualisationData[node.xIndexPosition, node.yIndexPosition];
        visualNode.ColorNode(goalColor);
    }

    public void ChangePositionOfArrow()
    {
        foreach(var nodeVisual in nodesVisualisationData)
        {
            nodeVisual.ArrowPosition();
        }
    }
}
    
    
