using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoStandradDev : MonoBehaviour
{

    Text showingText;
    public Slider speed;
    public Slider sigma;


    // Start is called before the first frame update
    void Start()
    {
        showingText = GetComponent<Text>();
    }


    public void ShowInfoStandardDev(float uselessFloat)
    {

        showingText.text = "95% of cars will have a speed between " + (speed.value-2*sigma.value).ToString("0.00") + " and " + (speed.value + 2 * sigma.value).ToString("0.00");
    }
}
