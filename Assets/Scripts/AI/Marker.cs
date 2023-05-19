using System;
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
    }

    public class Marker : MonoBehaviour
    {
        public Vector3 Position { get => transform.position;}

        public List<Marker> adjacentMarkers;

        public CarAI currentCar;
        public List<Marker> currentDependence;




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

        private void OnTriggerEnter(Collider other)
        {
            
            if (other.CompareTag("Car"))
            {
                var car = other.GetComponent<CarAI>(); //if collision with car we get its script
                currentCar = car;
                //Debug.Log("Collider "+GetComponent<Collider>().name+" has hit object "+other.name);
                //Debug.Log("Collider's parent "+GetComponent<Collider>().transform.parent.name+" has hit object "+other.name);
                //Debug.Log("Collider's parent's parent "+GetComponent<Collider>().transform.parent.transform.parent.name+" has hit object "+other.name);
                //Debug.Log("IS SET TO TRUE");
                IsOccupied +=1;
                int increment = 0;
                Debug.Log(GetComponent<Collider>().transform.parent.transform.parent.name);
                if (GetComponent<Collider>().transform.parent.transform.parent.name.Contains("3Way"))
                {
                    Debug.Log("A 3Way ahead");
                    increment=2;
                }
                if (GetComponent<Collider>().transform.parent.transform.parent.name.Contains("roundabout"))
                {
                    Debug.Log("A roundabout ahead");
                    increment = 2; //test with 3 instead of 4
                }
                else if (GetComponent<Collider>().transform.parent.transform.parent.name.Contains("4Way"))
                {
                    Debug.Log("A 4Way ahead");
                    increment=3; //test with 3 instead of 4
                }
                

                //Debug.Log("car path : " + car.path[car.index].ToString("F3") + car.path[car.index+1].ToString("F3") + car.path[car.index+2].ToString("F3") + car.path[car.index+3].ToString("F3") + car.path[car.index+4].ToString("F3"));

                foreach(Dependency dependency in dependencyList)
                {
                    Debug.Log("check"+dependency.destination.name);
                    if (dependency.destination.Position==car.path[car.index+increment])
                    {
                        Debug.Log("Found destination");
                        currentDependence = dependency.toCheck;
                        if (!CheckDependency(dependency.toCheck))
                        {
                            car.Stop = true;
                            return;
                        }
                    }
                }

            }
        }

        private void Update()
        {
            //Debug.Log("HEY");
            if (currentCar != null)
            {
                if(CheckDependency(currentDependence))
                {
                    currentCar.Stop = false;
                }
                if(!CheckDependency(currentDependence))
                {
                    currentCar.Stop = true;
                }
            }
        }

        //return true if can go otherwise false
        private bool CheckDependency(List<Marker> toCheck)
        {
            foreach(Marker marker in toCheck)
            {
                if(marker.IsOccupied!=0)
                {
                    //Debug.Log("WE ARE OCUPIED");
                    return false;
                }
            }
            return true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Car"))
            {
               IsOccupied-=1;
               currentCar = null;
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
