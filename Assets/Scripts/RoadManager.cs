using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadManager : MonoBehaviour
{
    public PlacementManager placementManager;

    public List<Vector3Int> temporaryPlacementPosition = new List<Vector3Int>();

    public GameObject roadStraight;

    public void PlaceRoad(Vector3Int position)
    {
        if(placementManager.CheckIfPositionInBound(position)==false)
        {
            return;
        }
        if(placementManager.CheckIfPositionIsFree(position)==false)
        {
            return;
        }
        placementManager.PlaceTemporaryStructure(position, roadStraight,  CellType.Road); 
    }
}
