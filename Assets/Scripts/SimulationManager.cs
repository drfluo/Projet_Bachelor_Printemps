using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using SimpleCity.AI;

public class SimulationManager : MonoBehaviour
{
    public AiDirector aiDirector;
    CarAI[] allcars;


    public InputField durationInput;
    public Dropdown mapChoosing;
    public InputField simulationNameInput;
    public InputField carLoadInput;
    public Toggle constantSpeedInput;
    public InputField stopRespectInput;
    public InputField secondBestPathInput;
    public InputField thirdBestPathInput;
    public InputField IntersectionBlockingInput;

    float fTime=300f;//if no time indicated, runs for 5min
    string mapName = "";
    string strSimulationName="NoNameGiven";
    int carLoad = 2;
    string speedChosen = "constant";
    int stopRespect = 0;
    int secondBestPath = 0;
    int thirdBestPath = 0;
    int intersectionBlocking = 0;

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
        if (!constantSpeedInput.isOn)
        {
            speedChosen = "gaussian";
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
        //intersection blocking
        if (IntersectionBlockingInput.text != "")
        {
            intersectionBlocking = int.Parse(IntersectionBlockingInput.text);
        }

        allcars = new CarAI[(int)(carLoad * fTime / 60)];
        aiDirector.placementManager.LoadMap(mapName);
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
        for(int i=1;i<carLoad* fTime/60; i++) //for each car
        {
            fSecondToWait = Random.Range(1, fTime); //choose a random nb of seconds after which it spawns
            StartCoroutine(SpawnAfter(fSecondToWait,i)); //make it spawn after waiting x seconds
            Debug.Log("A car will spawn after " + fSecondToWait + " seconds");

        }
    }

    IEnumerator SpawnAfter(float fSeconds, int iIdCar)
    {
        yield return new WaitForSeconds(fSeconds);

        allcars[iIdCar]=aiDirector.SpawnACar();
    }


    IEnumerator StopEverything()
    {
        yield return new WaitForSeconds(fTime);

        StreamWriter sw = new StreamWriter(Application.dataPath + "/Result.txt",true);


        sw.WriteLine("\n ---------------" + strSimulationName +  "--------------- \n");

        //write specifications to file
        sw.WriteLine("Specifications : ");
        sw.WriteLine("\t  map used : "+mapName);
        sw.WriteLine("\t  time: " + (fTime/60).ToString() + "min");
        sw.WriteLine("\t  car load: " + carLoad.ToString() + "cars/min");
        sw.WriteLine("\t  speed type: " + speedChosen);
        sw.WriteLine("\t  cars that don't respect stops: " + stopRespect + "%");
        sw.WriteLine("\t  cars that take:");
        sw.WriteLine("\t \t 2nd best path: " + secondBestPath + "% ");
        sw.WriteLine("\t \t 3rd best path: " + thirdBestPath + "%");
        sw.WriteLine("\t  cars that block intersections: " + intersectionBlocking + "%");

        //write results to file
        sw.WriteLine("Results : ");
        sw.WriteLine("\t  # of cars that were created : " + CarAI.numberStarted);
        sw.WriteLine("\t  # of cars that arrived to their destination before the end of the timer: " + CarAI.numberArrived);
        sw.WriteLine("\t  Total time traveled by cars which reached their destination: " + CarAI.totalTimeTaken+"s");
        sw.WriteLine("\t  Average time traveled: " + CarAI.totalTimeTaken/ CarAI.numberArrived+"s");

        sw.Close();


        Debug.Log("SIMULATION FINISHED AT " + System.DateTime.UtcNow.ToString("HH:mm"));

    }

}
