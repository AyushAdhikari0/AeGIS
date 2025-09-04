using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderValueButtonSetter : MonoBehaviour
{
    // Start is called before the first frame update

    public Slider mySlider;

    public float storedValue;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void setValueOnSlider()
    {
        mySlider.value = storedValue;
    }
    
    public void updateStoredValue(string stringInput)
    {
        // validate string as a float
        if (float.TryParse(stringInput, out float value)) {
            storedValue = value;
        }
    }
}
