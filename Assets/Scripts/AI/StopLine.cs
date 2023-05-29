using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopLine : MonoBehaviour
{
    public bool nextCarHasToStop = false;
    public bool isStop;
    private CarAI currentCar;
    

    //When a car enters we get its script
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Car"))
        {
            var car = other.GetComponent<CarAI>(); //if collision with car we get its script
            currentCar = car;
            if (isStop)
            {
                //TODO : Stop the car for 1 second
            }
        }
       
    }

   /* private IEnumerator ResetStopVariable(CarAI currentCar)
    {
        yield return new WaitForSeconds(1f); // Wait for 2 seconds
        currentCar.Stop = false; // Set the Stop variable to false after 2 seconds
    }*/


    private void Update()
    {
        if(currentCar!=null) //if we have a car
        {
            if(nextCarHasToStop) //it has to stop
            {
                currentCar.Stop = true;
            }
            else 
            {
                currentCar.Stop = false;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        currentCar = null;
        nextCarHasToStop = false;
    }
}
