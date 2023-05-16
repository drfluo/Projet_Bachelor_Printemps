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
        /*private List<Vector3> GetCarPath(List<Vector3Int> path, Vector3 startPosition, Vector3 endPosition)
        {
            carGraph.ClearGraph();
            CreatACarGraph(path);

            return AdjacencyGraph.AStarSearch(carGraph, startPosition, endPosition);
        }*/

        private List<Vector3> GetCarPath(List<Vector3Int> path, Vector3 startPosition, Vector3 endPosition)
        {
            if(!carGraph.isEmpty())
            {
                return AdjacencyGraph.AStarSearch(carGraph, startPosition, endPosition);
            }
            else
            {
                Debug.Log("no car graph computed");
                return null;
            }
            
        }




        //creates graph of marker for all road tiles, puts the result in carGraph
        public void GraphWholeMarkerMap()
        {
            Debug.Log("Start graph");
            carGraph.ClearGraph();
            
            //Basically a BFS kind of (BFS to go through the entire graph, however we don't exactly label things as parents but go through their markers to create the markers-graph)
            
            StructureModel currentRoad = placementManager.GetRandomRoad();
            List<StructureModel> roadNeighbor;
            List<StructureModel> visited=new List<StructureModel>();
            Queue<StructureModel> roadToVisit = new Queue<StructureModel>();


            roadToVisit.Enqueue(currentRoad);

            while(roadToVisit.Count>0) 
            {
                //need to find its road, add its marker to the graph we are creating then find all neighbors to connect them

                currentRoad = roadToVisit.Dequeue();
                Debug.Log("current road : " + currentRoad.RoadPosition);
                visited.Add(currentRoad);
                roadNeighbor = placementManager.GetRoadNeighbours(currentRoad);

                //remove everyneihbour that was already visited
                visited.ForEach(c => roadNeighbor.Remove(c));



                //add remaining neighbours to the tovisit queue
                //Debug.Log("ENQUEUE ALL "+ roadNeighbor.Count+ " NEIHBOURS");
                roadNeighbor.ForEach(x => roadToVisit.Enqueue(x));



                //create a vertex foreach marker and interconnets them, now we need to connect it to other roads
                foreach (Marker marker in currentRoad.GetCarMarkers())
                {
                    //Debug.Log("ADD VERTEX at " + marker.Position);


                    //the marker was never visited before (so we have to check its connection to the exterior)
                    if (carGraph.AddVertex(marker.Position)!=null)
                    {
                        //if it has not visited neighbours
                        if (roadNeighbor.Count != 0)
                        {
                            //if incoming another tile may be connected to it
                            if (currentRoad.GetIncomingMarkers().Contains(marker))
                            {
                                //Debug.Log("create vertex to " + marker.Position + " from " + GetClosestOutgoingMarkerOfAllRoads(roadNeighbor, marker).Position);
                                carGraph.AddEdge(GetClosestOutgoingMarkerOfAllRoads(roadNeighbor, marker).Position, marker.Position);
                            }
                            //if outgoing it might be connected to another tile
                            else if (currentRoad.GetOutgoingMarkers().Contains(marker))
                            {
                                //Debug.Log("create vertex from " + marker.Position + " to " + GetClosestIncomingMarkerOfAllRoads(roadNeighbor, marker).Position);
                                carGraph.AddEdge(marker.Position, GetClosestIncomingMarkerOfAllRoads(roadNeighbor, marker).Position);
                            }
                        }
                    }
                }

                foreach(Marker marker in currentRoad.GetCarMarkers())
                {
                    //Add neighbors from same road
                    foreach (Marker neighbor in marker.adjacentMarkers)
                    {
                        carGraph.AddEdge(marker.Position, neighbor.Position);
                    }
                }
            }
        }

        private Marker GetClosestIncomingMarkerOfAllRoads(List<StructureModel> roadList, Marker marker)
        {

            Marker closestMarker = null;
            float distance = float.MaxValue;

            foreach(StructureModel road in roadList)
            {
                foreach(Marker neihbor in road.GetIncomingMarkers())
                {

                    var newDistance = Vector3.Distance(marker.Position, neihbor.Position);
                    if(newDistance<distance)
                    {
                        closestMarker = neihbor;
                        distance = newDistance;
                    }
                }
            }

            return closestMarker;
        }

        public Marker GetClosestOutgoingMarkerOfAllRoads(List<StructureModel> roadList, Marker marker)
        {
            Marker closestMarker = null;
            float distance = float.MaxValue;

           

            foreach (StructureModel road in roadList)
            {
                foreach (Marker neihbor in road.GetOutgoingMarkers())
                {
                    var newDistance = Vector3.Distance(marker.Position, neihbor.Position);
                    if (newDistance < distance)
                    {
                        closestMarker = neihbor;
                        distance = newDistance;
                    }
                }
            }

            return closestMarker;
        }

        //useless function replaced by cargraph
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

