using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.UI;

public class ThicknessSliderManager : MonoBehaviour
{

    public Slider slider;
    public TextMeshProUGUI sliderText;

    public String parameter;

    public GameObject cameraGameObject;

    // Start is called before the first frame update
    void Start()
    {
        switch (parameter)
        {
            case "finThickness":
                slider.maxValue = 0.05f *1000;
                slider.minValue = 0.001f * 1000;
                break;

            case "altitude":
                slider.maxValue = 30000;
                slider.minValue = 0;
                break;
        }

        updateSliderValue(slider.value);
    }

    void LateUpdate()
    {
        // if (GameObject.FindGameObjectWithTag("Fin Mesh") != null)
        // {
        //     Debug.Log("hi");
        //     updateSliderValue(slider.value);
        // }
    }

    // Update is called once per frame
    public void updateSliderValue(float v)
    {

        switch (parameter)
        {
            case "finThickness":
                {

                    cameraGameObject.GetComponent<FinDimensionInputManager>().finThickness = (float)v;
                    sliderText.text = cameraGameObject.GetComponent<FinDimensionInputManager>().finThickness.ToString("0.0");
                    cameraGameObject.GetComponent<FinDimensionInputManager>().generateFinMesh();
                }
                break;

            case "altitude":
                {
                    cameraGameObject.GetComponent<FinDimensionInputManager>().altitude = (float)v;
                    sliderText.text = cameraGameObject.GetComponent<FinDimensionInputManager>().altitude.ToString("0");
                    cameraGameObject.GetComponent<FinDimensionInputManager>().generateFinMesh();
                }
                break;


        }


    }
}
