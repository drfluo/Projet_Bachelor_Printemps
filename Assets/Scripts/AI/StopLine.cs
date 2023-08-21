using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleCity.AI;

public class StopLine : MonoBehaviour
{
    public bool isStop = false;
    private CarAI currentCar;
    private bool currentCarHasStopped = false;

    public List<Marker> toCheck;



    //When a car enters we get its script
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Car"))
        {
            if(currentCar==null)
            {
                var car = other.GetComponent<CarAI>(); //if collision with car we get its script
                currentCar = car;
                if(isStop && currentCar.respectStops)
                {
                    currentCar.Stop = true;
                    currentCarHasStopped = true;
                    StartCoroutine(ResetStopVariable(currentCar));
                }
                else
                {

                    InvokeRepeating("CheckIfCanGo", 0f, 1f);
                }
            }
        }
    }

    private IEnumerator ResetStopVariable(CarAI currentCar)
    {
        yield return new WaitForSeconds(currentCar.stopTime); // Wait for 2 seconds
        InvokeRepeating("CheckIfCanGo", 0f, 1f);
    }


    private void OnTriggerExit(Collider other)
    {
        
        if(isStop && !currentCarHasStopped && currentCar)
        {
            currentCar.numberStopDisrespected++;
        }
        currentCar = null;
        currentCarHasStopped = false;
    }


    private void CheckIfCanGo()
    {
        
        foreach(Marker marker in toCheck)
        {
            if(marker.IsOccupied!=0)
            {
                currentCar.Stop = true;
                currentCarHasStopped = true;
                return;
            }
        }
        if(currentCar)
        {
            currentCar.Stop = false;
            CancelInvoke();
        }
        
        toCheck = new List<Marker>();

        return;

    }
}
