using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DivergenceSpeedLabeller : MonoBehaviour
{
    // Start is called before the first frame update

    public TextMeshProUGUI textLabel;

    private float divergenceSpeed;

    private float finVolume;

    void Start()
    {
    
        
    }

    // Update is called once per frame
    void Update()
    {
        divergenceSpeed = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FinDimensionInputManager>().divergenceSpeed;
        finVolume = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FinDimensionInputManager>().finVolume;
        textLabel.text = "Divergence Velocity: " + divergenceSpeed.ToString() + " m/s\n" + $"<size=80%><i>Fin Volume: {finVolume:F2} cm<sup>3</sup></i></size>";
    }
}
