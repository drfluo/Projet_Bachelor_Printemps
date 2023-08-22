using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class errorPercentagesUI : MonoBehaviour
{

    Button launch;



    public Text TextPourcentage;
    public Text TextMissingValue;
    public InputField SimulationName;
    public InputField Duration;
    public InputField CarLoad;

    public GameObject specsChoosing;
    public InputField percentageStop;
    public InputField percentage2ndBest;
    public InputField percentage3rdBest;
    public InputField percentageReactionTime;
    public InputField percentageBlock;


    void Start()
    {
        //Fetch the Dropdown GameObject the script is attached to
        launch = GetComponent<Button>();
    }


    private void Update()
    {
        //if we are in the choosing specs panel
        if(specsChoosing.activeSelf)
        {
            if(SimulationName.text == "" || Duration.text == "" || CarLoad.text == "")
            {
                TextMissingValue.gameObject.SetActive(true);
                launch.interactable = false;
                return;
            }
            if (percentageStop.text != "")
            {
                if (!Valid(int.Parse(percentageStop.text)))
                {
                    TextPourcentage.gameObject.SetActive(true);
                    launch.interactable = false;
                    return;
                }
            }
            if (percentageBlock.text != "")
            {
                 if (!Valid(int.Parse(percentageBlock.text)))
                 {
                    TextPourcentage.gameObject.SetActive(true);
                    launch.interactable = false;
                    return;
                 }
            }
            if(percentageReactionTime.text!="")
            {
                if (!Valid(int.Parse(percentageReactionTime.text)))
                {
                    TextPourcentage.gameObject.SetActive(true);
                    launch.interactable = false;
                    return;
                }
            }
            if(percentage3rdBest.text != "")
            {
                if (!Valid(int.Parse(percentage3rdBest.text)))
                {
                    TextPourcentage.gameObject.SetActive(true);
                    launch.interactable = false;
                    return;
                }
            }
            if(percentage2ndBest.text!="")
            {
                if (!Valid(int.Parse(percentage2ndBest.text)))
                {
                    TextPourcentage.gameObject.SetActive(true);
                    launch.interactable = false;
                    return;
                }
            }
            if(percentage3rdBest.text != "" && percentage2ndBest.text != "")
            {
                if (!Valid(int.Parse(percentage2ndBest.text)+ int.Parse(percentage3rdBest.text)))
                {
                    TextPourcentage.gameObject.SetActive(true);
                    launch.interactable = false;
                    return;
                }
            }

            TextPourcentage.gameObject.SetActive(false);
            TextMissingValue.gameObject.SetActive(false);
            launch.interactable = true;
        }
    }


    private bool Valid(int percent)
    {
        return (percent>=0 && percent<=100);
    }
}
