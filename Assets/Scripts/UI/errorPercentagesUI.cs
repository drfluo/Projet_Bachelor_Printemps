using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class errorPercentagesUI : MonoBehaviour
{

    Button launch;



    public Text thisText;
    public GameObject specsChoosing;
    public InputField percentageStop;
    public InputField percentage2ndBest;
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
            if (percentageStop.text != "")
            {
                if (!valid(int.Parse(percentageStop.text)))
                {
                    thisText.gameObject.SetActive(true);
                    launch.interactable = false;
                    return;
                }
            }
            if (percentageBlock.text != "")
            {
                 if (!valid(int.Parse(percentageBlock.text)))
                 {
                    thisText.gameObject.SetActive(true);
                    launch.interactable = false;
                    return;
                 }
            }
            if(percentageReactionTime.text!="")
            {
                if (!valid(int.Parse(percentageReactionTime.text)))
                {
                    thisText.gameObject.SetActive(true);
                    launch.interactable = false;
                    return;
                }
            }
            if(percentage2ndBest.text!="")
            {
                if (!valid(int.Parse(percentage2ndBest.text)))
                {
                    thisText.gameObject.SetActive(true);
                    launch.interactable = false;
                    return;
                }
            }

            thisText.gameObject.SetActive(false);
            launch.interactable = true;
        }
    }


    private bool valid(int percent)
    {
        return (percent>=0 && percent<=100);
    }
}
