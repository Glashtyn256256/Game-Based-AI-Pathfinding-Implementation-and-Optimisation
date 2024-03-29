﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

public class MinHeapSceneController : MonoBehaviour
{
    public MapData mapData;
    public GridManager grid;
    public MinHeapUnit unitPrefab;
    public List<MinHeapUnit> unitData;

    GridVisualisation gridVisualisation;
    NodeVisualisation startNode;
    NodeVisualisation goalNode;

    public InputField xGoalInput;
    public InputField yGoalInput;

    public InputField xUnitInput;
    public InputField yUnitInput;

    public InputField distanceDiagnol;
    public InputField distanceStraight;

    public Text unitInfoDisplay;
    public ScrollRect scrollObject;

    Ray ray;
    RaycastHit hit;
    
    int xStart = 1;
    int yStart = 4;
    int xGoal = 20;
    int yGoal = 19;
    int xGoalInputValue;
    int yGoalInputValue;
    int xUnitInputValue;
    int yUnitInputValue;

    int algorithmIndex;
    int terrainIndex;
    int heuristicIndex;

    bool PathfindingVisualisationAid = true;

    void Start()
    {
        if (mapData != null && grid != null)
        {
            int[,] mapinstance = mapData.MakeMap();
            grid.CreateGrid(mapinstance);

            gridVisualisation = grid.gameObject.GetComponent<GridVisualisation>();

            if (gridVisualisation != null)
            {
                gridVisualisation.CreateGridVisualisation(grid);
            }

            //if our goal is not in the map range it will throw.
            if(grid.IsWithinBounds(xGoal,yGoal) && grid.IsWithinBounds(xStart, yStart))
            {
                xGoalInput.text = xGoal.ToString();
                yGoalInput.text = yGoal.ToString();
                xUnitInput.text = xStart.ToString();
                yUnitInput.text = yStart.ToString();
                startNode = gridVisualisation.nodesVisualisationData[xStart, yStart];
                goalNode = gridVisualisation.nodesVisualisationData[xGoal, yGoal];
                InstantiateUnit(startNode);
            }
        }
    }

    void Update()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        LeftMouseClickToAlterNodeTerrain();
        RightMouseClickToChangeGoalNodePositon();
        MiddleMouseClickToSpawnUnit();
    }

    //Get the intgeger value selected from the dropdown
    public void SetCurrentAlgorithmFromDropdown(int algorithmindex)
    {
        algorithmIndex = algorithmindex;
    }
    public void SetTerrainTypeFromDropdown(int terrainindex)
    {
        terrainIndex = terrainindex;
    }

    public void SetHeuristicAlgorithmFromDropdown(int heuristicindex)
    {
        heuristicIndex = heuristicindex;
    }

    public void BackToMainMenuOnClick()
    {
        SceneManager.LoadScene("Main_Menu_Pathfinding");
    }
    public void ChangeCustomDistanceOnClick()
    {
        if (distanceStraight.text != "" && distanceDiagnol.text != "")
        {
            grid.SetCustomDistanceValues(float.Parse(distanceDiagnol.text), float.Parse(distanceStraight.text));
        }
    }

    public void ToggleVisualisationAid(bool toggle)
    {
        PathfindingVisualisationAid = toggle;

        if (PathfindingVisualisationAid)
        {
            //shouldn't need to do this but just incase
            gridVisualisation.ResetGridVisualisation();
            foreach (var unit in unitData)
            {
                if(!(unitData.Count > 1))
                {
                    unit.UnitSearchVisualisation();
                }
                unit.UnitPathVisualisation();
                gridVisualisation.ChangeToGoalNodeColourOnly(goalNode);
            }
        }
        else
        {
            gridVisualisation.ResetGridVisualisation();
            gridVisualisation.ChangeToGoalNodeColourOnly(goalNode);
        }
    }
    public void ChangeGoalPositionOnButtonClick()
    {
        //If the text box is not blank then crack on otherwise throw debug message.
        if (xGoalInput.text != "" && yGoalInput.text != "")
        {
            xGoalInputValue = int.Parse(xGoalInput.text);
            yGoalInputValue = int.Parse(yGoalInput.text);

            if (grid.IsWithinBounds(xGoalInputValue, yGoalInputValue))
            {
                NodeVisualisation node = gridVisualisation.nodesVisualisationData[xGoalInputValue, yGoalInputValue];
                if (node.gridNode.nodeType != NodeType.Blocked)
                {
                    ChangeTileToGoalNode(node);
                    RecalculateUnitPath();
                }
                else
                {
                    Debug.Log("Can't place goal on a blocked node please input a valid walkable node");
                }
            }
            else
            {
                Debug.Log("Number Inputted is out of bounds. Please put a number that corresponds to the size of the map");
            }
        }
        else
        {
            Debug.Log("A value must be given");
        }
    }

    public void ChangeUnitPositionOnButtonClick()
    {
        
        if (xUnitInput.text != "" && yUnitInput.text != "")
        {
            xUnitInputValue = int.Parse(xUnitInput.text);
            yUnitInputValue = int.Parse(yUnitInput.text);
           
            if (grid.IsWithinBounds(xUnitInputValue, yUnitInputValue))
            {
                NodeVisualisation node = gridVisualisation.nodesVisualisationData[xUnitInputValue, yUnitInputValue];
                if (node.gridNode.nodeType != NodeType.Blocked)
                {                
                    unitData[0].ChangeUnitPositionWithoutUsingSpawnPosition(node.gridNode.xIndexPosition, node.gridNode.yIndexPosition);
                    unitData[0].ClearList();
                    gridVisualisation.ResetGridVisualisation();
                    gridVisualisation.ChangeToGoalNodeColourOnly(goalNode);
                }
                else
                {
                    Debug.Log("Can't place unit on a blocked node please input a valid walkable node");
                }
            }
            else
            {
                Debug.Log("Number Inputted is out of bounds. Please put a number that corresponds to the size of the map");
            }
        }
        else
        {
            Debug.Log("A value must be given for the unit");
        }
    }


    void ChangeTileToGoalNode(NodeVisualisation node)
    {
        gridVisualisation.ResetGridVisualisation();
        gridVisualisation.ChangeToFloorNode(goalNode);
        gridVisualisation.ChangeToGoalNode(node);
        goalNode = node;
    }

    void ChangeTileTerrain(NodeVisualisation node, NodeType nodeType, bool wallStatus)
    {
        node.gridNode.nodeType = nodeType;
        node.EnableObject(node.wall, wallStatus);
        gridVisualisation.ResetGridVisualisation();
    }
    
    void InstantiateUnit(NodeVisualisation node)
    {
        MinHeapUnit unit = Instantiate(unitPrefab, node.transform.position + unitPrefab.transform.position, Quaternion.identity);
        unit.SetUnitPositionInGrid(node.gridNode.xIndexPosition, node.gridNode.yIndexPosition);
        unit.InitiatePathfinding(grid, gridVisualisation);
        unitData.Add(unit);
        unit.UnitFindPath(goalNode.transform, algorithmIndex, heuristicIndex);
        AddMessageToTextBox(unit.ReturnUnitMessage(), unitData.Count - 1);
        UpdateScrollBarToBottom();
        if (PathfindingVisualisationAid)
        {
            if(!(unitData.Count > 1))
            {
                unit.UnitSearchVisualisation();
            }
            unit.UnitPathVisualisation();
            gridVisualisation.ChangeToGoalNodeColourOnly(goalNode);
        }
    }

    void AddMessageToTextBox(string unitmessage, int unitnumber) {
        unitInfoDisplay.text += "Unit " + unitnumber + " - " + unitmessage;
        unitInfoDisplay.text += "\n";
       
    }
    void ClearTextFromDisplay()
    {
        if (unitData.Count > 1)
        {
            unitInfoDisplay.text = "";
        }
    }
    void UpdateScrollBarToBottom()
    {
        Canvas.ForceUpdateCanvases();
        scrollObject.verticalNormalizedPosition = 0.0f;
    }

    void RecalculateUnitPath() 
    {
        ClearTextFromDisplay();
        for (int i = 0; i < unitData.Count; i++)
        {
            unitData[i].UnitFindPath(goalNode.transform, algorithmIndex, heuristicIndex);
            
            AddMessageToTextBox(unitData[i].ReturnUnitMessage(), i);
            if (PathfindingVisualisationAid)
            {
                if (!(unitData.Count > 1))
                {
                    unitData[i].UnitSearchVisualisation();
                }
                unitData[i].UnitPathVisualisation();
                gridVisualisation.ChangeToGoalNodeColourOnly(goalNode);
            }
        }
        UpdateScrollBarToBottom();
    }

    void SearchUnitPathRecalculate(NodeVisualisation node)
    {
        ClearTextFromDisplay();
        for (int i = 0; i < unitData.Count; i++)
        {
            if (unitData[i].SearchPathChangedNode(node.gridNode))
            {
                unitData[i].UnitFindPath(goalNode.transform, algorithmIndex, heuristicIndex);
            }
            AddMessageToTextBox(unitData[i].ReturnUnitMessage(), i);
            if (PathfindingVisualisationAid)
            {
                if (!(unitData.Count > 1))
                {
                    unitData[i].UnitSearchVisualisation();
                }
                unitData[i].UnitPathVisualisation();
                gridVisualisation.ChangeToGoalNodeColourOnly(goalNode);
            }
          
        }
        UpdateScrollBarToBottom();
    }

    //When you left click the mouse button, it will
    void LeftMouseClickToAlterNodeTerrain()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log(hit.collider.gameObject.tag);
                if (hit.collider.gameObject.tag == "Node")
                {
                    NodeVisualisation nodeVisualisation = hit.collider.gameObject.GetComponentInParent<NodeVisualisation>();
                    if (nodeVisualisation != goalNode)
                    {
                        
                            switch (terrainIndex)
                            {
                                case 0:
                                    ChangeTileTerrain(nodeVisualisation, NodeType.Open, false);
                                    RecalculateUnitPath();
                                    break;
                                case 1:
                                    ChangeTileTerrain(nodeVisualisation, NodeType.Blocked, true);
                                    SearchUnitPathRecalculate(nodeVisualisation);
                                    break;
                                case 2:
                                    ChangeTileTerrain(nodeVisualisation, NodeType.Grass, false);
                                    RecalculateUnitPath();
                                    break;
                                case 3:
                                    ChangeTileTerrain(nodeVisualisation, NodeType.Water, false);
                                    RecalculateUnitPath();
                                    break;
                                default:
                                    break;
                            }
                    }
                }
            }
        }
    }

    void RightMouseClickToChangeGoalNodePositon() 
    {
        //Right click
        if (Input.GetMouseButtonDown(1))
        {
            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log(hit.collider.gameObject.tag);
                //added mesh chollider to tile to make sure we are able to hit the tile plane.
                if (hit.collider.gameObject.tag == "Node")
                {
                    NodeVisualisation nodeVisualisation = hit.collider.gameObject.GetComponentInParent<NodeVisualisation>();
                    if (nodeVisualisation.gridNode.nodeType != NodeType.Blocked)
                    {
                        ChangeTileToGoalNode(nodeVisualisation);
                        RecalculateUnitPath();
                    }
                }
            }
        }
    }

    void MiddleMouseClickToSpawnUnit()
    {
        if (Input.GetMouseButtonDown(2))
        {
            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log(hit.collider.gameObject.tag);
                //added mesh chollider to tile to make sure we are able to hit the tile plane.
                if (hit.collider.gameObject.tag == "Node")
                {
                    NodeVisualisation nodeVisualisation = hit.collider.gameObject.GetComponentInParent<NodeVisualisation>();
                    if (nodeVisualisation.gridNode.nodeType != NodeType.Blocked && nodeVisualisation.gridNode.nodeType != NodeType.GoalNode)
                    {
                        InstantiateUnit(nodeVisualisation);
                    }
                }
            }
        }
    }
}

