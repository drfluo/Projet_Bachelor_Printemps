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
        private GameObject road;
        public int greenTime, orangeTime;


        private void Start()
        {
            Debug.Log(transform.parent.transform.parent.gameObject);
            road = transform.parent.transform.parent.gameObject;

            if(!isRedMarker)
            {
                StartCoroutine(rotate());
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


        IEnumerator rotate()
        {
            while(true)
            {
                road.transform.Rotate(new Vector3(0, -90, 0));
                //ModifyStructureModel(positionTest, self, Quaternion.Euler(0, (rot+90)%360, 0));

                yield return new WaitForSeconds(greenTime);
                isOrange = true;

                yield return new WaitForSeconds(orangeTime);
                isOrange = false;
            }
            
        }
    }
}