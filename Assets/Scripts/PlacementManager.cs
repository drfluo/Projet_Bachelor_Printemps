using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    public int width, height;
    Grid placementGrid;
    public RoadManager roadManager;


    private Dictionary<Vector3Int, StructureModel> temporaryRoadobjects = new Dictionary<Vector3Int, StructureModel>();
    private Dictionary<Vector3Int, StructureModel> structureDictionary = new Dictionary<Vector3Int, StructureModel>();

    private void Start()
    {
        placementGrid = new Grid(width, height);
    }

    internal CellType[] GetNeighboursTypes(Vector3Int position)
    {
        return placementGrid.GetAllAdjacentCellTypes(position.x, position.z);
    }

    internal bool CheckIfPositionInBound(Vector3Int position)
    {
        if (position.x >= 0 && position.x < width && position.z >= 0 && position.z < height)
        {
            return true;
        }
        return false;
    }

    internal void PlaceObjectOnTheMap(Vector3Int position, GameObject structurePrefab, CellType type, int width = 1, int height = 1)
    {
        StructureModel structure = CreateANewStructureModel(position, structurePrefab, type);

        var structureNeedingRoad = structure.GetComponent<INeedingRoad>();
        if (structureNeedingRoad != null)
        {
            structureNeedingRoad.RoadPosition = GetNearestRoad(position, width, height).Value;
            Debug.Log("My nearest road position is: " + structureNeedingRoad.RoadPosition);
        }

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                var newPosition = position + new Vector3Int(x, 0, z);
                placementGrid[newPosition.x, newPosition.z] = type;
                structureDictionary.Add(newPosition, structure);
                DestroyNatureAt(newPosition);
            }
        }

    }

    private Vector3Int? GetNearestRoad(Vector3Int position, int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var newPosition = position + new Vector3Int(x, 0, y);
                var roads = GetNeighboursOfTypeFor(newPosition, CellType.Road);
                if (roads.Count > 0)
                {
                    return roads[0];
                }
            }
        }
        return null;
    }

    private void DestroyNatureAt(Vector3Int position)
    {
        RaycastHit[] hits = Physics.BoxCastAll(position + new Vector3(0, 0.5f, 0), new Vector3(0.5f, 0.5f, 0.5f), transform.up, Quaternion.identity, 1f, 1 << LayerMask.NameToLayer("Nature"));
        foreach (var item in hits)
        {
            Destroy(item.collider.gameObject);
        }
    }

    internal bool CheckIfPositionIsFree(Vector3Int position)
    {
        return CheckIfPositionIsOfType(position, CellType.Empty);
    }

    private bool CheckIfPositionIsOfType(Vector3Int position, CellType type)
    {
        return placementGrid[position.x, position.z] == type;
    }

    internal void PlaceTemporaryStructure(Vector3Int position, GameObject structurePrefab, CellType type)
    {
        placementGrid[position.x, position.z] = type;
        StructureModel structure = CreateANewStructureModel(position, structurePrefab, type);
        temporaryRoadobjects.Add(position, structure);
    }

    internal List<Vector3Int> GetNeighboursOfTypeFor(Vector3Int position, CellType type)
    {
        var neighbourVertices = placementGrid.GetAdjacentCellsOfType(position.x, position.z, type);
        List<Vector3Int> neighbours = new List<Vector3Int>();
        foreach (var point in neighbourVertices)
        {
            neighbours.Add(new Vector3Int(point.X, 0, point.Y));
        }
        return neighbours;
    }

    private StructureModel CreateANewStructureModel(Vector3Int position, GameObject structurePrefab, CellType type)
    {
        GameObject structure = new GameObject(type.ToString());
        structure.transform.SetParent(transform);
        structure.transform.localPosition = position;

        StructureModel structureModel = structure.AddComponent<StructureModel>();
        structureModel.CreateModel(structurePrefab);
        return structureModel;
    }

    internal List<Vector3Int> GetPathBetween(Vector3Int startPosition, Vector3Int endPosition, bool isAgent = false)
    {
        var resultPath = GridSearch.AStarSearch(placementGrid, new Point(startPosition.x, startPosition.z), new Point(endPosition.x, endPosition.z), isAgent);
        List<Vector3Int> path = new List<Vector3Int>();
        foreach (Point point in resultPath)
        {
            path.Add(new Vector3Int(point.X, 0, point.Y));
        }
        return path;
    }

    internal void RemoveAllTemporaryStructures()
    {
        foreach (var structure in temporaryRoadobjects.Values)
        {
            var position = Vector3Int.RoundToInt(structure.transform.position);
            placementGrid[position.x, position.z] = CellType.Empty;
            Destroy(structure.gameObject);
        }
        temporaryRoadobjects.Clear();
    }

    internal void AddtemporaryStructuresToStructureDictionary()
    {
        foreach (var structure in temporaryRoadobjects)
        {
            structureDictionary.Add(structure.Key, structure.Value);
            DestroyNatureAt(structure.Key);
        }
        temporaryRoadobjects.Clear();
    }

    public void ModifyStructureModel(Vector3Int position, GameObject newModel, Quaternion rotation)
    {
        if (temporaryRoadobjects.ContainsKey(position))
            temporaryRoadobjects[position].SwapModel(newModel, rotation);
        else if (structureDictionary.ContainsKey(position))
            structureDictionary[position].SwapModel(newModel, rotation);
    }

    public StructureModel GetRandomRoad()
    {
        var point = placementGrid.GetRandomRoadPoint();
        return GetStructureAt(point);
    }

    public StructureModel GetRandomSpecialStrucutre()
    {
        var point = placementGrid.GetRandomSpecialStructurePoint();
        return GetStructureAt(point);
    }

    public StructureModel GetRandomHouseStructure()
    {
        var point = placementGrid.GetRandomHouseStructurePoint();
        return GetStructureAt(point);
    }

    public List<StructureModel> GetAllHouses()
    {
        List<StructureModel> returnList = new List<StructureModel>();
        var housePositions = placementGrid.GetAllHouses();
        foreach (var point in housePositions)
        {
            returnList.Add(structureDictionary[new Vector3Int(point.X, 0, point.Y)]);
        }
        return returnList;
    }

    internal List<StructureModel> GetAllSpecialStructures()
    {
        List<StructureModel> returnList = new List<StructureModel>();
        var housePositions = placementGrid.GetAllSpecialStructure();
        foreach (var point in housePositions)
        {
            returnList.Add(structureDictionary[new Vector3Int(point.X, 0, point.Y)]);
        }
        return returnList;
    }


    private StructureModel GetStructureAt(Point point)
    {
        if (point != null)
        {
            return structureDictionary[new Vector3Int(point.X, 0, point.Y)];
        }
        return null;
    }

    public StructureModel GetStructureAt(Vector3Int position)
    {
        if (structureDictionary.ContainsKey(position))
        {
            return structureDictionary[position];
        }
        return null;
    }


    //Check if the bulding still has a road next to it, if not then deletes it
    public void FixBuildingAtPosition(Vector3Int position)
    {
        var result = GetNeighboursTypes(position);
        int roadCount = 0;
        roadCount = result.Where(Matrix4x4 => Matrix4x4 == CellType.Road).Count();
        if (roadCount == 0)
        {
            Debug.Log("House shouldn't be here");
            Destroy(structureDictionary[position].gameObject);
            placementGrid.removeElementFromList(position);
            structureDictionary.Remove(position);
            placementGrid[position.x, position.z] = CellType.Empty;

            //DANS GRID ON ENLEVE DE LA BONNE LISTE
            
        }
    }




    public void RemoveRoad(Vector3Int position)
    {
        if (placementGrid[position.x, position.z] != CellType.Empty)
        {
            placementGrid[position.x, position.z] = CellType.Empty;
            Destroy(structureDictionary[position].gameObject);
            structureDictionary.Remove(position);

            /*TODO : implement this solution so it works (more elegant)
             * 
             * 
             * var neighbours = GetNeighboursOfTypeFor(position, CellType.Road);
            foreach(var positionToFix in neighbours)
            {
                roadFixer.FixRoadAtPosition(this, positionToFix);
            }*/

            //cell du dessus
            position.x += 1;
            if (placementGrid[position.x, position.z] == CellType.Road)
            {
                placementGrid[position.x, position.z] = CellType.Empty;
                Destroy(structureDictionary[position].gameObject);
                structureDictionary.Remove(position);


                roadManager.PlaceRoad(position);
                roadManager.FinishPlacingRoad();
            }
            else if (placementGrid[position.x, position.z] != CellType.Empty)
            {
                FixBuildingAtPosition(position);
            }


            position.x -= 2;
            if (placementGrid[position.x, position.z] == CellType.Road)
            {
                placementGrid[position.x, position.z] = CellType.Empty;
                Destroy(structureDictionary[position].gameObject);
                structureDictionary.Remove(position);


                roadManager.PlaceRoad(position);
                roadManager.FinishPlacingRoad();

            }
            else if (placementGrid[position.x, position.z] != CellType.Empty)
            {
                FixBuildingAtPosition(position);
            }

            position.x += 1;


            position.z -= 1;
            if (placementGrid[position.x, position.z] == CellType.Road)
            {
                placementGrid[position.x, position.z] = CellType.Empty;
                Destroy(structureDictionary[position].gameObject);
                structureDictionary.Remove(position);


                roadManager.PlaceRoad(position);
                roadManager.FinishPlacingRoad();
            }
            else if (placementGrid[position.x, position.z] != CellType.Empty)
            {
                FixBuildingAtPosition(position);
            }

            position.z += 2;
            if (placementGrid[position.x, position.z] == CellType.Road)
            {
                placementGrid[position.x, position.z] = CellType.Empty;
                Destroy(structureDictionary[position].gameObject);
                structureDictionary.Remove(position);


                roadManager.PlaceRoad(position);
                roadManager.FinishPlacingRoad();
            }
            else if (placementGrid[position.x, position.z] != CellType.Empty)
            {
                FixBuildingAtPosition(position);
            }

        }
    }

}
