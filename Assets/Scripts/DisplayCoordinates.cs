using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// using System.Numerics;
using System.Runtime.InteropServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DisplayCoordinates : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject mainCam;

    public List<Vector3> temp_finCoords;

    public float vertexSphereRadius = 4;

    public GameObject vertexSpherePrefab;

    public GameObject finCylinderPrefab;

    private bool hasGeneratedCylinders = false;

    public GameObject lastVertex;
    public GameObject secondLastVertex;
    public GameObject firstVertex;

    public Button saveButton;

    public TextMeshProUGUI errorTextBlock;
    void Start()
    {
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!hasGeneratedCylinders)
        {
            foreach (GameObject v in GameObject.FindGameObjectsWithTag("Fin Vertex Sphere"))
            {
                Destroy(v);
            }
            initialiseVertices();
            // temp_finCoords = new List<Vector3>();

            hasGeneratedCylinders = true;
            refreshEdgeCylindersAndFinCoords();
        }
        // updateTempFinCoords();
        // refreshEdgeCylindersAndFinCoords();

        if (finGeometryIsValid())
        {
            saveButton.interactable = true;
        }
        else
        {
            saveButton.interactable = false;
        }


    }

    // public void updateTempFinCoords()
    // {
    //     //updates tempfinCoords depending on the vertex sphere locations

    //     // GameObject[] spheres = GameObject.FindGameObjectsWithTag("Fin Vertex Sphere");

    //     // foreach (GameObject sphere in spheres)
    //     // {
    //     //     int sphereIndex = sphere.GetComponent<FinVertexSphereScript>().index;
    //     //     Debug.Log(sphereIndex);
    //     //     temp_finCoords[sphereIndex] = sphere.transform.position;
    //     // }

    // }

    void OnEnable()
    {
        // destroy all fin vertices that exist
        foreach (GameObject v in GameObject.FindGameObjectsWithTag("Fin Vertex Sphere"))
        {
            Destroy(v);
        }

        // initialise new vertices
        initialiseVertices();
        refreshEdgeCylindersAndFinCoords();
    }

    public void initialiseVertices()
    {

        //destroy all finVertexSpheres and finEdgeCylinders

        // List<GameObject> toDestroy = new List<GameObject>();

        // toDestroy.AddRange(GameObject.FindGameObjectsWithTag("Fin Vertex Sphere"));

        // foreach (GameObject g in toDestroy)
        // {
        //     Destroy(g);
        // }

        List<Vector3> finCoords = mainCam.GetComponent<FinDimensionInputManager>().finCoords;


        // create fin vertex spheres based on finCoords

        GameObject prev = null;

        for (int i = 0; i < finCoords.Count; i++)
        {
            int xConstrained = 0;
            int yConstrained = 0;

            if ((i == 0) || (i == finCoords.Count - 1))
            {
                xConstrained = 1;
                if (i == 0)
                {
                    yConstrained = 1;
                }
            }
            GameObject currentFinVertex = generateVertexSphere(finCoords[i], xConstrained, yConstrained);

            if (i == 0)
            {
                currentFinVertex.GetComponent<FinVertexSphereScript>().setPreviousVertex(null);
                currentFinVertex.GetComponent<FinVertexSphereScript>().setNextVertex(null);
                firstVertex = currentFinVertex;

            }
            else
            {
                currentFinVertex.GetComponent<FinVertexSphereScript>().setPreviousVertex(prev);
                currentFinVertex.GetComponent<FinVertexSphereScript>().setNextVertex(null);

                prev.GetComponent<FinVertexSphereScript>().setNextVertex(currentFinVertex);

                if (i == finCoords.Count - 1)
                {
                    lastVertex = currentFinVertex;
                }

                // if (i == finCoords.Count - 2)
                // {
                //     secondLastVertex = currentFinVertex;
                // }

            }

            prev = currentFinVertex;


        }

    }

    public void refreshEdgeCylindersAndFinCoords()
    {

        //destroy all finEdgeCylinders

        List<GameObject> toDestroy = new List<GameObject>();

        toDestroy.AddRange(GameObject.FindGameObjectsWithTag("Fin Edge Cylinder"));

        foreach (GameObject g in toDestroy)
        {
            Destroy(g);
        }


        // create fin edges using cylinders

        GameObject currentVertex = firstVertex;
        GameObject previousVertex = lastVertex;

        temp_finCoords.Clear();

        while (true)
        {


            generateEdgeCylinder(previousVertex.transform.position, currentVertex.transform.position);
            temp_finCoords.Add(currentVertex.transform.position);

            if (currentVertex == lastVertex)
            {
                break;
            }

            previousVertex = currentVertex;
            currentVertex = currentVertex.GetComponent<FinVertexSphereScript>().getNextVertex();

        }
        // for (int i = 0; i < temp_finCoords.Count; i++)
        // {
        //     if (i == temp_finCoords.Count - 1)
        //     {
        //         generateEdgeCylinder(temp_finCoords[i], temp_finCoords[0]);
        //     }
        //     else
        //     {
        //         generateEdgeCylinder(temp_finCoords[i], temp_finCoords[i + 1]);
        //     }
        // }
    }

    GameObject generateVertexSphere(Vector3 coordinate, int xConstrained, int yConstrained)
    {
        GameObject vertexSphere = Instantiate(vertexSpherePrefab, coordinate, Quaternion.Euler(xConstrained, yConstrained, 0f), transform);

        return vertexSphere;
    }

    void generateEdgeCylinder(Vector3 start, Vector3 end)
    {
        GameObject finCylinder = Instantiate(finCylinderPrefab);

        // position cylinder
        finCylinder.transform.position = (start + end) / 2;

        //scale and rotate cylinder

        Vector3 direction = end - start;
        finCylinder.transform.up = direction.normalized;

        finCylinder.transform.localScale = new Vector3(1, 1, 1) * 0.005f * Vector3.Distance(mainCam.transform.position, finCylinder.transform.position);
        finCylinder.transform.localScale = new Vector3(finCylinder.transform.localScale.x, direction.magnitude / 2, finCylinder.transform.localScale.z);

        finCylinder.tag = "Fin Edge Cylinder";
        finCylinder.transform.parent = transform;



        // GameObject arrowCylinder1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        // GameObject arrowCylinder2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
    }

    public void createNewFinVertex()
    {
        // first update the last sphere index

        // GameObject[] otherSpheres = GameObject.FindGameObjectsWithTag("Fin Vertex Sphere");
        // foreach (GameObject sphere in otherSpheres)
        // {
        //     if (sphere.GetComponent<FinVertexSphereScript>().index == temp_finCoords.Count-1)
        //     {
        //         sphere.transform.eulerAngles = new Vector3(sphere.transform.eulerAngles.x + 1, sphere.transform.eulerAngles.y, sphere.transform.eulerAngles.z);
        //     }
        // }

        secondLastVertex = lastVertex.GetComponent<FinVertexSphereScript>().previousVertex;

        Vector3 secondLastSpot = 0.5f * (lastVertex.transform.position + secondLastVertex.transform.position);

        GameObject newSphere = generateVertexSphere(secondLastSpot, 0, 0);

        // alter the last vertex link
        newSphere.GetComponent<FinVertexSphereScript>().setNextVertex(lastVertex);
        lastVertex.GetComponent<FinVertexSphereScript>().setPreviousVertex(newSphere);

        // alter the second last vertex link
        newSphere.GetComponent<FinVertexSphereScript>().setPreviousVertex(secondLastVertex);
        secondLastVertex.GetComponent<FinVertexSphereScript>().setNextVertex(newSphere);

        secondLastVertex = newSphere;

        // temp_finCoords.Insert(temp_finCoords.Count-1,secondLastSpot);
    }

    public bool finGeometryIsValid()
    {

        String statusString = "";


        //check self intersecting points

        HashSet<Vector3> pointSet = new HashSet<Vector3>();

        //checks for both self intersecting points
        foreach (Vector3 coord in temp_finCoords)
        {
            if (pointSet.Contains(coord))
            {
                statusString += "Self Intersecting Points!\n";
                break;
            }

            pointSet.Add(coord);

            // foreach (Vector3 coord2 in finCoords)
        }

        // check none of the fin edge cylinders are coloured red (intersectig)

        bool hasIntersectingEdges = false;
        int numRootChordEdges = 0;

        foreach (GameObject cylinder in GameObject.FindGameObjectsWithTag("Fin Edge Cylinder"))
        {
            if ((cylinder.transform.position.y < 0.01) && ((cylinder.transform.eulerAngles.z == -90) ||(cylinder.transform.eulerAngles.z == 90))){
                numRootChordEdges += 1;
            }

            if (cylinder.GetComponent<Renderer>().material.color == Color.red)
                {
                hasIntersectingEdges = true;
                }
        }

        if (hasIntersectingEdges) {
            statusString += "Self Intersecting Edges or Corners Too Sharp!\n";
        }

        if (numRootChordEdges > 1) {
            statusString += "Too many edges on the Root Chord!\n";           
        }

        // check that the fin geometry is clockwise by checking the relative position of the second last and last vertex

        if (lastVertex.transform.position.x <= firstVertex.transform.position.x)
        {
            statusString += "Vertex winding order must be clockwise!";
        }

        //display error message
        errorTextBlock.text = statusString;

        return statusString.Length == 0;
    }
}