﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace SimpleCity.AI
{

    [Serializable]
    public struct Dependency
    {
        public Marker destination;
        public List<Marker> toCheck;
        public StopLine stopLine;
    }

    public class Marker : MonoBehaviour
    {
        public Vector3 Position { get => transform.position; }

        public List<Marker> adjacentMarkers;

        public CarAI currentCar;

        public bool canCommandCar;

        //line where the car stops, need it to be able to let one car go if a stuck situation occurs
        public StopLine currentStopLine;

        public List<Dependency> dependencyList;

        [SerializeField]
        public int IsOccupied=0;

        [SerializeField]
        private bool openForConnections;

        public bool OpenForconnections
        {
            get { return openForConnections; }
        }

        public List<Vector3> GetAdjacentPositions()
        {
            return new List<Vector3>(adjacentMarkers.Select(x => x.Position).ToList());
        }

        public bool Equals(Marker p)
        {
            return p.Position == Position;
        }


        //private bool hold = false;
        private int count = 0;

        private IEnumerator ResetStopVariable(CarAI currentCar)
        {
            yield return new WaitForSeconds(currentCar.stopTime); // Wait for 2 seconds
            //hold = false;
            count += 1;
        }

        private IEnumerator ResetIsOccupiedVariable()
        {
            yield return new WaitForSeconds(1f); // Wait for 2 seconds
            IsOccupied--;
        }


        private void OnTriggerEnter(Collider other)
        {
            bool isStop = false;
            
            if (other.CompareTag("Car"))
            {
                //Debug.Log("Collider " + GetComponent<Collider>().name);



                var car = other.GetComponent<CarAI>(); //if collision with car we get its script
                currentCar = car;

                if (GetComponent<Collider>().name.Contains("STOP") && count==0)
                {
                    isStop = true;
                    //hold = true;

                    StartCoroutine(ResetStopVariable(currentCar));
                }

                //Debug.Log("Collider "+GetComponent<Collider>().name+" has hit object "+other.name);
                //Debug.Log("Collider's parent "+GetComponent<Collider>().transform.parent.name+" has hit object "+other.name);
                //Debug.Log("Collider's parent's parent "+GetComponent<Collider>().transform.parent.transform.parent.name+" has hit object "+other.name);
                //Debug.Log("IS SET TO TRUE");
                IsOccupied +=1;

                if (canCommandCar)
                {
                    int increment = 0;
                    if (GetComponent<Collider>().transform.parent.transform.parent.name.Contains("3Way"))
                    {
                        increment = 2;
                    }
                    if (GetComponent<Collider>().transform.parent.transform.parent.name.Contains("roundabout"))
                    {
                        increment = 2; 
                    }
                    else if (GetComponent<Collider>().transform.parent.transform.parent.name.Contains("4Way"))
                    {
                        increment = 3; 
                    }

                   // Debug.Log("Position checked marker : " + car.path[car.index + increment]);

                    foreach (Dependency dependency in dependencyList)
                    {
                       // Debug.Log("check" + dependency.destination.name + " at " + dependency.destination.Position);
                        if (car.index + increment<car.path.Count && dependency.destination.Position == car.path[car.index + increment])
                        {
                            //Debug.Log("Found destination");
                            dependency.stopLine.toCheck = dependency.toCheck;
                            currentStopLine = dependency.stopLine;
                            if (isStop)
                            {
                                dependency.stopLine.isStop = true;
                            }

                            return;
                        }
                    }
                }

            }
        }

      /*  private void Update()
        {
            if (canCommandCar && currentCar != null)
            {
                if (!hold)
                {
                    if (CheckDependency(currentDependence.toCheck))
                    {
                        if(currentDependence.stopLine)
                        {
                            currentDependence.stopLine.nextCarCanGo = true;
                        }
                        
                    }
                    else
                    {
                        if (currentDependence.stopLine)
                        {
                            currentDependence.stopLine.nextCarCanGo = false;
                        }
                    }
                }
            }
        }*/

        //return true if can go otherwise false
        private bool CheckDependency(List<Marker> toCheck)
        {
            if(toCheck==null)
            {
                return true;
            }
            foreach(Marker marker in toCheck)
            {
                if(marker.IsOccupied!=0)
                {
                    return false;
                }
            }
            return true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Car"))
            {
               if(IsOccupied==1)
                {
                    StartCoroutine(ResetIsOccupiedVariable());
                }
                else
                {
                    IsOccupied -= 1;
                }
                
                currentCar = null;
                /*if(IsOccupied==0)
                 {
                     currentDependence = null;
                 }*/
                count = 0;
            }

           
        }

        /*public List<Marker> ToCheck(Marker carDestination)
        {
            foreach(Dependency dependency in dependencyList)
            {
                if(dependency.destination.Equals(carDestination))
                {
                    return dependency.toCheck;
                }
            }
            return null;
        }*/
    
    }

}
