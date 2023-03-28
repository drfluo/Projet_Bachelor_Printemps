using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    public int width, height;
    Grid placementGrid;

    private void Start() 
    {
        placementGrid=new Grid(width,height);
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

    public void PlaceTemporaryStructure(Vector3Int position, GameObject roadStraight, CellType type)//, CellType.Road
    {
        placementGrid[position.x,position.z]= type;
        GameObject newStructure = Instantiate(roadStraight, position, Quaternion.identity);
    }
}
