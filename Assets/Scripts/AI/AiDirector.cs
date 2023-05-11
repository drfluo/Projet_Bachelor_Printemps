using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SimpleCity.AI
{
    public class AiDirector : MonoBehaviour
    {
        public PlacementManager placementManager;

        public GameObject carPrefab;

        AdjacencyGraph carGraph = new AdjacencyGraph();

        List<Vector3> carPath = new List<Vector3>();
        
        public void SpawnACar()
        {
            foreach (var house in placementManager.GetAllHouses())
            {
                TrySpawninACar(house, placementManager.GetRandomSpecialStrucutre());
            }
        }



        private void TrySpawninACar(StructureModel startStructure, StructureModel endStructure)
        {
            if (startStructure != null && endStructure != null)
            {
                var startRoadPosition = ((INeedingRoad)startStructure).RoadPosition;
                var endRoadPosition = ((INeedingRoad)endStructure).RoadPosition;

                var path = placementManager.GetPathBetween(startRoadPosition, endRoadPosition, true);
                path.Reverse();
                
                if (path.Count == 0 && path.Count>2)
                    return;
                var startMarkerPosition = placementManager.GetStructureAt(startRoadPosition).GetCarSpawnMarker(path[1]);

                var endMarkerPosition = placementManager.GetStructureAt(endRoadPosition).GetCarEndMarker(path[path.Count-2]);

                carPath = GetCarPath(path, startMarkerPosition.Position, endMarkerPosition.Position);

                if(carPath.Count > 0)
                {
                    var car = Instantiate(carPrefab, startMarkerPosition.Position, Quaternion.identity);
                    car.GetComponent<CarAI>().SetPath(carPath);
                }
            }
        }


//Choose a path by A* in graph of possible path
        private List<Vector3> GetCarPath(List<Vector3Int> path, Vector3 startPosition, Vector3 endPosition)
        {
            carGraph.ClearGraph();
            CreatACarGraph(path);

            return AdjacencyGraph.AStarSearch(carGraph, startPosition, endPosition);
        }


        //creates graph of marker for all road tiles, puts the result in carGraph
        private void GraphWholeMarkerMap(Grid roadGrid)
        {
            carGraph.ClearGraph();
            
            //Basically a BFS kind of (BFS to go through the entire graph, however we don't exactly label things as parents but go through their markers to create the markers-graph)
            StructureModel initialRoad = placementManager.GetRandomRoad();
            StructureModel currentRoad;
            List<StructureModel> roadNeighbor;
            List<StructureModel> visited=new List<StructureModel>();
            Queue<StructureModel> roadToVisit = new Queue<StructureModel>();

            roadToVisit.Enqueue(initialRoad);
            visited.Add(initialRoad);

            while(roadToVisit.Count>0) 
            {
                //need to find its road, add its marker to the graph we are creating then find all neighbors to connect them

                currentRoad = roadToVisit.Dequeue();
                roadNeighbor = placementManager.GetRoadNeighbours(currentRoad);

                foreach (StructureModel road in roadNeighbor) //keep only the not visited one and remind to check them, the others are done
                {
                    if(visited.Contains(road))
                    {
                        roadNeighbor.Remove(road);
                    }
                    else
                    {
                        roadToVisit.Enqueue(road);
                    }
                }


                //create a vertex foreach marker and interconnets them, now we need to connect it to other roads
                foreach(Marker marker in currentRoad.GetCarMarkers())
                {
                    carGraph.AddVertex(marker.Position);

                    //Add neighbors from same road
                    foreach(Marker neighbor in marker.adjacentMarkers)
                    {
                        carGraph.AddEdge(marker.Position, neighbor.Position);
                    }

                    if(marker.OpenForconnections)
                    {
                        //need to find closest neighbour and add edge
                    }

                }

                //Needs to create edge from neighbours to incomming-> how to check if neihbour in right direction ? need closest from a list of road not just one I THINL CHECKING CENTER OF ROAD AND THEN CALLING THE FUNCTION TO FIND THE NEIGHBOUR WORKS
                foreach (Marker marker in currentRoad.GetIncomingMarkers())
                {
                    //adds edge from closest outgoing (from other road) to us (incoming)
                    carGraph.AddEdge(GetClosestOutgoingMarkerOfAllRoads(roadNeighbor,marker).Position, marker.Position);
                }

                //needs to create edge from outgoing to neighbours -> how to check if neihbour in right direction ? need closest from a list of road not just one
                foreach(Marker marker in currentRoad.GetOutgoingMarkers())
                {
                    //adds edge from us (outgoing) to closest incoming (from other road)
                    carGraph.AddEdge(marker.Position,GetClosestIncomingMarkerOfAllRoads(roadNeighbor, marker).Position);
                }

            }

        }

        private Marker GetClosestIncomingMarkerOfAllRoads(List<StructureModel> roadList, Marker marker)
        {
            StructureModel closestRoad = null;
            float distance = float.MaxValue;

            //go through list of road, finds one where center is the closest from our marker
            foreach (StructureModel road in roadList)
            {
                var newDistance = Vector3.Distance(road.RoadPosition, marker.Position);
                if (newDistance > distance)
                {
                    distance = newDistance;
                    closestRoad = road;
                }
            }

            //returns the "spawn point" aka the closest incoming marker of the nearest road
            return closestRoad.GetCarSpawnMarker(marker.Position);
        }

        private Marker GetClosestOutgoingMarkerOfAllRoads(List<StructureModel> roadList, Marker marker)
        {
            StructureModel closestRoad = null;
            float distance = float.MaxValue;

            //go through list of road, finds one where center is the closest from our marker
            foreach (StructureModel road in roadList)
            {
                var newDistance = Vector3.Distance(road.RoadPosition, marker.Position);
                if (newDistance > distance)
                {
                    distance = newDistance;
                    closestRoad = road;
                }
            }

            //returns the "End point" aka the closest outgoing marker of the nearest road
            return closestRoad.GetCarEndMarker(marker.Position);
        }

        //creates graph of all possible path for a car
        private void CreatACarGraph(List<Vector3Int> path)
        {

            Dictionary<Marker, Vector3> tempDictionary = new Dictionary<Marker, Vector3>();
            for (int i = 0; i < path.Count; i++)
            {
                var currentPosition = path[i];
                var roadStructure = placementManager.GetStructureAt(currentPosition);
                var markersList = roadStructure.GetCarMarkers();
                var limitDistance = markersList.Count > 3;
                tempDictionary.Clear();

                foreach (var marker in markersList)
                {
                    carGraph.AddVertex(marker.Position);
                    foreach (var markerNeighbour in marker.adjacentMarkers)
                    {
                        carGraph.AddEdge(marker.Position, markerNeighbour.Position);
                    }
                    //openforconnections -> can connect to another cell
                    if(marker.OpenForconnections && i + 1 < path.Count)
                    {
                        var nextRoadPosition = placementManager.GetStructureAt(path[i + 1]);
                        if (limitDistance)
                        {
                            tempDictionary.Add(marker, nextRoadPosition.GetNearestCarMarkerTo(marker.Position));
                        }
                        else
                        {
                            carGraph.AddEdge(marker.Position, nextRoadPosition.GetNearestCarMarkerTo(marker.Position));
                        }
                    }
                }
                if (limitDistance && tempDictionary.Count > 2)
                {
                    var distanceSortedMarkers = tempDictionary.OrderBy(x => Vector3.Distance(x.Key.Position, x.Value)).ToList();
                    for (int j = 0; j < 2; j++)
                    {
                        carGraph.AddEdge(distanceSortedMarkers[j].Key.Position, distanceSortedMarkers[j].Value);
                    }
                }
            }
        }
    }
}

