using SimpleCity.AI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureModel : MonoBehaviour, INeedingRoad
{
    float yHeight = 0;

    public Vector3Int RoadPosition { get; set; }

    public void CreateModel(GameObject model)
    {
        var structure = Instantiate(model, transform);
        yHeight = structure.transform.position.y;
    }

    public void SwapModel(GameObject model, Quaternion rotation)
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        var structure = Instantiate(model, transform);
        structure.transform.localPosition = new Vector3(0, 0, 0);
        structure.transform.localRotation = rotation;
    }

    public List<Marker> GetCarMarkers()
    {
        return transform.GetChild(0).GetComponent<RoadHelper>().GetAllCarMarkers();
    }

    public Vector3 GetNearestCarMarkerTo(Vector3 position)
    {
        return transform.GetChild(0).GetComponent<RoadHelper>().GetClosestCarMarkerPosition(position);
    }

    public Marker GetCarSpawnMarker(Vector3 nextPathPosition)
    {
        return transform.GetChild(0).GetComponent<RoadHelper>().GetPositioForCarToSpawn(nextPathPosition);
    }

    public Marker GetCarEndMarker(Vector3 previousPathPosition)
    {
        return transform.GetChild(0).GetComponent<RoadHelper>().GetPositioForCarToEnd(previousPathPosition);
    }

    public List<Marker> GetIncomingMarkers()
    {
        return transform.GetChild(0).GetComponent<RoadHelper>().GetAllIncomingMarkers();
    }

    public List<Marker> GetOutgoingMarkers()
    {
        return transform.GetChild(0).GetComponent<RoadHelper>().GetAllOutgoingMarkers();
    }
}
