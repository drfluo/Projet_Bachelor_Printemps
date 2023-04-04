using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlacementManager : MonoBehaviour
{
    public RoadManager roadManager;
    public int width, height;
    Grid placementGrid;

    private Dictionary<Vector3Int, StructureModel> temporaryRoadObjects = new Dictionary<Vector3Int, StructureModel>();
    private Dictionary<Vector3Int, StructureModel> structureDictionary = new Dictionary<Vector3Int, StructureModel>();
    public RoadFixer roadFixer;

    private void Start() 
    {
        roadFixer = GetComponent<RoadFixer>();

        placementGrid=new Grid(width,height);
    }

    internal CellType[] GetNeighbourtTypesFor(Vector3Int position)
    {
        return placementGrid.GetAllAdjacentCellTypes(position.x, position.z);
    
    }

    public bool CheckIfPositionInBound(Vector3Int position)
    {
         if(position.x>=0 && position.x < width && position.z>=0 && position.z<height)
         {
            return true;
         }
         return false;
    }

    public bool CheckIfPositionIsFree(Vector3Int position)
    {
        return CheckIfPositionIsOfType(position, CellType.Empty);
    }

    public bool CheckIfPositionIsOfType(Vector3Int position, CellType type)
    {
        return placementGrid[position.x, position.z]==type;
    }

    public void PlaceTemporaryStructure(Vector3Int position, GameObject structurePrefab, CellType type)//, CellType.Road
    {
        placementGrid[position.x,position.z]= type;
        StructureModel structure = CreateANewStructureModel(position, structurePrefab, type);
        temporaryRoadObjects.Add(position, structure);
    }



    internal List<Vector3Int> GetNeighboursOfTypeFor(Vector3Int position, CellType road)
    {
        var neighbourVertices = placementGrid.GetAdjacentCellsOfType(position.x, position.z, road);
        List<Vector3Int> neighbours = new List<Vector3Int>();
        foreach(var point in neighbourVertices)
        {
            neighbours.Add(new Vector3Int(point.X, 0, point.Y));
        }
        return neighbours;
    }

    private StructureModel CreateANewStructureModel(Vector3Int position, GameObject srtucturePrefab, CellType type)
    {
        GameObject structure = new GameObject(type.ToString());
        structure.transform.SetParent(transform);
        structure.transform.localPosition = position;
        var structureModel = structure.AddComponent<StructureModel>();
        structureModel.CreateModel(srtucturePrefab);
        return structureModel;
    }


    internal List<Vector3Int> GetPathBetween(Vector3Int startPosition, Vector3Int endPosition)
    {
        var resultPath = GridSearch.AStarSearch(placementGrid, new Point(startPosition.x, startPosition.z), new Point(endPosition.x, endPosition.z));
        List<Vector3Int> path = new List<Vector3Int>();
        foreach(Point point in resultPath)
        {
            path.Add(new Vector3Int(point.X, 0, point.Y));
        }
        return path;
    }

    internal void RemoveAllTemporaryStructures()
    {
        foreach(var structure in temporaryRoadObjects.Values)
        {
            var position = Vector3Int.RoundToInt(structure.transform.position);
            placementGrid[position.x, position.z] = CellType.Empty;
            Destroy(structure.gameObject);
        }
        temporaryRoadObjects.Clear();
    }

    internal void AddtemporaryStructureToStructureDictionary()
    {
        foreach(var structure in temporaryRoadObjects)
        {
            structureDictionary.Add(structure.Key, structure.Value);
        }
        temporaryRoadObjects.Clear();
    }

    public void ModifyStructureModel(Vector3Int position, GameObject newModel, Quaternion rotation)
    {
        if (temporaryRoadObjects.ContainsKey(position))
        {
            temporaryRoadObjects[position].SwapModel(newModel, rotation);
        }else if (structureDictionary.ContainsKey(position))
        {
            structureDictionary[position].SwapModel(newModel, rotation);
        }
    }


    public void RemoveRoad(Vector3Int position)
    {
            Debug.Log(placementGrid[position.x,position.z]);
            
            //Debug.Log(structureDictionary[position]);
            if(placementGrid[position.x,position.z]!=CellType.Empty)
            {
                placementGrid[position.x, position.z] = CellType.Empty;
                Destroy(structureDictionary[position].gameObject); 
                structureDictionary.Remove(position);

                /*var neighbours = GetNeighboursOfTypeFor(position, CellType.Road);
                foreach(var positionToFix in neighbours)
                {
                    roadFixer.FixRoadAtPosition(this, positionToFix);
                }*/

                //cell du dessus
                position.x+=1;
                if(placementGrid[position.x,position.z]!=CellType.Empty)
                {
                    placementGrid[position.x, position.z] = CellType.Empty;
                    Destroy(structureDictionary[position].gameObject); 
                    structureDictionary.Remove(position);
                    
                    Debug.Log("just avamt le üéace rpad");

                    roadManager.PlaceRoad(position);
                    roadManager.FinishPlacingRoad();
                }


                position.x-=2;
                if(placementGrid[position.x,position.z]!=CellType.Empty)
                {
                    placementGrid[position.x, position.z] = CellType.Empty;
                    Destroy(structureDictionary[position].gameObject); 
                    structureDictionary.Remove(position);
                    
                    Debug.Log("just avamt le üéace rpad");

                    roadManager.PlaceRoad(position);
                    roadManager.FinishPlacingRoad();

                }

                position.x+=1;


                position.z-=1;
                if(placementGrid[position.x,position.z]!=CellType.Empty)
                {
                    placementGrid[position.x, position.z] = CellType.Empty;
                    Destroy(structureDictionary[position].gameObject); 
                    structureDictionary.Remove(position);
                    
                    Debug.Log("just avamt le üéace rpad");

                    roadManager.PlaceRoad(position);
                    roadManager.FinishPlacingRoad();
                }

                position.z+=2;
                if(placementGrid[position.x,position.z]!=CellType.Empty)
                {
                    placementGrid[position.x, position.z] = CellType.Empty;
                    Destroy(structureDictionary[position].gameObject); 
                    structureDictionary.Remove(position);
                    
                    Debug.Log("just avamt le üéace rpad");

                    roadManager.PlaceRoad(position);
                    roadManager.FinishPlacingRoad();
                }

            }
    }


}
