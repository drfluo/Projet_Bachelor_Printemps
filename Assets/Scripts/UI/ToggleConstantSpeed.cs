//Attach this script to a Toggle GameObject. To do this, go to Create>UI>Toggle.
//Set your own Text in the Inspector window

using UnityEngine;
using UnityEngine.UI;

public class ToggleConstantSpeed : MonoBehaviour
{
    private Toggle m_Toggle;
    public Toggle ToggletoDisable;
    public Slider[] m_Sliders;
    public Slider[] slidersToDisable;

    void Start()
    {
        //Fetch the Toggle GameObject
        m_Toggle = GetComponent<Toggle>();
        //Add listener for when the state of the Toggle changes, to take action
        m_Toggle.onValueChanged.AddListener(delegate {
            ToggleValueChanged(ToggletoDisable,m_Sliders,slidersToDisable);
        });
    }

    //Output the new state of the Toggle into Text
    void ToggleValueChanged(Toggle OtherToggle, Slider[] ourSlider, Slider[] otherSlider)
    {
        foreach(Slider slider in ourSlider)
        {
            slider.interactable = m_Toggle.isOn;
        }
        foreach(Slider slider in otherSlider)
        {
            slider.interactable = !m_Toggle.isOn;
        }
        OtherToggle.isOn = !m_Toggle.isOn;
       
    }
}