using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleCity.AI
{
    public class RoadHelper : MonoBehaviour
    {

        public List<Marker> carMarkers;
        [SerializeField]
        protected bool isCorner;
        [SerializeField]
        protected bool canGetStuck;
        [SerializeField]
        protected bool is3Way;

        float approximateThresholdCorner = 0.3f;

        [SerializeField]
        private List<Marker> incommingMarkers, outgoingMarkers;

        private CarAI[] waitingCars;
        


        //Need position because multiple car markers inherits from this
        public Marker GetPositioForCarToSpawn(Vector3 nextPathPosition)
        {
            return GetClosestMarkeTo(nextPathPosition, outgoingMarkers);
        }


        public Marker GetPositioForCarToEnd(Vector3 previousPathPosition)
        {
            return GetClosestMarkeTo(previousPathPosition, incommingMarkers);
        }

        //returns all incoming markers (overriden in multiplecarMarkers)
        public virtual List<Marker> GetAllIncomingMarkers()
        {
            return incommingMarkers;
        }

        //returns all outgoing markers (overriden in multiplecarMarkers)
        public virtual List<Marker> GetAllOutgoingMarkers()
        {
            return outgoingMarkers;
        }

        protected Marker GetClosestMarkeTo(Vector3 structurePosition, List<Marker> markers, bool isCorner = false)
        {
            if (isCorner)
            {
                foreach (var marker in markers)
                {
                    var direction = marker.Position - structurePosition;
                    direction.Normalize();
                    if(Mathf.Abs(direction.x) < approximateThresholdCorner || Mathf.Abs(direction.z) < approximateThresholdCorner)
                    {
                        return marker;
                    }
                }
                return null;
            }
            else
            {
                Marker closestMarker = null;
                float distance = float.MaxValue;
                foreach (var marker in markers)
                {
                    var markerDistance = Vector3.Distance(structurePosition, marker.Position);
                    if(distance > markerDistance)
                    {
                        distance = markerDistance;
                        closestMarker = marker;
                    }
                }
                return closestMarker;
            }
        }



        public Vector3 GetClosestCarMarkerPosition(Vector3 currentPosition)
        {
            return GetClosestMarkeTo(currentPosition, carMarkers, false).Position;
        }


        public List<Marker> GetAllCarMarkers()
        {
            return carMarkers;
        }


        void Start()
        {
            waitingCars = new CarAI[incommingMarkers.Count];
            if (canGetStuck )
            {
                if(is3Way)
                {
                    InvokeRepeating("CheckStuck", 3.0f, 3f);
                }
                else if(transform.name.Contains("round"))
                {
                    Debug.Log("ROUNDABOUT");
                    InvokeRepeating("CheckStuck", 3.0f, 3f);
                }
                else
                {
                    InvokeRepeating("CheckStuck", 3.0f, 2f);
                }
                
            }
        }

        void CheckStuck()
        {
            bool allEmpty = true;
            int i = 0;
            foreach(Marker marker in incommingMarkers)
            {
                if(allEmpty && marker.currentCar!=null)
                {
                    allEmpty = false;
                }

                if(marker.currentCar!=waitingCars[i])
                {
                    UpdateListWaitingCar();
                    return;
                }
                i++;
            }


            if(!allEmpty)
            {
                //STUCK situation, now need to choose a random car and let it go
                Debug.Log("STUCK STUATION");

                List<int> indexes = new List<int>();
                for (int j = 0; j < waitingCars.Length; j++)
                {
                    if (waitingCars[j] != null && !(incommingMarkers[j].transform.name.Contains("GW") || incommingMarkers[j].transform.name.Contains("STOP")))
                        indexes.Add(j);
                }

                if(indexes.Count>1)
                {
                    int index = indexes[UnityEngine.Random.Range(0, indexes.Count - 1)];

                    //need to get the stop line and tell it to stop stopping
                    incommingMarkers[index].currentStopLine.toCheck = new List<Marker>();
                    waitingCars[index].Stop = false;
                }
                
            }
            UpdateListWaitingCar();
        }

        void UpdateListWaitingCar()
        {
            int i = 0;
            foreach(Marker marker in incommingMarkers)
            {
                waitingCars[i] = marker.currentCar;
                i++;
            }
        }
    }
}

