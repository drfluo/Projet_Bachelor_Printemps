using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SimpleCity.AI;

public class SmartRoad : MonoBehaviour
{
    Queue<CarAI> trafficQueue = new Queue<CarAI>();
    public CarAI currentCar;

    public RoadHelper model = null;
    public List<Marker> markers = null;
    
    public List<Marker> toStopCar=null;

    //IDEA
    /*
    Create variable isACarBlocking qui dit si une voiture en empeche une autr de passer, si non alors on met pas car.Stop=true 
    (typiquement dans un croisement trois voies, tant qu'aucune voiture ne tourne c'est comme siu c'était une seule voie)


    Aussi, pour la priorité de droite:
        Liste de pair de waypoint ? Genre si ton prochain c'est celui là check que personne est là ?
        Une liste de qui domine qui ? genre un nombre associé au waypoint avec un modulo style 1>2>3>4>1... ?
        
    */

    private void Start() 
    {
        model= GetComponent<RoadHelper>();
        markers=model.carMarkers;

        Debug.Log(markers[0].Position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            var car = other.GetComponent<CarAI>();
            bool occupied=false;

            foreach(Marker marker in toStopCar)
            {
                Debug.Log("TO STOP :"+marker.Position+"TRUE"+car.path[car.index+1]);
                if(marker.Position==car.path[car.index+1])
                {
                    //car.Stop = true;
                    return;
                }
                else if(marker.name.Contains("Entry_Bottom"))
                {
                    occupied=marker.IsOccupied;
                }
            }
            //check collider entry_bottom (if empty Go else wait)
            if(occupied)
            {
                trafficQueue.Enqueue(car);
                car.Stop = true;
            }



            
            /*if (car != null && car != currentCar && car.IsThisLastPathIndex() == false)
            {
                trafficQueue.Enqueue(car);
                car.Stop = true;
            }*/
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
    