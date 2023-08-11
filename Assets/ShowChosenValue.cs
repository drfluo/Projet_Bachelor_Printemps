using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowChosenValue : MonoBehaviour
{
    Text showingChosenValue;


    // Start is called before the first frame update
    void Start()
    {
        showingChosenValue = GetComponent<Text>();
    }

    public void OnSliderValueChanged(float value)
    {
        showingChosenValue.text = value.ToString("0.00");
    }

}
