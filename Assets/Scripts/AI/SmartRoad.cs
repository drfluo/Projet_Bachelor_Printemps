using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SmartRoad : MonoBehaviour
{
    Queue<CarAI> trafficQueue = new Queue<CarAI>();
    public CarAI currentCar;
    


    //IDEA
    /*
    Create variable isACarBlocking qui dit si une voiture en empeche une autr de passer, si non alors on met pas car.Stop=true 
    (typiquement dans un croisement trois voies, tant qu'aucune voiture ne tourne c'est comme siu c'était une seule voie)


    Aussi, pour la priorité de droite:
        Liste de pair de waypoint ? Genre si ton prochain c'est celui là check que personne est là ?
        Une liste de qui domine qui ? genre un nombre associé au waypoint avec un modulo style 1>2>3>4>1... ?
        
    */

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            var car = other.GetComponent<CarAI>();
            if (car != null && car != currentCar && car.IsThisLastPathIndex() == false)
            {
                trafficQueue.Enqueue(car);
                car.Stop = true;
            }
        }
    }

    private void Update()
    {
        if (currentCar == null)
        {
            if (trafficQueue.Count > 0)
            {
                currentCar = trafficQueue.Dequeue();
                currentCar.Stop = false;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            var car = other.GetComponent<CarAI>();
            if (car != null)
            {
                RemoveCar(car);
            }
        }
    }

    private void RemoveCar(CarAI car)
    {
        if (car == currentCar)
        {
            currentCar = null;
        }
    }
}
    