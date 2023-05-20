using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleCity.AI
{
    public class Light_Management : MonoBehaviour
    {
        public bool isRedMarker;
        private bool isOrange=false;
        public Marker marker;
        public GameObject road;
        public GameObject greenRoad;
        public GameObject orangeRoad;
        public int greenTime, orangeTime;
        public bool is4Way;

        public GameObject nextLight;

        //public GameObject[] roads3WayLight;
        //private currentRoad;

        private void Start()
        {
            is4Way = road.name.Contains("4");

            if (!isRedMarker)
            {
                StartCoroutine(change());
            }

        }



        // Update is called once per frame
        void Update()
        {
            if (!isRedMarker && marker.currentCar != null)
            {
                if (isOrange == true)
                {
                    marker.currentCar.Stop = true;
                }
                else
                {
                    marker.currentCar.Stop = false;
                }
            }

        }


        private void OnTriggerEnter(Collider other)
        {
            if (isRedMarker && marker.currentCar!=null)
            {
                marker.currentCar.Stop = true;
            }

        }


        IEnumerator change()
        {
            while(true)
            {
                
               
                //ModifyStructureModel(positionTest, self, Quaternion.Euler(0, (rot+90)%360, 0));

                yield return new WaitForSeconds(greenTime);
                isOrange = true;
                orangeRoad.SetActive(true);
                greenRoad.SetActive(false);

                yield return new WaitForSeconds(orangeTime);
                isOrange = false;
                orangeRoad.SetActive(false);
                greenRoad.SetActive(true);

                if (is4Way)
                {
                    road.transform.Rotate(new Vector3(0, -90, 0));
                }
                else
                {
                    transform.parent.transform.parent.transform.parent.transform.parent.GetComponent<PlacementManager>().ModifyStructureModel(Vector3Int.FloorToInt(road.transform.position), nextLight, road.transform.rotation);
                }
            }
        }
    }
}