using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class CarAI : MonoBehaviour
{
    public static int numberArrived=0;
    public static int numberStarted=0;
    public static double totalTimeTaken=0;



    /*previously in CarController*/
    Rigidbody rb;
    private double timeTaken;
    Stopwatch stopWatch = new Stopwatch();

    private float power = 10;
    private float torque = 0.02f;
    private float maxSpeed = 0.4f;

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
    public float stopTime = 1f;



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
        set { stop = value; }
    }

    public CarAI()
    {
        numberStarted++;
        stopWatch.Start();
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

        if(Physics.Raycast(raycastStartingPoint.transform.position, transform.forward,out hit,raycastObstacleAhead, 1 << gameObject.layer))
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
            
        }
        else
        {
            distanceObstacleAhead=-10f;
        }
        
        if(rb.velocity.magnitude < maxSpeed)
        {
            rb.AddForce(movementVector.y * transform.forward * power);
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
                maxSpeed =0.5f;
                stopTime = 0.7f;

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
        }
        else
        {
            collisionStop = false;
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
            Stop = true;
            numberArrived++;
            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            timeTaken = stopWatch.Elapsed.TotalSeconds;
            totalTimeTaken += timeTaken;
            Destroy(gameObject);
        }
        else
        {
            currentTargetPosition = path[index];
        }
    }
}
