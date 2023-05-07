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
        public bool IsOccupied=false;

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

                Debug.Log("IS SET TO TRUE");
                IsOccupied =true;

                Debug.Log("car path" + car.path[car.index+2].ToString("F3"));

                foreach(Dependency dependency in dependencyList)
                {
                    Debug.Log("check"+dependency.destination.Position.ToString("F3"));
                    if (dependency.destination.Position==car.path[car.index+2])
                    {
                        Debug.Log("YAY");
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
            if (currentCar != null)
            {
                if(CheckDependency(currentDependence))
                {
                    currentCar.Stop = false;
                }
            }
        }

        //return true if can go otherwise false
        private bool CheckDependency(List<Marker> toCheck)
        {
            foreach(Marker marker in toCheck)
            {
                if(marker.IsOccupied)
                {
                    Debug.Log("WE ARE OCUPIED");
                    return false;
                }
            }
            return true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Car"))
            {
               IsOccupied=false;
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
