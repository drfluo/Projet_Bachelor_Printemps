﻿using System;
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

        float approximateThresholdCorner = 0.3f;

        [SerializeField]
        private List<Marker> incommingMarkers, outgoingMarkers;


        //Need position because multiple car markers inherits from this
        public Marker GetPositioForCarToSpawn(Vector3 nextPathPosition)
        {
            return GetClosestMarkeTo(nextPathPosition, outgoingMarkers);
        }

        public Marker GetPositioForCarToEnd(Vector3 previousPathPosition)
        {
            return GetClosestMarkeTo(previousPathPosition, incommingMarkers);
        }

        /*public virtual Marker GetPositioForCarToSpawn(Vector3 nextPathPosition)
        {
            return outgoing;
        }

        public virtual Marker GetPositioForCarToEnd(Vector3 previousPathPosition)
        {
            return incomming;
        }*/

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
    }
}

