using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SimpleCity.AI
{
    public class Marker : MonoBehaviour
    {
        public Vector3 Position { get => transform.position;}

        public List<Marker> adjacentMarkers;

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
    
    
        private void OnTriggerEnter(Collider other)
        {
            
            if (other.CompareTag("Car"))
            {
                IsOccupied=true;
                Debug.Log("COLLIDED BOUMSKI");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Car"))
            {
                IsOccupied=false;
                Debug.Log("COLLIDED DEBOUMSKITATION");
            }
        }
    
    }

}
