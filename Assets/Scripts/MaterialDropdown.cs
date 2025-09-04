using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialDropdown : MonoBehaviour
{
    // Start is called before the first frame update

    void Start()
    {
        
    }

    // Update is called once per frame
    void HandleInputData(int dropDownIndex)
    {
        switch (dropDownIndex)
        {
            case 0:
                {
                    GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FinDimensionInputManager>().finMaterial = "plywood";
                }
                break;

            case 1:
                {
                    GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FinDimensionInputManager>().finMaterial = "carbon";
                }
                break;

            case 2:
                {
                    GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FinDimensionInputManager>().finMaterial = "fibreglass";
                }
            break;
        }
        
    }
}
