using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    public int width, height;
    public Grid placementGrid;
    public RoadManager roadManager;



    public GameObject[] Prefab4Way;
    public int id4WayPrefab = 0;

    public GameObject[] Prefab3Way;
    public int id3WayPrefab = 0;


    public GameObject[] roadPrefab;
    public int idPrefab = 0;

    public GameObject[] special3Way;

    public GameObject special4Way;

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

        var structureNeedingRoad = structure.GetComponent<StructureModel>();
        if (structureNeedingRoad != null)
        {
            structureNeedingRoad.RoadPosition = GetNearestRoad(position, width, height).Value;
        }

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                var newPosition = position + new Vector3Int(x, 0, z);
                placementGrid[newPosition.x, newPosition.z] = type;
                structureDictionary.Add(newPosition, structure);
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

    public List<Vector3Int> GetNeighboursOfTypeFor(Vector3Int position, CellType type)
    {
        var neighbourVertices = placementGrid.GetAdjacentCellsOfType(position.x, position.z, type);
        List<Vector3Int> neighbours = new List<Vector3Int>();
        foreach (var point in neighbourVertices)
        {
            neighbours.Add(new Vector3Int(point.X, 0, point.Y));
        }
        return neighbours;
    }

    //takes a road and return THE STRUCTURE of its neighbours
    public List<StructureModel> GetRoadNeighbours(StructureModel givenRoad)
    {
        List<StructureModel> neighbours=new List<StructureModel>();
        List<Vector3Int> positions = GetNeighboursOfTypeFor(givenRoad.RoadPosition, CellType.Road);
        foreach(Vector3Int position in positions)
        {
            neighbours.Add(GetStructureAt(position));
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
        if(resultPath==null)
        {
            return null;
        }
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
            structure.Value.RoadPosition = structure.Key;
            structureDictionary.Add(structure.Key, structure.Value);
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


    public StructureModel GetStructureAt(Point point)
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






    public void Swap(Vector3Int position)
    {
        if (placementGrid[position.x, position.z] == CellType.Road)
        {

            if (structureDictionary[position].gameObject.transform.GetChild(0).gameObject.name.Contains("4Way"))
            {
                id4WayPrefab += 1;
                id4WayPrefab = id4WayPrefab % 3;
                ModifyStructureModel(position, Prefab4Way[id4WayPrefab], Quaternion.identity);
            }

            if (structureDictionary[position].gameObject.transform.GetChild(0).gameObject.name.Contains("3Way"))
            {
                Debug.Log("YOU CLICKED A 3WAY");
                id3WayPrefab += 1;
                id3WayPrefab = id3WayPrefab % 2;
                ModifyStructureModel(position, Prefab3Way[id3WayPrefab], Quaternion.identity);

                var result = GetNeighboursTypes(position);


                if (result[0] == CellType.Road && result[1] == CellType.Road && result[2] == CellType.Road)
                {
                    //Debug.Log("180Road");
                    ModifyStructureModel(position, Prefab3Way[id3WayPrefab], Quaternion.Euler(0, 180, 0));

                }
                else if (result[1] == CellType.Road && result[2] == CellType.Road && result[3] == CellType.Road)
                {
                    //Debug.Log("270Road");
                    ModifyStructureModel(position, Prefab3Way[id3WayPrefab], Quaternion.Euler(0, 270, 0));
                }
                else if (result[2] == CellType.Road && result[3] == CellType.Road && result[0] == CellType.Road)
                {
                    //Debug.Log("idRoad");
                    ModifyStructureModel(position, Prefab3Way[id3WayPrefab], Quaternion.identity);
                }
                else if (result[3] == CellType.Road && result[0] == CellType.Road && result[1] == CellType.Road)
                {
                    //Debug.Log("90Road");
                    ModifyStructureModel(position, Prefab3Way[id3WayPrefab], Quaternion.Euler(0, 90, 0));
                }
            }

            if (structureDictionary[position].gameObject.transform.GetChild(0).gameObject.name.Contains("Straight"))
            {
                idPrefab+=1;
                idPrefab = idPrefab%5;
                ModifyStructureModel(position, roadPrefab[idPrefab], Quaternion.identity);

                //roadManager.roadFixer.FixRoadAtPosition(this, position);

                var result = GetNeighboursTypes(position);

   
                if (result[0] == CellType.Road && result[2] == CellType.Road)
                {
                    //Debug.Log("idRoad");
                    ModifyStructureModel(position, roadPrefab[idPrefab], Quaternion.identity);
                
                }
                else if (result[1] == CellType.Road && result[3] == CellType.Road)
                {
                    //Debug.Log("90Road");
                    ModifyStructureModel(position, roadPrefab[idPrefab], Quaternion.Euler(0,90,0));

                }

                var positionTest = position;
                ///////////////////////////////////////////////////////////////////////
                ////    ON A UN CROISEMENT AU DESSOUS
                ///////////////////////////////////////////////////////////////////////

                positionTest.z -= 1;
                if (structureDictionary.ContainsKey(positionTest))
                {
                    if (structureDictionary[positionTest].gameObject.transform.GetChild(0).gameObject.name.Contains("Way"))
                    {

                        ModifyStructureModel(position, roadPrefab[idPrefab], Quaternion.Euler(0,270,0));

                        if (roadPrefab[idPrefab].name.Contains("GW") || roadPrefab[idPrefab].name.Contains("STOP"))
                        {

                            //Si c'est un 4way, avec un cedez le passage au dessus on prend la forme 4Way bottom et on la retourne
                            if (structureDictionary[positionTest].gameObject.transform.GetChild(0).gameObject.name.Contains("4Way"))
                            {
                                ModifyStructureModel(positionTest, special4Way, Quaternion.Euler(0,180,0));
                            }


                            //3WAY case
                            else
                            {
                                //check si case au dessous du croisement est vide :
                                positionTest.z -= 1;
                                if (!structureDictionary.ContainsKey(positionTest))
                                {
                                    positionTest.z += 1;
                                    ModifyStructureModel(positionTest, special3Way[1], Quaternion.Euler(0,180,0));  //spe[1] = Bottom - Euler(180) -> top is at Bottom
                                    positionTest.z -= 1;
                                }
                                positionTest.z += 1;


                                //check si case a gauche du croisement est vide :
                                positionTest.x -= 1;
                                if (!structureDictionary.ContainsKey(positionTest))
                                {
                                    positionTest.x += 1;
                                    ModifyStructureModel(positionTest, special3Way[2], Quaternion.Euler(0, 270, 0));  //spe[0] = Right - Euler (270) ->
                                    positionTest.x -= 1;
                                }
                                positionTest.x += 1;


                                //check si case a droite du croisement est vide :
                                positionTest.x += 1;
                                if (!structureDictionary.ContainsKey(positionTest))
                                {
                                    positionTest.x -= 1;
                                    ModifyStructureModel(positionTest, special3Way[0], Quaternion.Euler(0, 90, 0));  //spe[0] = Left - Euler (90) -> 
                                    positionTest.x += 1;
                                }
                                positionTest.x -= 1;

                            }
                        }
                    }
                }
                positionTest.z+=1;

                ///////////////////////////////////////////////////////////////////////
                ////    ON A UN CROISEMENT AU DESSUS
                ///////////////////////////////////////////////////////////////////////

                positionTest.z += 1;
                if (structureDictionary.ContainsKey(positionTest))
                {
                    if (structureDictionary[positionTest].gameObject.transform.GetChild(0).gameObject.name.Contains("Way"))
                    {

                        if (roadPrefab[idPrefab].name.Contains("GW") || roadPrefab[idPrefab].name.Contains("STOP"))
                        {
                       
                            //4WAY case
                            if (structureDictionary[positionTest].gameObject.transform.GetChild(0).gameObject.name.Contains("4Way"))
                            {
                                ModifyStructureModel(positionTest, special4Way, Quaternion.identity);
                            }

                            //3WAY case
                            else
                            {
                                //check si case au dessus du croisement est vide :
                                positionTest.z += 1;
                                if (!structureDictionary.ContainsKey(positionTest))
                                {
                                    positionTest.z -= 1;
                                    ModifyStructureModel(positionTest, special3Way[1], Quaternion.identity);  //spe[1] = Bottom - identity -> real Bottom
                                    positionTest.z += 1;
                                }
                                positionTest.z -= 1;


                                //check si case a gauche du croisement est vide :
                                positionTest.x -= 1;
                                if (!structureDictionary.ContainsKey(positionTest))
                                {
                                    positionTest.x += 1;
                                    ModifyStructureModel(positionTest, special3Way[0], Quaternion.Euler(0,270,0));  //spe[0] = Left - Euler (270) -> left is at Bottom
                                    positionTest.x -= 1;
                                }
                                positionTest.x += 1;


                                //check si case a droite du croisement est vide :
                                positionTest.x += 1;
                                if (!structureDictionary.ContainsKey(positionTest))
                                {
                                    positionTest.x -= 1;
                                    ModifyStructureModel(positionTest, special3Way[2], Quaternion.Euler(0, 90, 0));  //spe[0] = Right - Euler (90) -> right is at Bottom
                                    positionTest.x += 1;
                                }
                                positionTest.x -= 1;

                            }
                        }
                    }
                }
                positionTest.z -= 1;

                ///////////////////////////////////////////////////////////////////////
                ////    ON A UN CROISEMENT A DROITE
                ///////////////////////////////////////////////////////////////////////

                positionTest.x+=1;
                if (structureDictionary.ContainsKey(positionTest))
                {
                    if (structureDictionary[positionTest].gameObject.transform.GetChild(0).gameObject.name.Contains("Way"))
                    {
                        ModifyStructureModel(position, roadPrefab[idPrefab], Quaternion.Euler(0, 180, 0));

                        if (roadPrefab[idPrefab].name.Contains("GW") || roadPrefab[idPrefab].name.Contains("STOP"))
                        {

                            //4WAY case
                            if (structureDictionary[positionTest].gameObject.transform.GetChild(0).gameObject.name.Contains("4Way"))
                            {
                                ModifyStructureModel(positionTest, special4Way, Quaternion.Euler(0,90,0));
                            }

                            //3WAY case
                            else
                            {
                                //check si case au dessus du croisement est vide :
                                positionTest.z += 1;
                                if (!structureDictionary.ContainsKey(positionTest))
                                {
                                    positionTest.z -= 1;
                                    ModifyStructureModel(positionTest, special3Way[0], Quaternion.identity);  
                                    positionTest.z += 1;
                                }
                                positionTest.z -= 1;


                                //check si case au dessous du croisement est vide :
                                positionTest.z -= 1;
                                if (!structureDictionary.ContainsKey(positionTest))
                                {
                                    positionTest.z += 1;
                                    ModifyStructureModel(positionTest, special3Way[2], Quaternion.Euler(0, 180, 0)); 
                                    positionTest.z -= 1;
                                }
                                positionTest.z += 1;


                                //check si case a droite du croisement est vide :
                                positionTest.x += 1;
                                if (!structureDictionary.ContainsKey(positionTest))
                                {
                                    positionTest.x -= 1;
                                    ModifyStructureModel(positionTest, special3Way[1], Quaternion.Euler(0, 90, 0));  
                                    positionTest.x += 1;
                                }
                                positionTest.x -= 1;

                            }
                        }
                    }
                }
                positionTest.x-=1;

                ///////////////////////////////////////////////////////////////////////
                ////    ON A UN CROISEMENT A GAUCHE
                ///////////////////////////////////////////////////////////////////////

                positionTest.x -= 1;
                if (structureDictionary.ContainsKey(positionTest))
                {
                    if (structureDictionary[positionTest].gameObject.transform.GetChild(0).gameObject.name.Contains("Way"))
                    {

                        if (roadPrefab[idPrefab].name.Contains("GW") || roadPrefab[idPrefab].name.Contains("STOP"))
                        {

                            //4WAY case
                            if (structureDictionary[positionTest].gameObject.transform.GetChild(0).gameObject.name.Contains("4Way"))
                            {
                                ModifyStructureModel(positionTest, special4Way, Quaternion.Euler(0, 270, 0));
                            }

                            //3WAY case
                            else
                            {
                                //check si case au dessus du croisement est vide :
                                positionTest.z += 1;
                                if (!structureDictionary.ContainsKey(positionTest))
                                {
                                    positionTest.z -= 1;
                                    ModifyStructureModel(positionTest, special3Way[2], Quaternion.identity);
                                    positionTest.z += 1;
                                }
                                positionTest.z -= 1;


                                //check si case au dessous du croisement est vide :
                                positionTest.z -= 1;
                                if (!structureDictionary.ContainsKey(positionTest))
                                {
                                    positionTest.z += 1;
                                    ModifyStructureModel(positionTest, special3Way[0], Quaternion.Euler(0, 180, 0));
                                    positionTest.z -= 1;
                                }
                                positionTest.z += 1;


                                //check si case a gauche du croisement est vide :
                                positionTest.x -= 1;
                                if (!structureDictionary.ContainsKey(positionTest))
                                {
                                    positionTest.x += 1;
                                    ModifyStructureModel(positionTest, special3Way[1], Quaternion.Euler(0, 270, 0));
                                    positionTest.x -= 1;
                                }
                                positionTest.x += 1;

                            }
                        }
                    }
                }
                positionTest.x += 1;
            }
        }
    }
}