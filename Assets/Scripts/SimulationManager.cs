using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using SimpleCity.AI;
using System;

public class SimulationManager : MonoBehaviour
{
    public AiDirector aiDirector;
    CarAI[] allcars;


    public InputField durationInput;
    public Dropdown mapChoosing;
    public InputField simulationNameInput;
    public InputField carLoadInput;
    public Toggle boolConstantSpeedInput;
    public Slider cstSpeedValue;
    public Slider gaussianSpeedValue;
    public Slider gaussianStandardDevi;
    public InputField stopRespectInput;
    public InputField secondBestPathInput;
    public InputField thirdBestPathInput;
    public InputField reactionTimeInput;
    public InputField IntersectionBlockingInput;

    public GameObject buttonsPanelToReactivateWhenFinished;

    float fTime=300f;//if no time indicated, runs for 5min
    string mapName = "";
    string strSimulationName="NoNameGiven";
    int carLoad = 2;
    string speedChosen = "constant";
    int stopRespect = 0;
    int secondBestPath = 0;
    int thirdBestPath = 0;
    int reactionTime = 0;
    int intersectionBlocking = 0;
    double meanSpeed = 0;
    double stdDeviationSpeed = 0;

    StreamWriter sw;
    System.Random random = new System.Random();
    bool simulationGoing = false;

    public void StartSimulation()
    {


        //get simulation specs 

        //duration
        if (durationInput.text != "")
        {
            fTime = 60*int.Parse(durationInput.text);
        }
        //map name
        mapName = mapChoosing.options[mapChoosing.value].text;

        //simulation name
        if (simulationNameInput.text!="")
        {
            strSimulationName = simulationNameInput.text;
        }
        //car load
        if (carLoadInput.text != "")
        {
            carLoad = int.Parse(carLoadInput.text);
        }
        //speed type
        if (!boolConstantSpeedInput.isOn) //gaussian speed
        {
            speedChosen = "gaussian";
            meanSpeed = gaussianSpeedValue.value;
            stdDeviationSpeed = gaussianStandardDevi.value;
        }
        else //constant speed for all cars
        {
            speedChosen = "constant";
            stdDeviationSpeed = 0;
            meanSpeed = cstSpeedValue.value;
        }
        //stop respected
        if (stopRespectInput.text != "")
        {
            stopRespect = int.Parse(stopRespectInput.text);
        }
        //secondBestPath
        if (secondBestPathInput.text != "")
        {
            secondBestPath = int.Parse(secondBestPathInput.text);
        }
        //thirdBestPath
        if (thirdBestPathInput.text != "")
        {
            thirdBestPath = int.Parse(thirdBestPathInput.text);
        }
        //secondBestPath
        if (reactionTimeInput.text != "")
        {
            reactionTime = int.Parse(reactionTimeInput.text);
        }
        //intersection blocking
        if (IntersectionBlockingInput.text != "")
        {
            intersectionBlocking = int.Parse(IntersectionBlockingInput.text);
        }

        allcars = new CarAI[(int)(carLoad * fTime / 60)];
        aiDirector.placementManager.LoadMap(mapName);


        sw = new StreamWriter(Application.dataPath + "/Result.txt", true);


        sw.WriteLine("\n ---------------" + strSimulationName + "--------------- \n");

        //write specifications to file
        sw.WriteLine("Specifications: ");
        sw.WriteLine("\t  map used: " + mapName);
        sw.WriteLine("\t  time: " + (fTime / 60).ToString() + "min");
        sw.WriteLine("\t  car load: " + carLoad.ToString() + "cars/min");
        sw.WriteLine("\t  speed type: " + speedChosen);
        if(speedChosen=="constant")
        {
            sw.WriteLine("\t  Max speed of all cars: " + meanSpeed.ToString("0.##"));
        }
        else
        {
            sw.WriteLine("\t  Mean max speed of all cars: " + meanSpeed.ToString("0.##"));
            sw.WriteLine("\t  Standard Deviation on speed: " + stdDeviationSpeed.ToString("0.##"));
        } 
        sw.WriteLine("\t  cars that don't respect stops: " + stopRespect + "%");
        sw.WriteLine("\t  cars that take 2nd best path: " + secondBestPath + "% ");
        sw.WriteLine("\t  cars that take 3rd best path: " + thirdBestPath + "% ");
        sw.WriteLine("\t  cars that have high reaction time: " + reactionTime + "% ");
        sw.WriteLine("\t  cars that block intersections: " + intersectionBlocking + "%");


        sw.WriteLine("Individual cars results:");

        simulationGoing = true;
        CarAI.numberStarted = 0;
        CarAI.numberArrived = 0;
        CarAI.totalNumberStops = 0;
        CarAI.totalTimeMaxSpeed = 0;
        CarAI.totalTimeStopped = 0;
        CarAI.totalTimeTaken = 0;
        StartCoroutine(waitMapFinished());

    }

    IEnumerator waitMapFinished()
    {
        yield return new WaitForSeconds(0.1f);
        aiDirector.GraphWholeMarkerMap(); //need to have the grapoh marker to spawn a car

        StartCoroutine(StopEverything());

        SpawnAllCars();
    }

    private void SpawnAllCars()
    {
        //we want to have an average car load of the value given so we calculate the # of cars that is and choose a random time of spawn for each of them
        float fSecondToWait = 0;
        for(int i=0;i<(int)(carLoad* fTime/60); i++) //for each car
        {
            fSecondToWait = UnityEngine.Random.Range(1, fTime); //choose a random nb of seconds after which it spawns
            StartCoroutine(SpawnAfter(fSecondToWait,i)); //make it spawn after waiting x seconds
            Debug.Log("A car will spawn after " + fSecondToWait + " seconds");

        }
    }

    IEnumerator SpawnAfter(float fSeconds, int iIdCar)
    {
        yield return new WaitForSeconds(fSeconds);

        PathChosen pathChose;

        //chooses which path the car will take
        double rand = random.NextDouble();
        if (rand * 100 < secondBestPath)
        {
            pathChose = PathChosen.Second;
        }
        else if (rand * 100 < secondBestPath+thirdBestPath)
        {
            pathChose = PathChosen.Third;
        }
        else
        {
            pathChose = PathChosen.Best;
        }

        allcars[iIdCar] = aiDirector.SpawnACarWithReturn(pathChose);
        if(allcars[iIdCar]!=null)
        {
            allcars[iIdCar].CarDestroyed += OnCarDestroyed;
            allcars[iIdCar].pathChosen = pathChose;

            //chooses if car respects stops
            if (random.NextDouble() * 100 < stopRespect)
            {
                allcars[iIdCar].respectStops = false;
            }
            //chooses if car has high reaction time
            if (random.NextDouble() * 100 < reactionTime)
            {
                allcars[iIdCar].onHisPhone = true;
            }

            allcars[iIdCar].maxSpeed = NormalDistributionGenerator(meanSpeed, stdDeviationSpeed);
            allcars[iIdCar].effectiveMaxSpeed = allcars[iIdCar].maxSpeed;

            Debug.Log(allcars[iIdCar].pathChosen);

        }

    }



    private void OnCarDestroyed(CarAI car)
    {
        if(simulationGoing)
        {



            string results = "\t" +mapName+"/"+carLoad+"/"+ new Vector2Int((int)car.path[0].x, (int)car.path[0].z) + "/" + new Vector2Int((int)car.path[car.path.Count - 1].x, (int)car.path[car.path.Count - 1].z) + "/" +
                car.timeTaken.ToString("0.##") + "/" + car.numberStop + "/" + car.timeStopped.ToString("0.##") + "/" + car.timeFullSpeed.ToString("0.##") + "/";
            results = results + car.respectStops;
            if(!car.respectStops)
            {
                results = results + "-" + car.numberStopDisrespected;
            }
            results = results + "/" + car.pathChosen;
            results = results + "/" + car.path.Count;
            results = results + "/" + car.onHisPhone;
            results = results + "/" + car.maxSpeed.ToString("0.##");



            sw.WriteLine(results);
        }
    }


    private double NormalDistributionGenerator(double mean, double stdDev) //using Box Muller transform
    {
       
        double u1 = 1.0 - random.NextDouble(); //uniform(0,1] random doubles
        double u2 = 1.0 - random.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
        return mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
    }





    IEnumerator StopEverything()
    {
        yield return new WaitForSeconds(fTime);


        //write results to file
        sw.WriteLine("Total Results: ");
        sw.WriteLine("\t  # of cars that were created: " + CarAI.numberStarted);
        sw.WriteLine("\t  # of cars that arrived to their destination before the end of the timer: " + CarAI.numberArrived);
        sw.WriteLine("\t  Total time traveled by cars which reached their destination: " + CarAI.totalTimeTaken.ToString("0.##") + "s");
        sw.WriteLine("\t  Average time traveled: " + (CarAI.totalTimeTaken/ CarAI.numberArrived).ToString("0.##") + "s");
        sw.WriteLine("\t  Total time stopped: " + CarAI.totalTimeStopped.ToString("0.##") + "s");
        sw.WriteLine("\t  Average time stopped: " + (CarAI.totalTimeStopped / CarAI.numberArrived).ToString("0.##") + "s");
        sw.WriteLine("\t  Total number of times stopped: " + CarAI.totalNumberStops);
        sw.WriteLine("\t  Average number of time stopped: " + (float)CarAI.totalNumberStops / CarAI.numberArrived);
        sw.WriteLine("\t  Total time at max speed: " + CarAI.totalTimeMaxSpeed.ToString("0.##"));
        sw.WriteLine("\t  Average time at max speed: " + (CarAI.totalTimeMaxSpeed / CarAI.numberArrived).ToString("0.##"));


        sw.Close();
        simulationGoing = false;

        Debug.Log("SIMULATION FINISHED AT " + System.DateTime.UtcNow.ToString("HH:mm"));
        buttonsPanelToReactivateWhenFinished.SetActive(true);

    }

}
