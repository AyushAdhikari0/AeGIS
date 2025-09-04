using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraPositionManager : MonoBehaviour
{
    // Start is called before the first frame update

    private float cameraFOV;

    private float yaw = 0.0f;   // Y axis
    private float pitch = 0.0f; // X axis

    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;

    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    public GameObject menuManager;

    public GameObject displayCoordinatesObject;

    private float previousTime;
    public float timeInterval;


    // private float currentMenu;


    void Start()
    {
        // get camera FOV

        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;


        

    }

    void Update()
    {
        
        updateCameraPosition(GameObject.FindGameObjectWithTag("Menu Manager").GetComponent<MenuManager>().currentMenu);
           
    }
    // Update is called once per frame
    void updateCameraPosition(string currentMenu)
    {




        cameraFOV = gameObject.GetComponent<Camera>().fieldOfView;
        List<Vector3> finCoords = gameObject.GetComponent<FinDimensionInputManager>().finCoords;


        // find the fin mesh object if it exists

        GameObject[] existingFinMeshes = GameObject.FindGameObjectsWithTag("Fin Mesh");

        // Vector3 finCentre = gameObject.GetComponent<FinDimensionInputManager>().getFinCentre();

        Vector3 finCenter;
        float halfFOV = cameraFOV / 2;
        float biggestDimension;
        float distanceAwayFromTarget;


        if (currentMenu == "mainMenu")
        {
            GameObject finMesh = existingFinMeshes[0];
            // transform.LookAt(finMesh.transform);

            Bounds finBoundingBox = finMesh.GetComponent<MeshFilter>().mesh.bounds;
            finCenter = finBoundingBox.center;

            Vector3 finBBDimensions = finBoundingBox.size;

            biggestDimension = finBBDimensions.magnitude;



            distanceAwayFromTarget = (float)(0.5f * biggestDimension / Math.Tan(halfFOV * Math.PI / 180));

            if (Input.GetMouseButton(1)) // left mouse button held
            {
                yaw += Input.GetAxis("Mouse X") * xSpeed * Time.deltaTime;
                pitch -= Input.GetAxis("Mouse Y") * ySpeed * Time.deltaTime;
                pitch = Mathf.Clamp(pitch, yMinLimit, yMaxLimit);
            }

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f); // Z = 0 always

            // Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            // Vector3 position = rotation * negDistance + target.position;

            transform.rotation = rotation;
            // transform.position = position;

            transform.position = finCenter - transform.forward * (float)distanceAwayFromTarget;
            transform.LookAt(finCenter, transform.up);





            // transform.rotation.z = 0;

            if (Time.time - previousTime > timeInterval)
            {
                GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FinDimensionInputManager>().generateFinMesh();

                previousTime = Time.time;


            }


        }
        else if (currentMenu == "selectDimensionMenu")
        {
            // need to write code here that would centre the FOV of the camera at a 2D list of finCoords.

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

            finCenter = new Vector3((maxX + minX) / 2, (maxY + minY) / 2, 0);
            biggestDimension = Math.Max(maxX - minX, maxY - minY);
            distanceAwayFromTarget = (float)(0.5 * biggestDimension / Math.Tan(halfFOV * Math.PI / 180));

            distanceAwayFromTarget = distanceAwayFromTarget * 1.5f;

            transform.position = finCenter - Vector3.forward * (float)distanceAwayFromTarget;
            transform.LookAt(finCenter, Vector3.up);
            
                        if (Time.time - previousTime > timeInterval){
                GameObject.FindGameObjectWithTag("Display Coordinates").GetComponent<DisplayCoordinates>().refreshEdgeCylindersAndFinCoords();

                previousTime = Time.time;


            }









        }
            

        
    }
}
