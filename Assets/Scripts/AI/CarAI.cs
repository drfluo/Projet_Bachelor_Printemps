using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;

[RequireComponent(typeof(Rigidbody))]
public class CarAI : MonoBehaviour
{

    //to pass data to the simulation manager
    public delegate void CarDestroyedEventHandler(CarAI car);
    public event CarDestroyedEventHandler CarDestroyed;



    public static int numberArrived=0;
    public static double totalTimeTaken=0;
    public static double totalTimeStopped=0;
    public static double totalTimeMaxSpeed=0;
    public static int totalNumberStops=0;



    /*previously in CarController*/
    Rigidbody rb;
    public double timeTaken=0;
    public double timeStopped=0;
    public double timeFullSpeed=0;
    public int numberStop=0;
    public int numberStopDisrespected = 0;

    Stopwatch stopWatchTotalTime = new Stopwatch();
    Stopwatch stopWatchMaxSpeedTime = new Stopwatch();
    Stopwatch stopWatchStoppedTime = new Stopwatch();

    private float power = 10;
    private float torque = 0.02f;
    public double maxSpeed = 0.4f;
    private double effectiveMaxSpeed = 0.4f; //if a car in front is slower then take it's speed as the new max to stop stopping too much
    public bool respectStops = true;

    [SerializeField]
    private Vector2 movementVector;
    /* END   */

    public List<Vector3> path = null;
    [SerializeField]
    private float arriveDistance = .3f, lastPointArriveDistance = .1f;
    [SerializeField]
    private float turningAngleOffset = 5;
  
    public Vector3 currentTargetPosition;

    [SerializeField]
    private GameObject raycastStartingPoint = null;

    private float raycastSafetyDistance=0.75f;
    private float raycastObstacleAhead=0.9f;

    private float distanceObstacleAhead=-10f;
    public float stopTime = 0.7f;



    internal bool IsThisLastPathIndex()
    {
        return index >= path.Count-1;
    }

    public int index = 0;

    private bool stop;
    private bool collisionStop = false;

    public bool Stop
    {
        get { return stop || collisionStop; }
        set {
            if(stop!=value) //because collisions checked cointinuously, need to check if value actually changedf not to count same stop twice
            {
                stop = value;

                if (value) //want to set stop to true so want to stop
                {
                    if (!stopWatchStoppedTime.IsRunning) //if timer is not already running
                    {
                        stopWatchStoppedTime.Start();
                        numberStop++;
                        totalNumberStops++;
                    }
                }
                else //want to set stop to false
                {
                    if (stopWatchStoppedTime.IsRunning)//if stopwatch runnning (honestly it should but just in case)
                    {
                        stopWatchStoppedTime.Stop();
                    }
                }

            }
        }
    }



    public CarAI()
    {
        stopWatchTotalTime.Start();
    }

    /*previously in CarController*/
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        InitializeCar();

        if(path == null || path.Count == 0)
        {
            Stop = true;
        }
        else
        {
            currentTargetPosition = path[index];
        }

    }

    private void Update()
    {
        CheckIfArrived();
        Drive();
        CheckForCollisions();
    }

    
    private RaycastHit hit;
    private void FixedUpdate()
    {
        // Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
        if (Physics.Raycast(raycastStartingPoint.transform.position, transform.forward,out hit,raycastObstacleAhead, 1 << gameObject.layer))
        {
            if(distanceObstacleAhead<0)
            {
                distanceObstacleAhead=hit.distance;
            }
            if(hit.distance< distanceObstacleAhead)
            {
                distanceObstacleAhead=hit.distance;
                if(movementVector.x == 0) //no turning
                {
                    rb.AddForce(-movementVector.y * transform.forward * 1 / hit.distance * power / 10);
                }
            }
            CarAI carHit = hit.transform.GetComponentInParent<CarAI>();
            if(carHit)
            {
                effectiveMaxSpeed = carHit.effectiveMaxSpeed;
            }
            else
            {
                Debug.Log("OHOH");
            }
            
        }
        else
        {
            distanceObstacleAhead=-10f;
            effectiveMaxSpeed = maxSpeed;
        }
        
        if(rb.velocity.magnitude < effectiveMaxSpeed)
        {
            if (stopWatchMaxSpeedTime.IsRunning)
            {
                stopWatchMaxSpeedTime.Stop();
                
            }
            rb.AddForce(movementVector.y * transform.forward * power);
        }
        else
        {
            if(!stopWatchMaxSpeedTime.IsRunning)
            {
                stopWatchMaxSpeedTime.Start();
            }
        }
        rb.AddTorque(movementVector.x * Vector3.up * torque * movementVector.y);
    }



    private void InitializeCar()
    {
        //if sports car then safetydistance shorter and maxspeed greater
        foreach (Transform child in transform)
        {
            if(child.name.Contains("Sports"))
            {
                raycastSafetyDistance = 0.55f;
                raycastObstacleAhead = 0.75f;
            }
        }
    }




    public void SetPath(List<Vector3> path)
    {
        if(path.Count == 0)
        {
            Destroy(gameObject);
            return;
        }
        this.path = path;
        index = 0;
        currentTargetPosition = this.path[index];

        Vector3 relativepoint = transform.InverseTransformPoint(this.path[index + 1]);

        float angle = Mathf.Atan2(relativepoint.x, relativepoint.z) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, angle, 0);
        Stop = false;
    }






    private void CheckForCollisions()
    {
        if(Physics.Raycast(raycastStartingPoint.transform.position, transform.forward,raycastSafetyDistance, 1 << gameObject.layer))
        {
            collisionStop = true;
            if(!stopWatchStoppedTime.IsRunning) //we need to stop because collision and we were not already stopped
            {
                stopWatchStoppedTime.Start();
                numberStop++;
                totalNumberStops++;
            }

        }
        else
        {
            collisionStop = false;
            if (!Stop)
            {
                if(stopWatchStoppedTime.IsRunning) //we were only stopped by the collision cause setting it to false stoped the lock
                {
                    stopWatchStoppedTime.Stop();
                }
            }
        }
    }





    private void Drive()
    {
        if (Stop)
        {
            this.movementVector = Vector2.zero;
        }
        else
        {
            Vector3 relativepoint = transform.InverseTransformPoint(currentTargetPosition);
            float angle = Mathf.Atan2(relativepoint.x, relativepoint.z) * Mathf.Rad2Deg;
            var rotateCar = 0;
            if(angle > turningAngleOffset)
            {
                rotateCar = 1;
            }else if(angle < -turningAngleOffset)
            {
                rotateCar = -1;
            }
            this.movementVector = new Vector2(rotateCar, 1);
        }
    }

    private void CheckIfArrived()
    {
        if(Stop == false)
        {
            var distanceToCheck = arriveDistance;
            if(index == path.Count - 1)
            {
                distanceToCheck = lastPointArriveDistance;
            }
            if(Vector3.Distance(currentTargetPosition,transform.position) < distanceToCheck)
            {
                SetNextTargetIndex();
            }
        }
    }

    private void SetNextTargetIndex()
    {
        index++;
        if(index >= path.Count)
        {
            numberArrived++;

            //stop all runing timers
            stopWatchTotalTime.Stop();
            stopWatchMaxSpeedTime.Stop();
            stopWatchStoppedTime.Stop();

            // journey time
            timeTaken = stopWatchTotalTime.Elapsed.TotalSeconds;
            totalTimeTaken += timeTaken;

            //time at max speed
            timeFullSpeed= stopWatchMaxSpeedTime.Elapsed.TotalSeconds;
            totalTimeMaxSpeed += timeFullSpeed;

            //time stopped
            timeStopped += stopWatchStoppedTime.Elapsed.TotalSeconds;
            totalTimeStopped += timeStopped;

            //tell the simulationManager to look at the cars data
            CarDestroyed?.Invoke(this);
            Destroy(gameObject);
        }
        else
        {
            currentTargetPosition = path[index];
        }
    }
}
