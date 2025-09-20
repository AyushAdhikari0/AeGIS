using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraPositionManager : MonoBehaviour
{
    // Start is called before the first frame update

    private float cameraFOV;

    private float yaw = 0.0f;   // Y axis, yaw coordinate
    private float pitch = 0.0f; // X axis, pitch coordinate

    public float xSpeed = 120.0f;   // x and y-dir sensitivity
    public float ySpeed = 120.0f; 

    public float yMinLimit = -20f;  // min and max camera pitch 
    public float yMaxLimit = 80f;

    public GameObject menuManager;  

    public GameObject displayCoordinatesObject;

    private float previousTime;
    public float timeInterval;  // time interval to refresh drawing 2D and 3D fin mesh  

    void Start()
    {
        // get initial camera angles

        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

    }

    void Update()
    {
        // updates camera position based on current menu and mouse input
        updateCameraPosition(GameObject.FindGameObjectWithTag("Menu Manager").GetComponent<MenuManager>().currentMenu);
           
    }
    // Update is called once per frame
    void updateCameraPosition(string currentMenu)
    {
        cameraFOV = gameObject.GetComponent<Camera>().fieldOfView;

        // calculate fin dimensions
        Vector3 finCenter;
        float halfFOV = cameraFOV / 2;
        float biggestDimension;
        float distanceAwayFromTarget;


        if (currentMenu == "mainMenu")
        {
            GameObject finMesh = GameObject.FindGameObjectWithTag("Fin Mesh");

            // get largest dimension and fin center from bounding box
            Bounds finBoundingBox = finMesh.GetComponent<MeshFilter>().mesh.bounds;
            Vector3 finBBDimensions = finBoundingBox.size;

            biggestDimension = finBBDimensions.magnitude;
            finCenter = finBoundingBox.center;

            // calculate minimum required distance to fit fin in camera FOV
            distanceAwayFromTarget = (float)(0.5f * biggestDimension / Math.Tan(halfFOV * Math.PI / 180));

            // update yaw and pitch if left mouse button held
            if (Input.GetMouseButton(1))
            {
                yaw += Input.GetAxis("Mouse X") * xSpeed * Time.deltaTime;
                pitch -= Input.GetAxis("Mouse Y") * ySpeed * Time.deltaTime;
                pitch = Mathf.Clamp(pitch, yMinLimit, yMaxLimit);
            }

            // set new rotation and position
            Quaternion newRotation = Quaternion.Euler(pitch, yaw, 0f); // Z = 0 always
            transform.rotation = newRotation;

            transform.position = finCenter - transform.forward * (float)distanceAwayFromTarget;
            transform.LookAt(finCenter, transform.up);

            // refresh fin mesh every time interval
            if (Time.time - previousTime > timeInterval)
            {
                GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FinDimensionInputManager>().generateFinMesh();
                previousTime = Time.time;
            }
        }
        
        else if (currentMenu == "selectDimensionMenu")
        {

            // get biggest dimension of the 2D fin geometry
            float maxX = 0;
            float maxY = 0;
            float minX = 0;
            float minY = 0;

            foreach (Vector3 coord in displayCoordinatesObject.GetComponent<DisplayCoordinates>().temp_finCoords)
            {

                if (coord.x > maxX)
                {
                    maxX = coord.x;
                }
                else if (coord.x < minX)
                {
                    minX = coord.x;
                }

                if (coord.y > maxY)
                {
                    maxY = coord.y;
                }
                else if (coord.y < minY)
                {
                    minY = coord.y;
                }
            }

            // calculate required distance away from fin
            finCenter = new Vector3((maxX + minX) / 2, (maxY + minY) / 2, 0);
            biggestDimension = Math.Max(maxX - minX, maxY - minY);
            distanceAwayFromTarget = (float)(0.5 * biggestDimension / Math.Tan(halfFOV * Math.PI / 180));

            // add padding
            distanceAwayFromTarget = distanceAwayFromTarget * 1.5f;

            // set position and rotation
            transform.position = finCenter - Vector3.forward * (float)distanceAwayFromTarget;
            transform.LookAt(finCenter, Vector3.up);

            // refresh 2D fin geometry every time interval
            if (Time.time - previousTime > timeInterval)
            {
                GameObject.FindGameObjectWithTag("Display Coordinates").GetComponent<DisplayCoordinates>().refreshEdgeCylindersAndFinCoords();
                previousTime = Time.time;
            }
        }   
    }
}
