using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;


namespace SimpleCity.AI
{
    public class AiDirector : MonoBehaviour
    {
        public PlacementManager placementManager;

        public GameObject carPrefab;

        AdjacencyGraph carGraph = new AdjacencyGraph();

        List<Vector3> carPath = new List<Vector3>();


        public CarAI SpawnACarWithReturn(PathChosen path)
        {
            return TrySpawninACar(placementManager.GetRandomHouseStructure(), placementManager.GetRandomSpecialStrucutre(), path);
        }

        public void SpawnACar()//for button spawn a car on UI
        {
            TrySpawninACar(placementManager.GetRandomHouseStructure(), placementManager.GetRandomSpecialStrucutre(), PathChosen.Best);
        }

        public void SpawnACarSecond()//for button spawn a car on UI
        {
            TrySpawninACar(placementManager.GetRandomHouseStructure(), placementManager.GetRandomSpecialStrucutre(), PathChosen.Second);
        }

        public void SpawnACarThird()//for button spawn a car on UI
        {
            TrySpawninACar(placementManager.GetRandomHouseStructure(), placementManager.GetRandomSpecialStrucutre(), PathChosen.Third);
        }


        public IEnumerator SpawnCarCoroutine()
        {
            GameObject spawnedCar = Instantiate(carPrefab);
            CarAI carComponent = spawnedCar.GetComponent<CarAI>();

            yield return spawnedCar; // Renvoie la référence de la voiture instanciée
        }



        private CarAI TrySpawninACar(StructureModel startStructure, StructureModel endStructure,PathChosen pathChose, int i=0)
        {
            if (startStructure != null && endStructure != null)
            {
                //var startRoadPosition = startStructure.RoadPosition;
                //var endRoadPosition = endStructure.RoadPosition;


                List<Marker> allStartPositions= placementManager.GetSpawnAround(startStructure);
                List<Vector3> allEndPositions= placementManager.GetEndAround(endStructure);

                //a buildin can have up to 4 entrance or exit so has to test all of them for a path
                //also remembers the second best for if we need it
                int sizeBestPath = int.MaxValue;
                int sizeSeconBestPath = int.MaxValue;
                int sizeThirdBestPath = int.MaxValue;
                List<Vector3> bestPath = new List<Vector3>();
                List<Vector3> SecondBestPath = new List<Vector3>();
                List<Vector3> ThirdBestPath = new List<Vector3>();
                foreach (Marker start in allStartPositions)
                {
                    foreach(Vector3 stop in allEndPositions)
                    {
                        //try getting the fastes path from this start to this stop
                        carPath = GetCarPath(start.Position, stop, PathChosen.Best);
                        //if not null and better
                        if(carPath!=null && carPath.Count< sizeBestPath && carPath.Count != 0)
                        {
                            bestPath = carPath;
                            sizeBestPath = carPath.Count;
                        }
                        //if not better than fastsest but better than second
                        else if(carPath != null && carPath.Count < sizeSeconBestPath && carPath.Count!=0)
                        {
                            SecondBestPath = carPath;
                            sizeSeconBestPath = carPath.Count;
                        }
                        else if(carPath != null && carPath.Count < sizeThirdBestPath && carPath.Count != 0)
                        {
                            ThirdBestPath = carPath;
                            sizeThirdBestPath = carPath.Count;
                        }
                    }
                }

                if(bestPath.Count==0)
                {
                    return null;
                }


                //if want best : easy just give best found
                if(pathChose==PathChosen.Best)
                {
                    carPath = bestPath;
                }
                //if want second best compare Yen of first and second found, if none found then give first
                else
                {
                    carPath = GetCarPath(bestPath[0], bestPath[bestPath.Count-1], PathChosen.Second);
                    if(pathChose == PathChosen.Second)
                    {
                        if ((SecondBestPath.Count != 0 && carPath != null && SecondBestPath.Count < carPath.Count) || (SecondBestPath.Count != 0 && carPath == null))
                        {
                            carPath = SecondBestPath;
                        }
                        if (carPath == null || carPath.Count == 0)
                        {
                            carPath = bestPath;
                        }
                    }
                    else//want 3rd best path
                    {
                        if(ThirdBestPath.Count!=0 && carPath==null)
                        {
                            carPath = ThirdBestPath;
                        }
                        else if(ThirdBestPath.Count==0 && carPath==null)
                        {
                            if(SecondBestPath.Count!=0)
                            {
                                carPath = SecondBestPath;
                            }
                            else
                            {
                                carPath = bestPath;
                            }
                        }
                        else if(ThirdBestPath.Count != 0 && carPath!=null)
                        {
                            if(carPath.Count<SecondBestPath.Count || carPath.Count > ThirdBestPath.Count)
                            {
                                carPath = ThirdBestPath;
                            }
                        }
                    }

                }




                if (carPath != null && carPath.Count > 0)
                {
                    Marker marker = null;

                    foreach(Marker markerStart in allStartPositions)
                    {
                        if(markerStart.Position==carPath[0])
                        {
                            marker = markerStart;
                        }
                    }




                    if(marker==null)
                    {
                        Debug.Log("HGE ISSUE : Path found but not marker start");
                        return null;
                    }


                    if(marker.IsOccupied!=0 && i<15)
                    {
                        i++;
                        return TrySpawninACar(placementManager.GetRandomHouseStructure(), placementManager.GetRandomSpecialStrucutre(), pathChose,i);
                    }

                    if(i>=15)
                    {
                        return null;
                    }


                    

                    var car = Instantiate(carPrefab, carPath[0], Quaternion.identity);
                    CarAI toreturn = car.GetComponent<CarAI>();
                    toreturn.SetPath(carPath);

                    return toreturn;
                }
                Debug.Log("No path found for this car");

                return null;
            }
            return null;
        }



        private List<Vector3> GetCarPath(Vector3 startPosition, Vector3 endPosition, PathChosen pathChose)
        {
            if(!carGraph.isEmpty())
            {
                if(pathChose==PathChosen.Best)
                {
                    return AdjacencyGraph.FindKthShortestPath(carGraph, startPosition, endPosition, 1);
                }
                else
                {
                    return AdjacencyGraph.FindKthShortestPath(carGraph, startPosition, endPosition, 2);
                }
                
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
                visited.Add(currentRoad);
                roadNeighbor = placementManager.GetRoadNeighbours(currentRoad);

                //remove everyneihbour that was already visited
                visited.ForEach(c => roadNeighbor.Remove(c));



                //add remaining neighbours to the tovisit queue
                //Debug.Log("ENQUEUE ALL "+ roadNeighbor.Count+ " NEIHBOURS");
                roadNeighbor.ForEach(x => roadToVisit.Enqueue(x));



                //create a vertex foreach marker and connects them with other roads
                foreach (Marker marker in currentRoad.GetCarMarkers())
                {
                    //Debug.Log("ADD VERTEX at " + marker.Position + " (" + marker.name + ")");


                    //the marker was never visited before (so we have to check its connection to the exterior)
                    if (carGraph.AddVertex(marker.Position)!=null)
                    {
                        //if it has not visited neighbours
                        if (roadNeighbor.Count != 0)
                        {
                            //if incoming another tile may be connected to it
                            if (currentRoad.GetIncomingMarkers().Contains(marker))
                            {
                                
                                Marker closest = GetClosestOutgoingMarkerOfAllRoads(roadNeighbor, marker);
                                if (closest != null) //otherwise risk of linking to marker of another tile if road next to it is closed
                                {
                                    //Debug.Log("create vertex to " + marker.Position + " from other road " + closest.Position + " (" + closest.name + ")");
                                    carGraph.AddEdge(closest.Position, marker.Position);
                                }
                                else
                                {
                                    //Debug.Log("Something weird happened, a marker was reeeeeally far");
                                }
                            }
                            //if outgoing it might be connected to another tile
                            else if (currentRoad.GetOutgoingMarkers().Contains(marker))
                            {
                                Marker closest = GetClosestIncomingMarkerOfAllRoads(roadNeighbor, marker);
                                
                                if (closest!=null)//otherwise risk of linking to marker of another tile if road next to it is closed
                                {
                                    //Debug.Log("create edge from " + marker.Position + " to other road : " + closest.Position + " ("+closest.name+")");

                                    carGraph.AddEdge(marker.Position, closest.Position);
                                }
                                else
                                {
                                   // Debug.Log("Something weird happened, a marker was reeeeeally far");
                                }
                            }
                        }
                    }
                }

                //interconnets the markers of the same road
                foreach(Marker marker in currentRoad.GetCarMarkers())
                {
                    //Add neighbors from same road
                    foreach (Marker neighbor in marker.adjacentMarkers)
                    {
                        //Debug.Log("create edge from " + marker.Position + " to (same road) " + neighbor.Position);
                        //Debug.Log("create edge from " + marker.gameObject.name + " to (same road) " + neighbor.gameObject.name);
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
            if (distance < 0.6)//otherwise risk of linking to marker of another tile if road next to it is closed
            {
                return closestMarker;
            }
            return null;
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
            if(distance<0.6) //otherwise risk of linking to marker of another tile if road next to it is closed
            {
                return closestMarker;
            }
            return null;
        }

        //useless function replaced by cargraph
        /*private void CreatACarGraph(List<Vector3Int> path)
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
        }*/
    }
}

