//Attach this script to a Toggle GameObject. To do this, go to Create>UI>Toggle.
//Set your own Text in the Inspector window

using UnityEngine;
using UnityEngine.UI;

public class ToggleConstantSpeed : MonoBehaviour
{
    private Toggle m_Toggle;
    public Toggle ToggletoDisable;
    public Slider m_Slider;
    public Slider sliderToDisable;

    void Start()
    {
        //Fetch the Toggle GameObject
        m_Toggle = GetComponent<Toggle>();
        //Add listener for when the state of the Toggle changes, to take action
        m_Toggle.onValueChanged.AddListener(delegate {
            ToggleValueChanged(ToggletoDisable,m_Slider,sliderToDisable);
        });
    }

    //Output the new state of the Toggle into Text
    void ToggleValueChanged(Toggle OtherToggle, Slider ourSlider, Slider otherSlider)
    {
        OtherToggle.isOn = !m_Toggle.isOn;
        ourSlider.interactable= m_Toggle.isOn;
        otherSlider.interactable = !m_Toggle.isOn;
    }
}