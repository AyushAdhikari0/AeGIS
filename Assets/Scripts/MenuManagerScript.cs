using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    // Start is called before the first frame update

    public string currentMenu;

    public GameObject mainMenuUI;

    public GameObject selectDimensionsUI;

    public GameObject displayCoordsObject;

    private GameObject mainCamera;

    void Start()
    {
        // mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

    }

    // Update is called once per frame
    void updateMenu()
    {
        switch (currentMenu)
        {
            case "mainMenu":
                {
                    // GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FinDimensionInputManager>().finCoords = displayCoordsObject.GetComponent<DisplayCoordinates>().temp_finCoords.ToList();


                    // bool isValid = true;

                    // implement fin geometry validation logic here


                    //

                    mainMenuUI.SetActive(true);
                    selectDimensionsUI.SetActive(false);

                    // Debug.Log("FIN COORDS");
                    // Debug.Log(GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FinDimensionInputManager>().finCoords.Count);
                    // Debug.Log("TEMP  COORDS");
                    // Debug.Log(displayCoordsObject.GetComponent<DisplayCoordinates>().temp_finCoords.Count);

                    GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FinDimensionInputManager>().generateFinMesh();


                }
                break;

            case "selectDimensionMenu":
                {

                    mainMenuUI.SetActive(false);
                    selectDimensionsUI.SetActive(true);
                    // displayCoordsObject.GetComponent<DisplayCoordinates>().initialiseVertices();
                }
                break;
        }
    }

    public void saveDesignButtonClick()
    {
        // verify vertices are valid

        if (displayCoordsObject.GetComponent<DisplayCoordinates>().finGeometryIsValid() == false)
        {
            return;
        }
        
        // save temporary fin design vertices

        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FinDimensionInputManager>().finCoords = displayCoordsObject.GetComponent<DisplayCoordinates>().temp_finCoords.ToList();


        // go to main menu
        setMainMenu();
    }

    public void cancelDesignButtonClick()
    {
        // reset temporary fin vertices 
        displayCoordsObject.GetComponent<DisplayCoordinates>().temp_finCoords = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FinDimensionInputManager>().finCoords.ToList();

        // go to main menu
        setMainMenu();
    }

    public void setMainMenu()
    {
        currentMenu = "mainMenu";
        updateMenu();
    }

    public void setSelectDimensionMenu()
    {
        currentMenu = "selectDimensionMenu";
        updateMenu();
    }
    
}
